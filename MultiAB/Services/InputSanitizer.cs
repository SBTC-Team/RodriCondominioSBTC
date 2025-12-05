using System.Text.RegularExpressions;

namespace MultiAB.Services;

/// <summary>
/// Servicio para sanitizar entrada del usuario y prevenir inyección
/// </summary>
public interface IInputSanitizer
{
    string SanitizeString(string input);
    bool IsValidEmail(string email);
    bool ContainsSqlInjection(string input);
    bool ContainsXss(string input);
}

public class InputSanitizer : IInputSanitizer
{
    private static readonly Regex SqlInjectionPattern = new(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|SCRIPT)\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex XssPattern = new(
        @"<[^>]*>|javascript:|on\w+\s*=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    public string SanitizeString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remover caracteres peligrosos
        var sanitized = input.Trim();
        
        // Remover caracteres de control
        sanitized = Regex.Replace(sanitized, @"[\x00-\x1F\x7F]", "");
        
        return sanitized;
    }

    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailPattern.IsMatch(email) && email.Length <= 255;
    }

    public bool ContainsSqlInjection(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return SqlInjectionPattern.IsMatch(input);
    }

    public bool ContainsXss(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return XssPattern.IsMatch(input);
    }
}

