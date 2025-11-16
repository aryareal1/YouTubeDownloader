using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Text.Json;

namespace TubeDL.Tools;

public class Ffmpeg(string path)
{
    // -- STATIC VARIABLES --

    /// <summary>
    /// Local directory path for ffmpeg executables
    /// </summary>
    public static readonly string localPath = Path.Combine(Utils.depPath, "ffmpeg");
    /// <summary>
    /// Represents the URL of the FFmpeg release essentials archive.
    /// </summary>
    public static readonly string url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.7z";

    // -- STATIC METHODS --

    /// <summary>
    /// Get available ffmpeg.exe path
    /// </summary>
    /// <returns>
    /// Return local or global path if found, otherwise <see langword="null"/>
    /// </returns>
    public static async Task<string?> GetPath()
    {
        if (Directory.Exists(localPath))
            return localPath;

        try
        {
            string output = (await Utils.Eval("cmd.exe", "/c where ffmpeg")).Trim();
            if (
                string.IsNullOrWhiteSpace(output) ||
                output == "INFO: Could not find files for the given pattern(s)."
            )
                return null;

            return Path.GetFullPath(Path.Combine(output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0], "..\\.."));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Downloads the latest version of a file from a predefined URL, extracts its contents, and organizes the extracted
    /// files into a specified directory.
    /// </summary>
    /// <remarks>This method downloads a compressed archive file, extracts its contents, and organizes the
    /// extracted files into a target directory.  If the extraction results in a nested directory structure, the method
    /// flattens the structure by moving the contents of the inner  directory to the target directory. The archive file
    /// is deleted after extraction, regardless of success or failure.</remarks>
    /// <param name="progress">A callback that reports the download progress. The first parameter represents the number of bytes downloaded so
    /// far,  and the second parameter represents the total number of bytes to download.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the download and
    /// extraction  succeed; otherwise, <see langword="false"/>.</returns>
    public static async Task<bool> DownloadLatest(Action<long, long> progress)
    {
        string archivePath = Path.Combine(Utils.depPath, "ffmpeg.7z");
        try
        {
            await Utils.DownloadFileAsync(url, archivePath, progress);
            Directory.CreateDirectory(localPath);

            using (var archive = ArchiveFactory.Open(archivePath))
            {
                using var reader = archive.ExtractAllEntries();
                reader.WriteAllToDirectory(localPath, new ExtractionOptions()
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
            var subDirs = Directory.GetDirectories(localPath);
            if (subDirs.Length == 1)
            {
                string innerDir = subDirs[0];
                foreach (var dir in Directory.GetDirectories(innerDir))
                    Directory.Move(dir, Path.Combine(localPath, Path.GetFileName(dir)));

                foreach (var file in Directory.GetFiles(innerDir))
                    File.Move(file, Path.Combine(localPath, Path.GetFileName(file)));

                Directory.Delete(innerDir, true);
            }

            File.Delete(archivePath);
            return true;
        }
        catch
        {
            File.Delete(archivePath);
            return false;
        }
    }

    // -- INSTANCE METHODS --

    /// <summary>
    /// Gets the full file path to the ffmpeg executable.
    /// </summary>
    public string ExePath => Path.Combine(path, "bin", "ffmpeg.exe");

    /// <summary>
    /// Asynchronously retrieves the version of the installed FFmpeg executable.
    /// </summary>
    /// <remarks>This method executes the FFmpeg command-line tool with the `-version` argument and parses the
    /// output to extract the version number. If FFmpeg is not installed or the version cannot be determined, the method
    /// returns <see langword="null"/>.</remarks>
    /// <returns>A <see cref="string"/> containing the FFmpeg version number if successfully retrieved; otherwise, <see
    /// langword="null"/>.</returns>
    public async Task<string?> GetVersion()
    {
        try
        {
            string output = await Utils.Eval(ExePath, "-version");
            var firstLine = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0];
            var match = System.Text.RegularExpressions.Regex.Match(firstLine, @"ffmpeg version (\S+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines whether the current version is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the version is
    /// valid; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> IsValid()
    {
        var version = await GetVersion();
        return version is not null;
    }

    // -- ARGS BUILDERS --

    public static (string codec, string? mux) CreateAudioArgs(Schema.Format format, string ext)
    {
        // Sanitize ext
        ext = (ext ?? "").Trim().ToLowerInvariant();

        // Check if audio extension matches
        string aext = (format.AudioExt ?? format.Ext ?? "").ToLowerInvariant();
        if (!string.IsNullOrEmpty(aext) && aext == ext)
            return ("copy", null);

        // Check audio codec
        string acodec = (format.Acodec ?? "none").ToLowerInvariant();
        if (acodec == "none") return BuildAudioReencodeArgs(ext);

        acodec = acodec switch
        {
            string s when s.StartsWith("mp4a") || s == "aac" => "aac",
            string s when s.Contains("opus") => "opus",
            string s when s.Contains("vorbis") => "vorbis",
            string s when s.Contains("flac") => "flac",
            string s when s.Contains("mp3") || s == "libmp3lame" => "mp3",
            string s when s.StartsWith("pcm") => "pcm",
            _ => acodec,
        };
        bool isCodecCompatibleWithExt = ext switch
        {
            "mp3" => acodec == "mp3",
            "m4a" or "mp4" => acodec == "aac",
            "wav" => acodec == "pcm",
            "flac" => acodec == "flac",
            "ogg" => acodec == "vorbis",
            "opus" => acodec == "opus",
            "webm" => acodec == "opus" || acodec == "vorbis",
            _ => false,
        };
        if (isCodecCompatibleWithExt) return ("copy", null);

        return BuildAudioReencodeArgs(ext);
    }

    public static (string codec, string? mux) CreateVideoArgs(Schema.Format format, string ext)
    {
        // Sanitize ext
        ext = (ext ?? "").Trim().ToLowerInvariant();

        // Check if video extension matches
        string vext = (format.VideoExt ?? format.Ext ?? format.Container ?? "").ToLowerInvariant();
        vext = vext.ToLowerInvariant();

        // Check video codec
        string vcodec = (format.Vcodec ?? "none").ToLowerInvariant();
        if (vcodec == "none") return BuildVideoReencodeArgs(ext);

        vcodec = vcodec switch
        {
            string s when s.StartsWith("avc1") || s.Contains("h264") || s.Contains("avc") => "h264",
            string s when s.Contains("hevc") || s.Contains("h265") || s.Contains("hev1") => "hevc",
            string s when s.Contains("vp9") => "vp9",
            string s when s.Contains("vp8") => "vp8",
            string s when s.Contains("av1") => "av1",
            string s when s.Contains("mpeg4") => "mpeg4",
            string s when s.Contains("mpeg2") => "mpeg2",
            _ => vcodec,
        };
        bool isCodecCompatibleWithExt = ext switch
        {
            "mp4" or "mov" => vcodec == "h264" || vcodec == "hevc" || vcodec == "mpeg4",
            "mkv" => true,
            "webm" => vcodec == "vp8" || vcodec == "vp9" || vcodec == "av1",
            "avi" => vcodec == "mpeg4" || vcodec == "h264",
            _ => false,
        };

        // Check if both extension and codec are compatible
        if (!string.IsNullOrEmpty(vext) && vext == ext && isCodecCompatibleWithExt)
            return ("copy", null);
        if (isCodecCompatibleWithExt) return ("copy", null);

        return BuildVideoReencodeArgs(ext);
    }

    static (string codec, string? mux) BuildAudioReencodeArgs(string toExt)
    {
        var map = new Dictionary<string, (string encoder, string? mux)>
        {
            // pure audio
            ["mp3"] = ("libmp3lame", "mp3"),
            ["m4a"] = ("aac", "mp4"),
            ["wav"] = ("pcm_s16le", "wav"),
            ["flac"] = ("flac", "flac"),
            ["ogg"] = ("libvorbis", "ogg"),
            ["opus"] = ("libopus", "opus"),
            ["webm"] = ("libopus", "webm"),

            // video containers (no explicit mux, handled by ffmpeg container)
            ["mp4"] = ("aac", null),
            ["mkv"] = ("copy", null),
            ["avi"] = ("libmp3lame", null),
            ["mov"] = ("aac", null),
        };

        toExt = (toExt ?? "").ToLowerInvariant();
        if (!map.ContainsKey(toExt))
            return ("libmp3lame", "mp3");

        var (encoder, mux) = map[toExt];
        return (encoder, mux);
    }
    static (string codec, string mux) BuildVideoReencodeArgs(string toExt)
    {
        toExt = (toExt ?? "").ToLowerInvariant();
        return toExt switch
        {
            "mp4" or "mov" => ("libx264", "mp4"),
            "mkv" => ("libx264", "matroska"),
            "avi" => ("mpeg4", "avi"),
            "webm" => ("libvpx-vp9", "webm"), // webm prefers vp9/opus; here we re-encode both to be safe
            _ => ("libx264", "mp4"),
        };
    }
}

public class FfmpegProgress
{
    public string Type { get; set; } = "";
    public string Bitrate { get; set; } = "";
    public long TotalSize { get; set; }
    public TimeSpan OutTime { get; set; }
    public int DupFrames { get; set; }
    public int DropFrames { get; set; }
    public double Speed { get; set; }
    public bool IsEnd { get; set; } = false;

    public override string ToString()
    {
        return $"Type={Type}; Bitrate={Bitrate}; TotalSize={TotalSize}; OutTime={OutTime}; DupFrames={DupFrames}; DropFrames={DropFrames}; Speed={Speed}; IsEnd={IsEnd}";
    }

    public static FfmpegProgress Parse(string line)
    {
        FfmpegProgress ffmpegProgress = new();
        if (string.IsNullOrEmpty(line)) return ffmpegProgress;
        var parts = line.Split(["\r", "\n"], StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;
            string key = kv[0];
            string value = kv[1];
            switch (key)
            {
                case "bitrate":
                    ffmpegProgress.Bitrate = value;
                    break;
                case "total_size":
                    if (long.TryParse(value, out long size))
                        ffmpegProgress.TotalSize = size;
                    break;
                case "out_time":
                    if (TimeSpan.TryParse(value, out TimeSpan ts))
                        ffmpegProgress.OutTime = ts;
                    break;

                case "dup_frames":
                    if (int.TryParse(value, out int dup))
                        ffmpegProgress.DupFrames = dup;
                    break;
                case "drop_frames":
                    if (int.TryParse(value, out int drop))
                        ffmpegProgress.DropFrames = drop;
                    break;
                case "speed":
                    if (
                        double.TryParse(
                            value.Replace("x", ""),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double spd
                        )
                    )
                        ffmpegProgress.Speed = spd;
                    break;
                case "progress":
                    ffmpegProgress.IsEnd = value == "end";
                    break;
                default:
                    break;
            }
        }
        return ffmpegProgress;
    }
}