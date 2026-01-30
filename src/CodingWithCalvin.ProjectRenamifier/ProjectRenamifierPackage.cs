global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using CodingWithCalvin.Otel4Vsix;

namespace CodingWithCalvin.ProjectRenamifier
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(VsixInfo.DisplayName, VsixInfo.Description, VsixInfo.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(VSCommandTableVsct.ProjectRenamifierString)]
    public sealed class ProjectRenamifierPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var builder = VsixTelemetry.Configure()
                .WithServiceName(VsixInfo.DisplayName)
                .WithServiceVersion(VsixInfo.Version)
                .WithVisualStudioAttributes(this)
                .WithEnvironmentAttributes();

#if !DEBUG
            builder
                .WithOtlpHttp("https://api.honeycomb.io")
                .WithHeader("x-honeycomb-team", HoneycombConfig.ApiKey);
#endif

            builder.Initialize();

            await Task.Run(() =>
            {
                RenamifyProjectCommand.Initialize(this);
            });

            VsixTelemetry.LogInformation("Project Renamifier initialized successfully");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                VsixTelemetry.Shutdown();
            }

            base.Dispose(disposing);
        }
    }
}
