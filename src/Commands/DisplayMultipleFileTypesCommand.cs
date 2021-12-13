namespace HelpExplorer.Commands
{
    [Command(PackageIds.MultipleFileTypeDisplay)]
    internal sealed class DisplayMultipleFileTypesCommand : BaseCommand<DisplayMultipleFileTypesCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (General.Instance.MultipleFilesOption)
                {
                    General.Instance.MultipleFilesOption = false;
                    await General.Instance.SaveAsync();
                }
                else
                {
                    General.Instance.MultipleFilesOption = true;
                    await General.Instance.SaveAsync();
                }
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ToolWindowMessenger messenger = await Package.GetServiceAsync<ToolWindowMessenger, ToolWindowMessenger>();
                messenger.Send("Refresh HelpExplorer File Links");
            }).FireAndForget();
        }
    }
}
