using System;

namespace TelegramBotClient.Models
{
    public class Reminder
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
        public TimeSpan RepeatPeriod { get; set; }
    }
}
