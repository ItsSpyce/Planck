using Planck.Controls;
using Planck.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Planck.Modules.Internal
{
  public class FileSystemModule : Module
  {
    public FileSystemModule(IPlanckWindow planck, IServiceProvider services) : base(planck, services)
    {
    }

    [ExportProperty]
    public string DirectorySeparator => Path.DirectorySeparatorChar.ToString();

    [ExportMethod]
    public InteropStream? ReadFile(string path)
    {
      if (!File.Exists(path))
      {
        return null;
      }
      var stream = File.OpenRead(path);
      if (stream == default)
      {
        return null;
      }
      return new InteropStream(stream);
    }
  }
}
