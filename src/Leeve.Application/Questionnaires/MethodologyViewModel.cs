using System.Collections.Specialized;

namespace Leeve.Application.Questionnaires;

public interface IMethodologyViewModel
{
}

public sealed partial class MethodologyViewModel : ObservableObject, IMethodologyViewModel
{
    public MethodologyViewModel(IRelayCommand addMethodologyCommand,
        IRelayCommand deleteMethodologyCommand)
    {
        AddMethodologyCommand = addMethodologyCommand;
        DeleteMethodologyCommand = deleteMethodologyCommand;
        Questions = new ObservableCollection<QuestionViewModel>();
        Questions.CollectionChanged += QuestionsOnCollectionChanged;
    }

    public MethodologyViewModel()
    {
        Questions = new ObservableCollection<QuestionViewModel>();
        Questions.CollectionChanged += QuestionsOnCollectionChanged;
    }

    private void QuestionsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DeleteCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    public ObservableCollection<QuestionViewModel> Questions { get; }

    [RelayCommand]
    private void Add(QuestionViewModel viewModel)
    {
        Initialize();
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete(QuestionViewModel viewModel)
    {
        var index = Questions.IndexOf(viewModel);
        Questions.RemoveAt(index);
    }

    private bool CanDelete() => Questions.Count > 1;

    public IRelayCommand? AddMethodologyCommand { get; }
    public IRelayCommand? DeleteMethodologyCommand { get; }

    public void Initialize()
    {
        var question = new QuestionViewModel(DeleteCommand);
        question.Initialize();
        Questions.Add(question);
    }
}