using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using HelpExplorer.Schema;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace HelpExplorer
{
public partial class MyToolWindowControl : UserControl
{
    private readonly Projects _projects;
    //private Guid projectGuid = Guid.Empty;
    //private string[] capabilities = null;
    public IVsHierarchy hierarchy = null;

    public MyToolWindowControl(Projects projects, Project activeProject)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        _projects = projects;
        InitializeComponent();

        GetActiveProjectcapabilities(activeProject);

        UpdateProjects(this.hierarchy);

        VS.Events.SelectionEvents.SelectionChanged += SelectionEvents_SelectionChanged;
    }
    private void GetActiveProjectcapabilities(Project activeProject)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (activeProject != null)
        {
            activeProject.GetItemInfo(out IVsHierarchy hierarchy, out var itemId, out IVsHierarchyItem item);
            HierarchyUtilities.TryGetHierarchyProperty<string>(hierarchy, itemId, (int)__VSHPROPID5.VSHPROPID_ProjectCapabilities, out string value);
            this.hierarchy = hierarchy;
            //The following capabilities line allows you to check the projects capabilities so they can be added to project.json.
            string[] capabilities = (value ?? "").Split(' ');
        }
    }

    private void SelectionEvents_SelectionChanged(object sender, Community.VisualStudio.Toolkit.SelectionChangedEventArgs e)
    {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            Project project = await VS.Solutions.GetActiveProjectAsync();
            GetActiveProjectcapabilities(project);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            UpdateProjects(this.hierarchy);
        }).FireAndForget();
    }

    private void UpdateProjects(IVsHierarchy hierarchy)
    {
        Widgets.Children.Clear();

        foreach (Widget widget in _projects.Widgets)
        {
            try
            {
                //string capabilityMatch = widget.projects.First().projectTypeExpression;
                if (!hierarchy.IsCapabilityMatch(widget.Projects.First()))
                {
                    continue;
                }
                var text = new Label { Content = widget.Text };
                Widgets.Children.Add(text);

                foreach (Link link in widget.Links)
                {
                    Hyperlink h = new Hyperlink();
                    h.NavigateUri = new Uri(link.Url);
                    h.RequestNavigate += OnRequestNavigate;
                    h.Inlines.Add(link.Text);
                    var textBlock = new TextBlock();
                    textBlock.Inlines.Add(h);

                    //var hyperlink = new Label { Content = link.Text };
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
