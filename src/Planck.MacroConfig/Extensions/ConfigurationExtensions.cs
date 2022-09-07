using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Planck.MacroConfig.Extensions
{
  public static class ConfigurationExtensions
  {
    static readonly Regex _macroRegex = new(@"\$\(([A-Za-z_]+(?:\:[A-Za-z_]+)*)\)");

    public static IConfigurationBuilder BindMacros(
      this IConfigurationBuilder builder, Dictionary<string, string>? macros = null)
    {
      var builtConfig = builder.Build();
      var inMemoryConfig = new Dictionary<string, string>(macros ?? new());
      var contents = builtConfig.AsEnumerable().Reverse();
      // TODO: build dependency tree and process off of that as opposed to a foreach loop
      foreach (var kvp in contents)
      {
        var value = kvp.Value;
        if (inMemoryConfig.ContainsKey(kvp.Key) || string.IsNullOrEmpty(value))
        {
          continue;
        }
        var macroMatch = _macroRegex.Match(value);
        while (macroMatch.Success)
        {
          var keyMatch = macroMatch.Groups[1]?.Value;
          if (string.IsNullOrEmpty(keyMatch))
          {
            continue;
          }
          if (inMemoryConfig.ContainsKey(keyMatch))
          {
            value = value.Replace(macroMatch.Groups[0].Value, inMemoryConfig[keyMatch]);
          }
          macroMatch = macroMatch.NextMatch();
        }
        inMemoryConfig.Add(kvp.Key, value);
      }

      builder.Add(new MemoryConfigurationSource
      {
        InitialData = inMemoryConfig,
      });
      return builder;
    }
  }
}
