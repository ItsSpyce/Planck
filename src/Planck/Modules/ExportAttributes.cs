namespace Planck.Modules
{
  [AttributeUsage(AttributeTargets.Method)]
  public class ExportMethodAttribute : Attribute
  {
    public string? Name { get; }

    public ExportMethodAttribute() { }

    public ExportMethodAttribute(string name)
    {
      Name = name;
    }
  }

  [AttributeUsage(AttributeTargets.Property)]
  public class ExportPropertyAttribute : Attribute
  {
    public string? Name { get; }

    public ExportPropertyAttribute() { }

    public ExportPropertyAttribute(string name)
    {
      Name = name;
    }
  }
}
