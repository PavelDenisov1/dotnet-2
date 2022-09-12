using Grpc.Core;
using System.Threading.Tasks;
using TelegramBot.Proto;
using TelegramBot.Model;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using TelegramBot.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace TelegramBot.Services
{
    public class TelegramEventService : Proto.TelegramEventService.TelegramEventServiceBase
    {
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<TelegramEventService> _logger;

        public TelegramEventService(IUsersRepository usersRepository, ILogger<TelegramEventService> logger)
        {
            _usersRepository = usersRepository;
            _logger = logger;
        }
        public override Task<EventOperationResponse> AddEvent(Event request, ServerCallContext context)
        {
            try
            {
                var reminder = new UserEvent(
                request.Id,
                request.Name,
                request.Description,
                request.DateTime.ToDateTime(),
                request.RepeatPeriod.ToTimeSpan());
                _usersRepository.AddEvent(request.UserId, reminder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Task.FromResult(new EventOperationResponse { UserId = request.UserId, Result = false });
            }
            _logger.LogTrace($"Add new event for User {request.UserId}");
            return Task.FromResult(new EventOperationResponse { UserId = request.UserId, Result = true });
        }

        public override Task<EventOperationResponse> ChangeEvent(Event request, ServerCallContext context)
        { 
            try
            {
                var reminder = new UserEvent(
                request.Id,
                request.Name,
                request.Description,
                request.DateTime.ToDateTime(),
                request.RepeatPeriod.ToTimeSpan());
                _usersRepository.ChangeEvent(request.UserId, request.Id, reminder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Task.FromResult(new EventOperationResponse { UserId = request.UserId, Result = false });
            }
            _logger.LogTrace($"Change event {request.Id} of User {request.UserId}");
            return Task.FromResult(new EventOperationResponse { UserId = request.UserId, Result = true });
        }

        public override Task<EventOperationResponse> RemoveEvent(Event request, ServerCallContext context)
        {
            try
            {
                _usersRepository.RemoveEvent(request.UserId, request.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Task.FromResult(new EventOperationResponse { UserId = request.UserId, Result = false });
            }
            _logger.LogTrace($"Remove event {request.Id} of User {request.UserId}");
            return Task.FromResult(new EventOperationResponse { UserId = request.UserId, Result = true });
        }

        public override Task<UserResponse> GetEvents(GetEventsRequest request, ServerCallContext context)
        {
            var reminders = new List<Event>();
            var response = new UserResponse();
            var user = _usersRepository.FindUser(request.UserId);
            try
            {
                foreach (var eventReminder in user.Events)
                {
                    reminders.Add(new Event
                    {
                        Id = eventReminder.Id,
                        Name = eventReminder.Name,
                        Description = eventReminder.Description,
                        DateTime = Timestamp.FromDateTime(eventReminder.Time),
                        RepeatPeriod = Duration.FromTimeSpan(eventReminder.RepeatPeriod),
                        UserId = request.UserId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Task.FromResult(response);
            }

            response.Reminders.AddRange(reminders);
            _logger.LogTrace($"Send events to User {request.UserId}");
            return Task.FromResult(response);
        }

        public override Task<EventOperationResponse> ContainsUser(UserRequest request, ServerCallContext context)
        {
            var user = _usersRepository.FindUser(request.UserId);
            if (user is not null)
            {
                return Task.FromResult(new EventOperationResponse { UserId = request.UserId, Result = true });
            }
            return Task.FromResult(new EventOperationResponse { UserId = request.UserId, Result = false });
        }
    }
}