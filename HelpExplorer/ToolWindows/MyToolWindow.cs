﻿using System.Runtime.InteropServices;
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
            Projects projects = await Projects.LoadAsync();
            Project project = await VS.Solutions.GetActiveProjectAsync();
            return new MyToolWindowControl(projects, project);
        }

        [Guid("1948a5b4-cb3b-42c5-848c-9f1bb6167caf")]
        internal class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}