using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using HelpExplorer.Schema;
using Microsoft.VisualStudio.Shell.Interop;

namespace HelpExplorer
{
    public partial class MyToolWindowControl : UserControl
    {
        private readonly Projects _projects;
        private Guid projectGuid = Guid.Empty;

        public MyToolWindowControl(Projects projects, Project activeProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _projects = projects;
            InitializeComponent();

            GetActiveProjectGuid(activeProject);

            UpdateProjects();

            VS.Events.SelectionEvents.SelectionChanged += SelectionEvents_SelectionChanged;
        }

        private void GetActiveProjectGuid(Project activeProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (activeProject != null)
            {
                activeProject.GetItemInfo(out IVsHierarchy hier, out var itemId, out IVsHierarchyItem item);
                hier.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_TypeGuid, out projectGuid);
            }
        }

        private void SelectionEvents_SelectionChanged(object sender, Community.VisualStudio.Toolkit.SelectionChangedEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                Project project = await VS.Solutions.GetActiveProjectAsync();
                GetActiveProjectGuid(project);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                UpdateProjects();

            }).FireAndForget();
        }

        private void UpdateProjects()
        {
            foreach (Widget widget in _projects.Widgets)
            {
                try
                {
                    if (!widget.Projects.Contains(projectGuid.ToString()))
                    {
                        continue;
                    }

                    var text = new Label { Content = widget.Text };
                    Widgets.Children.Add(text);

                    foreach (Link link in widget.Links)
                    {
                        var h = new Hyperlink();
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
    }
}