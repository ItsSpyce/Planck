namespace Planck.Commands
{
  [AttributeUsage(AttributeTargets.Struct)]
  public class CommandAttribute : Attribute
  {
    public string Name { get; }

    public CommandAttribute(string name)
    {
      Name = name;
    }
  }

  public class InvalidCommandException : Exception
  {
    public InvalidCommandException(Type type) : base($"Invalid command, requires CommandAttribute")
    {

    }
  }
}
