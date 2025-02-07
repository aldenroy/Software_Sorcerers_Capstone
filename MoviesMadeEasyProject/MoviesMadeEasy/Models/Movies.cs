using System.Collections.Generic;

namespace MoviesMadeEasy.DTOs
{
    // No need for a wrapper class. The API response is a list of movies.
    public class Movie
    {
        public string ItemType { get; set; }
        public string ShowType { get; set; }
        public string Id { get; set; }
        public string ImdbId { get; set; }
        public string TmdbId { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public int ReleaseYear { get; set; }
        public string OriginalTitle { get; set; }
        public List<Genre> Genres { get; set; } // Updated to match the API response
        public List<string> Directors { get; set; }
        public List<string> Cast { get; set; }
        public int Rating { get; set; }
        public int Runtime { get; set; }
        public ImageSet ImageSet { get; set; }
        public Dictionary<string, List<StreamingOption>> StreamingOptions { get; set; }
    }

    public class Genre
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class ImageSet
    {
        public VerticalPoster VerticalPoster { get; set; }
        public HorizontalPoster HorizontalPoster { get; set; }
        public VerticalBackdrop VerticalBackdrop { get; set; }
        public HorizontalBackdrop HorizontalBackdrop { get; set; }
    }

    public class VerticalPoster
    {
        public string W240 { get; set; }
        public string W360 { get; set; }
        public string W480 { get; set; }
        public string W600 { get; set; }
        public string W720 { get; set; }
    }

    public class HorizontalPoster
    {
        public string W360 { get; set; }
        public string W480 { get; set; }
        public string W720 { get; set; }
        public string W1080 { get; set; }
        public string W1440 { get; set; }
    }

    public class VerticalBackdrop
    {
        public string W240 { get; set; }
        public string W360 { get; set; }
        public string W480 { get; set; }
        public string W600 { get; set; }
        public string W720 { get; set; }
    }

    public class HorizontalBackdrop
    {
        public string W360 { get; set; }
        public string W480 { get; set; }
        public string W720 { get; set; }
        public string W1080 { get; set; }
        public string W1440 { get; set; }
    }

    public class StreamingOption
    {
        public Service Service { get; set; }
        public string Type { get; set; }
        public string Link { get; set; }
        public string Quality { get; set; }
        public List<Audio> Audios { get; set; }
        public List<Subtitle> Subtitles { get; set; }
        public Price Price { get; set; }
        public bool ExpiresSoon { get; set; }
        public long AvailableSince { get; set; }
    }

    public class Service
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string HomePage { get; set; }
        public string ThemeColorCode { get; set; }
        public ServiceImageSet ImageSet { get; set; }
    }

    public class ServiceImageSet
    {
        public string LightThemeImage { get; set; }
        public string DarkThemeImage { get; set; }
        public string WhiteImage { get; set; }
    }

    public class Audio
    {
        public string Language { get; set; }
        public string Region { get; set; }
    }

    public class Subtitle
    {
        public bool ClosedCaptions { get; set; }
        public Locale Locale { get; set; }
    }

    public class Locale
    {
        public string Language { get; set; }
        public string Region { get; set; }
    }

    public class Price
    {
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Formatted { get; set; }
    }
}