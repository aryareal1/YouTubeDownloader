using System.Text.Json.Serialization;

namespace TubeDL.Schema;

public class YoutubeMetadata
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("formats")]
    public List<Format> Formats { get; set; }

    [JsonPropertyName("thumbnails")]
    public List<Thumbnail> Thumbnails { get; set; }

    [JsonPropertyName("thumbnail")]
    public string Thumbnail { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; }

    [JsonPropertyName("channel_url")]
    public string ChannelUrl { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("duration_string")]
    public string DurationString { get; set; }

    [JsonPropertyName("view_count")]
    public long ViewCount { get; set; }

    [JsonPropertyName("average_rating")]
    public double? AverageRating { get; set; }

    [JsonPropertyName("age_limit")]
    public int AgeLimit { get; set; }

    [JsonPropertyName("webpage_url")]
    public string WebpageUrl { get; set; }

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    [JsonPropertyName("playable_in_embed")]
    public bool PlayableInEmbed { get; set; }

    [JsonPropertyName("live_status")]
    public string LiveStatus { get; set; }

    [JsonPropertyName("media_type")]
    public string MediaType { get; set; }

    [JsonPropertyName("album")]
    public string Album { get; set; }

    [JsonPropertyName("artists")]
    public List<string> Artists { get; set; }

    [JsonPropertyName("track")]
    public string Track { get; set; }

    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; }

    [JsonPropertyName("release_year")]
    public int? ReleaseYear { get; set; }

    [JsonPropertyName("comment_count")]
    public int? CommentCount { get; set; }

    [JsonPropertyName("like_count")]
    public long? LikeCount { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; }

    [JsonPropertyName("channel_follower_count")]
    public long? ChannelFollowerCount { get; set; }

    [JsonPropertyName("channel_is_verified")]
    public bool? ChannelIsVerified { get; set; }

    [JsonPropertyName("uploader")]
    public string Uploader { get; set; }

    [JsonPropertyName("uploader_id")]
    public string UploaderIdvalue { get; set; }

    [JsonPropertyName("uploader_url")]
    public string UploaderUrl { get; set; }

    [JsonPropertyName("upload_date")]
    public string UploadDate { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("availability")]
    public string Availability { get; set; }

    [JsonPropertyName("heatmap")]
    public List<Heatmap> Heatmap { get; set; }

    [JsonPropertyName("requested_downloads")]
    public List<RequestedDownload> RequestedDownloads { get; set; }

    public string _RawJson { get; set; }
}

public class Format
{
    [JsonPropertyName("format_id")]
    public string FormatId { get; set; }

    [JsonPropertyName("format_note")]
    public string FormatNote { get; set; }

    [JsonPropertyName("ext")]
    public string Ext { get; set; }

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; }

    [JsonPropertyName("acodec")]
    public string Acodec { get; set; }

    [JsonPropertyName("vcodec")]
    public string Vcodec { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("fps")]
    public double? Fps { get; set; }

    [JsonPropertyName("rows")]
    public int? Rows { get; set; }

    [JsonPropertyName("columns")]
    public int? Columns { get; set; }

    [JsonPropertyName("fragments")]
    public List<Fragment> Fragments { get; set; }

    [JsonPropertyName("audio_ext")]
    public string AudioExt { get; set; }

    [JsonPropertyName("video_ext")]
    public string VideoExt { get; set; }

    [JsonPropertyName("vbr")]
    public double? Vbr { get; set; }

    [JsonPropertyName("abr")]
    public double? Abr { get; set; }

    [JsonPropertyName("tbr")]
    public double? Tbr { get; set; }

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; }

    [JsonPropertyName("aspect_ratio")]
    public double? AspectRatio { get; set; }

    [JsonPropertyName("filesize")]
    public long? Filesize { get; set; }

    [JsonPropertyName("filesize_approx")]
    public long? FilesizeApprox { get; set; }

    [JsonPropertyName("http_headers")]
    public Dictionary<string, string> HttpHeaders { get; set; }

    [JsonPropertyName("format")]
    public string FormatDescription { get; set; }

    [JsonPropertyName("asr")]
    public int? Asr { get; set; }

    [JsonPropertyName("audio_channels")]
    public int? AudioChannels { get; set; }

    [JsonPropertyName("quality")]
    public double? Quality { get; set; }

    [JsonPropertyName("has_drm")]
    public bool? HasDrm { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("language_preference")]
    public int? LanguagePreference { get; set; }

    [JsonPropertyName("preference")]
    public int? Preference { get; set; }

    [JsonPropertyName("dynamic_range")]
    public string DynamicRange { get; set; }

    [JsonPropertyName("container")]
    public string Container { get; set; }

    [JsonPropertyName("source_preference")]
    public int? SourcePreference { get; set; }

    [JsonPropertyName("available_at")]
    public long? AvailableAt { get; set; }

    [JsonPropertyName("downloader_options")]
    public DownloaderOptions DownloaderOptions { get; set; }

    [JsonPropertyName("manifest_url")]
    public string ManifestUrl { get; set; }
}

public class Fragment
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }
}

public class DownloaderOptions
{
    [JsonPropertyName("http_chunk_size")]
    public int HttpChunkSize { get; set; }
}

public class Thumbnail
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("preference")]
    public int Preference { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; }
}

public class Heatmap
{
    [JsonPropertyName("start_time")]
    public double StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public double EndTime { get; set; }

    [JsonPropertyName("value")]
    public double Value { get; set; }
}

public class RequestedDownload : Format
{
    [JsonPropertyName("filename")]
    public string FileName { get; set; }
}