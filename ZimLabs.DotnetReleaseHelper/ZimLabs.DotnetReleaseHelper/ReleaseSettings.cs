using System.IO.Compression;
using ZimLabs.DotnetReleaseHelper.Common.Enum;

namespace ZimLabs.DotnetReleaseHelper;

/// <summary>
/// Provides the release settings like the path of the solution or the csproj file
/// <para/>
/// For more information about the settings, see here: <a href="https://github.com/InvaderZim85/DotnetReleaseHelper#settings">DotnetReleaseHelper - Settings (GitHub)</a>
/// </summary>
public class ReleaseSettings
{
    /// <summary>
    /// Gets or sets the path of the solution file (<c>*.sln</c>)
    /// </summary>
    public required string SolutionFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of the project file (<c>*.csproj</c>)
    /// </summary>
    public required string ProjectFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of the publish profile (<c>*.pubxml</c>)
    /// </summary>
    public string PublishProfileFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path of the bin directory
    /// </summary>
    public required string BinDir { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicate if the bin directory should be cleared
    /// </summary>
    public bool CleanBin { get; set; }

    /// <summary>
    /// Gets or sets the version
    /// <para />
    /// If nothing is provided, a version number will be generated automatically or the <see cref="GenerateVersionNumber"/> method will be executed (if not null)
    /// </summary>
    public Version Version { get; set; } = new();

    /// <summary>
    /// Gets or sets the desired version type
    /// <para />
    /// Only needed when no version (property <see cref="Version"/>) is provided or the <see cref="GenerateVersionNumber"/> method is <see langword="null"/>
    /// </summary>
    public VersionType VersionType { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates if a zip archive of the release should be created
    /// </summary>
    public bool CreateZipArchive { get; set; }

    /// <summary>
    /// Gets or sets the name of the zip archive
    /// <para />
    /// Only needed when <see cref="CreateZipArchive"/> = <see langword="true"/>
    /// </summary>
    public string ZipArchiveName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value which indicates if the created version should be attached to the <see cref="ZipArchiveName"/>
    /// <para />
    /// Only needed when <see cref="CreateZipArchive"/> is set to <see langword="true"/>
    /// <para />
    /// <example>
    /// Example of the name creation
    /// <code>
    /// // Set to "true"
    /// ZipArchiveName_Version.zip > MyApp_1.2.3.4.zip
    ///
    /// // Set to "false"
    /// ZipArchiveName > MyApp.zip
    /// </code>
    /// </example>
    /// </summary>
    public bool AttachVersionToZipArchiveName { get; set; } = true;

    /// <summary>
    /// Gets or sets the path of the directory in which the ZIP archive is to be saved
    /// <para />
    /// Only needed when <see cref="CreateZipArchive"/> is set to <see langword="true"/>
    /// <para />
    /// <b>Note</b>: If nothing was specified, the path of the <see cref="BinDir"/> will be used
    /// </summary>
    public string ZipArchiveDestination { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the method to generate a new version number.
    /// <para />
    /// The parameter takes the "old" version number
    /// </summary>
    public Func<Version, Version>? GenerateVersionNumber { get; set; }

    /// <summary>
    /// Gets or sets the list with the custom actions
    /// </summary>
    public List<CustomAction> CustomActions { get; set; } = new();

    /// <summary>
    /// Gets or sets the compression level of the ZIP archive
    /// <para />
    /// Only needed when <see cref="CreateZipArchive"/> is set to <see langword="true"/>
    /// </summary>
    public CompressionLevel ZipCompressionLevel { get; set; } = CompressionLevel.Optimal;
}