using Jab;
using Leeve.Client.Common;
using Leeve.Client.Evaluations;
using Leeve.Client.Questionnaires;
using Leeve.Client.Users;

namespace Leeve.Wpf.Common.JabModules;

[ServiceProviderModule]
[Singleton(typeof(ChannelManager), typeof(ChannelManager))]
[Singleton(typeof(IAdminClientService), typeof(AdminClientService))]
[Singleton(typeof(ITeacherClientService), typeof(TeacherClientService))]
[Singleton(typeof(IQuestionnaireClientService), typeof(QuestionnaireClientService))]
[Singleton(typeof(IEvaluationClientService), typeof(EvaluationClientService))]
public interface IClientServicesModule
{
}