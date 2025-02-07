using Newtonsoft.Json;
using System.Collections.Generic;

namespace MoviesMadeEasy.Models
{
    public class Movie
    {
        [JsonProperty("itemType")]
        public string ItemType { get; set; }

        [JsonProperty("showType")]
        public string ShowType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("imdbId")]
        public string ImdbId { get; set; }

        [JsonProperty("tmdbId")]
        public string TmdbId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }

        [JsonProperty("releaseYear")]
        public int ReleaseYear { get; set; }

        [JsonProperty("originalTitle")]
        public string OriginalTitle { get; set; }

        [JsonProperty("genres")]
        public List<Genre> Genres { get; set; }

        [JsonProperty("directors")]
        public List<string> Directors { get; set; }

        [JsonProperty("cast")]
        public List<string> Cast { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("runtime")]
        public int Runtime { get; set; }

        [JsonProperty("imageSet")]
        public ImageSet ImageSet { get; set; }

        [JsonProperty("streamingOptions")]
        public Dictionary<string, List<StreamingOption>> StreamingOptions { get; set; }
    }

    public class Genre
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class ImageSet
    {
        [JsonProperty("verticalPoster")]
        public VerticalPoster VerticalPoster { get; set; }

        [JsonProperty("horizontalPoster")]
        public HorizontalPoster HorizontalPoster { get; set; }

        [JsonProperty("verticalBackdrop")]
        public VerticalBackdrop VerticalBackdrop { get; set; }

        [JsonProperty("horizontalBackdrop")]
        public HorizontalBackdrop HorizontalBackdrop { get; set; }
    }

    public class VerticalPoster
    {
        [JsonProperty("w240")]
        public string W240 { get; set; }

        [JsonProperty("w360")]
        public string W360 { get; set; }

        [JsonProperty("w480")]
        public string W480 { get; set; }

        [JsonProperty("w600")]
        public string W600 { get; set; }

        [JsonProperty("w720")]
        public string W720 { get; set; }
    }

    public class HorizontalPoster
    {
        [JsonProperty("w360")]
        public string W360 { get; set; }

        [JsonProperty("w480")]
        public string W480 { get; set; }

        [JsonProperty("w720")]
        public string W720 { get; set; }

        [JsonProperty("w1080")]
        public string W1080 { get; set; }

        [JsonProperty("w1440")]
        public string W1440 { get; set; }
    }

    public class VerticalBackdrop
    {
        [JsonProperty("w240")]
        public string W240 { get; set; }

        [JsonProperty("w360")]
        public string W360 { get; set; }

        [JsonProperty("w480")]
        public string W480 { get; set; }

        [JsonProperty("w600")]
        public string W600 { get; set; }

        [JsonProperty("w720")]
        public string W720 { get; set; }
    }

    public class HorizontalBackdrop
    {
        [JsonProperty("w360")]
        public string W360 { get; set; }

        [JsonProperty("w480")]
        public string W480 { get; set; }

        [JsonProperty("w720")]
        public string W720 { get; set; }

        [JsonProperty("w1080")]
        public string W1080 { get; set; }

        [JsonProperty("w1440")]
        public string W1440 { get; set; }
    }

    public class StreamingOption
    {
        [JsonProperty("service")]
        public Service Service { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("quality")]
        public string Quality { get; set; }

        [JsonProperty("audios")]
        public List<Audio> Audios { get; set; }

        [JsonProperty("subtitles")]
        public List<Subtitle> Subtitles { get; set; }

        [JsonProperty("price")]
        public Price Price { get; set; }

        [JsonProperty("expiresSoon")]
        public bool ExpiresSoon { get; set; }

        [JsonProperty("availableSince")]
        public long AvailableSince { get; set; }
    }

    public class Service
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("homePage")]
        public string HomePage { get; set; }

        [JsonProperty("themeColorCode")]
        public string ThemeColorCode { get; set; }

        [JsonProperty("imageSet")]
        public ServiceImageSet ImageSet { get; set; }
    }

    public class ServiceImageSet
    {
        [JsonProperty("lightThemeImage")]
        public string LightThemeImage { get; set; }

        [JsonProperty("darkThemeImage")]
        public string DarkThemeImage { get; set; }

        [JsonProperty("whiteImage")]
        public string WhiteImage { get; set; }
    }

    public class Audio
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }

    public class Subtitle
    {
        [JsonProperty("closedCaptions")]
        public bool ClosedCaptions { get; set; }

        [JsonProperty("locale")]
        public Locale Locale { get; set; }
    }

    public class Locale
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }

    public class Price
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("formatted")]
        public string Formatted { get; set; }
    }
}