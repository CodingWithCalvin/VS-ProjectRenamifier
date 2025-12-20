using System.IO;
using System.Xml;

namespace CodingWithCalvin.ProjectRenamifier.Services
{
    /// <summary>
    /// Service for updating project file (.csproj) elements.
    /// </summary>
    internal static class ProjectFileService
    {
        /// <summary>
        /// Renames the project file on disk.
        /// </summary>
        /// <param name="projectFilePath">Full path to the current .csproj file.</param>
        /// <param name="newName">The new project name (without extension).</param>
        /// <returns>The new full path to the renamed project file.</returns>
        public static string RenameProjectFile(string projectFilePath, string newName)
        {
            var directory = Path.GetDirectoryName(projectFilePath);
            var extension = Path.GetExtension(projectFilePath);
            var newFileName = newName + extension;
            var newFilePath = Path.Combine(directory, newFileName);

            File.Move(projectFilePath, newFilePath);

            return newFilePath;
        }

        /// <summary>
        /// Renames the parent directory if its name matches the old project name.
        /// </summary>
        /// <param name="projectFilePath">Full path to the .csproj file.</param>
        /// <param name="oldName">The old project name to match against.</param>
        /// <param name="newName">The new project name.</param>
        /// <returns>The new full path to the project file after directory rename, or the original path if no rename occurred.</returns>
        public static string RenameParentDirectoryIfMatches(string projectFilePath, string oldName, string newName)
        {
            var projectDirectory = Path.GetDirectoryName(projectFilePath);
            var parentDirectory = Directory.GetParent(projectDirectory);

            if (parentDirectory == null)
            {
                return projectFilePath;
            }

            var directoryName = new DirectoryInfo(projectDirectory).Name;

            // Only rename if directory name matches the old project name
            if (!directoryName.Equals(oldName, StringComparison.OrdinalIgnoreCase))
            {
                return projectFilePath;
            }

            var newDirectoryPath = Path.Combine(parentDirectory.FullName, newName);
            Directory.Move(projectDirectory, newDirectoryPath);

            // Return the new project file path
            var fileName = Path.GetFileName(projectFilePath);
            return Path.Combine(newDirectoryPath, fileName);
        }

        /// <summary>
        /// Updates the RootNamespace and AssemblyName elements in a project file
        /// if they match the old project name.
        /// </summary>
        /// <param name="projectFilePath">Full path to the .csproj file.</param>
        /// <param name="oldName">The old project name to match against.</param>
        /// <param name="newName">The new project name to set.</param>
        /// <returns>True if any changes were made, false otherwise.</returns>
        public static bool UpdateProjectFile(string projectFilePath, string oldName, string newName)
        {
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(projectFilePath);

            var modified = false;

            modified |= UpdateElement(doc, "RootNamespace", oldName, newName);
            modified |= UpdateElement(doc, "AssemblyName", oldName, newName);

            if (modified)
            {
                doc.Save(projectFilePath);
            }

            return modified;
        }

        /// <summary>
        /// Updates a specific element in the project file if its value matches the old name.
        /// </summary>
        private static bool UpdateElement(XmlDocument doc, string elementName, string oldName, string newName)
        {
            // Handle both SDK-style (no namespace) and legacy (with namespace) project files
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            var msbuildNs = "http://schemas.microsoft.com/developer/msbuild/2003";

            // Check if document uses the MSBuild namespace
            var hasNamespace = doc.DocumentElement?.NamespaceURI == msbuildNs;

            XmlNodeList nodes;
            if (hasNamespace)
            {
                namespaceManager.AddNamespace("ms", msbuildNs);
                nodes = doc.SelectNodes($"//ms:{elementName}", namespaceManager);
            }
            else
            {
                nodes = doc.SelectNodes($"//{elementName}");
            }

            if (nodes == null || nodes.Count == 0)
            {
                return false;
            }

            var modified = false;
            foreach (XmlNode node in nodes)
            {
                if (node.InnerText.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                {
                    node.InnerText = newName;
                    modified = true;
                }
            }

            return modified;
        }
    }
}
