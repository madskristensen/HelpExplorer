using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelpExplorer.Schema;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace HelpExplorer
{
    public partial class MyToolWindowControl : UserControl
    {
        private readonly ProjectTypeCollection _projectTypes;
        public IVsHierarchy hierarchy = null;
        public Project _activeProject;

        public MyToolWindowControl(ProjectTypeCollection projects, Project activeProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _projectTypes = projects;
            _activeProject = activeProject;
            InitializeComponent();

            GetActiveProjectcapabilities(activeProject);
            UpdateProjects(hierarchy);

            VS.Events.SelectionEvents.SelectionChanged += SelectionChanged;
            VS.Events.SolutionEvents.OnAfterCloseSolution += OnAfterCloseSolution;
        }

        private void OnAfterCloseSolution()
        {
            SelectionChanged(null, null);
        }

        private void GetActiveProjectcapabilities(Project activeProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (activeProject != null)
            {
                activeProject.GetItemInfo(out IVsHierarchy hierarchy, out var itemId, out IVsHierarchyItem item);
                HierarchyUtilities.TryGetHierarchyProperty<string>(hierarchy, itemId, (int)__VSHPROPID5.VSHPROPID_ProjectCapabilities, out var value);
                this.hierarchy = hierarchy;
                //The following capabilities line allows you to check the projects capabilities so they can be added to project.json.
                var capabilities = (value ?? "").Split(' ');
            }
        }

        private void SelectionChanged(object sender, Community.VisualStudio.Toolkit.SelectionChangedEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                Project project = await VS.Solutions.GetActiveProjectAsync();

                if (project != _activeProject)
                {
                    _activeProject = project;
                    GetActiveProjectcapabilities(project);
                    UpdateProjects(hierarchy);
                }
            }).FireAndForget();
        }

        private void UpdateProjects(IVsHierarchy hierarchy)
        {
            Widgets.Children.Clear();

            foreach (ProjectType pt in _projectTypes.ProjectTypes)
            {
                var capability = pt.Capability;
                if ((_activeProject != null && string.IsNullOrEmpty(capability)) || (hierarchy == null || !hierarchy.IsCapabilityMatch(capability)))
                {
                    continue;
                }

                var text = new TextBlock { Text = pt.Text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 5) };
                Widgets.Children.Add(text);

                foreach (Link link in pt.Links)
                {
                    var h = new Hyperlink
                    {
                        NavigateUri = new Uri(link.Url)
                    };

                    h.RequestNavigate += OnRequestNavigate;
                    h.Inlines.Add(link.Text);
                    var textBlock = new TextBlock { Text = "- ", Margin = new Thickness(15, 0, 0, 0) };
                    textBlock.Inlines.Add(h);

                    Widgets.Children.Add(textBlock);
                }

                var line = new Line { Margin = new Thickness(0, 0, 0, 20) };
                Widgets.Children.Add(line);
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
