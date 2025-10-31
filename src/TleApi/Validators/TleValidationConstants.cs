namespace TleApi.Validators;

/// <summary>
/// Validation constants for TLE API
/// </summary>
public static class TleValidationConstants
{
    public static readonly HashSet<string> ValidSystems = new(StringComparer.Ordinal)
    {
        "GPS",
        "GLONASS",
        "GALILEO",
        "BEIDOU"
    };

    public const int MinPage = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 200;
    public const int MaxPrnLength = 10;
}
