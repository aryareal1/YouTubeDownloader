using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Project
{
    public class Ffmpeg(string exePath)
    {
        /// <summary>
        /// Path to the ffmpeg executable
        /// </summary>
        public string ExePath => exePath;

        /// <summary>
        /// Checks if the ffmpeg executable is working by running a version check command.
        /// </summary>
        /// <returns><see langword="true"/> if the executable works, otherwise <see langword="false"/></returns>
        public async Task<bool> IsExecutableWorking()
        {
            try
            {
                string output = await Utils.RunExe(
                    exePath, "-version",
                    (sender, e) =>
                    {
                        if (string.IsNullOrEmpty(e.Data)) return;
                        Debug.WriteLine($"ffmpeg: {e.Data}");
                    }
                );
                return !string.IsNullOrEmpty(output);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes ffmpeg with specified arguments and captures its output.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="handleStd"></param>
        /// <returns></returns>
        public async Task<string> Exec(string arguments, DataReceivedEventHandler? handleStd = null)
        {
            return await Utils.RunExe(exePath, arguments, handleStd);
        }

        // === Static Methods ===
        public static async Task<Ffmpeg> Download(string savedirPath, IProgress<double>? progress = null)
        {
            string ffmpegUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.7z";
            string archivePath = Path.Combine(savedirPath, "ffmpeg.7z");
            string extractPath = Path.Combine(savedirPath, "ffmpeg");

            await Utils.DownloadFileAsync(ffmpegUrl, archivePath, progress);
            Directory.CreateDirectory(extractPath);

            using (var archive = ArchiveFactory.Open(archivePath))
            {
                using (var reader = archive.ExtractAllEntries())
                {
                    reader.WriteAllToDirectory(extractPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
            var subDirs = Directory.GetDirectories(extractPath);
            if (subDirs.Length == 1)
            {
                string innerDir = subDirs[0];
                foreach (var dir in Directory.GetDirectories(innerDir))
                    Directory.Move(dir, Path.Combine(extractPath, Path.GetFileName(dir)));

                foreach (var file in Directory.GetFiles(innerDir))
                    File.Move(file, Path.Combine(extractPath, Path.GetFileName(file)));

                Directory.Delete(innerDir, true);
            }

            File.Delete(archivePath);
            return new Ffmpeg(Path.Combine(extractPath, "bin", "ffmpeg.exe"));
        }

        // --- Args Builders ---
        public static (string codec, string? mux) CreateAudioArgs(JsonElement format, string ext)
        {
            // Sanitize ext
            ext = (ext ?? "").Trim().ToLowerInvariant();

            // Check if audio extension matches
            string aext = (
                JsonUtils.TryGetString(format, "audio_ext")
                ?? JsonUtils.TryGetString(format, "ext") ?? ""
            ).ToLowerInvariant();
            if (!string.IsNullOrEmpty(aext) && aext == ext)
                return ("copy", null);

            // Check audio codec
            string acodec = (JsonUtils.TryGetString(format, "acodec") ?? "none").ToLowerInvariant();
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

        public static (string codec, string? mux) CreateVideoArgs(JsonElement format, string ext)
        {
            // Sanitize ext
            ext = (ext ?? "").Trim().ToLowerInvariant();

            // Check if video extension matches
            string vext = (
                JsonUtils.TryGetString(format, "video_ext")
                ?? JsonUtils.TryGetString(format, "ext")
                ?? JsonUtils.TryGetString(format, "container") ?? ""
            ).ToLowerInvariant();
            vext = vext.ToLowerInvariant();

            // Check video codec
            string vcodec = (JsonUtils.TryGetString(format, "vcodec") ?? "none").ToLowerInvariant();
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
}
