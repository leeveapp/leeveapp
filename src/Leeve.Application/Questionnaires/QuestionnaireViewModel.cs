using System.Collections.Specialized;
using Leeve.Application.Messages;

namespace Leeve.Application.Questionnaires;

public abstract partial class QuestionnaireViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    protected QuestionnaireViewModel(IMessenger messenger)
    {
        _messenger = messenger;
        Methodologies = new ObservableCollection<MethodologyViewModel>();
        Methodologies.CollectionChanged += MethodologiesOnCollectionChanged;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void MethodologiesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DeleteCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    public ObservableCollection<MethodologyViewModel> Methodologies { get; }

    [RelayCommand]
    private void Add(MethodologyViewModel viewModel)
    {
        var index = Methodologies.IndexOf(viewModel);
        var methodology = new MethodologyViewModel(AddCommand, DeleteCommand);
        methodology.Initialize();
        Methodologies.Insert(index + 1, methodology);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete(MethodologyViewModel viewModel)
    {
        var index = Methodologies.IndexOf(viewModel);
        Methodologies.RemoveAt(index);
    }

    private bool CanDelete() => Methodologies.Count > 1;

    [RelayCommand(CanExecute = nameof(CanSave))]
    protected abstract Task SaveAsync();

    private bool CanSave()
    {
        if (string.IsNullOrWhiteSpace(Title)) return false;
        if (string.IsNullOrWhiteSpace(Description)) return false;

        return true;
    }

    [RelayCommand]
    private void Cancel() => _messenger.Send(new RemoveLastPageMessage());
}