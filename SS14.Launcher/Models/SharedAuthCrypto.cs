using System;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using static SS14.Launcher.Models.Connector;

namespace SS14.Launcher.Models;

/// <summary>
/// This file mirrors a number of lines of code from Robust.Shared.
/// (Possibly some of this code should be moved to a shared lib at some point where both projects can refer to it.)
/// </summary>
public static class SharedAuthCrypto
{
    internal const int SharedKeyLength = 32; // Match the Robust.Shared const
    internal const int ExpectedPublicKeyByteLength = 32; // Match SpaceWizards.Sodium.CryptoBox.PublicKeyBytes

    static internal (string authHash, string sharedSecret) GenerateSharedSecret(ServerInfo info)
    {
        var sharedSecret = new byte[SharedKeyLength];
        RandomNumberGenerator.Fill(sharedSecret);

        var serverPublicKeyBytes = Convert.FromBase64String(info.AuthInformation.PublicKey);

        if (serverPublicKeyBytes.Length != ExpectedPublicKeyByteLength)
        {
            var msg = $"Invalid public key length. Expected {ExpectedPublicKeyByteLength}, but was {serverPublicKeyBytes.Length}.";
            Log.Error(msg);
            throw new ConnectException(ConnectionStatus.ConnectionFailed);
        }

        // Data is [shared]+[verify]
        // var data = new byte[sharedSecret.Length + encRequest.VerifyToken.Length];
        // sharedSecret.CopyTo(data.AsSpan());
        // encRequest.VerifyToken.CopyTo(data.AsSpan(sharedSecret.Length));

        var authHashBytes = MakeAuthHash(sharedSecret, serverPublicKeyBytes!);
        var authHash = ConvertToBase64Url(authHashBytes);

        return (authHash, System.Convert.ToBase64String(sharedSecret));
    }

    private static byte[] MakeAuthHash(byte[] sharedSecret, byte[] pkBytes)
    {
        // This function mirrors Robust.Shared NetManager.MakeAuthHash

        var incHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        incHash.AppendData(sharedSecret);
        incHash.AppendData(pkBytes);
        return incHash.GetHashAndReset();
    }

    public static string ConvertToBase64Url(byte[]? data)
    {
        // This function mirrors Robust.Shared Base64Helpers.ConvertToBase64Url

        return data == null ? "" : ConvertToBase64Url(Convert.ToBase64String(data));
    }

    /// <summary>
    /// Converts a a Base64 string to one that is URL safe.
    /// </summary>
    /// <returns>A base64url formed string.</returns>
    public static string ConvertToBase64Url(string b64Str)
    {
        // This function mirrors Robust.Shared Base64Helpers.ConvertToBase64Url

        if (b64Str is null)
        {
            throw new ArgumentNullException(nameof(b64Str));
        }

        var cut = b64Str[^1] == '=' ? b64Str[^2] == '=' ? 2 : 1 : 0;
        b64Str = new StringBuilder(b64Str).Replace('+', '-').Replace('/', '_').ToString(0, b64Str.Length - cut);
        return b64Str;
    }
}
