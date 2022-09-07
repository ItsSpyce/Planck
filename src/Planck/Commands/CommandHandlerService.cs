using Planck.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Commands
{
  public interface ICommandHandlerService
  {
    Task<object?> InvokeAsync(string name, JsonElement? arguments);
  }

  internal class CommandHandlerService : ICommandHandlerService
  {
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, List<MethodInfo>> _commandMap = new();

    public CommandHandlerService(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public void BuildFromType<T>()
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
        if (!_commandMap.ContainsKey(commandHandlerAttr.Name))
        {
          _commandMap.Add(commandHandlerAttr.Name, new()
          {
            method,
          });
        }
        else
        {
          _commandMap[commandHandlerAttr.Name].Add(method);
        }
      }
    }

    public async Task<object?> InvokeAsync(string name, JsonElement? arguments)
    {
      JsonElement args;
      if (arguments.HasValue)
      {
        args = arguments.Value;
      }
      else
      {
        args = new();
      }
      if (_commandMap.TryGetValue(name, out var commands))
      {
        foreach (var command in commands)
        {
          var parameters = command.GetParameters();
          var serviceAttrs = parameters.Where(p => p.GetCustomAttribute<ServiceAttribute>() != null);
          var methodArgs = command.GetParameters().Select(p =>
          {
            if (p.GetCustomAttribute<ServiceAttribute>() != null)
            {
              return _serviceProvider.GetService(p.ParameterType);
            }
            if (args.TryGetProperty(p.Name, out var argValue))
            {
              return JsonSerializer.Deserialize(argValue, p.ParameterType);
            }
            return null;
          });
          var result = command.Invoke(null, methodArgs.ToArray());
          if (result is Task awaitable)
          {
            await awaitable;
          }
        }
      }
      return null;
    }
  }
}
