using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project
{
    /// <summary>
    /// Wrapper class for yt-dlp executable
    /// </summary>
    /// <param name="exePath">Path to the exe file</param>
    public class YtDlp(string exePath, Ffmpeg ffmpeg)
    {
        /// <summary>
        /// Path to the yt-dlp executable
        /// </summary>
        public string ExePath => exePath;

        /// <summary>
        /// Checks if the yt-dlp executable is working by running a version check command.
        /// </summary>
        /// <returns><see langword="true"/> if the executable works, otherwise <see langword="false"/></returns>
        public async Task<bool> IsExecutableWorking()
        {
            try
            {
                string output = await Utils.RunExe(
                    exePath, "--version",
                    (sender, e) =>
                    {
                        if (
                            string.IsNullOrEmpty(e.Data) ||
                            Regex.IsMatch(e.Data, @"/^\d{4}\.\d{2}\.\d{2}(?:-[\w.-]+)?$/")
                        ) return;
                        Debug.WriteLine($"yt-dlp: {e.Data}");
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
        /// Gets JSON information about a media URL using yt-dlp.
        /// </summary>
        /// <param name="url">Content url to fetch</param>
        /// <returns>An instance of <see cref="JsonDocument"/> if output correctly, otherwise <see langword="null"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<JsonDocument?> GetJsonInfo(string url)
        {
            try
            {
                string output = await Utils.RunExe(
                    exePath, $"-v -J \"{url}\"",
                    (sender, e) =>
                    {
                        if (string.IsNullOrEmpty(e.Data)) return;
                        Debug.WriteLine($"yt-dlp: {e.Data}");
                    }
                );

                if (!string.IsNullOrEmpty(output))
                    return JsonDocument.Parse(output.Split(["\r", "\n"], StringSplitOptions.RemoveEmptyEntries).Last());
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get JSON info: " + ex.Message);
            }
        }

        /// <summary>
        /// Downloads audio using yt-dlp and ffmpeg with progress reporting.
        /// </summary>
        /// <param name="jsonInfo"></param>
        /// <param name="savePath"></param>
        /// <param name="audioFormat"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DownloadAudio(
            JsonDocument jsonInfo,
            string savePath,
            JsonElement audioFormat,
            Action<YtDlpProgress?, FfmpegProgress?>? progress = null
        )
        {
            try
            {
                // Create temporary info file
                Debug.WriteLine("(debug) Creating temporary info file...");
                string tmpFile = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    $"yt-dlp-info-{Guid.NewGuid()}.json"
                );
                await File.WriteAllTextAsync(
                    tmpFile,
                    jsonInfo.RootElement.GetRawText()
                );
                // Create progress named pipe
                Debug.WriteLine("(debug) Creating progress named pipe...");
                string progressPipeName = $"yt-dlp-progress-{Guid.NewGuid()}";
                using var progressPipe = new NamedPipeServerStream(
                    progressPipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous
                );

                // Build and start ffmpeg process to handle audio encoding
                Debug.WriteLine("(debug) Starting ffmpeg process...");
                string ext = Path.GetExtension(savePath)[1..];
                (string codec, string? mux) = Ffmpeg.CreateAudioArgs(audioFormat, ext);
                Debug.WriteLine("Codec: " + codec + ", Mux: " + (mux ?? "null"));
                var psFfmpeg = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpeg.ExePath,
                        Arguments = $"-i pipe: -progress \\\\.\\pipe\\{progressPipeName} -c:a {codec} -vn " + (mux != null ? $"-f {mux} " : "") + $"\"{savePath}\"",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                psFfmpeg.Start();
                psFfmpeg.ErrorDataReceived += (sender, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;
                    Debug.WriteLine("(ffmpeg) " + e.Data);
                };
                psFfmpeg.BeginErrorReadLine();

                // Wait for progress pipe connection
                Debug.WriteLine("(debug) Waiting for progress pipe connection...");
                await progressPipe.WaitForConnectionAsync();

                // Run yt-dlp process to download audio
                Debug.WriteLine("(debug) Starting yt-dlp process...");
                var psYtDlp = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = $"-f {audioFormat.GetProperty("format_id").GetString()} --load-info-json {tmpFile} -o -",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                psYtDlp.Start();
                psYtDlp.BeginErrorReadLine();
                psYtDlp.ErrorDataReceived += (sender, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;
                    if (e.Data.StartsWith("[download]"))
                    {
                        var prog = YtDlpProgress.Parse(e.Data);
                        if (prog == null) return;
                        prog.Type = "audio";
                        progress?.Invoke(prog, null);
                    }
                };

                // Read progress from pipe
                var progressTask = Task.Run(async () =>
                {
                    Debug.WriteLine("(debug) Reading progress from pipe...");

                    string lines = "";
                    using var reader = new StreamReader(progressPipe);
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync() ?? "";
                        string key = line.Split("=", 2)[0];

                        lines += line + "\n";

                        if (key == "progress")
                        {
                            var prog = FfmpegProgress.Parse(lines.Trim());
                            lines = "";
                            prog.Type = "audio";
                            progress?.Invoke(null, prog);
                        }
                    }
                });

                // Pipe yt-dlp output to ffmpeg input
                Debug.WriteLine("(debug) Piping yt-dlp output to ffmpeg input...");
                await psYtDlp.StandardOutput.BaseStream.CopyToAsync(psFfmpeg.StandardInput.BaseStream);

                // Cleanup
                Debug.WriteLine("(debug) Waiting for yt-dlp to exit...");
                await psYtDlp.WaitForExitAsync();
                psFfmpeg.StandardInput.Close();
                Debug.WriteLine("(debug) Waiting for ffmpeg to exit...");
                await psFfmpeg.WaitForExitAsync();
                await progressTask;

                // Delete temporary info file
                Debug.WriteLine("(debug) Cleaning up temporary files...");
                File.Delete(tmpFile);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to download audio: " + ex.Message);
            }
        }
        /// <summary>
        /// Downloads video using yt-dlp and ffmpeg with progress reporting.
        /// </summary>
        /// <param name="jsonInfo"></param>
        /// <param name="savePath"></param>
        /// <param name="videoFormat"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DownloadVideo(
            JsonDocument jsonInfo,
            string savePath,
            JsonElement videoFormat,
            Action<YtDlpProgress?, FfmpegProgress?>? progress = null
        )
        {
            try
            {
                // Create temporary info file
                Debug.WriteLine("(debug) Creating temporary info file...");
                string tmpFile = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    $"yt-dlp-info-{Guid.NewGuid()}.json"
                );
                await File.WriteAllTextAsync(
                    tmpFile,
                    jsonInfo.RootElement.GetRawText()
                );
                // Create progress named pipe
                Debug.WriteLine("(debug) Creating progress named pipe...");
                string progressPipeName = $"yt-dlp-progress-{Guid.NewGuid()}";
                using var progressPipe = new NamedPipeServerStream(
                    progressPipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous
                );

                // Build and start ffmpeg process to handle audio encoding
                Debug.WriteLine("(debug) Starting ffmpeg process...");
                string ext = Path.GetExtension(savePath)[1..];
                (string codec, string? mux) = Ffmpeg.CreateVideoArgs(videoFormat, ext);
                Debug.WriteLine("Codec: " + codec + ", Mux: " + (mux ?? "null"));
                var psFfmpeg = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpeg.ExePath,
                        Arguments = $"-i pipe: -progress \\\\.\\pipe\\{progressPipeName} -c:v {codec} " + (mux != null ? $"-f {mux} " : "") + $"\"{savePath}\"",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                psFfmpeg.Start();
                psFfmpeg.ErrorDataReceived += (sender, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;
                };
                psFfmpeg.BeginErrorReadLine();

                // Wait for progress pipe connection
                Debug.WriteLine("(debug) Waiting for progress pipe connection...");
                await progressPipe.WaitForConnectionAsync();

                // Run yt-dlp process to download video
                Debug.WriteLine("(debug) Starting yt-dlp process...");
                var psYtDlp = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = $"-f {videoFormat.GetProperty("format_id").GetString()} --load-info-json {tmpFile} -o -",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                psYtDlp.Start();
                psYtDlp.BeginErrorReadLine();
                psYtDlp.ErrorDataReceived += (sender, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;
                    if (e.Data.StartsWith("[download]"))
                    {
                        var prog = YtDlpProgress.Parse(e.Data);
                        if (prog == null) return;
                        prog.Type = "video";
                        progress?.Invoke(prog, null);
                    }
                };

                // Read progress from pipe
                var progressTask = Task.Run(async () =>
                {
                    Debug.WriteLine("(debug) Reading progress from pipe...");

                    string lines = "";
                    using var reader = new StreamReader(progressPipe);
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync() ?? "";
                        string key = line.Split("=", 2)[0];

                        lines += line + "\n";

                        if (key == "progress")
                        {
                            var prog = FfmpegProgress.Parse(lines.Trim());
                            lines = "";
                            prog.Type = "video";
                            progress?.Invoke(null, prog);
                        }
                    }
                });

                // Pipe yt-dlp output to ffmpeg input
                Debug.WriteLine("(debug) Piping yt-dlp output to ffmpeg input...");
                await psYtDlp.StandardOutput.BaseStream.CopyToAsync(psFfmpeg.StandardInput.BaseStream);

                // Cleanup
                Debug.WriteLine("(debug) Waiting for yt-dlp to exit...");
                await psYtDlp.WaitForExitAsync();
                psFfmpeg.StandardInput.Close();
                Debug.WriteLine("(debug) Waiting for ffmpeg to exit...");
                await psFfmpeg.WaitForExitAsync();
                await progressTask;

                // Delete temporary info file
                Debug.WriteLine("(debug) Cleaning up temporary files...");
                File.Delete(tmpFile);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to download video: " + ex.Message);
            }
        }
        public async Task Download(
            JsonDocument jsonInfo,
            string savePath,
            JsonElement? videoFormat = null,
            JsonElement? audioFormat = null,
            Action<YtDlpProgress?, FfmpegProgress?>? progress = null
        )
        {
            // Download both audio and video and mux them together
            if (audioFormat != null && videoFormat != null)
            {
                try
                {
                    // Create temporary info file
                    Debug.WriteLine("(debug) Creating temporary info file...");
                    string tmpFile = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        $"yt-dlp-info-{Guid.NewGuid()}.json"
                    );
                    await File.WriteAllTextAsync(
                        tmpFile,
                        jsonInfo.RootElement.GetRawText()
                    );

                    // Create progress named pipe
                    Debug.WriteLine("(debug) Creating progress named pipe...");
                    string progressPipeName = $"yt-dlp-progress-{Guid.NewGuid()}";
                    using var progressPipe = new NamedPipeServerStream(
                        progressPipeName,
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous
                    );

                    string ext = Path.GetExtension(savePath)[1..];

                    // Download audio as temporary file
                    Debug.WriteLine("(debug) Downloading audio as temporary file...");
                    string tmpAudioFile = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        $"yt-dlp-audio-{Guid.NewGuid()}.tmp.{ext}"
                    );
                    await DownloadAudio(
                        jsonInfo,
                        tmpAudioFile,
                        audioFormat.Value,
                        (dlProg, ffmProg) => progress?.Invoke(dlProg, ffmProg)
                    );

                    // Build and start ffmpeg process to handle audio encoding
                    Debug.WriteLine("(debug) Starting ffmpeg process...");
                    (string acodec, string? _) = Ffmpeg.CreateAudioArgs(audioFormat.Value, ext);
                    (string vcodec, string? mux) = Ffmpeg.CreateVideoArgs(videoFormat.Value, ext);
                    Debug.WriteLine($"vcodec={vcodec}; acodec={acodec}; mux={mux ?? "null"}");
                    var psFfmpeg = new Process()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = ffmpeg.ExePath,
                            Arguments = $"-i pipe: -i {tmpAudioFile} -progress \\\\.\\pipe\\{progressPipeName} -c:v {vcodec} -c:a {acodec} " + (mux != null ? $"-f {mux} " : "") + $"\"{savePath}\"",
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    psFfmpeg.Start();
                    psFfmpeg.ErrorDataReceived += (sender, e) =>
                    {
                        if (string.IsNullOrEmpty(e.Data)) return;
                    };
                    psFfmpeg.BeginErrorReadLine();

                    // Wait for progress pipe connection
                    Debug.WriteLine("(debug) Waiting for progress pipe connection...");
                    await progressPipe.WaitForConnectionAsync();

                    // Run yt-dlp process to download video
                    Debug.WriteLine("(debug) Starting yt-dlp process...");
                    var psYtDlp = new Process()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = $"-f {videoFormat?.GetProperty("format_id").GetString()} --load-info-json {tmpFile} -o -",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    psYtDlp.Start();
                    psYtDlp.BeginErrorReadLine();
                    psYtDlp.ErrorDataReceived += (sender, e) =>
                    {
                        if (string.IsNullOrEmpty(e.Data)) return;
                        if (e.Data.StartsWith("[download]"))
                        {
                            var prog = YtDlpProgress.Parse(e.Data);
                            if (prog == null) return;
                            prog.Type = "video";
                            progress?.Invoke(prog, null);
                        }
                    };

                    // Read progress from pipe
                    var progressTask = Task.Run(async () =>
                    {
                        Debug.WriteLine("(debug) Reading progress from pipe...");

                        string lines = "";
                        using var reader = new StreamReader(progressPipe);
                        while (!reader.EndOfStream)
                        {
                            string line = await reader.ReadLineAsync() ?? "";
                            string key = line.Split("=", 2)[0];

                            lines += line + "\n";

                            if (key == "progress")
                            {
                                var prog = FfmpegProgress.Parse(lines.Trim());
                                lines = "";
                                prog.Type = "video";
                                progress?.Invoke(null, prog);
                            }
                        }
                    });

                    // Pipe yt-dlp output to ffmpeg input
                    Debug.WriteLine("(debug) Piping yt-dlp output to ffmpeg input...");
                    await psYtDlp.StandardOutput.BaseStream.CopyToAsync(psFfmpeg.StandardInput.BaseStream);

                    // Cleanup
                    Debug.WriteLine("(debug) Waiting for yt-dlp to exit...");
                    await psYtDlp.WaitForExitAsync();
                    psFfmpeg.StandardInput.Close();
                    Debug.WriteLine("(debug) Waiting for ffmpeg to exit...");
                    await psFfmpeg.WaitForExitAsync();
                    await progressTask;

                    // Delete temporary info file
                    Debug.WriteLine("(debug) Cleaning up temporary files...");
                    File.Delete(tmpFile);
                    File.Delete(tmpAudioFile);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to download: " + ex.Message);
                }
            }
            // Download only audio
            else if (audioFormat != null)
                await DownloadAudio(jsonInfo, savePath, audioFormat.Value, progress);
            // Download only video
            else if (videoFormat != null)
                await DownloadVideo(jsonInfo, savePath, videoFormat.Value, progress);
            // Nothing provided
            else
                throw new ArgumentException("Either audioFormat or videoFormat must be provided.");
        }

        /// <summary>
        /// Executes yt-dlp with specified arguments and captures its output.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="handleStd"></param>
        /// <returns></returns>
        public async Task<string> Exec(string arguments, DataReceivedEventHandler? handleStd = null)
        {
            return await Utils.RunExe(exePath, arguments, handleStd);
        }

        // === Static Helpers ===
        public static async Task<YtDlp> Download(string savePath, Ffmpeg ffmpeg, IProgress<double>? progress = null)
        {
            string ytDlpUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
            await Utils.DownloadFileAsync(ytDlpUrl, savePath, progress);
            return new YtDlp(savePath, ffmpeg);
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
}
