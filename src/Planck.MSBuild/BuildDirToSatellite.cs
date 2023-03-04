using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Linq;

namespace Planck.MSBuild
{
  public class BuildDirToSatellite : Task
  {
    [Required]
    public string Output { get; set; }

    [Required]
    public string Input { get; set; }

    public string Filter { get; set; } = "*.*";

    [Output]
    public string OutputFile { get; set; }

    public override bool Execute()
    {
      var outputFile = Path.Combine(Input, $"{Output}.resources");
      if (File.Exists(outputFile))
      {
        File.Delete(outputFile);
      }
      var files = Directory.EnumerateFiles(Input, Filter, SearchOption.AllDirectories);
      var resxWriter = new ResourceWriter($"{Output}.resources");
      foreach (var file in files)
      {
        resxWriter.AddResource(file.Replace(Input, ""), File.ReadAllBytes(file));
      }
      OutputFile = outputFile;

      return !Log.HasLoggedErrors;
    }
  }
}
