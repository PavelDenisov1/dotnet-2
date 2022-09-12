using Google.Protobuf.WellKnownTypes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TelegramBot.Proto;
using TelegramBotClient.Models;

namespace TelegramBotClient.ViewModels
{
    public class ReminderViewModel : ViewModelBase
    {
        [Reactive]
        public string Name { get; set; }

        [Reactive]
        public string Description { get; set; }

        [Reactive]
        public string Time { get; set; }

        [Reactive]
        public string RepeatPeriod { get; set; }

        public long IdReminder { get; set; }

        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        public ReminderViewModel()
        {
        }

        public ReminderViewModel(Client client, ObservableCollection<Reminder> reminders,
            Reminder? selectedReminder = null)    
        {
            var canOk = this
                .WhenAnyValue(
                o => o.Name,
                o => o.Time,
                o => o.RepeatPeriod,
                (name, time, repeatPeriod) =>
                    !string.IsNullOrWhiteSpace(name) &&
                    !string.IsNullOrWhiteSpace(time) &&
                    !string.IsNullOrWhiteSpace(repeatPeriod) &&
                    DateTime.TryParse(time, out _) &&
                    TimeSpan.TryParse(repeatPeriod, out _));

            _client = client;
            _reminders = reminders;
            if (selectedReminder is not null)
            {
                IdReminder = selectedReminder.Id;
                Name = selectedReminder.Name;
                Description = selectedReminder.Description;
                Time = selectedReminder.Time.ToString();
                RepeatPeriod = selectedReminder.RepeatPeriod.ToString();
                OkCommand = ReactiveCommand.CreateFromTask(EditImpl, canOk);
            }
            else
            {
                OkCommand = ReactiveCommand.CreateFromTask(AddImpl, canOk);
            }
        }

        private async Task<Unit> AddImpl()
        {
            _client.DisconnectedEvent.Subscribe(async _ =>
                await CloseWindowInteraction.Handle(Unit.Default));

            var time = DateTime.Parse(Time);
            var repeatPeriod = TimeSpan.Parse(RepeatPeriod);
            var success = _client.AddEvent(new Event
            {
                UserId = _client.UserId,
                Id = 0,
                Name = Name,
                Description = Description,
                DateTime = Timestamp.FromDateTime(time.ToUniversalTime()),
                RepeatPeriod = Duration.FromTimeSpan(repeatPeriod)
            });

            if (success is null) return Unit.Default;

            if (success is true)
            {
                _reminders.Add(new Reminder
                {
                    Id = (_reminders.Max(x => x.Id as long?) ?? 0) + 1,
                    Name = Name,
                    Description = Description,
                    Time = time,
                    RepeatPeriod = repeatPeriod
                });
            }
            else await ShowErrorInteraction.Handle("Operation failed.");
            return Unit.Default;
        }

        private async Task<Unit> EditImpl()
        {
            _client.DisconnectedEvent.Subscribe(async _ =>
                await CloseWindowInteraction.Handle(Unit.Default));

            var time = DateTime.Parse(Time);
            var repeatPeriod = TimeSpan.Parse(RepeatPeriod);
            var success = _client.ChangeEvent(new Event
            {
                UserId = _client.UserId,
                Id = IdReminder,
                Name = Name,
                Description = Description,
                DateTime = Timestamp.FromDateTime(time.ToUniversalTime()),
                RepeatPeriod = Duration.FromTimeSpan(repeatPeriod)
            });

            if (success is null) return Unit.Default;

            if (success is true)
            {
                for (int i = 0; i < _reminders.Count; i++)
                {
                    if (_reminders[i].Id == IdReminder)
                    {
                        _reminders[i] = new Reminder
                        {
                            Id = IdReminder,
                            Name = Name,
                            Description = Description,
                            Time = time,
                            RepeatPeriod = repeatPeriod
                        };
                        break;
                    }
                }
            }
            else await ShowErrorInteraction.Handle("Operation failed.");
            return Unit.Default;
        }

        private Client _client;
        private ObservableCollection<Reminder> _reminders;
    }
}
