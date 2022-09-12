using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using TelegramBotClient.ViewModels;

namespace TelegramBotClient.Views
{
    public partial class MainWindow : Window<ViewModelBase>
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            DataContext = mainViewModel;
            this.WhenActivated((CompositeDisposable cd) =>
            {
                if (ViewModel is not MainViewModel viewModel)
                    return;

                var d = viewModel.ShowDialogInteraction.RegisterHandler(ShowDialog);
                cd.Add(d);
            });
        }

        private async Task ShowDialog(InteractionContext<ReminderViewModel, Unit> ctx)
        {
            var reminderWindow = new ReminderWindow(ctx.Input);
            await reminderWindow.ShowDialog(this);
            ctx.SetOutput(Unit.Default);
        }
    }
}
