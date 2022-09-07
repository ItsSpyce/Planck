using Planck.Controls;
using Planck.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Planck.Commands;
using Planck.Resources;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Wpf;
using Planck.Utilities;
using Microsoft.Web.WebView2.Core;
using Planck.Messages;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using System.Diagnostics;
using System.Net.Http;
using Planck.HttpClients;
using Microsoft.Extensions.Http;
using Commands;

namespace Planck.Controls.Wpf
{
  internal class PlanckWindow : Window, IPlanckWindow
  {
    private static readonly Regex _commandRequestRegex = new(@"^([A-Za-z0-9\._]+)__request__([0-9]+)$");
    private static readonly Regex _commandResponseRegex = new(@"^([A-Za-z0-9\._]+)__response__([0-9]+)$");

    public static readonly DependencyProperty SslOnlyProperty = DependencyProperty.Register(
      "SslOnly",
      typeof(bool),
      typeof(PlanckWindow));

    public static readonly DependencyProperty OpenLinksInProperty = DependencyProperty.Register(
      "OpenLinksIn",
      typeof(PlanckConfiguration.LinkLaunchRule),
      typeof(PlanckWindow));

    public static readonly DependencyProperty SplashscreenProperty = DependencyProperty.Register(
      "SplashScreen",
      typeof(string),
      typeof(PlanckWindow));

    public static readonly DependencyProperty AllowExternalMessagesProperty = DependencyProperty.Register(
      "AllowExternalMessages",
      typeof(bool),
      typeof(PlanckWindow));

    public bool SslOnly
    {
      get => (bool)GetValue(SslOnlyProperty);
      set => SetValue(SslOnlyProperty, value);
    }

    public PlanckConfiguration.LinkLaunchRule OpenLinksIn
    {
      get => (PlanckConfiguration.LinkLaunchRule)GetValue(OpenLinksInProperty);
      set => SetValue(OpenLinksInProperty, value);
    }

    public string? Splashscreen
    {
      get => GetValue(SplashscreenProperty) as string;
      set => SetValue(SplashscreenProperty, value);
    }

    public bool AllowExternalMessages
    {
      get => (bool)GetValue(AllowExternalMessagesProperty);
      set => SetValue(AllowExternalMessagesProperty, value);
    }

    public CoreWebView2 CoreWebView2 => WebView.CoreWebView2;

    protected WebView2 WebView => (WebView2)Content;

    private readonly IResourceService _resourceService;
    private readonly ILogger<PlanckWindow> _logger;
    private readonly IPlanckSplashscreen _splashscreen;
    private readonly PlanckConfiguration _configuration;
    private readonly ICommandHandlerService _commandHandlerService;

    public PlanckWindow(
      IResourceService resourceService,
      IOptions<PlanckConfiguration> options,
      ILogger<PlanckWindow> logger,
      IPlanckSplashscreen splashscreen,
      ICommandHandlerService commandHandlerService)
    {
      _configuration = options.Value;
      SslOnly = options.Value.SslOnly;
      OpenLinksIn = options.Value.OpenLinksIn;
      Splashscreen = options.Value.Splashscreen;
      _resourceService = resourceService;
      _logger = logger;
      _splashscreen = splashscreen;
      _commandHandlerService = commandHandlerService;
      Content = new WebView2();
      Loaded += (_, _) =>
      {
        ConfigureNavigationEvents();
        ConfigureSecurity();
        ConfigureCommands();
        WebView.Source = new Uri(_configuration.DevUrl);
#if DEBUG
        ConfigureDebug();
#else
#endif
      };
    }

    public new void Show()
    {
      base.Show();
    }

    public new void Close()
    {
      base.Close();
    }

    public void ShowSplashscreen()
    {
      _splashscreen.Show();
    }

    public void CloseSplashscreen()
    {
      _splashscreen.Close();
    }

    async void ConfigureNavigationEvents()
    {
      await WebView.EnsureCoreWebView2Async();
      WebView.NavigationStarting += (sender, args) =>
      {
        // TODO: cancel if not devurl
      };
    }

    async void ConfigureSecurity()
    {
      await WebView.EnsureCoreWebView2Async();
      CoreWebView2.NewWindowRequested += (_, args) =>
      {
        args.Handled = true;
        // TODO: change based on config
        switch(OpenLinksIn)
        {
          case PlanckConfiguration.LinkLaunchRule.MachineDefault:
            break;
          case PlanckConfiguration.LinkLaunchRule.CurrentWindow:
            WebView.Source = new Uri(args.Uri);
            break;
          case PlanckConfiguration.LinkLaunchRule.NewWindow:
            break;
        }
      };
    }

    async void ConfigureCommands()
    {
      await WebView.EnsureCoreWebView2Async();
      CoreWebView2.WebMessageReceived += async (_, args) =>
      {
        if (args.Source != _configuration.DevUrl && !AllowExternalMessages)
        {
          throw new UnauthorizedAccessException("This container is configured to not accept external messages");
        }
        if (PlanckCommandMessage.TryParse(args.WebMessageAsJson, out var message))
        {
          if (!_commandRequestRegex.IsMatch(message.Command) && !_commandResponseRegex.IsMatch(message.Command))
          {
            return;
          }
          var (commandName, commandId) = GetCommandParts(message.Command);
          if (message.Command.Contains("__request__"))
          {
            var result = await _commandHandlerService.InvokeAsync(commandName, message.Body);
            var resultAsJson = JsonSerializer.SerializeToElement(result);
            var response = new PlanckCommandMessage
            {
              Command = $"{commandName}__response__${commandId}",
              Body = resultAsJson,
            };
            var responseBytes = JsonSerializer.SerializeToUtf8Bytes(response);
            CoreWebView2.PostWebMessageAsJson(Encoding.UTF8.GetString(responseBytes));
          }
          else if (message.Command.Contains("__response__"))
          {
            await _commandHandlerService.InvokeAsync(commandName, message.Body);
          }
          else
          {
            // shouldn't happen, ignore
          }
        }
      };
    }

    async void ConfigureDebug()
    {
      await WebView.EnsureCoreWebView2Async();
      WebView.CoreWebView2.OpenDevToolsWindow();
    }

    static (string commandName, string commandId) GetCommandParts(string fullCommand)
    {
      var commandSections = _commandRequestRegex.Matches(fullCommand)[0];
      var commandName = commandSections.Groups[1]?.Value!;
      var commandId = commandSections.Groups[2]?.Value!;
      return (commandName, commandId);
    }
  }
}
