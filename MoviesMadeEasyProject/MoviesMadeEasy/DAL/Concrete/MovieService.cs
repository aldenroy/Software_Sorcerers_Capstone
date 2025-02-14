using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.DTOs; // DTOs namespace
using MoviesMadeEasy.Models; // Models namespace
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MoviesMadeEasy.DAL.Concrete
{
    public class MovieService : IMovieService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey; 
        private const string ApiHost = "streaming-availability.p.rapidapi.com";

        public MovieService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _apiKey = _configuration["RapidApiKey"] ?? throw new ArgumentNullException("API Key not found in configuration.");
            
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", ApiHost);
        }


        public async Task<List<MoviesMadeEasy.Models.Movie>> SearchMoviesAsync(string query)
        {
            var url = $"https://streaming-availability.p.rapidapi.com/shows/search/title?country=us&title={query}&series_granularity=show&show_type=movie&output_language=en";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Test Response Content: " + content); // Debugging log

                // Deserialize the response into a list of DTOs
                var movies = JsonSerializer.Deserialize<List<MoviesMadeEasy.DTOs.Movie>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Map DTOs to Models
                return movies?.Select(movie => new MoviesMadeEasy.Models.Movie
                {
                    ItemType = movie.ItemType,
                    ShowType = movie.ShowType,
                    Id = movie.Id,
                    ImdbId = movie.ImdbId,
                    TmdbId = movie.TmdbId,
                    Title = movie.Title,
                    Overview = movie.Overview,
                    ReleaseYear = movie.ReleaseYear,
                    OriginalTitle = movie.OriginalTitle,

                    // Map Genres (DTO to Model)
                    Genres = movie.Genres?.Select(genre => new MoviesMadeEasy.Models.Genre
                    {
                        Id = genre.Id,
                        Name = genre.Name
                    }).ToList(),

                    // Map Directors
                    Directors = movie.Directors,

                    // Map Cast
                    Cast = movie.Cast,

                    Rating = movie.Rating,
                    Runtime = movie.Runtime,

                    // Map ImageSet (DTO to Model)
                    ImageSet = movie.ImageSet != null ? new MoviesMadeEasy.Models.ImageSet
                    {
                        VerticalPoster = movie.ImageSet.VerticalPoster != null ? new MoviesMadeEasy.Models.VerticalPoster
                        {
                            W240 = movie.ImageSet.VerticalPoster.W240,
                            W360 = movie.ImageSet.VerticalPoster.W360,
                            W480 = movie.ImageSet.VerticalPoster.W480,
                            W600 = movie.ImageSet.VerticalPoster.W600,
                            W720 = movie.ImageSet.VerticalPoster.W720
                        } : null,
                        HorizontalPoster = movie.ImageSet.HorizontalPoster != null ? new MoviesMadeEasy.Models.HorizontalPoster
                        {
                            W360 = movie.ImageSet.HorizontalPoster.W360,
                            W480 = movie.ImageSet.HorizontalPoster.W480,
                            W720 = movie.ImageSet.HorizontalPoster.W720,
                            W1080 = movie.ImageSet.HorizontalPoster.W1080,
                            W1440 = movie.ImageSet.HorizontalPoster.W1440
                        } : null,
                        VerticalBackdrop = movie.ImageSet.VerticalBackdrop != null ? new MoviesMadeEasy.Models.VerticalBackdrop
                        {
                            W240 = movie.ImageSet.VerticalBackdrop.W240,
                            W360 = movie.ImageSet.VerticalBackdrop.W360,
                            W480 = movie.ImageSet.VerticalBackdrop.W480,
                            W600 = movie.ImageSet.VerticalBackdrop.W600,
                            W720 = movie.ImageSet.VerticalBackdrop.W720
                        } : null,
                        HorizontalBackdrop = movie.ImageSet.HorizontalBackdrop != null ? new MoviesMadeEasy.Models.HorizontalBackdrop
                        {
                            W360 = movie.ImageSet.HorizontalBackdrop.W360,
                            W480 = movie.ImageSet.HorizontalBackdrop.W480,
                            W720 = movie.ImageSet.HorizontalBackdrop.W720,
                            W1080 = movie.ImageSet.HorizontalBackdrop.W1080,
                            W1440 = movie.ImageSet.HorizontalBackdrop.W1440
                        } : null
                    } : null,

                    // Map StreamingOptions (DTO to Model)
                    StreamingOptions = movie.StreamingOptions?.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Select(option => new MoviesMadeEasy.Models.StreamingOption
                        {
                            Service = option.Service != null ? new MoviesMadeEasy.Models.Service
                            {
                                Id = option.Service.Id,
                                Name = option.Service.Name,
                                HomePage = option.Service.HomePage,
                                ThemeColorCode = option.Service.ThemeColorCode,
                                ImageSet = option.Service.ImageSet != null ? new MoviesMadeEasy.Models.ServiceImageSet
                                {
                                    LightThemeImage = option.Service.ImageSet.LightThemeImage,
                                    DarkThemeImage = option.Service.ImageSet.DarkThemeImage,
                                    WhiteImage = option.Service.ImageSet.WhiteImage
                                } : null
                            } : null,
                            Type = option.Type,
                            Link = option.Link,
                            Quality = option.Quality,
                            Audios = option.Audios?.Select(audio => new MoviesMadeEasy.Models.Audio
                            {
                                Language = audio.Language,
                                Region = audio.Region
                            }).ToList(),
                            Subtitles = option.Subtitles?.Select(subtitle => new MoviesMadeEasy.Models.Subtitle
                            {
                                ClosedCaptions = subtitle.ClosedCaptions,
                                Locale = new MoviesMadeEasy.Models.Locale
                                {
                                    Language = subtitle.Locale.Language,
                                    Region = subtitle.Locale.Region
                                }
                            }).ToList(),
                            Price = option.Price != null ? new MoviesMadeEasy.Models.Price
                            {
                                Amount = option.Price.Amount,
                                Currency = option.Price.Currency,
                                Formatted = option.Price.Formatted
                            } : null,
                            ExpiresSoon = option.ExpiresSoon,
                            AvailableSince = option.AvailableSince
                        }).ToList()
                    )
                }).ToList() ?? new List<MoviesMadeEasy.Models.Movie>();
            }

            return new List<MoviesMadeEasy.Models.Movie>();
        }
    }
}