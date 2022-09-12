using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Repositories;

namespace TelegramBot
{
    public class TelegramBotUpdateHandler : IUpdateHandler
    {
        private readonly IUsersRepository _usersRepository;

        public TelegramBotUpdateHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        async Task IUpdateHandler.HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message)
                return;
            if (update.Message.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;
            if (messageText == "/start")
            {
                var newUser = new Model.User(
                    update.Message.From.FirstName,
                    update.Message.From.Id,
                    chatId);
                _usersRepository.AddUser(newUser);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"You are added. Your code: {update.Message.From.Id}",
                    cancellationToken: cancellationToken);
            }
        }
    }
}