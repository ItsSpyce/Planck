using Planck.Controls;
using System.IO;

namespace Planck.Modules.Internal
{
  public class FileSystemModule : Module
  {
    public FileSystemModule(IPlanckWindow planck, IServiceProvider services) : base(planck, services)
    {
    }

    [ExportProperty("directorySeparator")]
    public string DirectorySeparator => Path.DirectorySeparatorChar.ToString();

    [ExportMethod("readFile")]
    public Stream ReadFile(string path) => File.OpenRead(path);
  }
}
