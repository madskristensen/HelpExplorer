namespace HelpExplorer
{
    [Command(PackageIds.CreateCapabilitiyFile)]
    internal sealed class CreateCapabilitesFileCommand : BaseCommand<CreateCapabilitesFileCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (General.Instance.CreateCapabilitiesFile)
                {
                    General.Instance.CreateCapabilitiesFile = false;
                    await General.Instance.SaveAsync();
                }
                else
                {
                    General.Instance.CreateCapabilitiesFile = true;
                    await General.Instance.SaveAsync();
                }
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ToolWindowMessenger messenger = await Package.GetServiceAsync<ToolWindowMessenger, ToolWindowMessenger>();
                messenger.Send($"Changed Create Capabilites File Option to {General.Instance.CreateCapabilitiesFile}");
            }).FireAndForget();
        }
    }
}
