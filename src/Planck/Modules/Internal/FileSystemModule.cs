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
    public FileSystemModule(IPlanckWindow planck) : base(planck)
    {
    }

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

    public string GetDirectorySeparator() => Path.DirectorySeparatorChar.ToString();
  }
}
