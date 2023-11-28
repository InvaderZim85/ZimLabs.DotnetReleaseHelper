# ZimLabs.DotnetReleaseHelper

**Content**

- [General](#general)
- [Version number format](#version-number-format)
- [Settings](#settings)
- [Custom actions](#custom-actions)
- [Usage](#usage)

---

[![GitHub release (latest by date)](https://img.shields.io/github/v/release/InvaderZim85/ZimLabs.DotnetReleaseHelper)](https://github.com/InvaderZim85/ZimLabs.DotnetReleaseHelper/releases) [![Nuget](https://img.shields.io/nuget/v/ZimLabs.DotnetReleaseHelper)](https://www.nuget.org/packages/ZimLabs.DotnetReleaseHelper/)

# General

This repository provides a class library that can be used to easily create a .NET release.

The `CreateRelease` method performs the following steps:

1. Update the version number with the desired format. For more information see [Version number format](#version-number-format).
2. *Publish*
    1. Cleaning up the *bin* folder (if desired)
    2. Creation of the release with the help of "dotnet publish" (a publish profile can be specified)
3. Packing the publish folder as a ZIP archive (if desired)

For more information about the settings see [Settings](#settings)

**NOTE**: It's also possible to execute custom actions between the steps. For more information see [Custom actions](#custom-actions).

# Version number format

The following to formats are available:

1. Your own version number
2. Your own version generator
2. Year.CalendarWeek.Build.MinutesSinceMidnight
3. Year.DayOfTheYear.Build.MinutesSinceMidnight

**Explanation**

1. **Major** - *Year*: The last two digits of the current year. `2023` > `23`
2. **Minor**:
    - *CalendarWeek*: The current calendar week. **NOTE**: The german format will be used.
    - *DayOfTheYear*: The day of the year. For example, the 14.02 is the 35th day of the year
3. **Build**: The build number. The number starts with `0` and will be increased by one when the *Major* and *Minor* value of the *old* and *new* version are equal.
4. **Revision** - *MinutesSinceMidnight*: If it's 4:30 PM you will get `990`: 4 PM = 16 o'clock = `16 * 60` = `960` + `30` Minutes = `990`

# Settings

The following settings are available:

| Nr. | Property | Description | Example | Required |
|--:|---|---|---|---|
| 1. | `SolutionFile` | The path of the solution file (*.sln). | `D:\Repo\MyApp\MyApp.sln` | Yes |
| 2. | `ProjectFile` | The path of the project file (*.csproj). | `D:\Repo\MyApp\MyApp\MyApp.csproj` | Yes |
| 3. | `PublishProfileFile` | The path of your publish profile | `D:\Repo\MyApp\MyApp\Properties\PublishProfiles\FolderProfile.pubxml` | No |
| 4. | `BinDir` | The path of the *bin* directory. | `D:\Repo\MyApp\bin` | Yes |
| 5. | `CleanBin` | If set to `true`, the complete content of the directory will be deleted before the publish step. | / | No |
| 6. | `Version` | Your custom version number. **Note**: If you don't provide a version, a version will be generated (see [Version number format](#version-number-format)) and will be stored in this property so you can use it later (for example in a custom action). | / | No |
| 7. | `VersionType` | The desired version type (see [Version number format](#version-number-format)). **Note**: Only used if no version has been specified | / | No |
| 8. | `CreateZipArchive` | If set to `true`, the complete content of the *publish* directory will be added to a ZIP archive. | / | No |
| 9. | `ZipArchiveName` | The name of the ZIP archive. | `MyApp` | No |
| 10. | `AttachVersionToZipArchiveName` | If set to `true` the version number will be added to the ZIP archive name. | `MyApp_1.2.3.4.zip` | No |
| 11. | `ZipArchiveDestination` | Contains the path of the ZIP archive so you can use it in a custom action. | / | No |

# Custom actions

It's possible to perform a custom *action* between the normal steps. For example, you can use a custom action to copy some additional files to the publishing directory after the creation process so that they are added to the zip archive.

These are the *normal* steps:

1. Update the version number with the desired format. For more information see [Version number format](#version-number-format).
2. *Publish*
    1. Cleaning up the *bin* folder (if desired)
    2. Creation of the release with the help of "dotnet publish" (a publish profile can be specified)
3. Packing the publish folder as a ZIP archive (if desired)

**How to create a custom action**

A custom action has four properties:

1. *Name*: The name of the custom action (only for logging purpose)
2. *ExecutionType*: Specifies when the action should be excuted. The following options are available:
    - *BeforeVersionUpdate*: This is the first possibility (before step 1)
    - *BeforePublish*: Action will be executed after the version update and before the publish process (between step 2.1 and 2.2)
    - *AfterPublish*: Action will be executed after the publish process (between step 2 and 3)
    - *AfterZip*: Action will be executed after the zipping (after step 3)
3. *Action*: The action which should be executed
4. *StopOnException*: The value that specified whether the entire process should be stopped if an exception occurs during the execution of the action

**Example**

```csharp
public static void Main(string[] args)
{
    // Create a new instance of the release helper
    var releaseHelper = new ReleaseHelper(LogEventLevel.Information); 

    // Create the settings...
    var settings = new ReleaseSettings();

    // Create the custom action
    var customAction = new CustomAction
    {
        Name = "ExtractPackages", // The name
        Action = ExtractPackages, // The actual action
        ExecutionType = ActionExecutionType.BeforePublish, // Execute it before the publish process (between step 2.1 and 2.2)
        StopOnException = true // Stop the process when an error occurs
    };

    // Start the release
    releaseHelper.CreateRelease(settings, customAction);
}

// This method extracts the "PackageReference" entries of the project file
// and write them into a CSV file
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
```

# Usage