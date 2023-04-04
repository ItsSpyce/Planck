using Microsoft.Extensions.Logging;
using Planck.Commands;
using Planck.Utils;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
    readonly Dictionary<string, List<MethodInfo>> _messageTypeMap = new();
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
    }

    public MessageService AddHandlersFromType<T>()
    {
      var methods = typeof(T).GetMethods();
      foreach (var method in methods)
      {
        if (!method.IsPublic || !method.IsStatic)
        {
          continue;
        }
        var commandHandlerAttr = method.GetCustomAttribute<CommandHandlerAttribute>();
        if (commandHandlerAttr is null)
        {
          continue;
        }
        if (!_messageTypeMap.ContainsKey(commandHandlerAttr.Name))
        {
          _messageTypeMap.Add(commandHandlerAttr.Name, new()
          {
            method,
          });
        }
        else
        {
          _messageTypeMap[commandHandlerAttr.Name].Add(method);
        }
      }
      return this;
    }

    public async Task<(int, IEnumerable<object?>)> HandleMessageAsync(JsonElement message)
    {
      if (!_traceThread.IsAlive)
      {
        _traceThread.Start();
      }
      var deserialized = message.Deserialize<PlanckMessage>();
      var resultList = new List<object?>();
      if (deserialized is not null && _messageTypeMap.TryGetValue(deserialized.Command, out var messageMethods))
      {
        var now = DateTime.Now;
        var body = deserialized.Body ?? new();
        foreach (var method in messageMethods)
        {
          var methodArgs = InteropConverter.ConvertJsonToMethodArgs(body, method, _serviceProvider.GetService);
          var result = method.Invoke(null, methodArgs);
          if (result is Task<object> awaitableWithReturn)
          {
            var awaited = await awaitableWithReturn;
            resultList.Add(awaited);
          }
          else
          {
            resultList.Add(result);
          }
        }
        _timingTraceQueue.Enqueue(new()
        {
          Name = deserialized.Command,
          TimeToExecuteMs = (DateTime.Now - now).TotalMilliseconds
        });
        return (deserialized.OperationId, resultList);
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
