using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Leeve.Application.Main;
using Leeve.Application.Messages;
using MaterialDesignThemes.Wpf;

namespace Leeve.Wpf.Main;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainViewModel _viewModel;
    private readonly IMessenger _messenger;
    private SnackbarMessageQueue? _messageQueue;
    private SnackbarMessageQueue? _errorQueue;

    public MainWindow(MainViewModel viewModel, IMessenger messenger)
    {
        _viewModel = viewModel;
        _messenger = messenger;
        DataContext = viewModel;

        InitializeComponent();
        InitializeSnackBars();
        SubscribeToEvents();

        Loaded += OnLoaded;
    }

    private void InitializeSnackBars()
    {
        _messageQueue = new SnackbarMessageQueue { DiscardDuplicates = true };
        _errorQueue = new SnackbarMessageQueue { DiscardDuplicates = true };
        SnackBar.MessageQueue = _messageQueue;
        ErrorSnackBar.MessageQueue = _errorQueue;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Initialize();
    }

    private void SubscribeToEvents()
    {
        _messenger.Register<ActionFailedMessage>(this, (_, m) =>
        {
            _messageQueue?.Clear();
            _errorQueue?.Enqueue(m.Value, "×", () => { });
        });
        _messenger.Register<NotificationMessage>(this, (_, m) =>
        {
            _errorQueue?.Clear();
            _messageQueue?.Enqueue(m.Value, "×", () => { });
        });
    }
}