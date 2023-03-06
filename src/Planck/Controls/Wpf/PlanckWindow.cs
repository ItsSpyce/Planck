﻿using Planck.Commands;
using Planck.Configuration;
using System.Windows;
using Planck.Resources;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Text.RegularExpressions;
using Planck.Extensions;
using System.IO;
using System.Windows.Input;

namespace Planck.Controls.Wpf
{
  internal class PlanckWindow : Window, IPlanckWindow
  {
    private static readonly Regex _commandRequestRegex = new(@"^([A-Za-z0-9\._]+)__request__([0-9]+)$");
    private static readonly Regex _commandResponseRegex = new(@"^([A-Za-z0-9\._]+)__response__([0-9]+)$");

    #region Properties
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

    public static readonly DependencyProperty HasCompletedBootstrapProperty = DependencyProperty.Register(
      "HasCompletedBootstrap",
      typeof(bool),
      typeof(PlanckWindow));
    #endregion

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

    public bool HasCompletedBootstrap
    {
      get => (bool)GetValue(HasCompletedBootstrapProperty);
      set
      {
        SetValue(HasCompletedBootstrapProperty, value);
        BootstrapCompleted?.Invoke(this, EventArgs.Empty);
      }
    }

    public CoreWebView2 CoreWebView2 => WebView.CoreWebView2;
    public event EventHandler BootstrapCompleted;

    protected WebView2 WebView => (WebView2)Content;

    readonly IResourceService _resourceService;
    readonly ILogger<PlanckWindow> _logger;
    readonly IPlanckSplashscreen _splashscreen;
    readonly PlanckConfiguration _configuration;
    readonly ICommandHandlerService _commandHandlerService;
    readonly RoutedCommand _f12Command = new();

    public PlanckWindow(
      IResourceService resourceService,
      ILogger<PlanckWindow> logger,
      IPlanckSplashscreen splashscreen,
      ICommandHandlerService commandHandlerService,
      IOptions<PlanckConfiguration> configuration,
      CoreWebView2EnvironmentOptions envOptions)
    {
      Background = System.Windows.Media.Brushes.Transparent;
      _configuration = configuration.Value;

      if (SslOnly == default)
        SslOnly = _configuration.SslOnly;
      if (OpenLinksIn == default)
        OpenLinksIn = _configuration.OpenLinksIn;

      if (Splashscreen == default)
        Splashscreen = _configuration.Splashscreen;

      _resourceService = resourceService;
      _logger = logger;
      _splashscreen = splashscreen;
      _commandHandlerService = commandHandlerService;
      Content = new WebView2();
      Loaded += async (_, _) =>
      {
        var env = await CoreWebView2Environment.CreateAsync(null, null, envOptions);
        await WebView.EnsureCoreWebView2Async(env);
        this.ConfigureSecurityPolicies(OpenLinksIn);
        this.ConfigureResources(_resourceService, _configuration.DevUrl ?? Directory.GetCurrentDirectory());
        this.ConfigureCommands(_commandHandlerService);

        // WebView.NavigateToString(Constants.StartPageContent);
        WebView.Source = new Uri(IResourceService.AppUrl);

#if DEBUG
        WebView.CoreWebView2.OpenDevToolsWindow();
        _f12Command.InputGestures.Add(new KeyGesture(Key.F12));
        CommandBindings.Add(new(_f12Command, (_, args) => WebView.CoreWebView2.OpenDevToolsWindow()));
#endif
      };
      // this portion hides the window until it's ready to prevent white screen flicker
      var windowState = WindowState;
      var showInTaskbar = ShowInTaskbar;

      WindowState = WindowState.Minimized;
      ShowInTaskbar = false;
      BootstrapCompleted += (sender, args) =>
      {
        WindowState = windowState;
        ShowInTaskbar = showInTaskbar;
        // this.NavigateToEntry(_configuration);
        CloseSplashscreen();
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
      _splashscreen?.Show();
    }

    public void CloseSplashscreen()
    {
      _splashscreen?.Close();
    }
  }
}
