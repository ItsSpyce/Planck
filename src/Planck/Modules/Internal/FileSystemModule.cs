using System.ComponentModel;
using System.IO;
using System.Windows;

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
    public string? OpenFolderDialog(string root)
    {
      var dialog = new FolderBrowserDialog
      {
        InitialDirectory = root,
        
      };

      return dialog.ShowDialog() switch
      {
        DialogResult.OK => dialog.SelectedPath,
        _ => null,
      };
    }

    [ExportMethod("openFileDialog")]
    public string? OpenFileDialog(string root, string? filter = "")
    {
      var dialog = new OpenFileDialog
      {
        Filter = filter,
        InitialDirectory = root,
      };
      return dialog.ShowDialog() switch
      {
        DialogResult.OK or DialogResult.Yes => dialog.FileName,
        _ => null,
      };
    }
  }
}
