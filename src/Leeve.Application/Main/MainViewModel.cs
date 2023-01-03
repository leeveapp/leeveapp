using Leeve.Application.Evaluations;
using Leeve.Application.Messages;
using Leeve.Application.Users;
using Leeve.Client.Evaluations;
using Leeve.Client.Questionnaires;
using Leeve.Client.Users;
using Leeve.Core.Evaluations;

namespace Leeve.Application.Main;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IMessenger _messenger;
    private readonly IDialog _dialog;
    private readonly IAdminViewModel _adminViewModel;
    private readonly ILoginPageViewModel _loginPageViewModel;
    private readonly ITeacherListViewModel _teacherListViewModel;
    private readonly UserSelectionViewModel _userSelectionViewModel;
    private readonly IEvaluationViewModel _evaluationViewModel;

    public MainViewModel(IMessenger messenger,
        IDialog dialog,
        IAdminViewModel adminViewModel,
        IMainPageViewModel mainPage,
        ILoginPageViewModel loginPageViewModel,
        ITeacherListViewModel teacherListViewModel,
        ITeacherInfoViewModel teacherInfo,
        UserSelectionViewModel userSelectionViewModel,
        IEvaluationViewModel evaluationViewModel)
    {
        _messenger = messenger;
        _dialog = dialog;
        _adminViewModel = adminViewModel;
        _loginPageViewModel = loginPageViewModel;
        _teacherListViewModel = teacherListViewModel;
        _userSelectionViewModel = userSelectionViewModel;
        _evaluationViewModel = evaluationViewModel;

        MainPage = mainPage;
        TeacherInfo = teacherInfo;

        RegisterMessages();
    }

    private void RegisterMessages()
    {
        _messenger.Register<UserSelectionMessage>(this, (_, _) => Initialize());
        _messenger.Register<TeacherSelectionMessage>(this, (_, _) => SetLoginPage());
        _messenger.Register<AdminLoggedInMessage>(this, (_, _) => SetTeacherListPage());
        _messenger.Register<TeacherLoggedInMessage>(this, (_, _) => SetMainPage());
        _messenger.Register<UserLoggedOutMessage>(this, (_, _) => Initialize());
        _messenger.Register<EvaluationRequestedMessage>(this, (_, m) => SetEvaluationPage(m.Evaluation));
    }

    [ObservableProperty]
    private object? _page;

    [ObservableProperty]
    private bool _showAdminSettings = true;

    [ObservableProperty]
    private bool _showUserInfo = true;

    [ObservableProperty]
    private bool _showBack = true;

    public IMainPageViewModel MainPage { get; }
    public ITeacherInfoViewModel TeacherInfo { get; }

    [RelayCommand]
    private async void OpenAdminSettingsSelection()
    {
        await _dialog.ShowAsync(_adminViewModel);
        _adminViewModel.Initialize();
    }

    private async void SetTeacherListPage()
    {
        TeacherServiceHub.Clear();
        ShowAdminSettings = false;
        ShowUserInfo = false;
        ShowBack = false;

        await _teacherListViewModel.InitializeAsync();
        Page = _teacherListViewModel;
    }

    private void SetLoginPage()
    {
        ShowAdminSettings = false;
        ShowUserInfo = false;

        Page = _loginPageViewModel;
        _loginPageViewModel.Initialize();
    }

    private void SetMainPage()
    {
        EvaluationServiceHub.Clear();
        QuestionnaireServiceHub.Clear();

        ShowUserInfo = true;
        ShowBack = true;

        Page = MainPage;
        MainPage.Initialize();
    }

    private void SetEvaluationPage(Evaluation entity)
    {
        ShowAdminSettings = false;
        ShowUserInfo = false;
        ShowBack = false;

        _evaluationViewModel.Initialize(entity);
        Page = _evaluationViewModel;
    }

    public void Initialize()
    {
        ShowAdminSettings = true;
        ShowUserInfo = false;
        ShowBack = false;

        Page = _userSelectionViewModel;
    }
}