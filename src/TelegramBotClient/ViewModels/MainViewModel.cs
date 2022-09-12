using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TelegramBotClient.Models;

namespace TelegramBotClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public Client Client;

        [Reactive]
        public Reminder SelectedReminder { get; set; }

        public ObservableCollection<Reminder> EventReminders { get; }

        public ReactiveCommand<Unit, Unit> AddCommand { get; }

        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        public ReactiveCommand<Unit, Unit> EditCommand { get; }

        public Interaction<ReminderViewModel, Unit> ShowDialogInteraction { get; } = new();

        public MainViewModel()
        {
        }

        public MainViewModel(Client client)
        {
            var canExecute = this.WhenAnyValue(
                o => o.SelectedReminder, 
                (Models.Reminder o) =>
                    o is not null);

            Client = client;
            var userReminders = client.GetEvents();
            EventReminders = new ObservableCollection<Models.Reminder>();

            foreach (var reminder in userReminders.Reminders)
            {
                var time = reminder.DateTime.ToDateTime().ToLocalTime();
                EventReminders.Add(new Models.Reminder
                {
                    Id = reminder.Id,
                    Name = reminder.Name,
                    Description = reminder.Description,
                    Time = time,
                    RepeatPeriod = reminder.RepeatPeriod.ToTimeSpan()
                });
            }

            AddCommand = ReactiveCommand.CreateFromTask(AddImpl);
            EditCommand = ReactiveCommand.CreateFromTask(EditImpl, canExecute);
            RemoveCommand = ReactiveCommand.CreateFromTask(RemoveImpl, canExecute);
        }

        private async Task<Unit> AddImpl()
        {
            await ShowDialogInteraction.Handle(new ReminderViewModel(Client, EventReminders));
            return Unit.Default;
        }

        private async Task<Unit> EditImpl()
        {
            await ShowDialogInteraction.Handle(new ReminderViewModel(Client, EventReminders, SelectedReminder));
            return Unit.Default;
        }

        private async Task<Unit> RemoveImpl()
        {
            Client.DisconnectedEvent.Subscribe(async _ => 
                await CloseWindowInteraction.Handle(Unit.Default));

            var success = Client.RemoveEvent(new TelegramBot.Proto.Event
            {
                UserId = Client.UserId,
                Id = SelectedReminder.Id,
            });

            if (success is null) return Unit.Default;

            if (success is true)
            {
                foreach (var reminder in EventReminders)
                {
                    if (reminder.Id == SelectedReminder.Id)
                    {
                        EventReminders.Remove(reminder);
                        break;
                    }
                }
            }
            else await ShowErrorInteraction.Handle("Operation failed.");
            return Unit.Default;
        }
    }
}
