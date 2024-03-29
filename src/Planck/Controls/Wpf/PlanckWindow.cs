﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Planck.Configuration;
using Planck.Exceptions;
using Planck.Extensions;
using Planck.Messages;
using Planck.Modules;
using Planck.Resources;
using Planck.Services;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Planck.Controls.Wpf
{
  internal class PlanckWindow : Window, IPlanckWindow
  {
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

    readonly ILogger<PlanckWindow> _logger;
    readonly IPlanckSplashscreen _splashscreen;
    readonly PlanckConfiguration _configuration;
    readonly RoutedCommand _f12Command = new();

    public PlanckWindow(
      IServiceProvider services,
      ILogger<PlanckWindow> logger,
      IPlanckSplashscreen splashscreen,
      PlanckConfiguration configuration)
    {
      Background = null;
      _configuration = configuration;

      if (SslOnly == default)
        SslOnly = _configuration.SslOnly;
      if (OpenLinksIn == default)
        OpenLinksIn = _configuration.OpenLinksIn;

      if (Splashscreen == default)
        Splashscreen = _configuration.Splashscreen;

      _logger = logger;
      _splashscreen = splashscreen;
      Content = new WebView2
      {
        DefaultBackgroundColor = Color.Transparent,
        CreationProperties = new()
        {
          // UserDataFolder = new Uri
        },
      };

      Loaded += async (_, _) =>
      {
        Hide();
        var resourceService = services.GetService<IResourceService>();
        var messageService = services.GetService<IMessageService>();
        var moduleService = services.GetService<IModuleService>();

        if (resourceService is null)
        {
          throw new PlanckWindowException("No IResourceService found");
        }
        if (messageService is null)
        {
          throw new PlanckWindowException("No IMessageService found");
        }
        if (moduleService is null)
        {
          throw new PlanckWindowException("No IModuleService found");
        }

        // var env = await CoreWebView2Environment.CreateAsync(null, null, envOptions);
        await WebView.EnsureCoreWebView2Async();
        this.ConfigureCoreWebView2();
        this.ConfigureSecurityPolicies(OpenLinksIn);
        this.ConfigureResources(resourceService, _configuration.DevUrl ?? Directory.GetCurrentDirectory());
        this.ConfigureMessages(messageService);
        this.ConfigureModules(moduleService);
        // CoreWebView2.AddHostObjectToScript("clipboard", new ClipboardModule(this));

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

      //WindowState = WindowState.Minimized;
      //ShowInTaskbar = false;
      BootstrapCompleted += (sender, args) =>
      {
        Dispatcher.Invoke(() =>
        {
          WindowState = windowState;
          ShowInTaskbar = showInTaskbar;
          Show();
        });
        // this.NavigateToEntry(_configuration);
        CloseSplashscreen();
      };
    }

    public void SetWindowState(IPlanckWindow.WindowState state) => Dispatcher.Invoke(() =>
      WindowState = (WindowState)state);

    public IPlanckWindow.WindowState GetWindowState() => Dispatcher.Invoke(() =>
      (IPlanckWindow.WindowState)WindowState);

    public void ShowSplashscreen()
    {
      _splashscreen?.Show();
    }

    public void CloseSplashscreen()
    {
      _splashscreen?.Close();
    }

    public void PostWebMessage(string command, object? body)
    {
      Dispatcher.Invoke(() =>
      {
        CoreWebView2.PostWebMessage(command, body);
      });
    }

    public void PostWebMessage(int operationId, object? body)
    {
      Dispatcher.Invoke(() =>
      {
        CoreWebView2.PostWebMessage(operationId, body);
      });
    }
  }
}
