using Leeve.Application.Common;

namespace Leeve.Application.Questionnaires;

public interface IQuestionViewModel
{
}

public sealed partial class QuestionViewModel : NotifiableObject, IQuestionViewModel
{
    public QuestionViewModel(IRelayCommand deleteQuestionCommand)
    {
        DeleteQuestionCommand = deleteQuestionCommand;
        Answers = new ObservableCollection<AnswerViewModel>();
        Answers.CollectionChanged += (_, _) => { HasItems = Answers.Any(); };
    }

    public QuestionViewModel()
    {
        Answers = new ObservableCollection<AnswerViewModel>();
    }

    [ObservableProperty]
    private bool _hasItems;

    [ObservableProperty]
    private string _title = string.Empty;

    public ObservableCollection<AnswerViewModel> Answers { get; }

    [RelayCommand]
    private void Add()
    {
        Answers.Add(new AnswerViewModel(DeleteCommand));
    }

    [RelayCommand]
    private void Delete(AnswerViewModel viewModel)
    {
        var index = Answers.IndexOf(viewModel);
        Answers.RemoveAt(index);
    }

    public IRelayCommand? DeleteQuestionCommand { get; }

    public void Initialize()
    {
        Answers.Add(new AnswerViewModel(DeleteCommand));
        Answers.Add(new AnswerViewModel(DeleteCommand));
    }
}