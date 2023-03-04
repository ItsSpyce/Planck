using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Planck.MSBuild
{
  public class BuildClient : Task
  {
    [Required]
    public string SettingBuildDirectory { get; set; }

    [Required]
    public string SettingBuildCommand { get; set; }

    public override bool Execute()
    {
      return !Log.HasLoggedErrors;
    }
  }
}
