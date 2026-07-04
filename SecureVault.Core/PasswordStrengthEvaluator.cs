using System.Text.RegularExpressions;

namespace SecureVault.Core;

public enum PasswordStrengthBand
{
    Slaba,
    Medie,
    Puternica,
}

public static class PasswordStrengthEvaluator
{
    public static bool IsLocallyWeak(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return true;

        var hasLower = Regex.IsMatch(password, "[a-z]");
        var hasUpper = Regex.IsMatch(password, "[A-Z]");
        var hasDigit = Regex.IsMatch(password, "[0-9]");
        var hasSymbol = Regex.IsMatch(password, @"[\W_]", RegexOptions.CultureInvariant);

        if (password.Length < 12)
            return true;
        if (!hasLower || !hasUpper || !hasDigit)
            return true;
        if (!hasSymbol)
            return true;

        return false;
    }

    public static PasswordStrengthBand GetBand(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return PasswordStrengthBand.Slaba;

        if (IsLocallyWeak(password))
        {
            if (password.Length >= 10
                && Regex.IsMatch(password, "[a-z]")
                && Regex.IsMatch(password, "[A-Z]")
                && Regex.IsMatch(password, "[0-9]"))
            {
                return PasswordStrengthBand.Medie;
            }

            return PasswordStrengthBand.Slaba;
        }

        if (password.Length >= 16 && HasVariety(password))
            return PasswordStrengthBand.Puternica;

        return PasswordStrengthBand.Medie;
    }

    public static double BandToRatio(PasswordStrengthBand band) => band switch
    {
        PasswordStrengthBand.Slaba => 0.33,
        PasswordStrengthBand.Medie => 0.66,
        PasswordStrengthBand.Puternica => 1.0,
        _ => 0.0,
    };

    public static string BandDisplayNameRo(PasswordStrengthBand band) => band switch
    {
        PasswordStrengthBand.Slaba => "Slabă",
        PasswordStrengthBand.Medie => "Medie",
        PasswordStrengthBand.Puternica => "Puternică",
        _ => string.Empty,
    };

    private static bool HasVariety(string password)
    {
        var kinds = 0;
        if (Regex.IsMatch(password, "[a-z]"))
            kinds++;
        if (Regex.IsMatch(password, "[A-Z]"))
            kinds++;
        if (Regex.IsMatch(password, "[0-9]"))
            kinds++;
        if (Regex.IsMatch(password, @"[\W_]", RegexOptions.CultureInvariant))
            kinds++;

        return kinds >= 4;
    }
}
