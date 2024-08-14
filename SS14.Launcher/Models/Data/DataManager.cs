using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using DynamicData;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Toolkit.Mvvm.Messaging;
using ReactiveUI;
using Serilog;
using SS14.Launcher.Utility;

namespace SS14.Launcher.Models.Data;

/// <summary>
/// A CVar entry in the <see cref="DataManager"/>. This is a separate object to allow data binding easily.
/// </summary>
/// <typeparam name="T">The type of value stored by the CVar.</typeparam>
public interface ICVarEntry<T> : INotifyPropertyChanged
{
    public T Value { get; set; }
}

/// <summary>
///     Handles storage of all permanent data,
///     like username, current build, favorite servers...
/// </summary>
/// <remarks>
/// All data is stored in an SQLite DB. Simple config variables are stored K/V in a single table.
/// More complex things like logins is stored in individual tables.
/// </remarks>
public sealed class DataManager : ReactiveObject
{
    private delegate void DbCommand(SqliteConnection connection);

    private readonly SourceCache<FavoriteServer, string> _favoriteServers = new(f => f.Address);

    // I have changed this to a list instead of a dictionary, as it doesn't make sense to use guid as a key for non-guid
    // auth systems.
    private readonly SourceList<LoginInfo> _logins = new();

    // When using dynamic engine management, this is used to keep track of installed engine versions.
    private readonly SourceCache<InstalledEngineVersion, string> _engineInstallations = new(v => v.Version);

    private readonly HashSet<ServerFilter> _filters = new();
    private readonly List<Hub> _hubs = new();

    private readonly Dictionary<string, CVarEntry> _configEntries = new();

    // TODO: I got lazy and this is a flat list.
    // This probably results in some bad O(n*m) behavior.
    // I don't care for now.
    private readonly List<InstalledEngineModule> _modules = new();

    private readonly List<DbCommand> _dbCommandQueue = new();
    private readonly SemaphoreSlim _dbWritingSemaphore = new(1);

