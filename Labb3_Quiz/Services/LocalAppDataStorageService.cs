using System.IO;
using System.Text.Json;
using Labb3_Quiz.Models;

namespace Labb3_Quiz.Services;

// Hanterar lagring och laddning av frågepacks från/till JSON-fil
public class LocalAppDataStorageService : IStorageService
{
    private readonly string _appFolder;
    private readonly string _filePath;
    
    // JSON-options: WriteIndented gör JSON läsbart
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public LocalAppDataStorageService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _appFolder = Path.Combine(appDataPath, "Labb3_Quiz");
        _filePath = Path.Combine(_appFolder, "packs.json");

        if (!Directory.Exists(_appFolder))
            Directory.CreateDirectory(_appFolder);

        if (!File.Exists(_filePath))
            CopyInitialPacksFile();
    }

    // Kopierar standardfrågor vid första körningen
    private void CopyInitialPacksFile()
    {
        try
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var resourcesPath = Path.Combine(appDirectory, "Resources", "packs.json");
            if (File.Exists(resourcesPath))
                File.Copy(resourcesPath, _filePath, overwrite: false);
        }
        catch (Exception) { }
    }

    // Laddar packs från JSON (deserialisering: JSON → C#-objekt)
    public async Task<List<QuestionPack>> LoadPacksAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new List<QuestionPack>();

            await using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            var packs = await JsonSerializer.DeserializeAsync<List<QuestionPack>>(fileStream, _jsonOptions);
            return packs ?? new List<QuestionPack>();
        }
        catch (Exception)
        {
            return new List<QuestionPack>();
        }
    }

    // Sparar packs till JSON (serialisering: C#-objekt → JSON)
    public async Task SavePacksAsync(List<QuestionPack> packs)
    {
        try
        {
            await using var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await JsonSerializer.SerializeAsync(fileStream, packs, _jsonOptions);
        }
        catch (Exception)
        {
            throw;
        }
    }
}

