using Jab;
using Leeve.Client.Evaluations;
using Leeve.Client.Questionnaires;
using Leeve.Client.Users;

namespace Leeve.Wpf.Common.JabModules;

[ServiceProviderModule]
[Singleton(typeof(ITeacherNotificationService), typeof(TeacherNotificationService))]
[Singleton(typeof(ITeacherImageNotificationService), typeof(TeacherImageNotificationService))]
[Singleton(typeof(IQuestionnaireNotificationService), typeof(QuestionnaireNotificationService))]
[Singleton(typeof(IEvaluationNotificationService), typeof(EvaluationNotificationService))]
[Singleton(typeof(IEvaluationProcessNotificationService), typeof(EvaluationProcessNotificationService))]
[Singleton(typeof(IEvaluationSubmitNotificationService), typeof(EvaluationSubmitNotificationService))]
public interface INotificationServicesModule
{
}