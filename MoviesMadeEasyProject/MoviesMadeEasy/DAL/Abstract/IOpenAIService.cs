// IOpenAIService.cs
public interface IOpenAIService
{
    Task<List<MovieRecommendation>> GetSimilarMoviesAsync(string title);
    Task<string> GetChatCompletionAsync(string prompt);
    // Add other OpenAI operations you might need
}

public class MovieRecommendation
{
    public string Title { get; set; }
    public int Year { get; set; }  // Change from string to int
    public string Reason { get; set; }
}
