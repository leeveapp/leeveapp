using Jab;
using Leeve.Application.Evaluations;
using Leeve.Application.Questionnaires;
using Leeve.Application.Users;

namespace Leeve.Wpf.Common.JabModules;

[ServiceProviderModule]
[Transient(typeof(IMainPageViewModel), typeof(TeacherMainPageViewModel))]
[Transient(typeof(IHomePageViewModel), typeof(TeacherHomePageViewModel))]
[Transient(typeof(IQuestionnairesSelectionViewModel), typeof(QuestionnairesSelectionViewModel))]
[Transient(typeof(IAddQuestionnaireViewModel), typeof(AddQuestionnaireViewModel))]
[Transient(typeof(IEditQuestionnaireViewModel), typeof(EditQuestionnaireViewModel))]
[Transient(typeof(IAddTeacherViewModel), typeof(AddTeacherViewModel))]
[Transient(typeof(IEditTeacherViewModel), typeof(EditTeacherViewModel))]
[Transient(typeof(ITeacherCredentialsViewModel), typeof(TeacherCredentialsViewModel))]
[Transient(typeof(IEvaluationSelectionViewModel), typeof(EvaluationSelectionViewModel))]
public interface ITeacherViewModelsModule
{
}