using System.Reflection;

namespace Planck.Commands
{
  [AttributeUsage(AttributeTargets.Method)]
  public class CommandHandlerAttribute : Attribute
  {
    public string Name { get; }

    public CommandHandlerAttribute(string name)
    {
      Name = name;
    }
  }

  public class InvalidCommandHandlerException : Exception
  {
    public MethodInfo MethodInfo { get; }

    public InvalidCommandHandlerException(string message, MethodInfo method) : base(message)
    {
      MethodInfo = method;
    }
  }
}
