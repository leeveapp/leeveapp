using Leeve.Application.Evaluations;
using Leeve.Application.Questionnaires;
using Leeve.Application.Users;
using Leeve.Wpf.Evaluations;
using Leeve.Wpf.Questionnaires;
using Leeve.Wpf.Users;

namespace Leeve.Wpf.Common.JabModules;

internal sealed partial class ServiceProvider
{
    public IDialog DialogFactory()
    {
        var dialog = new Dialog()
            .Bind<AdminViewModel, AdminView>()
            .Bind<AdminCredentialsViewModel, AdminCredentialsView>()
            .Bind<AddTeacherViewModel, TeacherView>()
            .Bind<EditTeacherViewModel, TeacherView>()
            .Bind<TeacherCredentialsViewModel, TeacherCredentialsView>()
            .Bind<EvaluationCodeViewModel, EvaluationCodeView>()
            .Bind<AddEvaluationViewModel, AddEvaluationView>();

        return dialog;
    }
}