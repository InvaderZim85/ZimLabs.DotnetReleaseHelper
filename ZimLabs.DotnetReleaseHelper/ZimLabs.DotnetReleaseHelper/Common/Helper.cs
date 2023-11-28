using Serilog;
using Serilog.Events;

namespace ZimLabs.DotnetReleaseHelper.Common;

/// <summary>
/// Provides several helper functions
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Init the logger
    /// </summary>
    /// <param name="logEventLevel">The desired log level</param>
    public static void InitLog(LogEventLevel logEventLevel)
    {
        // Template
        const string template = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        // Init the logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(logEventLevel) // Set the desired min. log level
            .WriteTo.Console(outputTemplate: template) // Add the console sink
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "log", "log_.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: template) // Add the file sink
            .CreateLogger();
    }

    /// <summary>
    /// Checks if the specified file path is valid.
    /// </summary>
    /// <param name="filepath">The path of the file</param>
    /// <returns><see langword="true"/> when the path is valid, otherwise <see langword="false"/></returns>
    public static bool IsFileValid(string filepath)
    {
        return !string.IsNullOrWhiteSpace(filepath) && File.Exists(filepath);
    }

    /// <summary>
    /// Creates a backup of the desired file
    /// </summary>
    /// <param name="filepath">The path of the file</param>
    /// <returns>The path of the backup file</returns>
    public static string CreateBackup(string filepath)
    {
        var backupPath = Path.GetTempFileName();

        // We need to override the file, because the previous method generates an empty temp. file
        File.Copy(filepath, backupPath, true);

        return backupPath;
    }

    /// <summary>
    /// Executes a rollback
    /// </summary>
    /// <param name="originalFile">The original file</param>
    /// <param name="backupFile">The backup file</param>
    public static void RollbackFileChanges(string originalFile, string backupFile)
    {
        // Replace the original file with the backup
        File.Copy(backupFile, originalFile);

        // Delete the backup
        File.Delete(backupFile);
    }

    /// <summary>
    /// Cleans the directory (removes the complete content of the directory)
    /// </summary>
    /// <param name="path">The path of the directory</param>
    public static void CleanDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (!Directory.Exists(path))
            return;

        // Step 1: Delete all directories
        var subDirectories = Directory.GetDirectories(path);
        foreach (var directory in subDirectories)
        {
            Log.Debug("Delete directory '{path}'", directory);
            Directory.Delete(directory, true);
        }

        // Step 2: Delete all files
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            Log.Debug("Delete file '{file}'", file);
            File.Delete(file);
        }
    }

    /// <summary>
    /// Determines the directory which contains the release
    /// </summary>
    /// <param name="binPath">The path of the bin directory</param>
    /// <returns>The path of the directory which contains the exe</returns>
    public static string GetReleaseDir(string binPath)
    {
        var rootDir = new DirectoryInfo(binPath);
        var releaseDir = rootDir.GetDirectories("Release").FirstOrDefault();
        if (releaseDir == null)
            return string.Empty;

        // Check the "main" release dir
        if (ContainsExe(releaseDir))
            return releaseDir.FullName;

        // Check the sub directories
        var subDirectories = releaseDir.GetDirectories();
        foreach (var subDirectory in subDirectories)
        {
            if (ContainsExe(subDirectory))
                return subDirectory.FullName;

            var result = CheckSubDirectories(subDirectory);
            if (!string.IsNullOrEmpty(result))
                return result;
        }

        return string.Empty;

        // Check if the directory contains an exe file
        static bool ContainsExe(DirectoryInfo subDirInfo)
        {
            var files = subDirInfo.GetFiles("*.exe");
            return files.Length > 0;
        }

        // Check the sub directories of the given directory
        static string CheckSubDirectories(DirectoryInfo subDirInfo)
        {
            var subDirectories = subDirInfo.GetDirectories();

            foreach (var directory in subDirectories)
            {
                if (ContainsExe(directory))
                    return directory.FullName;

                var result = CheckSubDirectories(directory);
                if (!string.IsNullOrEmpty(result))
                    return result;
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Checks if the settings are valid
    /// </summary>
    /// <param name="settings">The settings</param>
    /// <returns><see langword="true"/> when the settings are valid, otherwise <see langword="false"/></returns>
    public static bool SettingsValid(this ReleaseSettings settings)
    {
        return !string.IsNullOrWhiteSpace(settings.SolutionFile) && File.Exists(settings.SolutionFile) &&
               !string.IsNullOrWhiteSpace(settings.ProjectFile) && File.Exists(settings.ProjectFile) &&
               !string.IsNullOrWhiteSpace(settings.BinDir) && Directory.Exists(settings.BinDir);
    }
}