using Leeve.Application.Common;
using Leeve.Application.Evaluations;
using Leeve.Application.Messages;
using Leeve.Application.Questionnaires;

namespace Leeve.Application.Users;

public interface IMainPageViewModel
{
    void Initialize();
    bool DarkTheme { get; }
    IRelayCommand SwitchThemeCommand { get; }
    IRelayCommand BackCommand { get; }
}

public sealed partial class TeacherMainPageViewModel : ObservableObject, IMainPageViewModel
{
    private readonly IMessenger _messenger;
    private readonly IHomePageViewModel _homePage;
    private readonly IQuestionnairesSelectionViewModel _questionnairesSelection;
    private readonly IEvaluationSelectionViewModel _evaluationSelection;

    private readonly List<PageItem> _pages = new();

    public TeacherMainPageViewModel(IMessenger messenger,
        IHomePageViewModel homePage,
        IQuestionnairesSelectionViewModel questionnairesSelection,
        IEvaluationSelectionViewModel evaluationSelection)
    {
        _messenger = messenger;
        _homePage = homePage;
        _questionnairesSelection = questionnairesSelection;
        _evaluationSelection = evaluationSelection;

        _pageSwitch = new Dictionary<int, Action>
        {
            { PageGroup.Home, OpenHomePage },
            { PageGroup.Questionnaires, OpenQuestionnairesSelectionPage },
            { PageGroup.Evaluation, OpenEvaluationSelectionPage }
        };

        RegisterMessages();
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        BackCommand.NotifyCanExecuteChanged();
    }

    private readonly Dictionary<int, Action> _pageSwitch;

    private void RegisterMessages()
    {
        _messenger.Register<ThemeAppliedMessage>(this, (_, m) => DarkTheme = m.Value);
        _messenger.Register<AddNewPageMessage>(this, (_, m) => OnAddPage(m));
        _messenger.Register<RemoveLastPageMessage>(this, (_, _) => OnRemovePage());
        _messenger.Register<ReplaceLastPageMessage>(this, (_, m) => OnReplacePage(m));
    }

    private async void OnAddPage(AddNewPageMessage m)
    {
        var lastPageItem = _pages.LastOrDefault();
        if (lastPageItem?.Page == m.Value.Page) return;

        await InitializePageAsync(m.Value);
        await InitializeEditPageAsync(m.Value);

        // remove last page if IInitializableForEdit
        if (lastPageItem?.Page is IInitializableForEdit)
            _pages.Remove(lastPageItem);

        // remove page if it is already in the list
        var pageToRemove = _pages.FirstOrDefault(p => p.Page == m.Value.Page);
        if (pageToRemove != null) _pages.Remove(pageToRemove);

        _pages.Add(m.Value);

        LoadCurrentPage();
    }

    private void LoadCurrentPage()
    {
        if (_pages.Count == 0) return;

        var pageItem = _pages.Last();

        Page = pageItem.Page;
        PageIndex = pageItem.Group;
    }

    private async void OnRemovePage()
    {
        if (_pages.Count <= 1) return;

        _pages.RemoveAt(_pages.Count - 1);
        var pageItem = _pages.Last();

        LoadCurrentPage();

        await InitializePageAsync(pageItem);
        await InitializeEditPageAsync(pageItem);

        if (_pages.Count == 1 && Page != _homePage)
        {
            _pages.Insert(0, new PageItem(_homePage, PageGroup.Home));
            BackCommand.NotifyCanExecuteChanged();
        }
    }

    private async void OnReplacePage(ReplaceLastPageMessage m)
    {
        if (_pages.Count == 0) return;

        _pages.RemoveAt(_pages.Count - 1);

        await InitializePageAsync(m.Value);
        await InitializeEditPageAsync(m.Value);

        // remove page if it is already in the list
        var pageToRemove = _pages.FirstOrDefault(p => p.Page == m.Value.Page);
        if (pageToRemove != null) _pages.Remove(pageToRemove);

        _pages.Add(m.Value);
        LoadCurrentPage();
    }

    private static async Task InitializePageAsync(PageItem pageItem)
    {
        if (pageItem.Page is IInitializable initializable) await initializable.InitializeAsync();
    }

    private async Task InitializeEditPageAsync(PageItem pageItem)
    {
        if (pageItem.Page is not IInitializableForEdit initializable) return;

        if (pageItem.Entity == null)
            _messenger.Send(new ActionFailedMessage("Cannot initialize page"));
        else
            await initializable.InitializeAsync(pageItem.Entity);
    }

    [ObservableProperty]
    private object? _page;

    [ObservableProperty]
    private int _pageIndex;

    [ObservableProperty]
    private bool _darkTheme;

    [RelayCommand]
    private void SwitchTheme()
    {
        _messenger.Send(new ThemeSwitchedMessage());
    }

    private void OpenHomePage()
    {
        var pageItem = new PageItem(_homePage, PageGroup.Home);
        _messenger.Send(new AddNewPageMessage(pageItem));
    }

    private void OpenQuestionnairesSelectionPage()
    {
        var pageItem = new PageItem(_questionnairesSelection, PageGroup.Questionnaires);
        _messenger.Send(new AddNewPageMessage(pageItem));
    }

    private void OpenEvaluationSelectionPage()
    {
        var pageItem = new PageItem(_evaluationSelection, PageGroup.Evaluation);
        _messenger.Send(new AddNewPageMessage(pageItem));
    }

    [RelayCommand]
    private void SwitchPage(int index)
    {
        _pageSwitch[index].Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanBack))]
    private void Back()
    {
        OnRemovePage();
    }

    private bool CanBack() => _pages.Count > 1;

    public void Initialize()
    {
        _pages.Clear();
        PageIndex = PageGroup.Home;

        var pageItem = new PageItem(_homePage, PageGroup.Home);
        _messenger.Send(new AddNewPageMessage(pageItem));
    }
}

public class PageItem
{
    public PageItem(object page, PageGroup group, object? entity = default)
    {
        Page = page;
        Group = group;
        Entity = entity;
    }

    public object? Entity { get; }
    public object Page { get; }
    public PageGroup Group { get; }
}

public struct PageGroup
{
    private readonly int _val;

    private PageGroup(int val)
    {
        _val = val;
    }

    public static implicit operator PageGroup(int value) => new(value);

    public static implicit operator int(PageGroup value) => value._val;

    public static int Home => 0;
    public static int Questionnaires => 1;
    public static int Evaluation => 2;
}