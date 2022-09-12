using Grpc.Net.Client;
using TelegramBot.Proto;
using System;
using System.Reactive;
using System.Reactive.Subjects;

namespace TelegramBotClient
{
    public sealed class Client : IDisposable
    {
        public long UserId { get; private set; }

        public IObservable<Unit> DisconnectedEvent => _disconnectedSubject;
        public Client(long userId)
        {
            UserId = userId;
            _channel = GrpcChannel.ForAddress("http://localhost:5000");
            _client = new TelegramEventService.TelegramEventServiceClient(_channel);
        }

        public UserResponse GetEvents()
        {
            try
            {
                return _client.GetEvents(new GetEventsRequest { UserId = UserId });
            }
            catch
            {
                OnDisconnected();
            }
            throw new InvalidOperationException();
        }

        public bool? AddEvent(Event reminder)
        {
            try
            {
                var response = _client.AddEvent(reminder);
                return response.Result ? true : false;
            }
            catch
            {
                OnDisconnected();
            }
            return null;
        }

        public bool? RemoveEvent(Event reminder)
        {
            try
            {
                var response = _client.RemoveEvent(reminder);
                return response.Result ? true : false;
            }
            catch
            {
                OnDisconnected();
            }
            return null;
        }

        public bool? ChangeEvent(Event reminder)
        {
            try
            {
                var response = _client.ChangeEvent(reminder);
                return response.Result ? true : false;
            }
            catch
            {
                OnDisconnected();
            }
            return null;
        }

        public bool? ContainsUser()
        {
            try
            {
                var response = _client.ContainsUser(new UserRequest { UserId = UserId });
                return response.Result ? true : false;
            }
            catch
            {
                OnDisconnected();
            }
            return null;
        }

        public void Dispose()
        {
            _channel.Dispose();
        }

        private void OnDisconnected()
        {
            _disconnectedSubject.OnNext(Unit.Default);
        }

        private readonly GrpcChannel _channel;
        private readonly TelegramEventService.TelegramEventServiceClient _client;
        private readonly Subject<Unit> _disconnectedSubject = new();
    }
}
