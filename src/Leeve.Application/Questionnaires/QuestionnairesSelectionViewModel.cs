using Leeve.Application.Common;
using Leeve.Application.Messages;
using Leeve.Application.Users;
using Leeve.Client.Questionnaires;
using Leeve.Core.Common;
using Leeve.Core.Questionnaires;

namespace Leeve.Application.Questionnaires;

public interface IQuestionnairesSelectionViewModel : IInitializable
{
}

public sealed partial class QuestionnairesSelectionViewModel : NotifiableObject, IQuestionnairesSelectionViewModel
{
    private readonly IMessenger _messenger;
    private readonly IDialog _dialog;
    private readonly IAddQuestionnaireViewModel _addQuestionnaireViewModel;
    private readonly IEditQuestionnaireViewModel _editQuestionnaireViewModel;
    private readonly IAddEvaluationViewModel _addEvaluationViewModel;
    private readonly IQuestionnaireClientService _clientService;

    public QuestionnairesSelectionViewModel(IMessenger messenger,
        IDialog dialog,
        IAddQuestionnaireViewModel addQuestionnaireViewModel,
        IEditQuestionnaireViewModel editQuestionnaireViewModel,
        IAddEvaluationViewModel addEvaluationViewModel,
        IQuestionnaireClientService clientService)
    {
        _messenger = messenger;
        _dialog = dialog;
        _addQuestionnaireViewModel = addQuestionnaireViewModel;
        _editQuestionnaireViewModel = editQuestionnaireViewModel;
        _addEvaluationViewModel = addEvaluationViewModel;
        _clientService = clientService;

        Questionnaires = new ObservableCollection<Questionnaire>();
        Questionnaires.CollectionChanged += (_, _) => NotifyPropertyChanged(nameof(Questionnaires));
        PropertyChanged += OnPropertyChanged;

        RegisterMessages();
    }

    private void RegisterMessages()
    {
        async void UpdateList(object _, QuestionnaireModifiedMessage m)
        {
            if (m.Questionnaire == null) return;
            if (m.Action == Actions.Add) await AddEntityAsync(m.Questionnaire);
            if (m.Action == Actions.Update)
            {
                var entity = Questionnaires.FirstOrDefault(x => x.Id == m.Questionnaire.Id);
                if (entity == null) return;

                Questionnaires.Remove(entity);
                await AddEntityAsync(m.Questionnaire);
            }
        }
        _messenger.Register<QuestionnaireModifiedMessage>(this, UpdateList);
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

    private async Task AddEntityAsync(Questionnaire entity)
    {
        if (!_searching || IncludedInSearch(entity))
            await InsertItemAsync(entity);
    }

    private bool IncludedInSearch(Questionnaire entity)
    {
        if (!_searching) return false;

        var search = SearchString.ToLowerInvariant();
        return entity.Title.ToLowerInvariant().Contains(search);
    }

    private async Task InsertItemAsync(Questionnaire entity)
    {
        var sorter = new ListItemSorter<Questionnaire>(Questionnaires, entity);
        await sorter.OrderBy(i => i.Title).SortAsync();
    }

    [ObservableProperty]
    private string _searchString = string.Empty;

    private bool CanSearch() => !string.IsNullOrWhiteSpace(SearchString);

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private void Search()
    {
        var result = _clientService.Search(Questionnaires, SearchString);
        if (result.IsFaulted) _messenger.Send(new ActionFailedMessage(result.ToString()));
        else _searching = true;
    }

    public ObservableCollection<Questionnaire> Questionnaires { get; }

    [RelayCommand]
    private void Add()
    {
        var page = new PageItem(_addQuestionnaireViewModel, PageGroup.Questionnaires);
        _messenger.Send(new AddNewPageMessage(page));
    }

    [RelayCommand]
    private void Edit(Questionnaire entity)
    {
        var page = new PageItem(_editQuestionnaireViewModel, PageGroup.Questionnaires, entity);
        _messenger.Send(new AddNewPageMessage(page));
    }

    [RelayCommand]
    private async void Delete(Questionnaire entity)
    {
        var dialogResult = await _dialog.ShowAsync("Are you sure you want to delete this questionnaire?", "Delete Questionnaire",
            button: DialogButton.AffirmativeAndNegative);
        if (dialogResult != DialogResult.Yes) return;

        var result = await _clientService.DeleteAsync(entity.Id);

        if (result.IsSuccess) Questionnaires.Remove(entity);
        else _messenger.Send(new ActionFailedMessage(result.ToString()));
    }

    [RelayCommand]
    private async Task AddEvaluation(Questionnaire entity)
    {
        _addEvaluationViewModel.Initialize(entity);
        await _dialog.ShowAsync(_addEvaluationViewModel);
    }

    private bool _initializing;
    private bool _searching;

    public async Task<bool> InitializeAsync()
    {
        _searching = false;
        _initializing = true;

        var result = await _clientService.GetAllAsync(Questionnaires);
        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
            return false;
        }

        _initializing = false;
        return true;
    }
}