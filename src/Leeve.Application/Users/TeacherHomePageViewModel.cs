using Leeve.Application.Common;
using Leeve.Application.Messages;
using Leeve.Client.Evaluations;
using Leeve.Core.Evaluations;

namespace Leeve.Application.Users;

public interface IHomePageViewModel : IInitializable
{
}

public sealed partial class TeacherHomePageViewModel : NotifiableObject, IHomePageViewModel
{
    private readonly IMessenger _messenger;
    private readonly IEvaluationClientService _clientService;

    public TeacherHomePageViewModel(IMessenger messenger,
        IGreetingsHelper greeter,
        IEvaluationClientService clientService)
    {
        _messenger = messenger;
        _clientService = clientService;
        Evaluations = new ObservableCollection<Evaluation>();
        Evaluations.CollectionChanged += (_, _) => NotifyPropertyChanged(nameof(Evaluations));

        greeter.GreetingsChanged += (_, s) => Greetings = $"{s},";
        greeter.StartAsync();// since this should not be awaited, it is started in the constructor

        RegisterMessages();
    }

    private void RegisterMessages()
    {
        _messenger.Register<EvaluationProcessStartedMessage>(this, (_, m) => Evaluations.Add(m.Evaluation));
        _messenger.Register<EvaluationProcessStoppedMessage>(this, (_, m) => Evaluations.Remove(m.Evaluation));
        _messenger.Register<EvaluationSubmittedMessage>(this, (_, m) => UpdateResponse(m.Evaluation));
    }

    private void UpdateResponse(Evaluation entity)
    {
        var index = Evaluations.IndexOf(entity);
        if (index < 0) return;

        Evaluations.Remove(entity);
        Evaluations.Insert(index, entity);
    }

    [ObservableProperty]
    private string _greetings = string.Empty;

    public ObservableCollection<Evaluation> Evaluations { get; }

    [RelayCommand]
    private async Task StopAsync(Evaluation entity)
    {
        var result = await _clientService.StopEvaluationAsync(entity.Id);
        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
        }
        else
        {
            Evaluations.Remove(entity);
            _messenger.Send(new NotificationMessage("Evaluation stopped"));
        }
    }

    public async Task<bool> InitializeAsync()
    {
        Evaluations.Clear();

        var result = await _clientService.GetAllActiveAsync(Evaluations);
        if (result.IsSuccess) return true;

        _messenger.Send(new ActionFailedMessage(result.ToString()));
        return false;
    }
}