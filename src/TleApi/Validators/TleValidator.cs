using System.Globalization;

namespace TleApi.Validators;

/// <summary>
/// Static validator methods for TLE API parameters
/// </summary>
public static class TleValidator
{
    public static bool IsValidSystem(string? system, out string? errorMessage)
    {
        errorMessage = null;
        
        if (string.IsNullOrWhiteSpace(system))
        {
            return true; // Optional parameter
        }

        if (!TleValidationConstants.ValidSystems.Contains(system))
        {
            errorMessage = $"Invalid system. Must be one of: {string.Join(", ", TleValidationConstants.ValidSystems)}";
            return false;
        }

        return true;
    }

    public static bool IsValidPrn(string? prn, out string? errorMessage)
    {
        errorMessage = null;
        
        if (string.IsNullOrWhiteSpace(prn))
        {
            return true; // Optional parameter
        }

        var trimmedPrn = prn.Trim();
        if (trimmedPrn != prn)
        {
            errorMessage = "PRN cannot have leading or trailing whitespace";
            return false;
        }

        if (prn.Length > TleValidationConstants.MaxPrnLength)
        {
            errorMessage = $"PRN length cannot exceed {TleValidationConstants.MaxPrnLength} characters";
            return false;
        }

        return true;
    }

    public static bool IsValidDateTime(string? datetime, out DateTime? parsedDateTime, out string? errorMessage)
    {
        errorMessage = null;
        parsedDateTime = null;
        
        if (string.IsNullOrWhiteSpace(datetime))
        {
            return true; // Optional parameter
        }

        if (!DateTime.TryParse(datetime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
        {
            errorMessage = "Invalid datetime format. Expected ISO 8601 format";
            return false;
        }

        parsedDateTime = dt.ToUniversalTime();
        return true;
    }

    public static bool IsValidPagination(int page, int pageSize, out string? errorMessage)
    {
        errorMessage = null;

        if (page < TleValidationConstants.MinPage)
        {
            errorMessage = $"Page must be >= {TleValidationConstants.MinPage}";
            return false;
        }

        if (pageSize < TleValidationConstants.MinPageSize || pageSize > TleValidationConstants.MaxPageSize)
        {
            errorMessage = $"PageSize must be between {TleValidationConstants.MinPageSize} and {TleValidationConstants.MaxPageSize}";
            return false;
        }

        return true;
    }
}
