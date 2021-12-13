using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HelpExplorer.Schema;
using Microsoft.VisualStudio.Imaging;

namespace HelpExplorer
{
    public class MyToolWindow : BaseToolWindow<MyToolWindow>
    {
        public override string GetTitle(int toolWindowId) => Vsix.Name;

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            ProjectTypeCollection projectTypes = await ProjectTypeCollection.LoadAsync();
            FileTypeCollection fileTypes = await FileTypeCollection.LoadAsync();
            Project project = await VS.Solutions.GetActiveProjectAsync();
            ToolWindowMessenger toolWindowMessenger = await Package.GetServiceAsync<ToolWindowMessenger, ToolWindowMessenger>();
            return new MyToolWindowControl(projectTypes, fileTypes, project, toolWindowMessenger);
        }

        [Guid("1948a5b4-cb3b-42c5-848c-9f1bb6167caf")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
               ToolBar = new CommandID(PackageGuids.HelpExplorer, PackageIds.TWindowToolbar);
            }
        }
    }
}