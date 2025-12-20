using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE;

namespace CodingWithCalvin.ProjectRenamifier.Services
{
    /// <summary>
    /// Service for updating namespace declarations and using statements in source files.
    /// </summary>
    internal static class SourceFileService
    {
        /// <summary>
        /// Updates fully qualified type references in all .cs files across the entire solution.
        /// For example: OldName.MyClass → NewName.MyClass
        /// </summary>
        /// <param name="solution">The solution to scan.</param>
        /// <param name="oldNamespace">The old namespace to find.</param>
        /// <param name="newNamespace">The new namespace to replace with.</param>
        /// <returns>The number of files modified.</returns>
        public static int UpdateFullyQualifiedReferencesInSolution(Solution solution, string oldNamespace, string newNamespace)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var modifiedCount = 0;

            foreach (Project project in solution.Projects)
            {
                modifiedCount += UpdateFullyQualifiedReferencesInProjectTree(project, oldNamespace, newNamespace);
            }

            return modifiedCount;
        }

        /// <summary>
        /// Recursively updates fully qualified references in a project (handles solution folders).
        /// </summary>
        private static int UpdateFullyQualifiedReferencesInProjectTree(Project project, string oldNamespace, string newNamespace)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (project == null)
            {
                return 0;
            }

            // Handle solution folders
            if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
            {
                var count = 0;
                foreach (ProjectItem item in project.ProjectItems)
                {
                    if (item.SubProject != null)
                    {
                        count += UpdateFullyQualifiedReferencesInProjectTree(item.SubProject, oldNamespace, newNamespace);
                    }
                }
                return count;
            }

            // Process actual project
            if (!string.IsNullOrEmpty(project.FullName) && File.Exists(project.FullName))
            {
                var projectDirectory = Path.GetDirectoryName(project.FullName);
                if (!string.IsNullOrEmpty(projectDirectory) && Directory.Exists(projectDirectory))
                {
                    return UpdateFullyQualifiedReferencesInDirectory(projectDirectory, oldNamespace, newNamespace);
                }
            }

