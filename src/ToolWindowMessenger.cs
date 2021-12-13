namespace HelpExplorer
{
    public class ToolWindowMessenger
    {
        public void Send(string message)
        {
            // The tooolbar button will call this method.
            // The tool window has added an event handler
            MessageReceived?.Invoke(this, message);
        }

        public event EventHandler<string> MessageReceived;
    }
}
