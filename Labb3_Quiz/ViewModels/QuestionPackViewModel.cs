using Labb3_Quiz.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Labb3_Quiz.ViewModels;

public class QuestionPackViewModel : ViewModelBase
{
    private readonly QuestionPack _model;

    private ObservableCollection<Category> _availableCategories = new();

    public QuestionPackViewModel(QuestionPack model)
    {
        _model = model;

        Questions = new ObservableCollection<Question>(_model.Questions);
        Questions.CollectionChanged += Questions_CollectionChanged;

        Difficulties = new List<PackDifficulty> { PackDifficulty.Easy, PackDifficulty.Medium, PackDifficulty.Hard };
    }

    private void Questions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            foreach (Question q in e.NewItems) _model.Questions.Add(q);

        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            foreach (Question q in e.OldItems) _model.Questions.Remove(q);

        if (e.Action == NotifyCollectionChangedAction.Replace && e.OldItems != null && e.NewItems != null)
            _model.Questions[e.OldStartingIndex] = (Question)e.NewItems[0]!;

        if (e.Action == NotifyCollectionChangedAction.Reset)
            _model.Questions.Clear();
    }

    public string Name
    {
        get => _model.Name;
        set
        {
            _model.Name = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(DisplayName));
        }
    }

    public string DisplayName => $"{Name} ({Difficulty})";

    public PackDifficulty Difficulty
    {
        get => _model.Difficulty;
        set
        {
            _model.Difficulty = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(DisplayName));
        }
    }

    public int TimePerQuestionSeconds
    {
        get => _model.TimePerQuestionSeconds;
        set
        {
            _model.TimePerQuestionSeconds = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<Question> Questions { get; set; }
    public List<PackDifficulty> Difficulties { get; }

    public ObservableCollection<Category> AvailableCategories
    {
        get => _availableCategories;
        set
        {
            _availableCategories = value ?? new ObservableCollection<Category>();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(SelectedCategory));
        }
    }

    public Category? SelectedCategory
    {
        get
        {
            if (_model.CategoryId == null) return null;
            return AvailableCategories.FirstOrDefault(c => c.Id == _model.CategoryId);
        }
        set
        {
            if (value == null)
            {
                _model.CategoryId = null;
                _model.CategoryName = null;
            }
            else
            {
                _model.CategoryId = value.Id;
                _model.CategoryName = value.Name;
            }
            RaisePropertyChanged();
        }
    }

    public string CategoryLabel => _model.CategoryName ?? "—";

    public QuestionPack Model => _model;
}

