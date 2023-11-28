namespace ZimLabs.DotnetReleaseHelper.Common.Enum;

/// <summary>
/// Provides the different version types
/// </summary>
public enum VersionType
{
    /// <summary>
    /// Creates a version, where the minor version represents the current calendar week
    /// </summary>
    VersionWithCalendarWeek,

    /// <summary>
    /// Creates a version, where the minor version represents the day of the year
    /// </summary>
    VersionWithDayOfYear,

    /// <summary>
    /// Custom version number
    /// </summary>
    Custom
}