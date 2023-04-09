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
      if (deserialized is not null && _messageTypeMap.TryGetValue(deserialized.Command, out var method))
      {
        await _messageQueue.QueueBackgroundWorkItemAsync(async (cancelToken) =>
        {
          object? resultToReturn = null;
          var now = DateTime.Now;
          var body = deserialized?.Body ?? new();
          using (var scope = _serviceProvider.CreateScope())
          {
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
                resultToReturn = taskResult;
              }
            }
            else
            {
              resultToReturn = result;
            }
          }
          return new(deserialized!.OperationId, resultToReturn);
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
