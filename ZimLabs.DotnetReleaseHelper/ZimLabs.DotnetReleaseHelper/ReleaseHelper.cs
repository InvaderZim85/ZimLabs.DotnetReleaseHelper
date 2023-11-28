using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.IO.Compression;
using ZimLabs.DotnetReleaseHelper.Common;
using ZimLabs.DotnetReleaseHelper.Common.Enum;

namespace ZimLabs.DotnetReleaseHelper;

/// <summary>
/// Provides the functions to create a .NET release
/// </summary>
public sealed class ReleaseHelper
{
    /// <summary>
    /// Creates a new instance of the <see cref="ReleaseHelper"/> and init all needed requirements (like serilog)
    /// </summary>
    /// <param name="minLogLevel">The desired min log level (optional)</param>
    public ReleaseHelper(LogEventLevel minLogLevel = LogEventLevel.Information)
    {
        Helper.InitLog(minLogLevel);
    }

    /// <summary>
    /// Creates the release
    /// </summary>
    /// <param name="settings">The settings</param>
    /// <returns><see langword="true"/> when the publish process was successful, otherwise <see langword="false"/></returns>
    public bool CreateRelease(ReleaseSettings settings)
    {
        Log.Information("Start publish process.");

        try
        {
            // Execute the actions before the update
            ExecuteAction(settings.CustomActions, ActionExecutionType.BeforeVersionUpdate, settings, out var stopProcess);

            if (stopProcess)
                return false;

            // Update the version
            if (!UpdateVersion(settings))
                return false;

            // Clean the output directory
            if (settings.CleanBin)
            {
                Log.Information("Clean bin directory.");
                Helper.CleanDirectory(settings.BinDir);
            }

            // Execute the actions before the release
            ExecuteAction(settings.CustomActions, ActionExecutionType.BeforePublish, settings, out stopProcess);

            if (stopProcess)
                return false;

            // Create the release
            CreateRelease(settings.SolutionFile, settings.PublishProfileFile);

            // Execute the actions after the release
            ExecuteAction(settings.CustomActions, ActionExecutionType.AfterPublish, settings, out stopProcess);

            if (stopProcess)
                return false;

            // Create a zip file
            if (!settings.CreateZipArchive)
            {
                Log.Information("Done.");
                return true;
            }

            var archiveName = settings.AttachVersionToZipArchiveName
                ? $"{settings.ZipArchiveName}_v{settings.Version}.zip"
                : $"{settings.ZipArchiveName}.zip";

            var zipFileDestination = string.IsNullOrWhiteSpace(settings.ZipArchiveDestination)
                ? settings.BinDir
                : settings.ZipArchiveDestination;

            var zipFile = Path.Combine(zipFileDestination, archiveName);
            settings.ZipArchiveDestination = zipFile;
            ZipRelease(settings.BinDir, zipFile, settings.ZipCompressionLevel);

            // Execute the actions after the zipping
            ExecuteAction(settings.CustomActions, ActionExecutionType.AfterZip, settings, out stopProcess);

            return !stopProcess;
        }
        finally
        {
            Log.Information("Publish process done.");
        }
    }

