# Planck

A library for creating cross-platform apps using WebView2 and WPF.

## Usage

1. Create your WPF project
2. Run `> dotnet add package Planck` or install via Visual Studio tools
3. Navigate to your `App.xaml.cs` and add the following code:

```cs
using Planck.Controls;

public partial class App : Application
{
  async void OnStartup(object sender, StartupEventArgs args)
  {
    await PlanckApplication.StartAsync();
  }
}
```

4. Add `appsettings.json` to your root project directory with the following lines:

```json
{
  "Planck": {
    // the directory the HTML/JS code lives in
    "ClientDirectory": "Client",
    // the output directory for production builds
    "BuildDirectory": "Client\\dist",
    // whether or not to use WPF. WinForms support is TBD
    "UseWpf": true,
    // the URL to navigate to during development. If null, it will
    // fallback to the Entry field
    "DevUrl": "http://localhost:3000/",
    // entry for production builds
    "Entry": "Client\\dist\\index.html",
    // the script to run during development for npm
    "DevCommand": "dev"
  }
}
```

5. Create your HTML files. It's recommended to use Vite as a starting point. If you do, ensure the port is the same as what's inside your `appsettings.json`.
6. Run!

## Communicating between C# and JS

Planck hooks into WebView2's messaging functionality and extends it a little further. Built with DependencyInjection as a first-class citizen, Planck allows you to pass a delegate into `StartAsync` that chains to `ConfigureServices`. All Planck functionality is built around DI.
