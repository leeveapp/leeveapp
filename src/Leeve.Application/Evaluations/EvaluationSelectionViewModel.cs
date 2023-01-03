using Leeve.Application.Common;
using Leeve.Application.Messages;
using Leeve.Client.Evaluations;
using Leeve.Core.Common;
using Leeve.Core.Evaluations;

namespace Leeve.Application.Evaluations;

public interface IEvaluationSelectionViewModel : IInitializable, ICancellable
{
}

public sealed partial class EvaluationSelectionViewModel : NotifiableObject, IEvaluationSelectionViewModel
{
    private readonly IDialog _dialog;
    private readonly IMessenger _messenger;
    private readonly IBrowserDialog _browserDialog;
    private readonly IEvaluationClientService _clientService;
    private CancellationTokenSource _tokenSource;

    public EvaluationSelectionViewModel(IDialog dialog,
        IMessenger messenger,
        IBrowserDialog browserDialog,
        IEvaluationClientService clientService)
    {
        _dialog = dialog;
        _messenger = messenger;
        _browserDialog = browserDialog;
        _clientService = clientService;
        _tokenSource = new CancellationTokenSource();

        Evaluations = new ObservableCollection<Evaluation>();
        Evaluations.CollectionChanged += (_, _) => NotifyPropertyChanged(nameof(Evaluations));
        PropertyChanged += OnPropertyChanged;

        RegisterMessages();
    }

    private void RegisterMessages()
    {
        async void UpdateList(object _, EvaluationModifiedMessage m)
        {
            if (m.Evaluation == null) return;
            if (m.Action == Actions.Add) await AddEntityAsync(m.Evaluation);
            if (m.Action == Actions.Delete)
            {
                var entity = Evaluations.FirstOrDefault(e => e.Id == m.Evaluation.Id);
                if (entity != null) Evaluations.Remove(entity);
            }
        }
        _messenger.Register<EvaluationModifiedMessage>(this, UpdateList);
        _messenger.Register<EvaluationProcessStartedMessage>(this, (_, m) => UpdateEvaluation(m.Evaluation));
        _messenger.Register<EvaluationProcessStoppedMessage>(this, (_, m) => UpdateEvaluation(m.Evaluation));
        _messenger.Register<EvaluationSubmittedMessage>(this, (_, m) => UpdateEvaluation(m.Evaluation));
    }

    private void UpdateEvaluation(Evaluation entity)
    {
        var index = Evaluations.IndexOf(entity);
        if (index < 0) return;

        Evaluations.Remove(entity);
        Evaluations.Insert(index, entity);
    }

    private async Task AddEntityAsync(Evaluation entity)
    {
        if (!_searching || IncludedInSearch(entity))
            await InsertItemAsync(entity);
    }

    private bool IncludedInSearch(Evaluation entity)
    {
        if (!_searching) return false;

        var search = SearchString.ToLowerInvariant();
        return entity.Title.ToLowerInvariant().Contains(search);
    }

    private async Task InsertItemAsync(Evaluation entity)
    {
        var sorter = new ListItemSorter<Evaluation>(Evaluations, entity);
        await sorter.OrderByDescending(i => i.CreatedOn).SortAsync();
    }

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_initializing) return;

        if (e.PropertyName == nameof(SearchString))
        {
            SearchCommand.NotifyCanExecuteChanged();
            await TryReloadItemsAsync();
        }
    }

    private async Task TryReloadItemsAsync()
    {
        if (!string.IsNullOrEmpty(SearchString) || !_searching) return;

        await _dialog.LoadAsync("inner-loading", InitializeAsync);
    }

    [ObservableProperty]
    private string _searchString = string.Empty;

    private bool CanSearch() => !string.IsNullOrWhiteSpace(SearchString);

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private void Search()
    {
        var result = _clientService.Search(Evaluations, SearchString);
        if (result.IsFaulted) _messenger.Send(new ActionFailedMessage(result.ToString()));
        else _searching = true;
    }

    public ObservableCollection<Evaluation> Evaluations { get; }

    [RelayCommand]
    private async Task GenerateReportAsync(Evaluation entity)
    {
        var save = _browserDialog.SaveFile("Generate Excel Report", "Excel Files (*.xlsx)|*.xlsx", "evaluation-report");
        if (!save) return;

        var result = await _clientService.GenerateReportAsync(entity.Id, _browserDialog.Path, _tokenSource.Token);
        if (result.IsFaulted) _messenger.Send(new ActionFailedMessage(result.ToString()));
    }

    [RelayCommand]
    private async Task StartAsync(Evaluation entity)
    {
        var result = await _clientService.StartEvaluationAsync(entity.Id);
        if (result.IsFaulted)
            _messenger.Send(new ActionFailedMessage(result.ToString()));
    }

    [RelayCommand]
    private async Task StopAsync(Evaluation entity)
    {
        var result = await _clientService.StopEvaluationAsync(entity.Id);
        if (result.IsFaulted)
            _messenger.Send(new ActionFailedMessage(result.ToString()));
    }

    [RelayCommand]
    private async Task DeleteAsync(Evaluation entity)
    {
        if (entity.IsActive)
        {
            _messenger.Send(new ActionFailedMessage("Cannot delete an active evaluation"));
            return;
        }

        var dialogResult = await _dialog.ShowAsync("Are you sure you want to delete this evaluation?", "Delete Evaluation",
            button: DialogButton.AffirmativeAndNegative);
        if (dialogResult != DialogResult.Yes) return;

        var result = await _clientService.DeleteAsync(entity.Id);

        if (result.IsSuccess) Evaluations.Remove(entity);
        else _messenger.Send(new ActionFailedMessage(result.ToString()));
    }

    private CancellationToken InitializeToken()
    {
        _tokenSource.Cancel();

        _tokenSource = new CancellationTokenSource();
        return _tokenSource.Token;
    }

    private bool _initializing;
    private bool _searching;

    public async Task<bool> InitializeAsync()
    {
        _searching = false;
        _initializing = true;

        SearchString = string.Empty;

        var token = InitializeToken();
        var result = await _clientService.GetAllAsync(Evaluations, token);

        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
            return false;
        }

        _initializing = false;
        return true;
    }

    public void Cancel()
    {
        _tokenSource.Cancel();
    }
}