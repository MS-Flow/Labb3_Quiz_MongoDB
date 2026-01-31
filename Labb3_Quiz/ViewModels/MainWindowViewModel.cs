using Labb3_Quiz.Command;
using Labb3_Quiz.Models;
using Labb3_Quiz.Services;
using Labb3_Quiz.Services.MongoDb;
using System.Collections.ObjectModel;
using System.Windows;

namespace Labb3_Quiz.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;
    private readonly IDialogService _dialogService;

    private QuestionPackViewModel? _activePack;

    private bool _isPlayMode;

    private bool _isFullScreen;

    public MainWindowViewModel()
    {
        _storageService = new MongoStorageService();

        _dialogService = new DialogService();

        Packs = new ObservableCollection<QuestionPackViewModel>();
        Categories = new ObservableCollection<Category>();

        PlayerViewModel = new PlayerViewModel(this, _dialogService);
        ConfigurationViewModel = new ConfigurationViewModel(this, _dialogService);

        ShowConfigCommand = new DelegateCommand(_ => IsPlayMode = false);
        ShowPlayerCommand = new DelegateCommand(_ => IsPlayMode = true, _ => ActivePack != null && ActivePack.Questions.Count > 0);
        ToggleFullScreenCommand = new DelegateCommand(_ => IsFullScreen = !IsFullScreen);

        CreateNewPackCommand = new DelegateCommand(async _ => await CreateNewPackAsync());
        RemoveQuestionPackCommand = new DelegateCommand(async _ => await RemoveQuestionPackAsync(), _ => ActivePack != null);
        SaveAllCommand = new DelegateCommand(async _ => await SaveAllAsync());

        SetActivePackCommand = new DelegateCommand(pack => ActivePack = pack as QuestionPackViewModel, _ => !_isPlayMode);
        ExitProgramCommand = new DelegateCommand(_ => Application.Current.Shutdown());
        ImportQuestionsCommand = new DelegateCommand(_ => ImportQuestions());
        ManageCategoriesCommand = new DelegateCommand(_ => ManageCategories());

        _ = LoadAllAsync();
    }

    public ObservableCollection<QuestionPackViewModel> Packs { get; }
    public ObservableCollection<Category> Categories { get; }

    public QuestionPackViewModel? ActivePack
    {
        get => _activePack;
        set
        {

            if (_isPlayMode && value != _activePack)
                IsPlayMode = false;
            _activePack = value;
            RaisePropertyChanged();

            ShowPlayerCommand.RaiseCanExecuteChanged();
            RemoveQuestionPackCommand.RaiseCanExecuteChanged();
            ConfigurationViewModel?.RaisePropertyChanged(nameof(ConfigurationViewModel.ActivePack));
            PlayerViewModel?.RaisePropertyChanged(nameof(PlayerViewModel.ActivePack));
        }
    }

    public bool IsPlayMode
    {
        get => _isPlayMode;
        set
        {
            _isPlayMode = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CurrentView));
            SetActivePackCommand.RaiseCanExecuteChanged();
        }
    }

    public object? CurrentView => IsPlayMode ? PlayerViewModel : ConfigurationViewModel;

    public bool IsFullScreen
    {
        get => _isFullScreen;
        set
        {
            _isFullScreen = value;
            RaisePropertyChanged();
        }
    }

    public PlayerViewModel PlayerViewModel { get; }
    public ConfigurationViewModel ConfigurationViewModel { get; }

    public DelegateCommand ShowConfigCommand { get; }
    public DelegateCommand ShowPlayerCommand { get; }
    public DelegateCommand ToggleFullScreenCommand { get; }
    public DelegateCommand CreateNewPackCommand { get; }
    public DelegateCommand RemoveQuestionPackCommand { get; }
    public DelegateCommand SaveAllCommand { get; }
    public DelegateCommand SetActivePackCommand { get; }
    public DelegateCommand ExitProgramCommand { get; }
    public DelegateCommand ImportQuestionsCommand { get; }
    public DelegateCommand ManageCategoriesCommand { get; }

    private async Task LoadAllAsync()
    {
        try
        {
            var categories = await _storageService.GetAllCategoriesAsync();
            var packs = await _storageService.GetAllPacksAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Categories.Clear();
                foreach (var c in categories) Categories.Add(c);

                Packs.Clear();
                foreach (var pack in packs)
                {
                    var vm = new QuestionPackViewModel(pack) { AvailableCategories = Categories };
                    Packs.Add(vm);
                }

                ActivePack = Packs.FirstOrDefault(p => p.Name == "C#frågor") ?? Packs.FirstOrDefault();
            });
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load from MongoDB: {ex.Message}");
            Application.Current.Dispatcher.Invoke(() => ActivePack = Packs.FirstOrDefault());
        }
    }

    private async Task CreateNewPackAsync()
    {
        try
        {
            var pack = new QuestionPack { Name = "New Pack" };
            await _storageService.CreatePackAsync(pack);

            var packViewModel = new QuestionPackViewModel(pack) { AvailableCategories = Categories };
            Packs.Add(packViewModel);
            ActivePack = packViewModel;
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to create pack: {ex.Message}");
        }
    }

    private async Task RemoveQuestionPackAsync()
    {
        if (ActivePack == null) return;

        if (!_dialogService.ShowConfirm($"Delete pack '{ActivePack.Name}'?", "Delete Pack"))
            return;

        try
        {
            await _storageService.DeletePackAsync(ActivePack.Model.Id);
            Packs.Remove(ActivePack);
            ActivePack = Packs.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to delete pack: {ex.Message}");
        }
    }

    private async Task SaveAllAsync(bool showMessage = true)
    {
        try
        {
            foreach (var pack in Packs.Select(p => p.Model))
                await _storageService.UpdatePackAsync(pack);

            if (showMessage)
                _dialogService.ShowInfo("Packs saved successfully.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to save packs: {ex.Message}");
        }
    }

    private void ImportQuestions()
    {
        if (ActivePack == null)
        {
            _dialogService.ShowError("Vänligen välj ett frågepack först innan du importerar frågor.");
            return;
        }
        _dialogService.ShowImportQuestionsDialog(
            getSelected: () => ActivePack,
            saveAll: async () => await SaveAllAsync(showMessage: false)
        );
    }

    private void ManageCategories()
    {
        try
        {

            var vm = new ManageCategoriesViewModel(_storageService, _dialogService, Categories);
            var dlg = new Labb3_Quiz_MongoDB.Views.Dialogs.ManageCategoriesDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            dlg.ShowDialog();


            foreach (var p in Packs)
            {
                if (!ReferenceEquals(p.AvailableCategories, Categories))
                    p.AvailableCategories = Categories;
                p.RaisePropertyChanged(nameof(QuestionPackViewModel.SelectedCategory));
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Could not open category manager: {ex.Message}");
        }
    }
}
