using ReactiveUI;
using System.Reactive;

namespace TelegramBotClient.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public Interaction<string, Unit> ShowErrorInteraction { get; } = new();

        public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

        public Interaction<Unit, Unit> HideWindowInteraction { get; } = new();

        public Interaction<Unit, Unit> ShowWindowInteraction { get; } = new();
    }
}
