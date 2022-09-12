using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TelegramBotClient.Models;
using Avalonia.Collections;

namespace TelegramBotClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public Client Client;

        [Reactive]
        public Reminder SelectedReminder { get; set; }

        [Reactive]
        public string Filter { get; set; } = string.Empty;

        public DataGridCollectionView RemindersView { get; }

        public ObservableCollection<Reminder> Reminders { get; }

        public ReactiveCommand<Unit, Unit> AddCommand { get; }

        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

        public ReactiveCommand<Unit, Unit> EditCommand { get; }

        public ReactiveCommand<Unit, Unit> FilterCommand { get; }

        public Interaction<ReminderViewModel, Unit> ShowDialogInteraction { get; } = new();

        public MainViewModel()
        {
        }

        public MainViewModel(Client client)
        {
            var canExecute = this.WhenAnyValue(
                o => o.SelectedReminder, 
                (Reminder o) =>
                    o is not null);

            Client = client;
            var userReminders = client.GetEvents();
            Reminders = new ObservableCollection<Reminder>();
            RemindersView = new DataGridCollectionView(Reminders);

            foreach (var reminder in userReminders.Reminders)
            {
                var time = reminder.DateTime.ToDateTime().ToLocalTime();
                Reminders.Add(new Reminder
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
            FilterCommand = ReactiveCommand.CreateFromTask(FilterImpl);
            RemindersView.Filter = o =>
                ((Reminder)o).Name.ToLower().Contains(Filter.ToLower()) || 
                ((Reminder)o).Description.ToLower().Contains(Filter.ToLower())
                ? true : false;
        }

        private async Task<Unit> AddImpl()
        {
            await ShowDialogInteraction.Handle(new ReminderViewModel(Client, Reminders));
            return Unit.Default;
        }

        private async Task<Unit> EditImpl()
        {
            await ShowDialogInteraction.Handle(new ReminderViewModel(Client, Reminders, SelectedReminder));
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
                foreach (var reminder in Reminders)
                {
                    if (reminder.Id == SelectedReminder.Id)
                    {
                        Reminders.Remove(reminder);
                        break;
                    }
                }
            }
            else await ShowErrorInteraction.Handle("Operation failed.");
            return Unit.Default;
        }

        private async Task<Unit> FilterImpl()
        {
            RemindersView.Refresh();
            return Unit.Default;
        }
    }
}
