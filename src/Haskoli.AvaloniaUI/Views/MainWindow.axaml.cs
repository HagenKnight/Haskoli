using Avalonia.Controls;
using Haskoli.AvaloniaUI.Messenger;

namespace Haskoli.AvaloniaUI.Views
{
    public partial class MainWindow : Window
    {
        private readonly MessageService _messageService;
        public MainWindow(MessageService messageService)
        {
            _messageService = messageService;

            InitializeComponent();
        }
    }
}