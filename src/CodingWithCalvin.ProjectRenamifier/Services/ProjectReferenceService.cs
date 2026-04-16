using System.Collections.Generic;
using System.IO;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodingWithCalvin.ProjectRenamifier.Services
{
    /// <summary>
    /// Service for managing project references during rename operations.
    /// </summary>
    internal static class ProjectReferenceService
    {
        /// <summary>
        /// Metadata for a project that references the target project being renamed.
        /// </summary>
        public sealed class ReferencingProjectInfo
        {
            public string FullPath { get; set; }
            public string UniqueName { get; set; }
        }

        /// <summary>
        /// Finds all projects in the solution that reference the specified project.
        /// </summary>
        /// <param name="solution">The solution to search.</param>
        /// <param name="targetProjectPath">The full path to the project being renamed.</param>
        /// <returns>A list of referencing project descriptors (full path + unique name).</returns>
        public static List<ReferencingProjectInfo> FindProjectsReferencingTarget(Solution solution, string targetProjectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var referencingProjects = new List<ReferencingProjectInfo>();
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
        private static void FindReferencesInProject(Project project, string targetProjectPath, string targetFileName, List<ReferencingProjectInfo> referencingProjects)
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
                    referencingProjects.Add(new ReferencingProjectInfo
                    {
                        FullPath = project.FullName,
                        UniqueName = project.UniqueName,
                    });
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
        /// Each referencing project is temporarily unloaded via <see cref="IVsSolution4"/> so Visual Studio
        /// releases its file handle before we rewrite the .csproj on disk, then reloaded afterwards.
        /// </summary>
        /// <param name="vsSolution">The Visual Studio solution service used to unload and reload projects.</param>
        /// <param name="referencingProjects">Projects that need their references updated.</param>
        /// <param name="oldProjectPath">The old path to the renamed project.</param>
        /// <param name="newProjectPath">The new path to the renamed project.</param>
        public static void UpdateProjectReferences(IVsSolution vsSolution, List<ReferencingProjectInfo> referencingProjects, string oldProjectPath, string newProjectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var oldFileName = Path.GetFileName(oldProjectPath);
            var solution4 = vsSolution as IVsSolution4;

            foreach (var info in referencingProjects)
            {
                UpdateSingleProjectReference(vsSolution, solution4, info, oldFileName, oldProjectPath, newProjectPath);
            }
        }

        private static void UpdateSingleProjectReference(
            IVsSolution vsSolution,
            IVsSolution4 solution4,
            ReferencingProjectInfo info,
            string oldFileName,
            string oldProjectPath,
            string newProjectPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectGuid = System.Guid.Empty;
            var unloaded = false;

            if (solution4 != null && TryGetProjectGuid(vsSolution, info.UniqueName, out projectGuid))
            {
                var hr = solution4.UnloadProject(ref projectGuid, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser);
                unloaded = ErrorHandler.Succeeded(hr);
            }

            try
            {
                UpdateReferencesInProject(info.FullPath, oldFileName, oldProjectPath, newProjectPath);
            }
            finally
            {
                if (unloaded)
                {
                    solution4.ReloadProject(ref projectGuid);
                }
            }
        }

        private static bool TryGetProjectGuid(IVsSolution vsSolution, string uniqueName, out System.Guid projectGuid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            projectGuid = System.Guid.Empty;

            if (string.IsNullOrEmpty(uniqueName))
            {
                return false;
            }

            var hr = vsSolution.GetProjectOfUniqueName(uniqueName, out var hierarchy);
            if (!ErrorHandler.Succeeded(hr) || hierarchy == null)
            {
                return false;
            }

            hr = vsSolution.GetGuidOfProject(hierarchy, out projectGuid);
            return ErrorHandler.Succeeded(hr) && projectGuid != System.Guid.Empty;
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
