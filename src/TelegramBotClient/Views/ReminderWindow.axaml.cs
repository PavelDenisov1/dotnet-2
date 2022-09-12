using Avalonia.Controls;
using Avalonia.Interactivity;
using TelegramBotClient.ViewModels;

namespace TelegramBotClient.Views
{
    public partial class ReminderWindow : Window<ViewModelBase>
    {
        public ReminderWindow()
        {
            InitializeComponent();
        }

        public ReminderWindow(ReminderViewModel reminderViewModel)
        {
            InitializeComponent();
            DataContext = reminderViewModel;
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
