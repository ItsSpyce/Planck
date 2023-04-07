using Planck.Controls;
using System.IO;

namespace Planck.Modules.Internal
{
  public class FileSystemModule : Module
  {
    public FileSystemModule(IPlanckWindow planck, IServiceProvider services) : base("fs", planck, services)
    {
    }

    [ExportProperty("directorySeparator")]
    public string DirectorySeparator => Path.DirectorySeparatorChar.ToString();

    [ExportMethod("readFile")]
    public Stream ReadFile(string path) => File.OpenRead(path);

    [ExportMethod("openFolderDialog")]
    public string? OpenFolderDialog(string root, string? filter = "")
    {
      var dialog = new FolderBrowserDialog
      {
        InitialDirectory = root,
      };

      switch (dialog.ShowDialog())
      {
        case DialogResult.OK:
          return dialog.SelectedPath;
        default:
          return null;
      }
    }

    [ExportMethod("openFileDialog")]
    public string? OpenFileDialog(string root, string? filter = "")
    {
      var dialog = new OpenFileDialog
      {
        Filter = filter,
        InitialDirectory = root,
      };
      switch (dialog.ShowDialog())
      {
        case DialogResult.OK:
        case DialogResult.Yes:
          return dialog.FileName;
        default:
          return null;
      }
    }
  }
}
