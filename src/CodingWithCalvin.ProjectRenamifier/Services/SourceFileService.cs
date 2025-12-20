using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CodingWithCalvin.ProjectRenamifier.Services
{
    /// <summary>
    /// Service for updating namespace declarations in source files.
    /// </summary>
    internal static class SourceFileService
    {
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
