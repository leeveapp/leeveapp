using Leeve.Application.Common;
using Leeve.Application.Messages;
using Leeve.Client.Users;
using Leeve.Core.Common;
using Leeve.Core.Users;

namespace Leeve.Application.Users;

public interface ITeacherListViewModel : IInitializable
{
}

public partial class TeacherListViewModel : NotifiableObject, ITeacherListViewModel
{
    private readonly IDialog _dialog;
    private readonly IMessenger _messenger;
    private readonly ITeacherClientService _clientService;
    private readonly IAddTeacherViewModel _addTeacherViewModel;
    private readonly IEditTeacherViewModel _editTeacherViewModel;

    public TeacherListViewModel(IDialog dialog,
        IMessenger messenger,
        ITeacherClientService clientService,
        IAddTeacherViewModel addTeacherViewModel,
        IEditTeacherViewModel editTeacherViewModel)
    {
        _dialog = dialog;
        _messenger = messenger;
        _clientService = clientService;
        _addTeacherViewModel = addTeacherViewModel;
        _editTeacherViewModel = editTeacherViewModel;

        Teachers = new ObservableCollection<Teacher>();
        Teachers.CollectionChanged += (_, _) => NotifyPropertyChanged(nameof(Teachers));
        PropertyChanged += OnPropertyChanged;

        RegisterMessages();
    }

    private void RegisterMessages()
    {
        async void UpdateList(object _, TeacherModifiedMessage m)
        {
            if (m.Teacher == null) return;
            if (m.Action == Actions.Add) await AddEntityAsync(m.Teacher);
            if (m.Action == Actions.Update)
            {
                var entity = Teachers.FirstOrDefault(x => x.Id == m.Teacher.Id);
                if (entity == null) return;

                Teachers.Remove(entity);
                await AddEntityAsync(m.Teacher);
            }
        }
        _messenger.Register<TeacherModifiedMessage>(this, UpdateList);
    }

    private async Task AddEntityAsync(Teacher entity)
    {
        if (!_searching || IncludedInSearch(entity))
            await InsertItemAsync(entity);
    }

    private bool IncludedInSearch(Teacher entity)
    {
        if (!_searching) return false;

        var search = SearchString.ToLowerInvariant();
        return entity.FullName.ToLowerInvariant().Contains(search);
    }

    private async Task InsertItemAsync(Teacher entity)
    {
        var sorter = new ListItemSorter<Teacher>(Teachers, entity);
        await sorter.OrderBy(i => i.FullName).SortAsync();
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
        var result = _clientService.Search(Teachers, SearchString);
        if (result.IsFaulted) _messenger.Send(new ActionFailedMessage(result.ToString()));
        else _searching = true;
    }

    public ObservableCollection<Teacher> Teachers { get; }

    [RelayCommand]
    private void Back()
    {
        _messenger.Send(new UserLoggedOutMessage());
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        _addTeacherViewModel.Initialize();
        await _dialog.ShowAsync(_addTeacherViewModel);
    }

    [RelayCommand]
    private async Task EditAsync(Teacher entity)
    {
        _editTeacherViewModel.Initialize(entity);
        await _dialog.ShowAsync(_editTeacherViewModel);
    }

    [RelayCommand]
    private async Task DeleteAsync(Teacher entity)
    {
        var dialogResult = await _dialog.ShowAsync("Are you sure you want to delete this teacher?", "Delete Teacher",
            button: DialogButton.AffirmativeAndNegative);
        if (dialogResult != DialogResult.Yes) return;

        var result = await _clientService.DeleteAsync(entity.Id);

        if (result.IsSuccess) Teachers.Remove(entity);
        else _messenger.Send(new ActionFailedMessage(result.ToString()));
    }

    private bool _initializing;
    private bool _searching;

    public async Task<bool> InitializeAsync()
    {
        _searching = false;
        _initializing = true;

        SearchString = string.Empty;

        var result = await _clientService.GetAllAsync(Teachers);
        if (result.IsFaulted)
        {
            _messenger.Send(new ActionFailedMessage(result.ToString()));
            return false;
        }

        _initializing = false;
        return true;
    }
}