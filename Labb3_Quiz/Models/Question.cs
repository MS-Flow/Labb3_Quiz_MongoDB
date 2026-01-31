namespace Labb3_Quiz.Models;

public class Question
{
    public string Text { get; set; } = "";
    public List<AnswerOption> Options { get; set; } = new();
}