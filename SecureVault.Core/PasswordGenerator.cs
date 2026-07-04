using System.Security.Cryptography;
using System.Text;

namespace SecureVault.Core;

public sealed class PasswordGenerator
{
    private const string Lower = "abcdefghijklmnopqrstuvwxyz";
    private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*()-_=+[]{};:,.?/";

    public string Generate(
        int length,
        bool useLower,
        bool useUpper,
        bool useDigits,
        bool useSymbols,
        bool excludeAmbiguous = false)
    {
        if (length is < 4 or > 128)
            throw new ArgumentOutOfRangeException(nameof(length));

        var pool = new StringBuilder();
        if (useLower)
            pool.Append(excludeAmbiguous ? RemoveChars(Lower, "ilo") : Lower);
        if (useUpper)
            pool.Append(excludeAmbiguous ? RemoveChars(Upper, "O") : Upper);
        if (useDigits)
            pool.Append(excludeAmbiguous ? RemoveChars(Digits, "01") : Digits);
        if (useSymbols)
            pool.Append(Symbols);

        if (pool.Length == 0)
            throw new InvalidOperationException("Select at least one character set.");

        var chars = pool.ToString().ToCharArray();
        var required = new List<char>();
        if (useLower)
            required.Add(PickOne(Lower, excludeAmbiguous));
        if (useUpper)
            required.Add(PickOne(Upper, excludeAmbiguous));
        if (useDigits)
            required.Add(PickOne(Digits, excludeAmbiguous));
        if (useSymbols)
            required.Add(PickOne(Symbols, excludeAmbiguous: false));

        var result = new char[length];
        var index = 0;
        foreach (var ch in required)
        {
            if (index >= length)
                break;
            result[index++] = ch;
        }

        while (index < length)
            result[index++] = chars[RandomNumberGenerator.GetInt32(chars.Length)];

        Shuffle(result);
        return new string(result);
    }

    private static string RemoveChars(string source, string remove)
    {
        var removeSet = remove.ToHashSet();
        var builder = new StringBuilder(source.Length);
        foreach (var ch in source)
        {
            if (!removeSet.Contains(ch))
                builder.Append(ch);
        }

        return builder.Length > 0 ? builder.ToString() : source;
    }

    private static char PickOne(string set, bool excludeAmbiguous)
    {
        var pool = excludeAmbiguous ? RemoveChars(set, "iloO01") : set;
        if (pool.Length == 0)
            pool = set;

        return pool[RandomNumberGenerator.GetInt32(pool.Length)];
    }

    private static void Shuffle(Span<char> span)
    {
        for (var i = span.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (span[i], span[j]) = (span[j], span[i]);
        }
    }
}
