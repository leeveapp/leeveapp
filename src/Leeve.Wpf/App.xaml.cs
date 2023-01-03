using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Leeve.Application.Messages;
using Leeve.Client.Common;
using Leeve.Core.Common;
using Leeve.Wpf.Common.JabModules;
using Leeve.Wpf.Main;
using Serilog;

namespace Leeve.Wpf;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ILogger _log;

    public App()
    {
        _serviceProvider = new ServiceProvider();
        _log = _serviceProvider.GetService<ILogger>();

        SetupExceptionHandling();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        StartClientServices();

        var window = GenerateMainWindow();
        ApplyInitialTheme();

        window.Show();
    }

    private void StartClientServices()
    {
        var clientService = _serviceProvider.GetService<IClientService>();
        clientService.StartAsync(); // this is not awaited on purpose
    }

    private MainWindow GenerateMainWindow()
    {
        var view = _serviceProvider.GetService<MainWindow>();
        Current.MainWindow = view;
        return view;
    }

    private void ApplyInitialTheme()
    {
        var hub = _serviceProvider.GetService<IMessenger>();
        var themeHelper = _serviceProvider.GetService<IThemeHelper>();

        hub.Register<ThemeSwitchedMessage>(this, (_, _) => themeHelper.ToggleTheme());
        themeHelper.ThemeApplied += (_, _) => hub.Send(new ThemeAppliedMessage(themeHelper.IsDarkMode));

        themeHelper.ApplyTheme(themeHelper.IsDarkMode);
    }
    
    private void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _log.Error("{Message}", $"{e.Exception.Message}{e.Exception.StackTrace}");
        ShowMessage(e.Exception.InnerExceptions[0].Message);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception exception) return;

        _log.Error("{Message}", $"{exception.Message}{exception.StackTrace}");
        ShowMessage(exception.Message);
    }

    private void ShowMessage(string message)
    {
        var threadWrapper = _serviceProvider.GetService<IThreadWrapper>();

        threadWrapper.Invoke(() =>
        {
            var dialog = new ErrorDialog(message);
            dialog.ShowDialog();
            Environment.Exit(0);
        });
    }
}