namespace HelpExplorer.Commands
{
    [Command(PackageIds.MultipleProjectTypeDisplay)]
    internal sealed class DisplayMultipleProjectTypesCommand : BaseCommand<DisplayMultipleProjectTypesCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (General.Instance.MultipleProjectsOption)
                {
                    General.Instance.MultipleProjectsOption = false;
                    await General.Instance.SaveAsync();
                }
                else
                {
                    General.Instance.MultipleProjectsOption = true;
                    await General.Instance.SaveAsync();
                }
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ToolWindowMessenger messenger = await Package.GetServiceAsync<ToolWindowMessenger, ToolWindowMessenger>();
                messenger.Send("Refresh HelpExplorer Project Links");
            }).FireAndForget();
        }
    }
}
