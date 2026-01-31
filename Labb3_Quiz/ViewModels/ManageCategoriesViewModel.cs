using Labb3_Quiz.Command;
using Labb3_Quiz.Models;
using Labb3_Quiz.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb3_Quiz.ViewModels
{
    public class ManageCategoriesViewModel : ViewModelBase
    {
        private readonly IStorageService _storage;
        private readonly IDialogService _dialogs;

        private readonly ObservableCollection<Category> _categories;

        private Category? _selectedCategory;

        private string _newCategoryName = "";

        public ManageCategoriesViewModel(
            IStorageService storage,
            IDialogService dialogs,
            ObservableCollection<Category> categories)
        {
            _storage = storage;
            _dialogs = dialogs;
            _categories = categories;

            AddCategoryCommand = new DelegateCommand(async _ => await AddCategoryAsync(), _ => CanAdd());
            DeleteCategoryCommand = new DelegateCommand(async _ => await DeleteCategoryAsync(), _ => SelectedCategory != null);
        }

        public ObservableCollection<Category> Categories => _categories;

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                RaisePropertyChanged();
                DeleteCategoryCommand.RaiseCanExecuteChanged();
            }
        }

        public string NewCategoryName
        {
            get => _newCategoryName;
            set
            {
                _newCategoryName = value;
                RaisePropertyChanged();
                AddCategoryCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand AddCategoryCommand { get; }
        public DelegateCommand DeleteCategoryCommand { get; }

        private bool CanAdd() => !string.IsNullOrWhiteSpace(NewCategoryName);

        private async Task AddCategoryAsync()
        {
            var name = NewCategoryName.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                var cat = new Category { Name = name };
                await _storage.CreateCategoryAsync(cat);

                var insertAt = 0;
                while (insertAt < _categories.Count &&
                       string.Compare(_categories[insertAt].Name, cat.Name, StringComparison.OrdinalIgnoreCase) < 0)
                    insertAt++;

                _categories.Insert(insertAt, cat);

                NewCategoryName = "";
            }
            catch (Exception ex)
            {
                _dialogs.ShowError($"Could not add category: {ex.Message}");
            }
        }

        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null) return;
            var toDelete = SelectedCategory;

            if (!_dialogs.ShowConfirm($"Delete category '{toDelete.Name}'?\nPacks using it will be cleared.", "Delete Category"))
                return;

            try
            {
                await _storage.DeleteCategoryAsync(toDelete.Id);
                _categories.Remove(toDelete);
                SelectedCategory = null;
            }
            catch (Exception ex)
            {
                _dialogs.ShowError($"Could not delete category: {ex.Message}");
            }
        }
    }





}
