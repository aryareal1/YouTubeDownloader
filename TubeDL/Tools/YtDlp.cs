using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using TubeDL.Schema;

namespace TubeDL.Tools;

public class YtDlp(string path)
{
    // -- STATIC VARIABLES --

    /// <summary>
    /// Local binary path for yt-dlp.exe
    /// </summary>
    public static readonly string localPath = System.IO.Path.Combine(Utils.depPath, "yt-dlp.exe");
    /// <summary>
    /// yt-dlp latest release download URL
    /// </summary>
    public static readonly string url = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
    /// <summary>
    /// YouTube URL regex pattern
    /// </summary>
    public static readonly string regexUrl = @"^(?:(?:https?:)?\/\/)?(?:(?:(?:www|m(?:usic)?)\.)?youtu(?:\.be|be\.com)\/(?:shorts\/|live\/|v\/|e(?:mbed)?\/|watch(?:\/|\?(?:\S+=\S+&)*v=)|oembed\?url=https?%3A\/\/(?:www|m(?:usic)?)\.youtube\.com\/watch\?(?:\S+=\S+&)*v%3D|attribution_link\?(?:\S+=\S+&)*u=(?:\/|%2F)watch(?:\?|%3F)v(?:=|%3D))?|www\.youtube-nocookie\.com\/embed\/)([\w-]{11})[\?&#]?\S*$";

    // -- STATIC METHODS --

    /// <summary>
    /// Get available yt-dlp.exe path
    /// </summary>
    /// <returns>
    /// Return local or global path if found, otherwise <see langword="null"/>
    /// </returns>
    public static async Task<string?> GetPath()
    {
        if (File.Exists(localPath))
            return localPath;

        try
        {
            string output = (await Utils.Eval("cmd.exe", "/c where yt-dlp")).Trim();
            if (
                string.IsNullOrWhiteSpace(output) ||
                output == "INFO: Could not find files for the given pattern(s)."
            )
                return null;

            return output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get latest yt-dlp version from GitHub API
    /// </summary>
    public static async Task<string?> GetLatestVersion()
    {
        try
        {
            string output = (await Utils.Eval("cmd.exe", "/c curl -s https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest")).Trim();
            using JsonDocument doc = JsonDocument.Parse(output);
            if (doc.RootElement.TryGetProperty("tag_name", out JsonElement tagNameElement))
                return tagNameElement.GetString();

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Downloads the latest file from a predefined URL to a local path.
    /// </summary>
    /// <remarks>This method attempts to download a file asynchronously. If an error occurs during the
    /// download,  the method returns <see langword="false"/> without throwing an exception.</remarks>
    /// <returns><see langword="true"/> if the file is successfully downloaded; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> DownloadLatest(Action<long, long> progress)
    {
        try
        {
            await Utils.DownloadFileAsync(url, localPath, progress);
            return true;
        }
        catch
        {
            File.Delete(localPath);
            return false;
        }
    }

    // -- INSTANCE METHODS --

    /// <summary>
    /// Get yt-dlp version
    /// </summary>
    public async Task<string?> GetVersion()
    {
        string output = (await Utils.Eval(path, "--version")).Trim();
        if (Regex.IsMatch(output, @"^\d{4}\.\d{2}\.\d{2}(?:-[\w.-]+)?$"))
            return output;

        return null;
    }

    /// <summary>
    /// Check if yt-dlp is valid binary (can get version)
    /// </summary>
    public async Task<bool> IsValid()
    {
        string? version = await GetVersion();
        return version is not null;
    }

    /// <summary>
    /// Check if an update is available
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsUpdateAvailable()
    {
        string? currentVersion = await GetVersion();
        string? latestVersion = await GetLatestVersion();
        if (currentVersion is null || latestVersion is null)
            return false;

        return !currentVersion.Equals(latestVersion);
    }
    /// <summary>
    /// yt-dlp executable path
    /// </summary>
    public string Path => path;

    // -- MAIN METHODS --

    /// <summary>
    /// Get video metadata from URL
    /// </summary>
    /// <param name="url">Youtube url to search</param>
    /// <exception cref="Exception"></exception>
    public async Task<YoutubeMetadata> GetMetadata(string url)
    {
        url = Regex.Replace(url, @"&list=[^&]+", "");
        string output = await Utils.Eval(path, $"-J \"{url}\"");
        output = output.Split(["\r", "\n"], StringSplitOptions.RemoveEmptyEntries).Last();

        YoutubeMetadata? metadata = JsonSerializer.Deserialize<YoutubeMetadata>(output)
            ?? throw new Exception("Failed to get metadata from yt-dlp.");

        metadata._RawJson = output;
        return metadata;
    }
}

public class YtDlpProgress
{
    public string Type { get; set; } = "";
    public double Percentage { get; set; }
    public double TotalSize { get; set; }
    public string SizeUnit { get; set; } = "MiB";
    public double Speed { get; set; }
    public string SpeedUnit { get; set; } = "KiB/s";
    public TimeSpan EstimatedTimeRemaining { get; set; }

    public override string ToString()
    {
        return $"{Type} | {Percentage}% of {TotalSize}{SizeUnit} at {Speed}{SpeedUnit}, ETA {EstimatedTimeRemaining:mm\\:ss}";
    }

    public static YtDlpProgress? Parse(string line)
    {
        var match = Regex.Match(line, @"\[download\]\s+(?<percent>[\d\.]+)%\s+of\s+(?<size>[\d\.]+)(?<unit>[KMG]iB)\s+at\s+(?<speed>[\d\.]+)(?<speedunit>[KMG]iB/s)\s+ETA\s+(?<eta>\d+:\d+)");
        if (!match.Success) return null;

        var result = new YtDlpProgress
        {
            Percentage = double.Parse(match.Groups["percent"].Value),
            TotalSize = double.Parse(match.Groups["size"].Value),
            SizeUnit = match.Groups["unit"].Value,
            Speed = double.Parse(match.Groups["speed"].Value),
            SpeedUnit = match.Groups["speedunit"].Value,
            EstimatedTimeRemaining = TimeSpan.ParseExact(match.Groups["eta"].Value, "mm\\:ss", null)
        };

        return result;
    }
}