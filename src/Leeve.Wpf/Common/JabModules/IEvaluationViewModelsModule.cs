using Jab;
using Leeve.Application.Evaluations;
using Leeve.Application.Questionnaires;

namespace Leeve.Wpf.Common.JabModules;

[ServiceProviderModule]
[Transient(typeof(IEvaluationCodeViewModel), typeof(EvaluationCodeViewModel))]
[Transient(typeof(IEvaluationViewModel), typeof(EvaluationViewModel))]
[Transient(typeof(IAddEvaluationViewModel), typeof(AddEvaluationViewModel))]
public interface IEvaluationViewModelsModule
{
}