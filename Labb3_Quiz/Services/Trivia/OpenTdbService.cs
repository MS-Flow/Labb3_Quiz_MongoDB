using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Labb3_Quiz.Services.Trivia;

public class OpenTdbService : ITriviaService
{
    private readonly HttpClient _http;

    public OpenTdbService(HttpClient? httpClient = null)
    {
        _http = httpClient ?? new HttpClient
        {
            BaseAddress = new Uri("https://opentdb.com/")
        };
        _http.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<List<OpenTdbCategory>> GetCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<OpenTdbCategoryListResponse>("api_category.php", ct)
                       ?? new OpenTdbCategoryListResponse();
            return resp.Trivia_Categories ?? new();
        }
        catch (HttpRequestException) { throw; }
        catch (TaskCanceledException) { throw; }
        catch (Exception)
        {
            throw new HttpRequestException("Failed to fetch categories from OpenTDB");
        }
    }

    public async Task<OpenTdbQuestionResponse> GetQuestionsAsync(
        int amount, int? categoryId, string? difficulty, CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"amount={Math.Clamp(amount, 1, 50)}",
                "type=multiple",
                "encode=url3986"
            };

            if (categoryId.HasValue)
                queryParams.Add($"category={categoryId.Value}");

            if (!string.IsNullOrWhiteSpace(difficulty))
                queryParams.Add($"difficulty={difficulty.ToLowerInvariant()}");

            var url = $"api.php?{string.Join("&", queryParams)}";
            using var resp = await _http.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync(ct);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<OpenTdbQuestionResponse>(json, options)
                       ?? new OpenTdbQuestionResponse();

            foreach (var q in data.Results)
            {
                q.Question = DecodeText(q.Question);
                q.Correct_Answer = DecodeText(q.Correct_Answer);
                for (int i = 0; i < q.Incorrect_Answers.Count; i++)
                    q.Incorrect_Answers[i] = DecodeText(q.Incorrect_Answers[i]);
                q.Category = DecodeText(q.Category);
            }

            return data;
        }
        catch (HttpRequestException) { throw; }
        catch (TaskCanceledException) { throw; }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to fetch questions from OpenTDB: {ex.Message}", ex);
        }
    }

    private static string DecodeText(string encodedText)
    {
        if (string.IsNullOrEmpty(encodedText)) return encodedText;
        var urlDecoded = Uri.UnescapeDataString(encodedText);
        return WebUtility.HtmlDecode(urlDecoded);
    }
}
