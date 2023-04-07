using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Planck.Utils;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Planck.Messages
{
    internal class MessageService : IMessageService, IDisposable
  {
    [StructLayout(LayoutKind.Sequential)]
    struct MessageTimingTrace
    {
      public string Name;
      public double TimeToExecuteMs;
    }

    readonly IServiceProvider _serviceProvider;
    readonly ILogger<MessageService> _logger;
    readonly Dictionary<string, MethodInfo> _messageTypeMap = new();
    readonly ConcurrentQueue<MessageTimingTrace> _timingTraceQueue = new();
    readonly Thread _traceThread;
    bool _isDisposed = false;

    public MessageService(IServiceProvider serviceProvider, ILogger<MessageService> logger)
    {
      _serviceProvider = serviceProvider;
      _logger = logger;
      _traceThread = new(ProcessTraceQueue)
      {
        IsBackground = true,
        Priority = ThreadPriority.BelowNormal,
        Name = "MessageTraceThread",
      };

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

    public async Task<(int, object?)> HandleMessageAsync(JsonElement message)
    {
      if (!_traceThread.IsAlive)
      {
        _traceThread.Start();
      }
      var deserialized = message.Deserialize<PlanckMessage>();
      object? resultToReturn = null;
      if (deserialized is not null && _messageTypeMap.TryGetValue(deserialized.Command, out var method))
      {
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
        _timingTraceQueue.Enqueue(new()
        {
          Name = deserialized.Command,
          TimeToExecuteMs = (DateTime.Now - now).TotalMilliseconds
        });
        return (deserialized.OperationId, resultToReturn);
      }
      throw new Exception("Failed to deserialize message");
    }

    void ProcessTraceQueue()
    {
      while (!_isDisposed)
      {
        if (_timingTraceQueue.TryDequeue(out var trace))
        {
          _logger.LogInformation("Processing time for {name}: {ms}ms", trace.Name, trace.TimeToExecuteMs);
          Thread.Sleep(5);
        }
        else
        {
          Thread.Sleep(150);
        }
      }
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      _isDisposed = true;
      _messageTypeMap.Clear();
      _timingTraceQueue.Clear();
    }
  }
}
