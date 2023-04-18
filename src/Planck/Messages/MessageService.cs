using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Planck.Services;
using Planck.Utilities;
using System.Reflection;
using System.Text.Json;

namespace Planck.Messages
{
  internal class MessageService : IMessageService, IDisposable
  {

    readonly IServiceProvider _serviceProvider;
    readonly ILogger<MessageService> _logger;
    readonly IBackgroundTaskQueue<MessageWorkResponse> _messageQueue;
    readonly Dictionary<string, MethodInfo> _messageTypeMap = new();

    public MessageService(
      IServiceProvider serviceProvider,
      ILogger<MessageService> logger,
      IBackgroundTaskQueue<MessageWorkResponse> messageQueue)
    {
      _serviceProvider = serviceProvider;
      _logger = logger;
      _messageQueue = messageQueue;

      var messageControllers = serviceProvider.GetServices<MessageController>();
      foreach (var messageController in messageControllers)
      {
        var handlerMethods = messageController.GetType().GetMethods()
          .Where(m => m.IsPublic && m.GetCustomAttribute<MessageHandlerAttribute>() is not null)
          .Select(m => (m.GetCustomAttribute<MessageHandlerAttribute>()?.Name ?? m.Name, m));
        foreach (var (name, method) in handlerMethods)
        {
          _messageTypeMap.Add(name, method);
        }
      }
    }

    public async void HandleMessage(JsonElement message)
    {
      var deserialized = message.Deserialize<PlanckMessage>();
      if (deserialized is not null)
      {
        await _messageQueue.QueueBackgroundWorkItemAsync(async (cancelToken) =>
        {
          MessageWorkResponse response;
          if (_messageTypeMap.TryGetValue(deserialized.Command, out var method))
          {
            try
            {
              var body = deserialized?.Body ?? new();
              using var scope = _serviceProvider.CreateScope();
              var controllerInstance = _serviceProvider.GetService(method.DeclaringType!);

              var methodArgs = InteropConverter.ConvertJsonToMethodArgs(body, method, _serviceProvider.GetService);
              var result = method.Invoke(controllerInstance, methodArgs);
              if (result is Task resultAsTask)
              {
                await resultAsTask;
                var resultProperty = resultAsTask.GetType().GetProperty("Result");
                var taskResult = resultProperty?.GetValue(resultAsTask);
                if (taskResult is not null)
                {
                  response = new(deserialized!.OperationId, taskResult);
                }
                else
                {
                  response = new(deserialized!.OperationId);
                }
              }
              else
              {
                response = new(deserialized!.OperationId, result);
              }
            }
            catch (Exception ex)
            {
              response = new(deserialized.OperationId, error: ex.Message);
            }
          }
          else
          {
            response = new(deserialized.OperationId, error: "Command not found");
          }
          return response;
        });
      }
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      _messageTypeMap.Clear();
    }
  }
}
