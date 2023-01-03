using Jab;
using Leeve.Application.Users;

namespace Leeve.Wpf.Common.JabModules;

[ServiceProviderModule]
[Transient(typeof(IAdminViewModel), typeof(AdminViewModel))]
[Transient(typeof(IAdminCredentialsViewModel), typeof(AdminCredentialsViewModel))]
[Transient(typeof(ITeacherListViewModel), typeof(TeacherListViewModel))]
public interface IAdminViewModelsModule
{
}