    static DataManager()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        SqlMapper.AddTypeHandler(new UriTypeHandler());
    }

    public DataManager()
    {
        Filters = new ServerFilterCollection(this);
        Hubs = new HubCollection(this);
        // Set up subscriptions to listen for when the list-data (e.g. logins) changes in any way.
        // All these operations match directly SQL UPDATE/INSERT/DELETE.

        // Favorites
        _favoriteServers.Connect()
            .WhenAnyPropertyChanged()
            .Subscribe(c => ChangeFavoriteServer(ChangeReason.Update, c!));

        _favoriteServers.Connect()
            .ForEachChange(c => ChangeFavoriteServer(c.Reason, c.Current))
            .Subscribe(_ => WeakReferenceMessenger.Default.Send(new FavoritesChanged()));

        // Logins
        _logins.Connect()
            .ForEachChange(c => ChangeLogin(c.Reason, c.Item.Current)) // NOTE - does not handle range changes (do we need to?)
            .Subscribe();

        _logins.Connect()
            .WhenAnyPropertyChanged()
            .Subscribe(c => ChangeLogin(ListChangeReason.Replace, c!));

        // Engine installations. Doesn't need UPDATE because immutable.
        _engineInstallations.Connect()
            .ForEachChange(c => ChangeEngineInstallation(c.Reason, c.Current))
            .Subscribe();
    }

    public Guid Fingerprint => Guid.Parse(GetCVar(CVars.Fingerprint));

    /// <summary>
    /// Remembers which login was last being used between sessions.
    /// </summary> <summary>
    public LoginInfo? SelectedLoginInfo
    {
        get
        {
            var loginMethod = GetCVar(CVars.SelectedLoginMethod);
            if (loginMethod == "")
                return null;

            var key = GetCVar(CVars.SelectedLogin);
            if (key == "")
                return null;

            var loginInfo = LookupLoginBasedOnMethodAndArbitraryKey(loginMethod, key);

            return loginInfo;
        }

        set
        {
            if (value != null)
            {
                string arbitraryKey = GetArbitraryKey(value);

                SetCVar(CVars.SelectedLoginMethod, value.GetType().ToString());
                SetCVar(CVars.SelectedLogin, arbitraryKey);
            } else {
                SetCVar(CVars.SelectedLoginMethod, "");
                SetCVar(CVars.SelectedLogin, "");
            }

            CommitConfig();
        }
    }

    /// <summary>
    /// Get login info from in memory _logins cache object (not DB directly)
    /// </summary>
    /// <param name="method">Name of corresponding LoginInfo type</param>
    /// <param name="key">Here this means an arbitrary key (ex: guid for accounts, public key for keyauth, etc)</param>
    /// <returns></returns>
    private LoginInfo? LookupLoginBasedOnMethodAndArbitraryKey(string method, string key)
    {
        foreach (var check in _logins.Items)
        {
            if (check.GetType().ToString() == method)
            {
                if (check is LoginInfoAccount accountInfo)
                {
                    if (accountInfo.UserId.ToString() == key)
                        return accountInfo;
                } else if (check is LoginInfoKey keyInfo)
                {
                    if (keyInfo.PublicKey == key)
                        return keyInfo;
                } else if (check is LoginInfoGuest guestInfo)
                {
                    if (guestInfo.Username == key)
                        return guestInfo;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a string key for the selected login info type.  Keys are NOT unique across all login methods, but they are
    /// unique for a specific login method.  (ie: different underlying tables for different login methods.)
    /// </summary>
    /// <param name="loginInfo"></param>
    /// <returns></returns>
    private string GetArbitraryKey(LoginInfo loginInfo)
    {
        // TODO - probably change this to be more OOPy?
        if (loginInfo is LoginInfoAccount accountInfo)
        {
            return accountInfo.UserId.ToString();
        } else if (loginInfo is LoginInfoKey keyInfo)
        {
            return keyInfo.PublicKey;
        } else if (loginInfo is LoginInfoGuest guestInfo)
        {
            return guestInfo.Username;
        }

        throw new Exception("Unknown loginInfo type passed to GetArbitraryKey");
    }

    public string? Locale
    {
        get
        {
            var value = GetCVar(CVars.Locale);
            if (value == "")
                return null;

            return value;
        }

        set
        {
            if (value == null)
                SetCVar(CVars.Locale, "");
            else
                SetCVar(CVars.Locale, value);
            CommitConfig();
        }
    }

    public IObservableCache<FavoriteServer, string> FavoriteServers => _favoriteServers;
    public IObservableList<LoginInfo> Logins => _logins;
    public IObservableCache<InstalledEngineVersion, string> EngineInstallations => _engineInstallations;
    public IEnumerable<InstalledEngineModule> EngineModules => _modules;
    public ICollection<ServerFilter> Filters { get; }
    public ICollection<Hub> Hubs { get; }

    public bool ActuallyMultiAccounts => true;
    /*
        I should probably change this to be a default on but cvar'able off, I guess.
        But just doing this quick for now.
#if DEBUG
        true;
#else
            GetCVar(CVars.MultiAccounts);
#endif
*/

    public void AddFavoriteServer(FavoriteServer server)
    {
        if (_favoriteServers.Lookup(server.Address).HasValue)
        {
            throw new ArgumentException("A server with that address is already a favorite.");
        }

        _favoriteServers.AddOrUpdate(server);
    }

    public void RemoveFavoriteServer(FavoriteServer server)
    {
        _favoriteServers.Remove(server);
    }

    public void RaiseFavoriteServer(FavoriteServer server)
    {
        _favoriteServers.Remove(server);
        server.RaiseTime = DateTimeOffset.UtcNow;
        _favoriteServers.AddOrUpdate(server);
    }

    public void AddEngineInstallation(InstalledEngineVersion version)
    {
        _engineInstallations.AddOrUpdate(version);
    }

    public void RemoveEngineInstallation(InstalledEngineVersion version)
    {
        _engineInstallations.Remove(version);
    }

    public void AddEngineModule(InstalledEngineModule module)
    {
        _modules.Add(module);
        AddDbCommand(c => c.Execute("INSERT INTO EngineModule VALUES (@Name, @Version)", module));
    }

    public void RemoveEngineModule(InstalledEngineModule module)
    {
        _modules.Remove(module);
        AddDbCommand(c => c.Execute("DELETE FROM EngineModule WHERE Name = @Name AND Version = @Version", module));
    }

    public void AddLogin(LoginInfo login)
    {
        if (_logins.Items.Contains(login))
        {
            throw new ArgumentException("That login already added.");
        }

        // The above does the most basic of checks to see if the object was already added.  However, it's theoretically
        // possible for the same login to be added twice.  Calling this should do a deep check based on what each
        // account method keys off of.
        if (HasLoginInfo(login))
            return;

        _logins.Add(login);
    }

    public bool HasLoginInfo(LoginInfo loginInfo)
    {
        string arbitraryKey = "";

        if (loginInfo is LoginInfoAccount accountInfo)
        {
            arbitraryKey = accountInfo.UserId.ToString();
        } else if (loginInfo is LoginInfoKey keyInfo)
        {
            arbitraryKey = keyInfo.PublicKey;
        } else if (loginInfo is LoginInfoGuest guestInfo)
        {
            arbitraryKey = guestInfo.Username;
        }

        if (arbitraryKey == "")
            throw new ArgumentException("Could not determine a unique key for provided LoginInfo");

        var existingLogin = LookupLoginBasedOnMethodAndArbitraryKey(loginInfo.GetType().ToString(), arbitraryKey);

        return existingLogin != null;
    }

    public void RemoveLogin(LoginInfo loginInfo)
    {
        _logins.Remove(loginInfo);

        if (loginInfo == SelectedLoginInfo)
        {
            SelectedLoginInfo = null;
        }
    }

    /// <summary>
    /// Overwrites hubs in database with a new list of hubs.
    /// </summary>
    public void SetHubs(List<Hub> hubs)
    {
        Hubs.Clear();
        foreach (var hub in hubs)
        {
            Hubs.Add(hub);
        }
        CommitConfig();
    }

    /// <summary>
    /// Overwrites hubs in database with default list of hubs.
    /// </summary>
    public void ResetHubs()
    {
        Hubs.Clear();

        long priority = 0;
        foreach (string url in ConfigConstants.DefaultHubUrls)
        {
            Hubs.Add(new Hub(new Uri(url), priority++));
        }

        CommitConfig();
    }

    /// <summary>
    ///     Loads config file from disk, or resets the loaded config to default if the config doesn't exist on disk.
    /// </summary>
    public void Load()
    {
        InitializeCVars();

        using var connection = new SqliteConnection(GetCfgDbConnectionString());
        connection.Open();

        var sw = Stopwatch.StartNew();
        var success = Migrator.Migrate(connection, "SS14.Launcher.Models.Data.Migrations");

        if (!success)
            throw new Exception("Migrations failed!");

        Log.Debug("Did migrations in {MigrationTime}", sw.Elapsed);

        // Load from SQLite DB.
        // (Even on first run, some of the db sql init scripts include useful configs like for 18+ filter flag,
        // so be sure to load these.)
        LoadSqliteConfig(connection);

        if (connection.ExecuteScalar<bool>("SELECT COUNT(*) > 0 FROM Config"))
        {
        }
        else
        {
            // SQLite DB empty, fresh launcher!

            // Reset hubs to default on first run
            // (Specifying here instead of SQL so the default values don't have to be managed in two locations)
            ResetHubs();

            // Add an unused config key so the above count check is always correct.
            AddDbCommand(con => con.Execute("INSERT INTO Config VALUES ('Populated', TRUE)"));
        }

        if (GetCVar(CVars.Fingerprint) == "")
        {
            // If we don't have a fingerprint yet this is either a fresh config or an older config.
            // Generate a fingerprint and immediately save it to disk.
            SetCVar(CVars.Fingerprint, Guid.NewGuid().ToString());
        }

        CommitConfig();
    }

    private void LoadSqliteConfig(SqliteConnection sqliteConnection)
    {
        // Load account logins
        foreach (var row in sqliteConnection.Query<(Guid id, string name, string token, DateTimeOffset expires)>(
            "SELECT UserId, UserName, Token, Expires FROM Login"))
        {
            _logins.Add(new LoginInfoAccount
            {
                UserId = row.id,
                Username = row.name,
                Token = new LoginToken(row.token, row.expires)
            });
        }

        // Load MV key pair logins
        foreach (var row in sqliteConnection.Query<(string name, string publicKey, string privateKey)>(
            "SELECT UserName, PublicKey, PrivateKey FROM LoginMVKey"))
        {
            _logins.Add(new LoginInfoKey
            {
                Username = row.name,
                PublicKey = row.publicKey,
                PrivateKey = row.privateKey
            });
        }

        // Favorites
        _favoriteServers.AddOrUpdate(
            sqliteConnection.Query<(string addr, string name, DateTimeOffset raiseTime)>(
                    "SELECT Address,Name,RaiseTime FROM FavoriteServer")
                .Select(l => new FavoriteServer(l.name, l.addr, l.raiseTime)));

        // Engine installations
        _engineInstallations.AddOrUpdate(
            sqliteConnection.Query<InstalledEngineVersion>("SELECT Version,Signature FROM EngineInstallation"));

        // Engine modules
        _modules.AddRange(sqliteConnection.Query<InstalledEngineModule>("SELECT Name, Version FROM EngineModule"));

        // Load CVars.
        var configRows = sqliteConnection.Query<(string, object)>("SELECT Key, Value FROM Config");
        foreach (var (k, v) in configRows)
        {
            if (!_configEntries.TryGetValue(k, out var entry))
                continue;

            if (entry.Type == typeof(string))
                Set((string) v);
            else if (entry.Type == typeof(bool))
                Set((long) v != 0);
            else if (entry.Type == typeof(int))
                Set((int)(long) v);

            void Set<T>(T value) => ((CVarEntry<T>)entry).ValueInternal = value;
        }

        _filters.UnionWith(sqliteConnection.Query<ServerFilter>("SELECT Category, Data FROM ServerFilter"));
        _hubs.AddRange(sqliteConnection.Query<Hub>("SELECT Address,Priority FROM Hub"));

        // Avoid DB commands from config load.
        _dbCommandQueue.Clear();
    }

    private void InitializeCVars()
    {
        Debug.Assert(_configEntries.Count == 0);

        var baseMethod = typeof(DataManager)
            .GetMethod(nameof(CreateEntry), BindingFlags.NonPublic | BindingFlags.Instance)!;

        foreach (var field in typeof(CVars).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            if (!field.FieldType.IsAssignableTo(typeof(CVarDef)))
                continue;

            var def = (CVarDef)field.GetValue(null)!;
            var method = baseMethod.MakeGenericMethod(def.ValueType);
            _configEntries.Add(def.Name, (CVarEntry)method.Invoke(this, new object?[] { def })!);
        }
    }

    private CVarEntry<T> CreateEntry<T>(CVarDef<T> def)
    {
        return new CVarEntry<T>(this, def);
    }

    [SuppressMessage("ReSharper", "UseAwaitUsing")]
    public async void CommitConfig()
    {
        if (_dbCommandQueue.Count == 0)
            return;

        var commands = _dbCommandQueue.ToArray();
        _dbCommandQueue.Clear();
        Log.Debug("Committing config to disk, running {DbCommandCount} commands", commands.Length);

        await Task.Run(async () =>
        {
            // SQLite is thread safe and won't have any problems with having multiple writers
            // (but they'll be synchronous).
            // That said, we need something to wait on when we shut down to make sure everything is written, so.
            await _dbWritingSemaphore.WaitAsync();
            try
            {
                using var connection = new SqliteConnection(GetCfgDbConnectionString());
                connection.Open();
                using var transaction = connection.BeginTransaction();

                foreach (var cmd in commands)
                {
                    cmd(connection);
                }

                var sw = Stopwatch.StartNew();
                transaction.Commit();
                Log.Debug("Commit took: {CommitElapsed}", sw.Elapsed);
            }
            finally
            {
                _dbWritingSemaphore.Release();
            }
        });
    }

    public void Close()
    {
        CommitConfig();
        // Wait for any DB writes to finish to make sure we commit everything.
        _dbWritingSemaphore.Wait();
    }

    private static string GetCfgDbConnectionString()
    {
        var path = Path.Combine(LauncherPaths.DirUserData, "settings.db");
        return $"Data Source={path};Mode=ReadWriteCreate";
    }

    private void AddDbCommand(DbCommand cmd)
    {
        _dbCommandQueue.Add(cmd);
    }

    private void ChangeFavoriteServer(ChangeReason reason, FavoriteServer server)
    {
        // Make immutable copy to avoid race condition bugs.
        var data = new
        {
            server.Address,
            server.RaiseTime,
            server.Name
        };
        AddDbCommand(con =>
        {
            con.Execute(reason switch
                {
                    ChangeReason.Add => "INSERT INTO FavoriteServer VALUES (@Address, @Name, @RaiseTime)",
                    ChangeReason.Update => "UPDATE FavoriteServer SET Name = @Name, RaiseTime = @RaiseTime WHERE Address = @Address",
                    ChangeReason.Remove => "DELETE FROM FavoriteServer WHERE Address = @Address",
                    _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
                },
                data
            );
        });
    }

    private void ChangeLogin(ListChangeReason reason, LoginInfo login)
    {
        // Might be better if LoginInfo objects had database access so this class didn't have to be aware of the
        // particulars of each auth type, but exposing database access isn't currently done here so I'm going to
        // keep to the existing pattern for now.

        if (login is LoginInfoGuest)
            return; // TODO - saving/loading offline usernames

        if (login is LoginInfoKey key)
        {
            // Make immutable copy to avoid race condition bugs.
            var data = new
            {
                UserName = login.Username,
                key.PublicKey,
                key.PrivateKey
            };
            AddDbCommand(con =>
            {
                con.Execute(reason switch
                    {
                        ListChangeReason.Add =>
                            "INSERT INTO LoginMVKey (UserName, PublicKey, PrivateKey) VALUES (@UserName, @PublicKey, @PrivateKey)",
                        ListChangeReason.Replace =>
                            "UPDATE LoginMVKey SET UserName = @UserName WHERE PublicKey = @PublicKey",
                        ListChangeReason.Remove =>
                            "DELETE FROM LoginMVKey WHERE PublicKey = @PublicKey",
                        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
                    },
                    data
                );
            });
        }

        if (login is LoginInfoAccount account)
        {
            // Make immutable copy to avoid race condition bugs.
            var data = new
            {
                account.UserId,
                UserName = login.Username,
                account.Token.Token,
                Expires = account.Token.ExpireTime
            };
            AddDbCommand(con =>
            {
                con.Execute(reason switch
                    {
                        ListChangeReason.Add => "INSERT INTO Login VALUES (@UserId, @UserName, @Token, @Expires)",
                        ListChangeReason.Replace =>
                            "UPDATE Login SET UserName = @UserName, Token = @Token, Expires = @Expires WHERE UserId = @UserId",
                        ListChangeReason.Remove => "DELETE FROM Login WHERE UserId = @UserId",
                        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
                    },
                    data
                );
            });
        }
    }

    private void ChangeEngineInstallation(ChangeReason reason, InstalledEngineVersion engine)
    {
        AddDbCommand(con => con.Execute(reason switch
            {
                ChangeReason.Add => "INSERT INTO EngineInstallation VALUES (@Version, @Signature)",
                ChangeReason.Update =>
                    "UPDATE EngineInstallation SET Signature = @Signature WHERE Version = @Version",
                ChangeReason.Remove => "DELETE FROM EngineInstallation WHERE Version = @Version",
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
            },
            // Already immutable.
            engine
        ));
    }

    public T GetCVar<T>([ValueProvider("SS14.Launcher.Models.Data.CVars")] CVarDef<T> cVar)
    {
        var entry = (CVarEntry<T>)_configEntries[cVar.Name];
        return entry.Value;
    }

    public ICVarEntry<T> GetCVarEntry<T>([ValueProvider("SS14.Launcher.Models.Data.CVars")] CVarDef<T> cVar)
    {
        return (CVarEntry<T>)_configEntries[cVar.Name];
    }

    public void SetCVar<T>([ValueProvider("SS14.Launcher.Models.Data.CVars")] CVarDef<T> cVar, T value)
    {
        var name = cVar.Name;
        var entry = (CVarEntry<T>)_configEntries[cVar.Name];
        if (EqualityComparer<T>.Default.Equals(entry.ValueInternal, value))
            return;

        entry.ValueInternal = value;
        entry.FireValueChanged();

        AddDbCommand(con => con.Execute(
            "INSERT OR REPLACE INTO Config VALUES (@Key, @Value)",
            new
            {
                Key = name,
                Value = value
            }));
    }

    private abstract class CVarEntry
    {
        public abstract Type Type { get; }
    }

    private sealed class CVarEntry<T> : CVarEntry, ICVarEntry<T>
    {
        private readonly DataManager _mgr;
        private readonly CVarDef<T> _cVar;

        public CVarEntry(DataManager mgr, CVarDef<T> cVar)
        {
            _mgr = mgr;
            _cVar = cVar;
            ValueInternal = cVar.DefaultValue;
        }

        public override Type Type => typeof(T);

        public event PropertyChangedEventHandler? PropertyChanged;

        public T Value
        {
            get => ValueInternal;
            set => _mgr.SetCVar(_cVar, value);
        }

        public T ValueInternal;

        public void FireValueChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    private sealed class ServerFilterCollection : ICollection<ServerFilter>
    {
        private readonly DataManager _parent;

        public ServerFilterCollection(DataManager parent)
        {
            _parent = parent;
        }

        public IEnumerator<ServerFilter> GetEnumerator() => _parent._filters.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(ServerFilter item)
        {
            if (!_parent._filters.Add(item))
                return;

            _parent.AddDbCommand(cmd => cmd.Execute(
                "INSERT INTO ServerFilter (Category, Data) VALUES (@Category, @Data)",
                new { item.Category, item.Data}));
        }

        public void Clear()
        {
            _parent._filters.Clear();

            _parent.AddDbCommand(cmd => cmd.Execute("DELETE FROM ServerFilter"));
        }

        public bool Remove(ServerFilter item)
        {
            if (!_parent._filters.Remove(item))
                return false;

            _parent.AddDbCommand(cmd => cmd.Execute(
                "DELETE FROM ServerFilter WHERE Category = @Category AND Data = @Data",
                new { item.Category, item.Data}));

            return true;
        }

        public bool Contains(ServerFilter item) => _parent._filters.Contains(item);
        public void CopyTo(ServerFilter[] array, int arrayIndex) => _parent._filters.CopyTo(array, arrayIndex);
        public int Count => _parent._filters.Count;
        public bool IsReadOnly => false;
    }

    private sealed class HubCollection : ICollection<Hub>
    {
        private readonly DataManager _parent;

        public HubCollection(DataManager parent)
        {
            _parent = parent;
        }

        public IEnumerator<Hub> GetEnumerator() => _parent._hubs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(Hub item)
        {
            _parent._hubs.Add(item);

            _parent.AddDbCommand(cmd => cmd.Execute(
            "INSERT INTO Hub (Address, Priority) VALUES (@Address, @Priority)",
            new { item.Address, item.Priority }));
        }

        public void Clear()
        {
            _parent._hubs.Clear();

            _parent.AddDbCommand(cmd => cmd.Execute("DELETE FROM Hub"));
        }

        public bool Remove(Hub item)
        {
            if (!_parent._hubs.Remove(item))
                return false;

            _parent.AddDbCommand(cmd => cmd.Execute(
                "DELETE FROM Hub WHERE Address = @Address",
                new { item.Address, item.Priority }));

            return true;
        }

        public void CopyTo(Hub[] array, int arrayIndex) => _parent._hubs.CopyTo(array, arrayIndex);
        public bool Contains(Hub item) => _parent._hubs.Contains(item);
        public int Count => _parent._hubs.Count;
        public bool IsReadOnly => false;
    }
}

public record FavoritesChanged;
