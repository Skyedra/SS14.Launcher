using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using Serilog;
using SS14.Launcher.Api;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.Data;
using SS14.Launcher.Models.EngineManager;
using SS14.Launcher.Models.Logins;
using SS14.Launcher.Utility;
using Splat;

namespace SS14.Launcher.Models;

/// <summary>
/// Logic for storing/retrieving user's age.
/// /// </summary>
public sealed class AgeManager : ReactiveObject
{
    private readonly DataManager _cfg;

    /// <summary>
    /// Below this is considered implausible for a user to have registered themselves.
    /// </summary>
    private const int MINIMUM_DAYS_FOR_VALID_AGE = 3;

    /// <summary>
    /// Above this is considered implausible for a user to have registered themselves.
    /// </summary>
    private const int MAXIMUM_DAYS_FOR_VALID_AGE = 120;

    public AgeManager()
    {
        _cfg = Locator.Current.GetRequiredService<DataManager>();
        _birthDate = _cfg.BirthDate;
    }

    public bool AgeKnown
    {
        get
        {
            return BirthDate.ToDateTime(TimeOnly.MinValue) < DateTime.Now;
        }
    }

    public DateOnly BirthDate
    {
        get => _birthDate;
        set
        {
            this.RaiseAndSetIfChanged(ref _birthDate, value);
            this.RaisePropertyChanged(nameof(BirthDate));
            this.RaisePropertyChanged(nameof(AgeKnown));
            _cfg.BirthDate = value;
        }
    }
    private DateOnly _birthDate = DateOnly.MaxValue;

    public bool IsValidAge(DateOnly testAge)
    {
        int yearsOld = YearsOld(testAge);

        return
            yearsOld >= MINIMUM_DAYS_FOR_VALID_AGE &&
            yearsOld <= MAXIMUM_DAYS_FOR_VALID_AGE;
    }

    public int YearsOld(DateOnly testAge)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Now);

        // Age Calculation from https://stackoverflow.com/a/4127396

        DateTime zeroTime = new DateTime(1, 1, 1);

        DateTime birthDateTime = new DateTime(testAge.Year, testAge.Month, testAge.Day);
        DateTime currentDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day);

        if (birthDateTime >= currentDateTime)
            return 0;

        TimeSpan span = currentDateTime - birthDateTime;
        span = span.Subtract(TimeSpan.FromDays(1)); // Comment about it being off by a day, which seems accurate based on my testing
        // Because we start at year 1 for the Gregorian
        // calendar, we must subtract a year here.
        int years = (zeroTime + span).Year - 1;

        return years;
    }
}