            return 0;
        }

        /// <summary>
        /// Updates fully qualified references in all .cs files within a directory.
        /// </summary>
        private static int UpdateFullyQualifiedReferencesInDirectory(string directory, string oldNamespace, string newNamespace)
        {
            var csFiles = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            var modifiedCount = 0;

            foreach (var filePath in csFiles)
            {
                if (UpdateFullyQualifiedReferencesInFile(filePath, oldNamespace, newNamespace))
                {
                    modifiedCount++;
                }
            }

            return modifiedCount;
        }

        /// <summary>
        /// Updates fully qualified type references in a single source file.
        /// Matches patterns like OldName.MyClass, OldName.Sub.Type but not SomeOldName.Type
        /// </summary>
        /// <param name="filePath">Full path to the .cs file.</param>
        /// <param name="oldNamespace">The old namespace to find.</param>
        /// <param name="newNamespace">The new namespace to replace with.</param>
        /// <returns>True if the file was modified, false otherwise.</returns>
        public static bool UpdateFullyQualifiedReferencesInFile(string filePath, string oldNamespace, string newNamespace)
        {
            var encoding = DetectEncoding(filePath);
            var content = File.ReadAllText(filePath, encoding);
            var originalContent = content;

            // Pattern matches OldName followed by a dot and an identifier
            // Uses word boundary \b to avoid matching partial names like SomeOldName
            // Matches: OldName.Class, OldName.Sub.Class, etc.
            var pattern = $@"\b{Regex.Escape(oldNamespace)}\.";
            content = Regex.Replace(content, pattern, $"{newNamespace}.");

            if (content != originalContent)
            {
                File.WriteAllText(filePath, content, encoding);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates using statements in all .cs files across the entire solution.
        /// </summary>
        /// <param name="solution">The solution to scan.</param>
        /// <param name="oldNamespace">The old namespace to find.</param>
        /// <param name="newNamespace">The new namespace to replace with.</param>
        /// <returns>The number of files modified.</returns>
        public static int UpdateUsingStatementsInSolution(Solution solution, string oldNamespace, string newNamespace)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var modifiedCount = 0;

            foreach (Project project in solution.Projects)
            {
                modifiedCount += UpdateUsingStatementsInProjectTree(project, oldNamespace, newNamespace);
            }

            return modifiedCount;
        }

        /// <summary>
        /// Recursively updates using statements in a project (handles solution folders).
        /// </summary>
        private static int UpdateUsingStatementsInProjectTree(Project project, string oldNamespace, string newNamespace)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (project == null)
            {
                return 0;
            }

            // Handle solution folders
            if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
            {
                var count = 0;
                foreach (ProjectItem item in project.ProjectItems)
                {
                    if (item.SubProject != null)
                    {
                        count += UpdateUsingStatementsInProjectTree(item.SubProject, oldNamespace, newNamespace);
                    }
                }
                return count;
            }

            // Process actual project
            if (!string.IsNullOrEmpty(project.FullName) && File.Exists(project.FullName))
            {
                var projectDirectory = Path.GetDirectoryName(project.FullName);
                if (!string.IsNullOrEmpty(projectDirectory) && Directory.Exists(projectDirectory))
                {
                    return UpdateUsingStatementsInDirectory(projectDirectory, oldNamespace, newNamespace);
                }
            }

            return 0;
        }

        /// <summary>
        /// Updates using statements in all .cs files within a directory.
        /// </summary>
        private static int UpdateUsingStatementsInDirectory(string directory, string oldNamespace, string newNamespace)
        {
            var csFiles = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            var modifiedCount = 0;

            foreach (var filePath in csFiles)
            {
                if (UpdateUsingStatementsInFile(filePath, oldNamespace, newNamespace))
                {
                    modifiedCount++;
                }
            }

            return modifiedCount;
        }

        /// <summary>
        /// Updates using statements in a single source file.
        /// </summary>
        /// <param name="filePath">Full path to the .cs file.</param>
        /// <param name="oldNamespace">The old namespace to find.</param>
        /// <param name="newNamespace">The new namespace to replace with.</param>
        /// <returns>True if the file was modified, false otherwise.</returns>
        public static bool UpdateUsingStatementsInFile(string filePath, string oldNamespace, string newNamespace)
        {
            var encoding = DetectEncoding(filePath);
            var content = File.ReadAllText(filePath, encoding);
            var originalContent = content;

            // Pattern for: using OldName;
            // Also handles nested: using OldName.SubNamespace;
            var usingPattern = $@"(\busing\s+){Regex.Escape(oldNamespace)}(\s*;|\.[\w.]*\s*;)";
            content = Regex.Replace(content, usingPattern, $"$1{newNamespace}$2");

            // Pattern for using aliases: using Alias = OldName.Type;
            // Also handles: using Alias = OldName;
            var aliasPattern = $@"(\busing\s+\w+\s*=\s*){Regex.Escape(oldNamespace)}(\.[\w.]*)?(\s*;)";
            content = Regex.Replace(content, aliasPattern, $"$1{newNamespace}$2$3");

            // Pattern for global using: global using OldName;
            var globalUsingPattern = $@"(\bglobal\s+using\s+){Regex.Escape(oldNamespace)}(\s*;|\.[\w.]*\s*;)";
            content = Regex.Replace(content, globalUsingPattern, $"$1{newNamespace}$2");

            // Pattern for using static: using static OldName.ClassName;
            var staticUsingPattern = $@"(\busing\s+static\s+){Regex.Escape(oldNamespace)}(\.[\w.]+\s*;)";
            content = Regex.Replace(content, staticUsingPattern, $"$1{newNamespace}$2");

            if (content != originalContent)
            {
                File.WriteAllText(filePath, content, encoding);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates namespace declarations in all .cs files within the project directory.
        /// </summary>
        /// <param name="projectFilePath">Full path to the .csproj file.</param>
        /// <param name="oldNamespace">The old namespace to find.</param>
        /// <param name="newNamespace">The new namespace to replace with.</param>
        /// <returns>The number of files modified.</returns>
        public static int UpdateNamespacesInProject(string projectFilePath, string oldNamespace, string newNamespace)
        {
            var projectDirectory = Path.GetDirectoryName(projectFilePath);
            if (string.IsNullOrEmpty(projectDirectory))
            {
                return 0;
            }

            var csFiles = Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);
            var modifiedCount = 0;

            foreach (var filePath in csFiles)
            {
                if (UpdateNamespacesInFile(filePath, oldNamespace, newNamespace))
                {
                    modifiedCount++;
                }
            }

            return modifiedCount;
        }

        /// <summary>
        /// Updates namespace declarations in a single source file.
        /// </summary>
        /// <param name="filePath">Full path to the .cs file.</param>
        /// <param name="oldNamespace">The old namespace to find.</param>
        /// <param name="newNamespace">The new namespace to replace with.</param>
        /// <returns>True if the file was modified, false otherwise.</returns>
        public static bool UpdateNamespacesInFile(string filePath, string oldNamespace, string newNamespace)
        {
            // Detect file encoding
            var encoding = DetectEncoding(filePath);
            var content = File.ReadAllText(filePath, encoding);
            var originalContent = content;

            // Pattern for block-scoped namespace: namespace OldName { or namespace OldName\n{
            // Also handles nested: namespace OldName.Something
            var blockPattern = $@"(\bnamespace\s+){Regex.Escape(oldNamespace)}(\s*[\{{\r\n]|\.|\s*$)";
            content = Regex.Replace(content, blockPattern, $"$1{newNamespace}$2");

            // Pattern for file-scoped namespace: namespace OldName;
            // Also handles nested: namespace OldName.Something;
            var fileScopedPattern = $@"(\bnamespace\s+){Regex.Escape(oldNamespace)}(\.[\w.]*)?(\s*;)";
            content = Regex.Replace(content, fileScopedPattern, $"$1{newNamespace}$2$3");

            if (content != originalContent)
            {
                File.WriteAllText(filePath, content, encoding);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Detects the encoding of a file, defaulting to UTF-8 if not determinable.
        /// </summary>
        private static Encoding DetectEncoding(string filePath)
        {
            // Read the BOM to detect encoding
            var bom = new byte[4];
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Detect encoding from BOM
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
            {
                return Encoding.UTF8;
            }
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0)
            {
                return Encoding.UTF32;
            }
            if (bom[0] == 0xff && bom[1] == 0xfe)
            {
                return Encoding.Unicode; // UTF-16 LE
            }
            if (bom[0] == 0xfe && bom[1] == 0xff)
            {
                return Encoding.BigEndianUnicode; // UTF-16 BE
            }

            // Default to UTF-8 without BOM
            return new UTF8Encoding(false);
        }
    }
}
