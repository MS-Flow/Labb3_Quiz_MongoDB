using Labb3_Quiz.Models;
using System.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Labb3_Quiz.Services.MongoDb;

public class MongoStorageService : IStorageService
{
    private const string PacksCollectionName = "questionPacks";
    private const string CategoriesCollectionName = "categories";

    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<QuestionPack> _packs;
    private readonly IMongoCollection<Category> _categories;

    private static bool _bsonConfigured;

    private static void EnsureBsonConfigured()
    {
        if (_bsonConfigured) return;

        var guidSerializer = new GuidSerializer(GuidRepresentation.Standard);
        BsonSerializer.RegisterSerializer(guidSerializer);
        BsonSerializer.RegisterSerializer(new NullableSerializer<Guid>(guidSerializer));

        _bsonConfigured = true;
    }

    public MongoStorageService(MongoDbSettings? settings = null)
    {
        EnsureBsonConfigured();

        settings ??= MongoDbSettings.FromEnvironment();

        var client = new MongoClient(settings.ConnectionString);
        _db = client.GetDatabase(settings.DatabaseName);

        _packs = _db.GetCollection<QuestionPack>(PacksCollectionName);
        _categories = _db.GetCollection<Category>(CategoriesCollectionName);


        EnsureDatabaseInitialized();
    }

    private void EnsureDatabaseInitialized()
    {
        var existing = _db.ListCollectionNames().ToList();

        if (!existing.Contains(PacksCollectionName))
            _db.CreateCollection(PacksCollectionName);
        if (!existing.Contains(CategoriesCollectionName))
            _db.CreateCollection(CategoriesCollectionName);

        var nameIndex = new CreateIndexModel<Category>(
            Builders<Category>.IndexKeys.Ascending(x => x.Name),
            new CreateIndexOptions { Unique = true, Name = "uniq_category_name" }
        );
        _categories.Indexes.CreateOne(nameIndex);

        if (_packs.EstimatedDocumentCount() == 0)
        {
            var seeded = TryLoadInitialPacksFromResources();
            if (seeded.Count > 0)
                _packs.InsertMany(seeded);
        }

        if (_categories.EstimatedDocumentCount() == 0)
        {
            var defaults = new[]
            {
            new Category { Name = "General" },
            new Category { Name = "Programming" },
            new Category { Name = "Movies" },
            new Category { Name = "Sports" }
        };

            try
            {
                _categories.InsertMany(defaults);
            }
            catch
            {
            }
        }
    }

    private static List<QuestionPack> TryLoadInitialPacksFromResources()
    {
        try
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var resourcesPath = Path.Combine(appDirectory, "Resources", "packs.json");
            if (!File.Exists(resourcesPath))
                return new List<QuestionPack>();

            var json = File.ReadAllText(resourcesPath);
            var packs = JsonSerializer.Deserialize<List<QuestionPack>>(json);
            return packs ?? new List<QuestionPack>();
        }
        catch
        {
            return new List<QuestionPack>();
        }
    }


    public async Task<List<QuestionPack>> GetAllPacksAsync()
    {
        return await _packs.Find(Builders<QuestionPack>.Filter.Empty).ToListAsync();
    }

    public async Task CreatePackAsync(QuestionPack pack)
    {
        await _packs.InsertOneAsync(pack);
    }

    public async Task UpdatePackAsync(QuestionPack pack)
    {
        var filter = Builders<QuestionPack>.Filter.Eq(x => x.Id, pack.Id);
        await _packs.ReplaceOneAsync(filter, pack, new ReplaceOptions { IsUpsert = true });
    }

    public async Task DeletePackAsync(Guid packId)
    {
        var filter = Builders<QuestionPack>.Filter.Eq(x => x.Id, packId);
        await _packs.DeleteOneAsync(filter);
    }


    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _categories.Find(Builders<Category>.Filter.Empty)
            .SortBy(x => x.Name)
            .ToListAsync();
    }

    public async Task CreateCategoryAsync(Category category)
    {
        category.Name = category.Name.Trim();
        await _categories.InsertOneAsync(category);
    }


    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var filter = Builders<Category>.Filter.Eq(x => x.Id, categoryId);
        await _categories.DeleteOneAsync(filter);

        var packFilter = Builders<QuestionPack>.Filter.Eq(x => x.CategoryId, categoryId);
        var update = Builders<QuestionPack>.Update
            .Set(x => x.CategoryId, null)
            .Set(x => x.CategoryName, null);
        await _packs.UpdateManyAsync(packFilter, update);
    }
}
