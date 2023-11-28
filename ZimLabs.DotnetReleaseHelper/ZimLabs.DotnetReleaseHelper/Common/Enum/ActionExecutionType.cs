namespace ZimLabs.DotnetReleaseHelper.Common.Enum;

/// <summary>
/// Provides the different action execution types
/// </summary>
public enum ActionExecutionType
{
    /// <summary>
    /// Executes the action before the version number will be updated
    /// </summary>
    BeforeVersionUpdate,

    /// <summary>
    /// Executes the action before the publish
    /// </summary>
    BeforePublish,

    /// <summary>
    /// Executes the action after the publish
    /// </summary>
    AfterPublish,

    /// <summary>
    /// Executes the action after the zip process
    /// <para />
    /// Only applies if <see cref="ReleaseSettings.CreateZipArchive"/> is set to <see langword="true"/>
    /// </summary>
    AfterZip
}