global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;

namespace HelpExplorer
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(MyToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideToolWindowVisibility(typeof(MyToolWindow.Pane), VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
    [ProvideToolWindowVisibility(typeof(MyToolWindow.Pane), VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    [ProvideToolWindowVisibility(typeof(MyToolWindow.Pane), VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideToolWindowVisibility(typeof(MyToolWindow.Pane), VSConstants.UICONTEXT.EmptySolution_string)]
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), "HelpExplorer", "General", 0, 0, true)]
    [ProvideProfile(typeof(OptionsProvider.GeneralOptions), "HelpExplorer", "General", 0, 0, true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideService(typeof(ToolWindowMessenger), IsAsyncQueryable = true)]
    [Guid(PackageGuids.HelpExplorerString)]
    public sealed class HelpExplorerPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            AddService(typeof(ToolWindowMessenger), (_, _, _) => Task.FromResult<object>(new ToolWindowMessenger()));
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();
        }
    }
}