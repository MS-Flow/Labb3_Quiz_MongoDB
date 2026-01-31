using System;
using System.Collections.Generic;

namespace Labb3_Quiz.Models;

public enum PackDifficulty { Easy, Medium, Hard }

public class QuestionPack
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Pack";
    public PackDifficulty Difficulty { get; set; } = PackDifficulty.Medium;
    public int TimePerQuestionSeconds { get; set; } = 20;
    public List<Question> Questions { get; set; } = new();
}