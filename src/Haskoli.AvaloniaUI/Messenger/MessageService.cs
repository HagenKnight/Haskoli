using System;

namespace Haskoli.AvaloniaUI.Messenger
{
    public class MessageService
    {
        public event EventHandler OnMainWindowClosing;

        public void PublishMainWindowClosingEvent()
        {
            OnMainWindowClosing?.Invoke(this, EventArgs.Empty);
        }
    }
}
