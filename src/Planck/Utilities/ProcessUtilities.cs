using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Planck.Utilities
{
  internal static class ProcessUtilities
  {
    public static bool TryRunCommand(string command, string workingDirectory, out Process process)
    {
      var commandParts = command.Split(new[] { ' ' }, 2);
      process = new Process
      {
        StartInfo = new()
        {
          WorkingDirectory = workingDirectory,
          UseShellExecute = true,
          CreateNoWindow = false,
          FileName = commandParts[0],
          Arguments = commandParts[1],
        },
        EnableRaisingEvents = true,
      };
      try
      {
        process.Start();
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}
