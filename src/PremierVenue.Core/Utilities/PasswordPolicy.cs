namespace PremierVenue.Core.Utilities;

public static class PasswordPolicy
{
    public const int MinimumLength = 8;

    public static List<string> GetValidationErrors(string password)
    {
        var errors = new List<string>();
        password ??= string.Empty;

        if (password.Length < MinimumLength)
            errors.Add($"Password must be at least {MinimumLength} characters.");
        if (!password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter.");
        if (!password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter.");
        if (!password.Any(char.IsDigit))
            errors.Add("Password must contain at least one digit.");

        return errors;
    }
}
