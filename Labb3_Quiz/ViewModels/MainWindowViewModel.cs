using Labb3_Quiz.Command;
using Labb3_Quiz.Models;
using Labb3_Quiz.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Labb3_Quiz.ViewModels;

// Huvud-ViewModel (koordinerar packs, laddning/sparande, edit/play mode)
public class MainWindowViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;
    private readonly IDialogService _dialogService;
    private QuestionPackViewModel? _activePack;
    private bool _isPlayMode;
    private bool _isFullScreen;

    public MainWindowViewModel()
    {
        _storageService = new LocalAppDataStorageService();
        _dialogService = new DialogService();
        Packs = new ObservableCollection<QuestionPackViewModel>();
        PlayerViewModel = new PlayerViewModel(this, _dialogService);
        ConfigurationViewModel = new ConfigurationViewModel(this, _dialogService);

        ShowConfigCommand = new DelegateCommand(_ => IsPlayMode = false);
        ShowPlayerCommand = new DelegateCommand(_ => IsPlayMode = true, _ => ActivePack != null && ActivePack.Questions.Count > 0);
        ToggleFullScreenCommand = new DelegateCommand(_ => IsFullScreen = !IsFullScreen);
        CreateNewPackCommand = new DelegateCommand(_ => CreateNewPack());
        RemoveQuestionPackCommand = new DelegateCommand(_ => RemoveQuestionPack(), _ => ActivePack != null);
        SaveAllCommand = new DelegateCommand(async _ => await SaveAllAsync());
        SetActivePackCommand = new DelegateCommand(pack => ActivePack = pack as QuestionPackViewModel, _ => !_isPlayMode);
        ExitProgramCommand = new DelegateCommand(_ => Application.Current.Shutdown());
        ImportQuestionsCommand = new DelegateCommand(_ => ImportQuestions());

        _ = LoadPacksAsync();
    }

    public ObservableCollection<QuestionPackViewModel> Packs { get; }

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

    private void CreateNewPack()
    {
        var pack = new QuestionPack { Name = "New Pack" };
        var packViewModel = new QuestionPackViewModel(pack);
        Packs.Add(packViewModel);
        ActivePack = packViewModel;
    }

    private void RemoveQuestionPack()
    {
        if (ActivePack == null) return;
        if (_dialogService.ShowConfirm($"Delete pack '{ActivePack.Name}'?", "Delete Pack"))
        {
            Packs.Remove(ActivePack);
            ActivePack = Packs.FirstOrDefault();
        }
    }

    private async Task LoadPacksAsync()
    {
        try
        {
            var packs = await _storageService.LoadPacksAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Packs.Clear();
                foreach (var pack in packs)
                    Packs.Add(new QuestionPackViewModel(pack));
                ActivePack = Packs.FirstOrDefault(p => p.Name == "C#frågor") ?? Packs.FirstOrDefault();
            });
        }
        catch (Exception)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ActivePack = Packs.FirstOrDefault();
            });
        }
    }

    private async Task SaveAllAsync(bool showMessage = true)
    {
        try
        {
            var packs = Packs.Select(p => p.Model).ToList();
            await _storageService.SavePacksAsync(packs);
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
}
