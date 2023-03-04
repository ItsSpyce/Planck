using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Resources;

var appsettingsFile = GetOptionalArgument(0) ?? "appsettings.json";
var planckConfig = JObject.Parse(File.ReadAllText(appsettingsFile));
var buildDirectory = planckConfig["BuildDirectory"]?.ToString();
var buildCommand = planckConfig["BuildCommand"]?.ToString();
var buildArgs = planckConfig["BuildArgs"]?.AsJEnumerable();

if (string.IsNullOrEmpty(buildDirectory))
{
  throw new ArgumentNullException("BuildDirectory", "BuildDirectory required to build");
}

var directory = args[0];
var assemblyName = args[1];
var filter = GetOptionalArgument(2) ?? "*.*";
var currentDirectory = Directory.GetCurrentDirectory();
var inputDirectory = Path.IsPathFullyQualified(directory) ? directory : Path.Join(currentDirectory, directory);
var outputFile = GetOptionalArgument(3) ?? Path.Join(currentDirectory, $"{assemblyName}.resources");

if (File.Exists(outputFile))
{
  File.Delete(outputFile);
}

var files = Directory.EnumerateFiles(inputDirectory, filter, SearchOption.AllDirectories);
var resxWriter = new ResXResourceWriter(Path.Join(currentDirectory, $"{assemblyName}.resources"));
foreach (var file in files)
{
  var resxName = Path.GetRelativePath(inputDirectory, file).Replace('\\', '/');
  resxWriter.AddResource(resxName, File.ReadAllBytes(file));
  Console.WriteLine(file);
}
// force generation else it'll just...stop somewhere lol
resxWriter.Generate();
resxWriter.Close();

// TODO: call al.exe and embed assets from the directory so we can create a signed DLL or even
// merge the two

string? GetOptionalArgument(int index)
  => args.Length > index ? args[index] : null;

static async Task ExecCommandAsync(string command, params string[] args)
{

}
