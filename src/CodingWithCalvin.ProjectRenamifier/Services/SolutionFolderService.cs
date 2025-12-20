using EnvDTE;
using EnvDTE80;

namespace CodingWithCalvin.ProjectRenamifier.Services
{
    /// <summary>
    /// Service for managing solution folder operations.
    /// </summary>
    internal static class SolutionFolderService
    {
        /// <summary>
        /// Gets the solution folder that contains the specified project, if any.
        /// </summary>
        /// <param name="project">The project to find the parent folder for.</param>
        /// <returns>The parent solution folder project, or null if the project is at the solution root.</returns>
        public static Project GetParentSolutionFolder(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (project?.ParentProjectItem?.ContainingProject != null)
            {
                var parent = project.ParentProjectItem.ContainingProject;

                // Verify it's actually a solution folder
                if (parent.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
                {
                    return parent;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a project to the solution, placing it in the specified solution folder if provided.
        /// </summary>
        /// <param name="solution">The solution to add the project to.</param>
        /// <param name="projectFilePath">The full path to the project file.</param>
        /// <param name="parentSolutionFolder">The solution folder to add the project to, or null for solution root.</param>
        /// <returns>The added project.</returns>
        public static Project AddProjectToSolution(Solution solution, string projectFilePath, Project parentSolutionFolder)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (parentSolutionFolder != null)
            {
                // Add to the solution folder
                var solutionFolder = (SolutionFolder)parentSolutionFolder.Object;
                return solutionFolder.AddFromFile(projectFilePath);
            }
            else
            {
                // Add to solution root
                return solution.AddFromFile(projectFilePath);
            }
        }
    }
}
