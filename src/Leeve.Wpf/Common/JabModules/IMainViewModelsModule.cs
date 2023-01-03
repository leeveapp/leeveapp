using Jab;
using Leeve.Application.Main;
using Leeve.Application.Users;

namespace Leeve.Wpf.Common.JabModules;

[ServiceProviderModule]
[Transient(typeof(MainViewModel))]
[Transient(typeof(UserSelectionViewModel), typeof(UserSelectionViewModel))]
[Transient(typeof(ILoginPageViewModel), typeof(TeacherLoginPageViewModel))]
[Transient(typeof(ITeacherInfoViewModel), typeof(TeacherInfoViewModel))]
public interface IMainViewModelsModule
{
}