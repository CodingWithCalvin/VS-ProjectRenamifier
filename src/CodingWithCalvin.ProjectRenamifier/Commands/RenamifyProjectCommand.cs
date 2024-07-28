using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;
using VSLangProj;

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

            var oldProjectFileName = Path.GetFileName(project.FullName);
            var projectFileNameNoExtension = Path.GetFileNameWithoutExtension(project.FullName);
            var projectExtension = Path.GetExtension(oldProjectFileName);

            var newProjectFileName = $"{projectFileNameNoExtension}-NEW{projectExtension}";

            var projectPath =
                Path.GetDirectoryName(project.FullName)
                ?? throw new InvalidOperationException();
            var parentDirectoryName = new DirectoryInfo(projectPath).Parent.Name;
            var oldParentDirectory = new DirectoryInfo(projectPath).Parent.FullName;
            
            var grandparentDirectory = new DirectoryInfo(parentDirectoryName).Parent.FullName;
            var newParentDirectory = Path.Combine(grandparentDirectory, $"{parentDirectoryName}-NEW");
                
            // hold onto all projects that reference this project
            var referencingProjects = new List<Project>();
            foreach (Project p in dte.Solution.Projects)
            {
                if(p.Object is VSProject referencingVSProject)
                {
                    foreach(Reference reference in referencingVSProject.References)
                    {
                        if(reference.SourceProject != null && reference.SourceProject.UniqueName == project.UniqueName)
                        {
                            referencingProjects.Add(p);
                        }
                    } 
                }
            }

            // hold onto all projects that this project references
            var referencedProjects = new List<Project>();
            
                if (project.Object is VSProject referencedVSProject)
                {
                    foreach (Reference reference in referencedVSProject.References)
                    {
                        if (reference.SourceProject != null)
                        {
                            referencedProjects.Add(reference.SourceProject);
                        }
                    }
                }
            
            // unload project, then do the work
            dte.Solution.Remove(project);

            //rename parent directory, if necessary
            if (parentDirectoryName.Equals(projectFileNameNoExtension))
            {
                Directory.Move(oldParentDirectory, newParentDirectory);
            }

            // rename project
            File.Move(oldProjectFileName, newProjectFileName);

            // add new project back to solution
            dte.Solution.AddFromFile(newProjectFileName, true);

            // need handle to new project now that its back 
            Project newProjectReference = null;
            foreach (Project p in dte.Solution.Projects)
            {
                if(p.UniqueName == newProjectFileName)
                {
                    newProjectReference = p;
                    break;
                }
            }
            
            // fix projects that reference the old project to reference the new one
            foreach (var referencingProject in referencingProjects)
            {
                if(referencingProject.Object is VSProject vsProject)
                {
                    vsProject.References.AddProject(newProjectReference);
                }
            }

            // fix this project's references
            foreach (var referencedProject in referencedProjects)
            {
                if(newProjectReference.Object is VSProject vsProject2)
                {
                    vsProject2.References.AddProject(referencedProject);
                }
            }
                        
            // sync new namespace?
        }
    }
}
