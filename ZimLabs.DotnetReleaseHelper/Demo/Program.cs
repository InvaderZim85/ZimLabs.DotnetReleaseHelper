using Serilog;
using System.Xml.Linq;
using ZimLabs.DotnetReleaseHelper;
using ZimLabs.DotnetReleaseHelper.Common.Enum;

namespace Demo;

internal static class Program
{
    private static void Main()
    {
        // Create the settings and set all needed values
        var settings = new ReleaseSettings
        {
            SolutionFile = @"D:\Repo\MyApp.sln",
            ProjectFile = @"D:\Repo\MyApp\MyApp.csproj",
            PublishProfileFile = @"D:\Repo\MyApp\MyApp\Properties\PublishProfiles\FolderProfile.pubxml",
            BinDir = @"D:\Repo\MyApp\MyApp\bin",
            CleanBin = true,
            VersionType = VersionType.VersionWithDayOfYear,
            CreateZipArchive = true,
            ZipArchiveName = "MyApp",
            AttachVersionToZipArchiveName = true
        };

        // Add a custom action to the settings
        settings.CustomActions.Add(new CustomAction
        {
            Name = "ExtractPackages",
            Action = ExtractPackages,
            ExecutionType = ActionExecutionType.BeforePublish,
            StopOnException = true
        });

        // Create a new instance of the release helper
        var releaseHelper = new ReleaseHelper();

        // Start the creation of the release
        var result = releaseHelper.CreateRelease(settings);
    }

    private static void ExtractPackages(ReleaseSettings settings)
    {
        var xmlDoc = XDocument.Load(settings.ProjectFile);

        var packages = (from element in xmlDoc.Descendants()
            where element.Name.LocalName.Equals("PackageReference")
            let package = element?.Attribute("Include")?.Value ?? string.Empty
            let version = element?.Attribute("Version")?.Value ?? string.Empty
            where !string.IsNullOrEmpty(package) &&
                  !string.IsNullOrEmpty(version)
            select $"{package};{version}").ToList();

        // Export the data
        Log.Information("{count} packages extracted.", packages.Count);
        File.WriteAllLines("SomePathHere", packages);
    }
}