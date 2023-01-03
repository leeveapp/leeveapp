namespace Leeve.Application.Questionnaires;

public interface IAnswerViewModel
{
}

public sealed partial class AnswerViewModel : ObservableObject, IAnswerViewModel
{
    private readonly IRelayCommand? _deleteAnswerCommand;

    public AnswerViewModel(IRelayCommand deleteAnswerCommand)
    {
        _deleteAnswerCommand = deleteAnswerCommand;
    }

    public AnswerViewModel(string id)
    {
        Group = id;
    }

    [ObservableProperty]
    private string _group = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _weight = string.Empty;

    [ObservableProperty]
    private bool _selected;

    [RelayCommand]
    private void DeleteAnswer()
    {
        _deleteAnswerCommand?.Execute(this);
    }
}