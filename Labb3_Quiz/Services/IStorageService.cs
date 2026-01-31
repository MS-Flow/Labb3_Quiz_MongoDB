using Labb3_Quiz.Models;

namespace Labb3_Quiz.Services;


public interface IStorageService
{
    Task<List<QuestionPack>> LoadPacksAsync();
    Task SavePacksAsync(List<QuestionPack> packs);
}

