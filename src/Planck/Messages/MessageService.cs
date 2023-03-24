using Microsoft.Extensions.Logging;
using Planck.Commands;
using System;
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
        if (commandHandlerAttr == null)
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

    public async Task HandleMessageAsync(JsonElement message)
    {
      if (!_traceThread.IsAlive)
      {
        _traceThread.Start();
      }
      var deserialized = message.Deserialize<PlanckCommandMessage>();
      if (deserialized != null && _messageTypeMap.TryGetValue(deserialized.Command, out var messageMethods))
      {
        var now = DateTime.Now;
        var body = deserialized.Body ?? new();
        var taskList = new List<Task>();
        foreach (var method in messageMethods)
        {
          var parameters = method.GetParameters();
          var methodArgs = parameters.Select(p =>
          {
            if (p.GetCustomAttribute<ServiceAttribute>() != null)
            {
              return _serviceProvider.GetService(p.ParameterType);
            }
            if (body.TryGetProperty(p.Name!, out var argValue))
            {
              return JsonSerializer.Deserialize(argValue, p.ParameterType);
            }
            return null;
          });
          var result = method.Invoke(null, methodArgs.ToArray());
          if (result is Task awaitable)
          {
            taskList.Add(awaitable);
          }
        }
        if (taskList.Count > 0)
        {
          await Task.WhenAll(taskList);
        }
        _timingTraceQueue.Enqueue(new()
        {
          Name = deserialized.Command,
          TimeToExecuteMs = (DateTime.Now - now).TotalMilliseconds
        });
      }
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
