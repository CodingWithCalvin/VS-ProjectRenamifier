using System.ComponentModel.Design;
using System.IO;
using System.Windows.Interop;
using CodingWithCalvin.ProjectRenamifier.Dialogs;
using CodingWithCalvin.ProjectRenamifier.Services;
using EnvDTE;
using EnvDTE80;

namespace CodingWithCalvin.ProjectRenamifier
{
    internal sealed class RenamifyProjectCommand
    {
        private readonly Package _package;

        private IServiceProvider ServiceProvider => _package;

        public static void Initialize(Package package)
        {
            _ = new RenamifyProjectCommand(package);
        }

        private RenamifyProjectCommand(Package package)
        {
            _package = package;

            var commandService = (OleMenuCommandService)
                ServiceProvider.GetService(typeof(IMenuCommandService));

            if (commandService == null)
            {
                return;
            }

            var menuCommandId = new CommandID(
                PackageGuids.CommandSetGuid,
                PackageIds.RenamifyProjectCommandId
            );
            var menuItem = new MenuCommand(Execute, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(ServiceProvider.GetService(typeof(DTE)) is DTE2 dte))
            {
                return;
            }

            var selectedItems = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;
            foreach (UIHierarchyItem selectedItem in selectedItems)
            {
                if (selectedItem.Object is Project project)
                {
                    RenameProject(project, dte);
                }
            }
        }

        private void RenameProject(Project project, DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var currentName = Path.GetFileNameWithoutExtension(project.FullName);

            var dialog = new RenameProjectDialog(currentName);

            // Set the owner to the VS main window for proper modal behavior
            var helper = new WindowInteropHelper(dialog)
            {
                Owner = dte.MainWindow.HWnd
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var newName = dialog.NewProjectName;
            var projectFilePath = project.FullName;

            // Remove project from solution before file operations
            dte.Solution.Remove(project);

            // Update RootNamespace and AssemblyName in .csproj
            ProjectFileService.UpdateProjectFile(projectFilePath, currentName, newName);

            // Update namespace declarations in source files
            SourceFileService.UpdateNamespacesInProject(projectFilePath, currentName, newName);

            // Rename the project file on disk
            var newProjectFilePath = ProjectFileService.RenameProjectFile(projectFilePath, newName);

            // Rename parent directory if it matches the old project name
            newProjectFilePath = ProjectFileService.RenameParentDirectoryIfMatches(newProjectFilePath, currentName, newName);

            // Re-add project to solution with new path
            dte.Solution.AddFromFile(newProjectFilePath);

            // TODO: Implement remaining rename operations
            // See open issues for requirements:
            // - #9: Update using statements across solution
            // - #11: Solution folder support
            // - #12: Progress indication
            // - #13: Error handling and rollback
        }
    }
}
