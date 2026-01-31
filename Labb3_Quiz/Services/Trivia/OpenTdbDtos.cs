namespace Labb3_Quiz.Services.Trivia;

public class OpenTdbCategoryListResponse
{
    public List<OpenTdbCategory> Trivia_Categories { get; set; } = new();
}

public class OpenTdbCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class OpenTdbQuestionResponse
{
    public int Response_Code { get; set; }
    public List<OpenTdbQuestion> Results { get; set; } = new();
}

public class OpenTdbQuestion
{
    public string Category { get; set; } = "";
    public string Type { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Question { get; set; } = "";
    public string Correct_Answer { get; set; } = "";
    public List<string> Incorrect_Answers { get; set; } = new();
}

