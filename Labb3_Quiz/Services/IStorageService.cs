using Labb3_Quiz.Models;

namespace Labb3_Quiz.Services;


public interface IStorageService
{
    Task<List<QuestionPack>> GetAllPacksAsync();

    Task CreatePackAsync(QuestionPack pack);

    Task UpdatePackAsync(QuestionPack pack);

    Task DeletePackAsync(Guid packId);


    Task<List<Category>> GetAllCategoriesAsync();

    Task CreateCategoryAsync(Category category);

    Task DeleteCategoryAsync(Guid categoryId);
}