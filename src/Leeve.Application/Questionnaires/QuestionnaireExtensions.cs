using Leeve.Core.Questionnaires;

namespace Leeve.Application.Questionnaires;

public static class QuestionnaireExtensions
{
    public static Questionnaire ToEntity(this QuestionnaireViewModel viewModel)
    {
        var result = new Questionnaire
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            Methodologies = viewModel.Methodologies.Select(e => e.ToEntity())
        };

        return result;
    }

    private static Answer ToEntity(this AnswerViewModel viewModel)
    {
        float.TryParse((string?) viewModel.Weight, out var weight);

        var result = new Answer
        {
            Title = viewModel.Title,
            Weight = weight,
            Selected = viewModel.Selected
        };

        return result;
    }

    private static Question ToEntity(this QuestionViewModel viewModel)
    {
        var result = new Question
        {
            Title = viewModel.Title,
            Answers = viewModel.Answers.Select(e => e.ToEntity())
        };

        return result;
    }

    public static Methodology ToEntity(this MethodologyViewModel viewModel)
    {
        var result = new Methodology
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            Questions = viewModel.Questions.Select(e => e.ToEntity())
        };

        return result;
    }

    private static AnswerViewModel ToViewModel(this Answer entity, string id)
    {
        var result = new AnswerViewModel(id)
        {
            Title = entity.Title,
            Weight = $"{entity.Weight}",
            Selected = entity.Selected
        };

        return result;
    }

    private static QuestionViewModel ToViewModel(this Question entity)
    {
        var id = Guid.NewGuid().ToString();
        var answers = new ObservableCollection<AnswerViewModel>(entity.Answers.Select(e => e.ToViewModel(id)));
        var result = new QuestionViewModel { Title = entity.Title };

        var randomArrays = answers.Count.GenerateArrayOfNumbers();
        foreach (var randomArray in randomArrays) result.Answers.Add(answers[randomArray]);

        return result;
    }

    public static MethodologyViewModel ToViewModel(this Methodology entity)
    {
        var result = new MethodologyViewModel
        {
            Title = entity.Title,
            Description = entity.Description
        };

        foreach (var question in entity.Questions) result.Questions.Add(question.ToViewModel());
        return result;
    }

    public static MethodologyViewModel ToViewModel(this Methodology entity, IRelayCommand addMethodologyCommand,
        IRelayCommand deleteMethodologyCommand)
    {
        var result = new MethodologyViewModel(addMethodologyCommand, deleteMethodologyCommand)
        {
            Title = entity.Title,
            Description = entity.Description
        };

        foreach (var question in entity.Questions) result.Questions.Add(question.ToViewModel(result.DeleteCommand));

        return result;
    }

    private static QuestionViewModel ToViewModel(this Question entity, IRelayCommand deleteQuestionCommand)
    {
        var result = new QuestionViewModel(deleteQuestionCommand)
        {
            Title = entity.Title
        };

        foreach (var answer in entity.Answers)
            result.Answers.Add(ToViewModel(answer, result.DeleteCommand));

        return result;
    }

    private static AnswerViewModel ToViewModel(this Answer entity, IRelayCommand deleteAnswerCommand)
    {
        var result = new AnswerViewModel(deleteAnswerCommand)
        {
            Title = entity.Title,
            Weight = $"{entity.Weight}",
            Selected = entity.Selected
        };

        return result;
    }
}