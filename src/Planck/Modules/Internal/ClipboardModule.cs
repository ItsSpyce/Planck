using Planck.Controls;

namespace Planck.Modules.Internal
{
  public class ClipboardModule : Module
  {
    public ClipboardModule(IPlanckWindow planckWindow, IServiceProvider services) : base("clipboard", planckWindow, services) { }

    [ExportMethod("writeText")]
    public string WriteText(string text)
    {
      System.Windows.Clipboard.SetText(text);
      Console.WriteLine(text);
      return text;
    }

    [ExportMethod("readText")]
    public string ReadText() => System.Windows.Clipboard.GetText();
  }
}
