using EnvDTE;
using EnvDTE80;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;

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
            var menuItem = new MenuCommand(RenamifyProject, menuCommandId);
            commandService.AddCommand(menuItem);
        }

        private void RenamifyProject(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!(ServiceProvider.GetService(typeof(DTE)) is DTE2 dte))
            {
                throw new ArgumentNullException(nameof(dte));
            }

            foreach (
                UIHierarchyItem selectedItem in (Array)
                    dte.ToolWindows.SolutionExplorer.SelectedItems
            )
            {
                switch (selectedItem.Object)
                {
                    case Project project:
                        try
                        {
                            RenameProject(project, dte);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $@"
                                Unable to rename selected project
                                {Environment.NewLine}
                                {Environment.NewLine}
                                Exception: {ex.Message}"
                            );
                        }

                        break;
                }
            }
        }

        void RenameProject(Project project, DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectFileName = Path.GetFileName(project.FullName);
            var projectFileNameNoExtension = Path.GetFileNameWithoutExtension(project.FullName);
            var projectPath =
                Path.GetDirectoryName(project.FullName)
                ?? throw new InvalidOperationException();
            var parentDirectoryName = new DirectoryInfo(projectPath).Parent.Name;
            var grandparentDirectory = new DirectoryInfo(parentDirectoryName).Parent.FullName; 

            if (parentDirectoryName.Equals(projectFileNameNoExtension))
            {
                // rename parent directory
            }

            // rename project
            
            // fix solution?
            // remove old project?
            // add new project?

            // fix references?
            
            // sync new namespace?
        }
    }
}