    /// <summary>
    /// Updates the version number
    /// </summary>
    /// <param name="settings">The release settings</param>
    /// <returns><see langword="true"/> when the version was updated, otherwise <see langword="false"/></returns>
    private static bool UpdateVersion(ReleaseSettings settings)
    {
        var format = settings.VersionType == VersionType.VersionWithCalendarWeek
            ? "with calendar week"
            : "with day of the year";

        Log.Information("Update version number. Format: {format}", format);

        var backupPath = string.Empty;
        try
        {
            // Step 1: 
            Log.Debug("Create backup of the original file.");
            backupPath = Helper.CreateBackup(settings.ProjectFile);
            Log.Debug("Backup file: {path}", backupPath);

            // Step 2:
            Log.Debug("Load current version number from file '{name}'.", Path.GetFileName(settings.ProjectFile));
            var oldVersion = XmlHelper.GetVersionNumber(settings.ProjectFile);
            Log.Information("Old version number: {version}", oldVersion);

            if (settings.Version == new Version())
            {
                Log.Debug("Generate new version number.");
                settings.Version = settings.GenerateVersionNumber != null
                    ? settings.GenerateVersionNumber(oldVersion) // Take the provided method to generate a new version
                    : oldVersion.GenerateNewVersion(settings.VersionType);
            }

            Log.Information("New version number: {version}", settings.Version);

            // Update the version
            XmlHelper.UpdateVersionNumber(settings.ProjectFile, settings.Version);
            Log.Information("Version updated.");

            // Delete the backup file
            Log.Debug("Delete backup");
            File.Delete(backupPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error has occurred while updating the version number");
            if (string.IsNullOrEmpty(backupPath))
                return false;

            Log.Information("Perform rollback of project file.");
            Helper.RollbackFileChanges(settings.ProjectFile, backupPath);
            return false;
        }
    }

    /// <summary>
    /// Creates a new release
    /// </summary>
    /// <param name="solutionFile">The path of the solution (*.sln) file</param>
    /// <param name="publishProfile">The path of the publish profile</param>
    private static void CreateRelease(string solutionFile, string publishProfile)
    {
        try
        {
            Log.Information("Start release build process.");
            var arguments = new List<string>
            {
                "publish",
                solutionFile
            };

            if (!string.IsNullOrWhiteSpace(publishProfile) && File.Exists(publishProfile))
            {
                arguments.Add($"-p:PublishProfile={publishProfile}");
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo("dotnet", arguments)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            // Add a reader to the output
            process.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                    Log.Information(args.Data);
            };

            // Start the process
            process.Start();

            // Wait until the process is done
            process.BeginOutputReadLine();
            process.WaitForExit();
            Log.Debug("Process done.");

            // Close the process
            process.Close();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error has occurred while creating / publishing the release.");
        }
    }

    /// <summary>
    /// Creates the zip release
    /// </summary>
    /// <param name="contentDir">The path of the directory, which contains the needed data</param>
    /// <param name="zipFile">The path of the zip file</param>
    /// <param name="compressionLevel">The desired compression level</param>
    private static void ZipRelease(string contentDir, string zipFile, CompressionLevel compressionLevel)
    {
        try
        {
            var releaseDir = Helper.GetReleaseDir(contentDir);
            if (string.IsNullOrWhiteSpace(releaseDir))
            {
                Log.Warning("Can't determine release dir. Skip ZIP process.");
                return;
            }

            Log.Information("ZIP content of '{source}' into '{destination}'. Compression level: {compressionLevel}",
                releaseDir, zipFile, compressionLevel);
            ZipFile.CreateFromDirectory(releaseDir, zipFile, compressionLevel, false);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "An error has occurred while create the ZIP archive.");
        }
    }

    /// <summary>
    /// Executes the actions
    /// </summary>
    /// <param name="actions">The actions which should be executed</param>
    /// <param name="type">The execution type</param>
    /// <param name="settings">The release settings</param>
    /// <param name="stopProcess"><see langword="true"/> when the complete process should be stopped, otherwise <see langword="false"/></param>
    private static void ExecuteAction(IEnumerable<CustomAction> actions, ActionExecutionType type, ReleaseSettings settings, out bool stopProcess)
    {
        stopProcess = false;

        foreach (var action in actions.Where(w => w.ExecutionType == type))
        {
            try
            {
                Log.Information("> Execute custom action '{name}'", action.Name);

                action.Action.Invoke(settings);

                Log.Information("> Execution done.");
            }
            catch (Exception ex)
            {
                // If the whole process should be stopped, log an error and exit
                if (action.StopOnException)
                {
                    Log.Error(ex, "An error has occurred while executing the custom action '{name}'. Process will be stopped.", action.Name);
                    stopProcess = true;
                    return;
                }

                // Log only a warning
                Log.Warning(ex, "An error has occurred while executing the custom action '{name}'", action.Name);
            }
        }
    }
}