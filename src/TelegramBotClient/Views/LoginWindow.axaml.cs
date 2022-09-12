using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using TelegramBotClient.ViewModels;

namespace TelegramBotClient.Views
{
    public partial class LoginWindow : Window<ViewModelBase>
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
    }
}
