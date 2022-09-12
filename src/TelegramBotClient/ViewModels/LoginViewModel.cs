using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Threading.Tasks;
using TelegramBotClient.Views;
using System;
using System.Reactive.Linq;

namespace TelegramBotClient.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        [Reactive]
        public string UserId { get; set; }

        public ReactiveCommand<Unit, Unit> IdCommand { get; }

        public LoginViewModel()
        {
            var canId = this
                .WhenAnyValue(
                o => o.UserId,
                (userId) =>
                    !string.IsNullOrWhiteSpace(userId) &&
                    int.TryParse(userId, out _));

            IdCommand = ReactiveCommand.CreateFromTask(IdImpl, canId);
        }

        private async Task<Unit> IdImpl()
        {
            Client client;
            try
            {
                client = new Client(int.Parse(UserId));
            }
            catch
            {
                await ShowErrorInteraction.Handle("Connection failed!");
                return Unit.Default;
            }

            client.DisconnectedEvent.Subscribe(async _ => {
                await ShowWindowInteraction.Handle(Unit.Default);
                await ShowErrorInteraction.Handle("Can't connect to server.");
            });

            try
            {
                switch (client.ContainsUser())
                {
                    case true:
                        var window = new MainWindow(new MainViewModel(client));
                        window.Show();
                        await HideWindowInteraction.Handle(Unit.Default);
                        break;
                    case false:
                        await ShowErrorInteraction.Handle("User not found.");
                        break;
                    case null:
                        return Unit.Default;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return Unit.Default;
        }
    }
}
