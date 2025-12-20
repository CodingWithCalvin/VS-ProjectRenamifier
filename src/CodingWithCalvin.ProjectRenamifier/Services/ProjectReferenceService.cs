using System.Collections.Generic;
using System.IO;
using System.Xml;
using EnvDTE;

namespace CodingWithCalvin.ProjectRenamifier.Services
{
    /// <summary>
    /// Service for managing project references during rename operations.
    /// </summary>
    internal static class ProjectReferenceService
    {
        /// <summary>
        /// Finds all projects in the solution that reference the specified project.
        /// </summary>
        /// <param name="solution">The solution to search.</param>
        /// <param name="targetProjectPath">The full path to the project being renamed.</param>
        /// <returns>A list of project paths that reference the target project.</returns>
        public static List<string> FindProjectsReferencingTarget(Solution solution, string targetProjectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var referencingProjects = new List<string>();
            var targetFileName = Path.GetFileName(targetProjectPath);

            foreach (Project project in solution.Projects)
            {
                FindReferencesInProject(project, targetProjectPath, targetFileName, referencingProjects);
            }

            return referencingProjects;
        }

        /// <summary>
        /// Recursively searches a project (and solution folders) for references to the target.
        /// </summary>
        private static void FindReferencesInProject(Project project, string targetProjectPath, string targetFileName, List<string> referencingProjects)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (project == null)
            {
                return;
            }

            // Handle solution folders
            if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    if (item.SubProject != null)
                    {
                        FindReferencesInProject(item.SubProject, targetProjectPath, targetFileName, referencingProjects);
                    }
                }
                return;
            }

            // Skip the target project itself
            if (string.Equals(project.FullName, targetProjectPath, System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Check if this project references the target
            if (!string.IsNullOrEmpty(project.FullName) && File.Exists(project.FullName))
            {
                if (ProjectReferencesTarget(project.FullName, targetFileName))
                {
                    referencingProjects.Add(project.FullName);
                }
            }
        }

        /// <summary>
        /// Checks if a project file contains a reference to the target project.
        /// </summary>
        private static bool ProjectReferencesTarget(string projectFilePath, string targetFileName)
        {
            var doc = new XmlDocument();
            doc.Load(projectFilePath);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            var msbuildNs = "http://schemas.microsoft.com/developer/msbuild/2003";
            var hasNamespace = doc.DocumentElement?.NamespaceURI == msbuildNs;

            XmlNodeList nodes;
            if (hasNamespace)
            {
                namespaceManager.AddNamespace("ms", msbuildNs);
                nodes = doc.SelectNodes("//ms:ProjectReference", namespaceManager);
            }
            else
            {
                nodes = doc.SelectNodes("//ProjectReference");
            }

            if (nodes == null)
            {
                return false;
            }

            foreach (XmlNode node in nodes)
            {
                var includeAttr = node.Attributes?["Include"]?.Value;
                if (!string.IsNullOrEmpty(includeAttr))
                {
                    var referencedFileName = Path.GetFileName(includeAttr);
                    if (string.Equals(referencedFileName, targetFileName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Updates project references in all projects that referenced the old project path.
        /// </summary>
        /// <param name="referencingProjectPaths">Projects that need their references updated.</param>
        /// <param name="oldProjectPath">The old path to the renamed project.</param>
        /// <param name="newProjectPath">The new path to the renamed project.</param>
        public static void UpdateProjectReferences(List<string> referencingProjectPaths, string oldProjectPath, string newProjectPath)
        {
            var oldFileName = Path.GetFileName(oldProjectPath);

            foreach (var projectPath in referencingProjectPaths)
            {
                UpdateReferencesInProject(projectPath, oldFileName, oldProjectPath, newProjectPath);
            }
        }

        /// <summary>
        /// Updates references in a single project file.
        /// </summary>
        private static void UpdateReferencesInProject(string projectFilePath, string oldFileName, string oldProjectPath, string newProjectPath)
        {
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(projectFilePath);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            var msbuildNs = "http://schemas.microsoft.com/developer/msbuild/2003";
            var hasNamespace = doc.DocumentElement?.NamespaceURI == msbuildNs;

            XmlNodeList nodes;
            if (hasNamespace)
            {
                namespaceManager.AddNamespace("ms", msbuildNs);
                nodes = doc.SelectNodes("//ms:ProjectReference", namespaceManager);
            }
            else
            {
                nodes = doc.SelectNodes("//ProjectReference");
            }

            if (nodes == null)
            {
                return;
            }

            var modified = false;
            var projectDirectory = Path.GetDirectoryName(projectFilePath);

            foreach (XmlNode node in nodes)
            {
                var includeAttr = node.Attributes?["Include"];
                if (includeAttr == null || string.IsNullOrEmpty(includeAttr.Value))
                {
                    continue;
                }

                var referencedFileName = Path.GetFileName(includeAttr.Value);
                if (!string.Equals(referencedFileName, oldFileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Calculate new relative path from referencing project to renamed project
                var newRelativePath = GetRelativePath(projectDirectory, newProjectPath);
                includeAttr.Value = newRelativePath;
                modified = true;
            }

            if (modified)
            {
                doc.Save(projectFilePath);
            }
        }

        /// <summary>
        /// Gets a relative path from one directory to a file.
        /// </summary>
        private static string GetRelativePath(string fromDirectory, string toFile)
        {
            var fromUri = new System.Uri(fromDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
            var toUri = new System.Uri(toFile);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
