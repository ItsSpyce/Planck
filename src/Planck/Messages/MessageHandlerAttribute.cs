using System.Reflection;

namespace Planck.Messages
{
  [AttributeUsage(AttributeTargets.Method)]
  public class MessageHandlerAttribute : Attribute
  {
    public string Name { get; }

    public MessageHandlerAttribute(string name) => Name = name;
  }

  public class InvalidCommandHandlerException : Exception
  {
    public MethodInfo MethodInfo { get; }

    public InvalidCommandHandlerException(string message, MethodInfo method) : base(message) => MethodInfo = method;
  }
}
