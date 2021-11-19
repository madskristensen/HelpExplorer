using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelpExplorer.Schema;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;

namespace HelpExplorer
{
    public partial class MyToolWindowControl : UserControl
    {
        private readonly ProjectTypeCollection _projectTypes;
        private readonly ContentTypeCollection _fileTypes;
        public IVsHierarchy hierarchy = null;
        public Project _activeProject;
        public string _activeFile;

        public MyToolWindowControl(ProjectTypeCollection projectTypes, ContentTypeCollection fileTypes, Project activeProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _projectTypes = projectTypes;
            _fileTypes = fileTypes;
            _activeProject = activeProject;
            InitializeComponent();

            GetActiveProjectcapabilities(activeProject);
            UpdateProjects(hierarchy);

            VS.Events.SelectionEvents.SelectionChanged += SelectionChanged;
            VS.Events.DocumentEvents.BeforeDocumentWindowShow += BeforeDocumentWindowShow;
            VS.Events.SolutionEvents.OnAfterCloseSolution += OnAfterCloseSolution;
        }

        private void BeforeDocumentWindowShow(DocumentView docView)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (docView?.Document?.FilePath != _activeFile)
                {
                    _activeFile = docView?.Document?.FilePath;
                    await UpdateFilesAsync(docView.TextBuffer.ContentType);
                }

            }).FireAndForget();
        }

        private void OnAfterCloseSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    GetActiveProjectcapabilities(project);
                    UpdateProjects(hierarchy);
                }
            }).FireAndForget();
        }

        private async Task UpdateFilesAsync(IContentType contentType)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            FileTypes.Children.Clear();

            foreach (ContentType ft in _fileTypes.ContentTypes.Where(f => contentType.IsOfType(f.Name)))
            {
                var text = new TextBlock { Text = ft.Text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 5) };
                FileTypes.Children.Add(text);

                foreach (Link link in ft.Links)
                {
                    var h = new Hyperlink
                    {
                        NavigateUri = new Uri(link.Url)
                    };

                    h.RequestNavigate += OnRequestNavigate;
                    h.Inlines.Add(link.Text);
                    var textBlock = new TextBlock { Text = "- ", Margin = new Thickness(15, 0, 0, 0) };
                    textBlock.Inlines.Add(h);

                    FileTypes.Children.Add(textBlock);
                }

                var line = new Line { Margin = new Thickness(0, 0, 0, 20) };
                FileTypes.Children.Add(line);
            }
        }

        private void UpdateProjects(IVsHierarchy hierarchy)
        {
            ProjectTypes.Children.Clear();

            foreach (ProjectType pt in _projectTypes.ProjectTypes)
            {
                var capability = pt.Capability;

                if ((_activeProject == null && !string.IsNullOrEmpty(capability)) || _activeProject != null && string.IsNullOrEmpty(capability))
                {
                    continue;
                }
                else if (!string.IsNullOrEmpty(capability) && hierarchy?.IsCapabilityMatch(capability) == false)
                {
                    continue;
                }

                var text = new TextBlock { Text = pt.Text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 5) };
                ProjectTypes.Children.Add(text);

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

                    ProjectTypes.Children.Add(textBlock);
                }

                var line = new Line { Margin = new Thickness(0, 0, 0, 20) };
                ProjectTypes.Children.Add(line);
                break;
            }
        }
        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            VsShellUtilities.OpenBrowser(e.Uri.AbsoluteUri, (int)__VSOSPFLAGS.OSP_LaunchSingleBrowser);
            e.Handled = true;
        }
    }
}
