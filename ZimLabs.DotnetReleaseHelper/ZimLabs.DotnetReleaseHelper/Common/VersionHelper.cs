using System.Globalization;
using ZimLabs.DotnetReleaseHelper.Common.Enum;

namespace ZimLabs.DotnetReleaseHelper.Common;

/// <summary>
/// Provides several functions for the interaction with a "version"
/// </summary>
internal static class VersionHelper
{
    /// <summary>
    /// Generates a new version number
    /// </summary>
    /// <param name="oldVersion">The old version</param>
    /// <param name="versionType">The desired version type</param>
    /// <returns>The new version number</returns>
    public static Version GenerateNewVersion(this Version oldVersion, VersionType versionType)
    {
        // The major number (last two digits of the current year)
        var major = DateTime.Now.Year - 2000; // We only need the last two digits, so subtract 2000 years

        // Get the minor (depends on the desired version type)
        var minor = versionType == VersionType.VersionWithCalendarWeek
            ? GetCalendarWeek()
            : DateTime.Now.DayOfYear;

        // Get the build number
        var build = major == oldVersion.Major && minor == oldVersion.Minor
            ? oldVersion.Build + 1
            : 0;

        // Get the minutes since midnight
        var revision = (int)DateTime.Now.TimeOfDay.TotalMinutes;

        return new Version(major, minor, build, revision);
    }

    /// <summary>
    /// Gets the current calendar week
    /// </summary>
    /// <returns>The calendar week</returns>
    private static int GetCalendarWeek()
    {
        return new GregorianCalendar(GregorianCalendarTypes.Localized).GetWeekOfYear(DateTime.Now,
            CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}