using System;
using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;
using Jab;
using Leeve.Application.Common;
using Leeve.Client.Common;
using Leeve.Client.Evaluations;
using Leeve.Client.Questionnaires;
using Leeve.Client.Users;
using Leeve.Core.Common;
using Leeve.Wpf.Main;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Leeve.Wpf.Common.JabModules;

[ServiceProvider]
[Singleton(typeof(MainWindow))]
[Import(typeof(IClientServicesModule))]
[Import(typeof(INotificationServicesModule))]
[Import(typeof(IMainViewModelsModule))]
[Import(typeof(IAdminViewModelsModule))]
[Import(typeof(ITeacherViewModelsModule))]
[Import(typeof(IEvaluationViewModelsModule))]
[Singleton(typeof(IBrowserDialog), typeof(BrowserDialog))]
[Singleton(typeof(IThreadWrapper), typeof(ThreadWrapper))]
[Singleton(typeof(ILogger), Factory = nameof(LoggerFactory))]
[Singleton(typeof(IDialog), Factory = nameof(DialogFactory))]
[Singleton(typeof(IMessenger), Factory = nameof(MessengerFactory))]
[Singleton(typeof(IGreetingsHelper), typeof(GreetingsHelper))]
[Singleton(typeof(IThemeHelper), Factory = nameof(ThemeHelperFactory))]
[Singleton(typeof(IClientService), Factory = nameof(ClientServiceFactory))]
internal sealed partial class ServiceProvider
{
    private readonly string _configFile;
    private readonly string _logPath;

    public ServiceProvider()
    {
        var title = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        var userPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.leeve");
        var configPath = Path.Combine(userPath, "configs");

        var appTitle = string.IsNullOrEmpty(title) ? "leeve" : title.ToLower().Replace(" ", "-");
        _configFile = Path.Combine(configPath, $"{appTitle}.json");
        _logPath = Path.Combine(userPath, "logs");

        Directory.CreateDirectory(configPath);
    }

    public ILogger LoggerFactory() =>
        new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(_logPath, "log.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();
    
    public IMessenger MessengerFactory() => WeakReferenceMessenger.Default;

    public IThemeHelper ThemeHelperFactory()
    {
        const string lightTheme = "pack://application:,,,/Themes/MaterialTheme.Light.xaml";
        const string darkTheme = "pack://application:,,,/Themes/MaterialTheme.Dark.xaml";
        return new ThemeHelper(_configFile, lightTheme, darkTheme);
    }

    private IClientService ClientServiceFactory()
    {
        var manager = GetService<ChannelManager>();
        var teacherNotificationService = GetService<ITeacherNotificationService>();
        var teacherImageNotificationService = GetService<ITeacherImageNotificationService>();
        var questionnaireNotificationService = GetService<IQuestionnaireNotificationService>();
        var evaluationNotificationService = GetService<IEvaluationNotificationService>();
        var evaluationProcessNotificationService = GetService<IEvaluationProcessNotificationService>();
        var evaluationSubmitNotificationService = GetService<IEvaluationSubmitNotificationService>();

        var clientService = new ClientServiceBase(manager, _configFile)
            .Register(() => teacherNotificationService.Subscribe())
            .Register(() => teacherImageNotificationService.Subscribe())
            .Register(() => questionnaireNotificationService.Subscribe())
            .Register(() => evaluationNotificationService.Subscribe())
            .Register(() => evaluationProcessNotificationService.Subscribe())
            .Register(() => evaluationSubmitNotificationService.Subscribe());

        return clientService;
    }
}