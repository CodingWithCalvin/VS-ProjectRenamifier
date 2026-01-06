using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Interop;
using System.Windows.Threading;
using CodingWithCalvin.ProjectRenamifier.Dialogs;
using CodingWithCalvin.ProjectRenamifier.Services;
using CodingWithCalvin.Otel4Vsix;
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

            using var activity = VsixTelemetry.StartCommandActivity("ProjectRenamifier.Execute");

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

            using var activity = VsixTelemetry.StartCommandActivity("ProjectRenamifier.RenameProject");

            var currentName = Path.GetFileNameWithoutExtension(project.FullName);
                        var dialog = new RenameProjectDialog(currentName);

            // Set the owner to the VS main window for proper modal behavior
            var helper = new WindowInteropHelper(dialog)
            {
                Owner = dte.MainWindow.HWnd
            };

            if (dialog.ShowDialog() != true)
            {
                VsixTelemetry.LogInformation("Rename cancelled by user");
                return;
            }

            var newName = dialog.NewProjectName;
            var projectFilePath = project.FullName;
            var originalProjectFilePath = projectFilePath;

                        VsixTelemetry.LogInformation("Renaming project");

            // Show progress dialog
            var progressDialog = new RenameProgressDialog(currentName);
            var progressHelper = new WindowInteropHelper(progressDialog)
            {
                Owner = dte.MainWindow.HWnd
            };
            progressDialog.Show();

            var stepIndex = 0;
            var projectRemovedFromSolution = false;
            var projectReaddedToSolution = false;
            List<string> referencingProjects = null;
            Project parentSolutionFolder = null;

            try
            {
                // Step 1: Collect projects that reference this project before removal
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    return ProjectReferenceService.FindProjectsReferencingTarget(dte.Solution, projectFilePath);
                }, out referencingProjects);
                var oldProjectFilePath = projectFilePath;

                // Step 2: Capture the parent solution folder before removal
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    return SolutionFolderService.GetParentSolutionFolder(project);
                }, out parentSolutionFolder);

                // Step 3: Remove project from solution before file operations
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    dte.Solution.Remove(project);
                    projectRemovedFromSolution = true;
                });

                // Step 4: Update RootNamespace and AssemblyName in .csproj
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    ProjectFileService.UpdateProjectFile(projectFilePath, currentName, newName);
                });

                // Step 5: Update namespace declarations in source files
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    SourceFileService.UpdateNamespacesInProject(projectFilePath, currentName, newName);
                });

                // Step 6: Rename the project file on disk
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    return ProjectFileService.RenameProjectFile(projectFilePath, newName);
                }, out projectFilePath);

                // Step 7: Rename parent directory if it matches the old project name
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    return ProjectFileService.RenameParentDirectoryIfMatches(projectFilePath, currentName, newName);
                }, out projectFilePath);

                // Step 8: Update references in projects that referenced this project
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    ProjectReferenceService.UpdateProjectReferences(referencingProjects, oldProjectFilePath, projectFilePath);
                });

                // Step 9: Re-add project to solution, preserving solution folder location
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    SolutionFolderService.AddProjectToSolution(dte.Solution, projectFilePath, parentSolutionFolder);
                    projectReaddedToSolution = true;
                });

                // Step 10: Update using statements across the entire solution
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    SourceFileService.UpdateUsingStatementsInSolution(dte.Solution, currentName, newName);
                });

                // Step 11: Update fully qualified type references across the solution
                ExecuteStep(progressDialog, stepIndex++, () =>
                {
                    SourceFileService.UpdateFullyQualifiedReferencesInSolution(dte.Solution, currentName, newName);
                });

                // Mark as complete and close after a brief delay
                progressDialog.Complete();
                DoEvents();
                System.Threading.Thread.Sleep(500);
                progressDialog.Close();

                activity?.SetTag("rename.success", true);
                activity?.SetTag("rename.steps_completed", stepIndex);
                VsixTelemetry.LogInformation("Successfully renamed project");
            }
            catch (Exception ex)
            {
                // Mark the current step as failed
                progressDialog.FailStep(stepIndex, ex.Message);
                DoEvents();

                activity?.RecordError(ex);
                activity?.SetTag("rename.success", false);
                activity?.SetTag("rename.failed_step", stepIndex);

                VsixTelemetry.TrackException(ex, new Dictionary<string, object>
                {
                    { "operation.name", "RenameProject" },
                    { "step.index", stepIndex }
                });

                // Attempt rollback if project was removed but not re-added
                if (projectRemovedFromSolution && !projectReaddedToSolution)
                {
                    try
                    {
                        // Try to re-add the project at its current location
                        var currentProjectPath = File.Exists(projectFilePath) ? projectFilePath : originalProjectFilePath;
                        if (File.Exists(currentProjectPath))
                        {                            SolutionFolderService.AddProjectToSolution(dte.Solution, currentProjectPath, parentSolutionFolder);
                            VsixTelemetry.LogInformation("Rollback: Re-added project");
                        }
                    }
                    catch (Exception rollbackEx)
                    {
                        VsixTelemetry.TrackException(rollbackEx, new Dictionary<string, object>
                        {
                            { "operation.name", "RenameProject.Rollback" }
                        });
                    }
                }

                // Show error message
                System.Windows.MessageBox.Show(
                    $"An error occurred while renaming the project:\n\n{ex.Message}\n\n" +
                    "The operation has been aborted. The project may be in a partially renamed state.",
                    "Rename Failed",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);

                progressDialog.Close();
            }
        }

        private void ExecuteStep(RenameProgressDialog dialog, int stepIndex, Action action)
        {
            dialog.StartStep(stepIndex);
            DoEvents();
            action();
            dialog.CompleteStep(stepIndex);
            DoEvents();
        }

        private void ExecuteStep<T>(RenameProgressDialog dialog, int stepIndex, Func<T> func, out T result)
        {
            dialog.StartStep(stepIndex);
            DoEvents();
            result = func();
            dialog.CompleteStep(stepIndex);
            DoEvents();
        }

        private void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(obj =>
                {
                    ((DispatcherFrame)obj).Continue = false;
                    return null;
                }), frame);
            Dispatcher.PushFrame(frame);
        }
    }
}
