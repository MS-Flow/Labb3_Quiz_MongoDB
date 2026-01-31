using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Labb3_Quiz.Models;

public enum PackDifficulty { Easy, Medium, Hard }

public class QuestionPack
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Pack";
    public PackDifficulty Difficulty { get; set; } = PackDifficulty.Medium;

    public int TimePerQuestionSeconds { get; set; } = 20;

    public Guid? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public List<Question> Questions { get; set; } = new();
}