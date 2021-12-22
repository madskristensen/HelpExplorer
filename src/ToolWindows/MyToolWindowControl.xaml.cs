using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HelpExplorer.Schema;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Microsoft.Build.Framework.XamlTypes;
using static System.Net.Mime.MediaTypeNames;

namespace HelpExplorer
{
    public partial class MyToolWindowControl : UserControl
    {
        private readonly ProjectTypeCollection _projectTypes;
        private readonly FileTypeCollection _fileTypes;
        public IVsHierarchy Hierarchy = null;
        public ToolWindowMessenger ToolWindowMessenger = null;
        public string CapabilityValues = null;
        public string fileExtension = null;
        public Project _activeProject;
        public string _activeFile;

        public MyToolWindowControl(ProjectTypeCollection projectTypes, FileTypeCollection fileTypes, Project activeProject, ToolWindowMessenger toolWindowMessenger)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _projectTypes = projectTypes;
            _fileTypes = fileTypes;
            _activeProject = activeProject;
            InitializeComponent();
            if (toolWindowMessenger == null)
            {
                toolWindowMessenger = new ToolWindowMessenger();
            }
            ToolWindowMessenger = toolWindowMessenger;
            toolWindowMessenger.MessageReceived += OnMessageReceived;
            GetActiveProjectCapabilities(activeProject);
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await UpdateProjectsAsync(Hierarchy);
            }).FireAndForget();

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
                    var ext = System.IO.Path.GetExtension(docView?.Document?.FilePath);
                    //await UpdateFilesAsync(docView.TextBuffer.ContentType, ext);
                    fileExtension = ext;
                    await UpdateFilesAsync(ext);
                }

            }).FireAndForget();
        }

        private void OnMessageReceived(object sender, string e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                switch (e)
                {
                    case "Refresh HelpExplorer Project Links":
                        await UpdateProjectsAsync(Hierarchy);
                        break;
                    case "Refresh HelpExplorer File Links":
                        await UpdateFilesAsync(fileExtension);
                        break;
                    default:
                        break;
                }
            }).FireAndForget();

        }
        private void OnAfterCloseSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SelectionChanged(null, null);
        }

        private void GetActiveProjectCapabilities(Project activeProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (activeProject != null)
            {
                activeProject.GetItemInfo(out IVsHierarchy hierarchy, out var itemId, out IVsHierarchyItem item);
                HierarchyUtilities.TryGetHierarchyProperty<string>(hierarchy, itemId, (int)__VSHPROPID5.VSHPROPID_ProjectCapabilities, out var capabilityValues);
                Hierarchy = hierarchy;
                CapabilityValues = capabilityValues;
            }
        }
        private void WriteCapabilitiesToFile(string capability, string fileName)
        {
            //This method only runs in debug mode
            var capabilities = (capability ?? "").Split(' ');
            var dir = General.Instance.CapabilitiesFilePathOption;
            var file = System.IO.Path.Combine(dir, $"{fileName}_Capabilities.txt");
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
                System.IO.File.WriteAllLines(file, capabilities);
            }
            else
            {
                System.IO.File.WriteAllLines(file, capabilities);
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
                    GetActiveProjectCapabilities(project);
                    await UpdateProjectsAsync(Hierarchy);
                }
            }).FireAndForget();
        }

        private async Task UpdateFilesAsync(string fileExtension)
        {
            if (fileExtension == null)
            {
                return;
            }
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            FileTypes.Children.Clear();
            foreach (FileType ft in _fileTypes.FileTypes.Where(f => fileExtension.Equals(f.Name)))
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
                    FileTypes.MaxWidth = ft.Text.ToString().Length + 200;
                    h.Inlines.Add(link.Text);
                    var textBlock = new TextBlock { Text = "- ", Margin = new Thickness(15, 0, 0, 0) };
                    textBlock.Inlines.Add(h);

                    FileTypes.Children.Add(textBlock);
                }

                var line = new Line { Margin = new Thickness(0, 0, 0, 20) };
                FileTypes.Children.Add(line);
                // read settings
                if (General.Instance.MultipleFilesOption)
                {
                    continue;
                }
                break;
            }
        }

        public async Task UpdateProjectsAsync(IVsHierarchy hierarchy)
        {
            ProjectTypes.Children.Clear();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (ProjectType pt in _projectTypes.ProjectTypes)
            {
                var capability = pt.CapabilityExpression;

                if ((_activeProject == null && !string.IsNullOrEmpty(capability)) || _activeProject != null && string.IsNullOrEmpty(capability))
                {
                    continue;
                }
                else if (!string.IsNullOrEmpty(capability) && hierarchy?.IsCapabilityMatch(capability) == false)
                {
                    continue;
                }
                if (General.Instance.CreateCapabilitiesFile)
                {
                    //The following capabilities line allows you to check the projects capabilities so they can be added to projectTypes.json.
                    if (!string.IsNullOrEmpty(CapabilityValues) && !string.IsNullOrEmpty(pt.CapabilitiesFileName))
                    {
                        WriteCapabilitiesToFile(CapabilityValues, pt.CapabilitiesFileName);
                    }
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
                    ProjectTypes.MaxWidth = pt.Text.ToString().Length + 200;
                    h.Inlines.Add(link.Text);
                    var textBlock = new TextBlock { Text = "- ", Margin = new Thickness(15, 0, 0, 0) };
                    textBlock.Inlines.Add(h);

                    ProjectTypes.Children.Add(textBlock);
                }

                var line = new Line { Margin = new Thickness(0, 0, 0, 20) };
                ProjectTypes.Children.Add(line);
                // read settings
                if (General.Instance.MultipleProjectsOption)
                {
                    continue;
                }
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
