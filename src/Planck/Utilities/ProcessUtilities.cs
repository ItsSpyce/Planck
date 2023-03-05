using System.IO;

namespace Planck.Utilities
{
  internal static class ProcessUtilities
  {
    public static string GetProjectLocation()
    {
      var currentDir = AppDomain.CurrentDomain.BaseDirectory;
      while (currentDir != null && Path.GetDirectoryName(currentDir) != currentDir)
      {
        if (Directory.GetFiles(currentDir).Any(file => file.EndsWith(".csproj")))
        {
          return currentDir;
        }
        currentDir = Path.GetDirectoryName(currentDir);
      }
      throw new FileNotFoundException("No csproj file found");
    }
  }
}
