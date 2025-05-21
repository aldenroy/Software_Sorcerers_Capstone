// OpenAIService.cs
using System.Net.Http.Headers;
using System.Text.Json;   
using System.Text.Json.Serialization;
public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(
        HttpClient httpClient, 
        IConfiguration config,
        ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
        
        // Configure HttpClient
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _config["OpenAI_ApiKey"]);
        _httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
    }

    public async Task<List<MovieRecommendation>> GetSimilarMoviesAsync(string title)
    {
        try
        {
            var prompt = $"""
                Provide 5 movies similar to '{title}'. For each movie include:
                - Title
                - Release year
                - Brief reason for the recommendation

                Format the response as a JSON array with properties: 
                title, year, reason.
                """;

            var response = await GetChatCompletionMoreLikeThisAsync(prompt);

            // Clean the response by removing the code block delimiters
            var cleanedResponse = response.Replace("```json\n", "").Replace("\n```", "");

            // Parse the cleaned JSON response
            var recommendations = JsonSerializer.Deserialize<List<MovieRecommendation>>(
                cleanedResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return recommendations ?? new List<MovieRecommendation>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar movies for {Title}", title);
            throw; // Or return empty list if you prefer graceful degradation
        }
    }


    public async Task<string> GetChatCompletionMoreLikeThisAsync(string prompt)
    {
        const int maxRetries = 3;
        const int initialDelayMs = 1000; // Start with 1 second delay
        int attempt = 0;

        while (true)
        {
            try
            {
                var request = new
                {
                    model = _config["OpenAI_Model"],
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.7,
                    max_tokens = 1000
                };

                var response = await _httpClient.PostAsJsonAsync("chat/completions", request);
                
                // Handle rate limiting specifically
                if ((int)response.StatusCode == 429)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta?.TotalMilliseconds 
                        ?? initialDelayMs * Math.Pow(2, attempt);
                    
                    _logger.LogWarning("Rate limited. Retrying after {DelayMs}ms...", retryAfter);
                    await Task.Delay((int)retryAfter);
                    continue;
                }

                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<OpenAICompletionResponse>();
                var resultContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw OpenAI response: {ResponseContent}", resultContent);

                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
            }
            catch (HttpRequestException ex) when (attempt < maxRetries)
            {
                attempt++;
                var delay = initialDelayMs * Math.Pow(2, attempt);
                _logger.LogWarning(ex, "Attempt {Attempt} failed. Retrying in {DelayMs}ms...", attempt, delay);
                await Task.Delay((int)delay);
            }
        }
    }

    public async Task<string> GetChatCompletionAsync(string prompt)
    {
        const int maxRetries = 3;
        const int initialDelayMs = 1000; // Start with 1 second delay
        int attempt = 0;
        prompt += " return any movies in this prompt with ^ on both sides not quotations or any other characters!";

        while (true)
        {
            try
            {
                var request = new
                {
                    model = _config["OpenAI_Model"],
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.7,
                    max_tokens = 1000
                };

                var response = await _httpClient.PostAsJsonAsync("chat/completions", request);
                
                // Handle rate limiting specifically
                if ((int)response.StatusCode == 429)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta?.TotalMilliseconds 
                        ?? initialDelayMs * Math.Pow(2, attempt);
                    
                    _logger.LogWarning("Rate limited. Retrying after {DelayMs}ms...", retryAfter);
                    await Task.Delay((int)retryAfter);
                    continue;
                }

                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<OpenAICompletionResponse>();
                var resultContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw OpenAI response: {ResponseContent}", resultContent);

                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
            }
            catch (HttpRequestException ex) when (attempt < maxRetries)
            {
                attempt++;
                var delay = initialDelayMs * Math.Pow(2, attempt);
                _logger.LogWarning(ex, "Attempt {Attempt} failed. Retrying in {DelayMs}ms...", attempt, delay);
                await Task.Delay((int)delay);
            }
        }
    }

public async Task<string> GetChatResponse(string query)
{
    try
    {
        var response = await GetChatCompletionAsync(query);
        
        // Clean the response by removing the code block delimiters
        var cleanedResponse = response.Replace("```json\n", "").Replace("\n```", "");

        // Convert the response to HTML with links for quoted movie titles
        var htmlResponse = ConvertResponseToHtml(cleanedResponse);
        
        return htmlResponse;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting similar movies for {Query}", query);
        return "Sorry, I couldn't process your request. Please try again.";
    }
}

private string ConvertResponseToHtml(string response)
{
    // Split into lines and process each one
    var lines = response.Split('\n')
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .Select(line => 
        {
            // take each line to convert quoted movies to links
            return System.Text.RegularExpressions.Regex.Replace(
                line,
                @"\^([^\^]+)\^",
                match => 
                {
                    var movieTitle = match.Groups[1].Value;
                    return $"<a href=\"#\" class=\"quoted-link\" data-movie=\"{movieTitle}\">\"{movieTitle}\"</a>";
                });
        });

    // Join lines with <br> tags so we get new lines
    return string.Join("<br>", lines);
}

    private record OpenAICompletionResponse(
        List<Choice> Choices);
    
    private record Choice(
        Message Message);
    
    private record Message(
        string Content);
}