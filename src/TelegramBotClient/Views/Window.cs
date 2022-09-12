using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using TelegramBotClient.ViewModels;

namespace TelegramBotClient.Views
{
    public class Window<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
    {
        protected Window()
        {
            this.WhenActivated((CompositeDisposable cd) =>
            {
                if (ViewModel is not ViewModelBase viewModel)
                    return;

                var d = viewModel.ShowErrorInteraction.RegisterHandler(ShowError);
                cd.Add(d);
                d = viewModel.CloseWindowInteraction.RegisterHandler(CloseWindow);
                cd.Add(d);
                d = viewModel.HideWindowInteraction.RegisterHandler(HideWindow);
                cd.Add(d);
                d = viewModel.ShowWindowInteraction.RegisterHandler(ShowWindow);
                cd.Add(d);
            });
        }

        private async Task ShowError(InteractionContext<string, Unit> ctx)
        {
            var messageWindow = new MessageWindow(ctx.Input);
            await messageWindow.ShowDialog(this);
            ctx.SetOutput(Unit.Default);
        }

        private void CloseWindow(InteractionContext<Unit, Unit> ctx)
        {
            var window = (MainWindow)this.Owner;
            if(window is not null) window.Close();
            Close();
            ctx.SetOutput(Unit.Default);
        }

        private void HideWindow(InteractionContext<Unit, Unit> ctx)
        {
            Hide();
            ctx.SetOutput(Unit.Default);
        }

        private void ShowWindow(InteractionContext<Unit, Unit> ctx)
        {
            Show();
            ctx.SetOutput(Unit.Default);
        }
    }
}
