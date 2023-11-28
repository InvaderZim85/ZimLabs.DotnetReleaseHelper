using Serilog;
using System.Xml.Linq;

namespace ZimLabs.DotnetReleaseHelper.Common;

/// <summary>
/// Provides several functions for the interaction with a XML file
/// </summary>
internal static class XmlHelper
{
    /// <summary>
    /// Contains the list with the node names which can contain the version number
    /// </summary>
    private static readonly List<string> NodeNames = new()
    {
        "AssemblyVersion",
        "FileVersion",
        "Version"
    };

    /// <summary>
    /// Tries to get the version number
    /// </summary>
    /// <param name="filepath">The path of the file</param>
    /// <returns>The determined version. If nothing was found, a default version (1.0.0.0) will be returned</returns>
    public static Version GetVersionNumber(string filepath)
    {
        // Load the XML document
        var xmlDoc = XDocument.Load(filepath);

        // Iterate through the different node names
        foreach (var content in NodeNames.Select(nodeName => GetNodeContent(xmlDoc, nodeName)))
        {
            if (!string.IsNullOrWhiteSpace(content) && Version.TryParse(content, out var version))
                return version;
        }

        // Return a "default" version
        var defaultVersion = new Version(1, 0);
        Log.Warning("No version node found. Fallback to default version: {defaultVersion}", defaultVersion);
        return defaultVersion;
    }

    /// <summary>
    /// Tries to extract the content of the desired node
    /// </summary>
    /// <param name="xmlDoc">The XML document</param>
    /// <param name="nodeName">The name of the desired node</param>
    /// <returns>The content of the node. If nothing was found, <see cref="string.Empty"/> will be returned.</returns>
    private static string GetNodeContent(XContainer xmlDoc, string nodeName)
    {
        var entry = GetNode(xmlDoc, nodeName);

        return entry?.Value ?? string.Empty;
    }

    /// <summary>
    /// Tries to get the desired node
    /// </summary>
    /// <param name="xmlDoc">The XML document</param>
    /// <param name="nodeName">The name of the desired node</param>
    /// <returns>The element</returns>
    private static XElement? GetNode(XContainer xmlDoc, string nodeName)
    {
        return (from element in xmlDoc.Descendants()
                where element.Name.LocalName.Equals(nodeName)
                select element).FirstOrDefault();
    }

    /// <summary>
    /// Updates the version number
    /// </summary>
    /// <param name="filepath">The path of the file</param>
    /// <param name="version">The new version number</param>
    public static void UpdateVersionNumber(string filepath, Version version)
    {
        var xmlDoc = XDocument.Load(filepath);

        // Iterate through the different node names
        foreach (var xElement in NodeNames.Select(nodeName => GetNode(xmlDoc, nodeName)))
        {
            if (xElement == null)
                continue;

            xElement.Value = version.ToString();
        }

        xmlDoc.Save(filepath);
    }
}