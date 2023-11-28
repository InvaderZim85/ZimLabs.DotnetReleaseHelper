using ZimLabs.DotnetReleaseHelper.Common.Enum;

namespace ZimLabs.DotnetReleaseHelper;

/// <summary>
/// Represents a custom action
/// </summary>
public class CustomAction
{
    /// <summary>
    /// Gets or sets the name of the custom action
    /// </summary>
    public required string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the desired execution type
    /// </summary>
    public required ActionExecutionType ExecutionType { get; set; }

    /// <summary>
    /// Gets or sets the action which should be executed
    /// </summary>
    public required Action<ReleaseSettings> Action { get; set; }

    /// <summary>
    /// Gets or sets the value that specifies whether the entire process should be stopped
    /// if an exception occurs during the execution of the action
    /// </summary>
    public bool StopOnException { get; set; }
}