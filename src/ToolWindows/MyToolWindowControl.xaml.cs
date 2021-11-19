using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using HelpExplorer.Schema;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using static Microsoft.VisualStudio.VSConstants;

namespace HelpExplorer
{
public partial class MyToolWindowControl : UserControl
{
    private readonly Projects _projects;
    //private Guid projectGuid = Guid.Empty;
    private IVsHierarchy _hierarchy;

    public MyToolWindowControl(Projects projects, Project activeProject)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        _projects = projects;
        InitializeComponent();

        GetActiveProjectcapabilities(activeProject);
        UpdateProjects();

        VS.Events.SelectionEvents.SelectionChanged += SelectionEvents_SelectionChanged;
    }
    private void GetActiveProjectcapabilities(Project activeProject)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (activeProject != null)
        {
            activeProject.GetItemInfo(out _hierarchy, out var itemId, out IVsHierarchyItem item);
            HierarchyUtilities.TryGetHierarchyProperty<string>(_hierarchy, itemId, (int)__VSHPROPID5.VSHPROPID_ProjectCapabilities, out var value);
                //The following capabilities line allows you to check the projects capabilities so they can be added to ProjectSdkStyle.json. Should be removed once all capabilities are learned and not put in released version.
                var capabilities = (value ?? "").Split(' ');
        }
    }

    private void SelectionEvents_SelectionChanged(object sender, Community.VisualStudio.Toolkit.SelectionChangedEventArgs e)
    {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            Project project = await VS.Solutions.GetActiveProjectAsync();
            GetActiveProjectcapabilities(project);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            UpdateProjects();

        }).FireAndForget();
    }

    private void UpdateProjects()
    {
        Widgets.Children.Clear();

        foreach (Widget widget in _projects.Widgets)
        {
            try
            {
                    var capabilityMatch = _hierarchy.IsCapabilityMatch(widget.Projects[0].ProjectTypeExpression);
                    if (!capabilityMatch)
                    {
                        continue;
                    }
                    var text = new Label { Content = widget.Text };
                Widgets.Children.Add(text);

                foreach (Link link in widget.Links)
                {
                    Hyperlink h = new();
                    h.NavigateUri = new Uri(link.Url);
                    h.RequestNavigate += OnRequestNavigate;
                    h.Inlines.Add(link.Text);
                    var textBlock = new TextBlock();
                    textBlock.Inlines.Add(h);
                    Widgets.Children.Add(textBlock);
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
    }
    private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        // for .NET Core you need to add UseShellExecute = true
        var pStartInfo = new ProcessStartInfo(e.Uri.AbsoluteUri)
        {
            UseShellExecute = true
        };
        Process.Start(pStartInfo);
        e.Handled = true;
    }
}
}
