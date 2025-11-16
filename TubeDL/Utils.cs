using Microsoft.VisualBasic.Devices;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using TubeDL.Schema;
using TubeDL.Tools;

namespace TubeDL;

public class Utils
{
    // -- VARIABLES --

    /// <summary>
    /// Dependency folder path
    /// </summary>
    public static readonly string depPath = Path.Combine(AppContext.BaseDirectory, "dependencies");
    /// <summary>
    /// Temporary files folder path
    /// </summary>
    public static readonly string tmpPath = Path.Combine(AppContext.BaseDirectory, "tmp");

    /// <summary>
    /// Video format filter for SaveFileDialog
    /// </summary>
    public static readonly string VideoFormat =
        "MP4 File (*.mp4)|*.mp4|" +
        "MKV File (*.mkv)|*.mkv|" +
        "AVI File (*.avi)|*.avi|" +
        "MOV File (*.mov)|*.mov";
    /// <summary>
    /// Audio format filter for SaveFileDialog
    /// </summary>
    public static readonly string AudioFormat =
        "MP3 File (*.mp3)|*.mp3|" +
        "M4A File (*.m4a)|*.m4a|" +
        "WAV File (*.wav)|*.wav|" +
        "FLAC File (*.flac)|*.flac|" +
        "OGG File (*.ogg)|*.ogg|" +
        "OPUS FIle (*.opus)|*.opus";

    // -- HELPER METHODS --

    /// <summary>
    /// Checks if the internet is available by pinging a well-known server.
    /// </summary>
    /// <returns><see langword="true"/> if ping successfully, else <see langword="false"/></returns>
    public static async Task<bool> IsInternetAvailable()
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 2000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Runs an executable with specified arguments and captures its output.
    /// </summary>
    /// <param name="exePath">Path to the exe file to execute</param>
    /// <param name="arguments">Specify arguments for the command</param>
    /// <param name="handleStd">Optional handler for output and error returns</param>
    /// <returns>Output from begining to end (same as `ReadToEnd()`)</returns>
    public static async Task<string> Eval(string exePath, string arguments, DataReceivedEventHandler? handleStd = null)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();

        StringBuilder sb = new();
        DataReceivedEventHandler handler = (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            handleStd?.Invoke(sender, e);
            sb.AppendLine(e.Data);
        };
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.ErrorDataReceived += handler;
        process.OutputDataReceived += handler;

        await process.WaitForExitAsync();

        return sb.ToString();
    }

    /// <summary>
    /// Downloads a file from a URL with progress reporting.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="savePath"></param>
    /// <param name="progress"></param>
    public static async Task DownloadFileAsync(string url, string savePath, Action<long, long>? progress = null)
    {
        using HttpClient client = new();
        using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        using Stream input = await response.Content.ReadAsStreamAsync();
        Debug.WriteLine(savePath);
        using FileStream output = File.Create(savePath);
        var buffer = new byte[81920];
        long totalRead = 0;
        long totalBytes = response.Content.Headers.ContentLength ?? -1;
        int read;

        while ((read = await input.ReadAsync(buffer)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, read));
            totalRead += read;
            if (totalBytes > 0)
                progress?.Invoke(totalRead, totalBytes);
        }
    }

    /// <summary>
    /// Copies data from source stream to destination stream with pause and cancel support.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="controller"></param>
    /// <param name="bufferSize"></param>
    /// <returns></returns>
    private static async Task CopyStreamWithControl(
        Stream source,
        Stream destination,
        DownloadController controller,
        int bufferSize = 81920
    )
    {
        byte[] buffer = new byte[bufferSize];
        int bytesRead;

        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await controller.CheckPauseAndCancel();
            await destination.WriteAsync(buffer, 0, bytesRead);
            await destination.FlushAsync();
        }
    }

    // -- DOWNLOADER METHODS --

    /// <summary>
    /// Downloads video and/or audio using yt-dlp and ffmpeg with pause and cancel support.
    /// </summary>
    /// <param name="ytDlp"></param>
    /// <param name="ffmpeg"></param>
    /// <param name="metadata"></param>
    /// <param name="savePath"></param>
    /// <param name="videoFormat"></param>
    /// <param name="audioFormat"></param>
    /// <param name="controller"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public static async Task Download(
        YtDlp ytDlp, Ffmpeg ffmpeg,
        YoutubeMetadata metadata,
        string savePath,
        Schema.Format? videoFormat,
        Schema.Format? audioFormat,
        DownloadController? controller = null,
        Action<YtDlpProgress?, FfmpegProgress?>? progress = null
    )
    {
        // Validate formats
        if (audioFormat == null && videoFormat == null)
            throw new ArgumentException("Both audioFormat and videoFormat cannot be null");

        // Handle muxing if both formats are provided
        if (audioFormat != null && videoFormat != null)
        {

            Process? psYtDlp = null;
            Process? psFfmpeg = null;
            NamedPipeServerStream? progressPipe = null;
            string? tmpFile = null;
            string? tempOutputFile = null;
            string? audioTempPath = null;

            try
            {
                controller ??= new DownloadController();

                // Check for pause/cancel
                await controller.CheckPauseAndCancel();

                // Create temporary info file
                Debug.WriteLine("(debug) Creating temporary info file...");
                tmpFile = Path.Combine(tmpPath, $"{Guid.NewGuid()}.json");
                await File.WriteAllTextAsync(tmpFile, metadata._RawJson);

                // Create temporary output file untuk support pause/resume
                string ext = Path.GetExtension(savePath)[1..];
                tempOutputFile = Path.ChangeExtension(savePath, $"partial.{ext}");

                // Download audio temporarily
                audioTempPath = Path.Combine(tmpPath, $"{Guid.NewGuid()}.tmp.{ext}");
                await DownloadStandalone(
                    ytDlp, ffmpeg,
                    tmpFile,
                    audioTempPath,
                    audioFormat,
                    controller,
                    progress
                );


                // Create progress named pipe
                Debug.WriteLine("(debug) Creating progress named pipe...");
                string progressPipeName = $"yt-dlp-progress-{Guid.NewGuid()}";
                progressPipe = new NamedPipeServerStream(
                    progressPipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous
                );

                // Build and start ffmpeg process
                Debug.WriteLine("(debug) Starting ffmpeg process...");
                (string acodec, string? _) = Ffmpeg.CreateAudioArgs(audioFormat, ext);
                (string vcodec, string? mux) = Ffmpeg.CreateVideoArgs(videoFormat, ext);
                Debug.WriteLine($"vcodec={vcodec}; acodec={acodec}; mux={mux ?? "null"}");

                psFfmpeg = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpeg.ExePath,
                        Arguments = $"-i pipe: -i {audioTempPath} -progress \\\\.\\pipe\\{progressPipeName} " +
                            $"-c:v {vcodec} -c:a {acodec} " +
                            (mux != null ? $"-f {mux} " : "") + $"\"{tempOutputFile}\"",
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

                await controller.CheckPauseAndCancel();

                // Run yt-dlp process
                Debug.WriteLine("(debug) Starting yt-dlp process...");
                psYtDlp = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ytDlp.Path,
                        Arguments = $"-f {videoFormat.FormatId} --load-info-json {tmpFile} -o -",
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
                        await controller.CheckPauseAndCancel();

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

                // Pipe yt-dlp output to ffmpeg input dengan pause/cancel support
                Debug.WriteLine("(debug) Piping yt-dlp output to ffmpeg input...");
                await CopyStreamWithControl(
                    psYtDlp.StandardOutput.BaseStream,
                    psFfmpeg.StandardInput.BaseStream,
                    controller
                );

                // Cleanup
                Debug.WriteLine("(debug) Waiting for yt-dlp to exit...");
                await psYtDlp.WaitForExitAsync();
                psFfmpeg.StandardInput.Close();
                Debug.WriteLine("(debug) Waiting for ffmpeg to exit...");
                await psFfmpeg.WaitForExitAsync();
                await progressTask;

                // Move file dari temporary ke final jika berhasil
                if (File.Exists(tempOutputFile))
                {
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }
                    File.Move(tempOutputFile, savePath);
                }

                Debug.WriteLine("(debug) Download completed successfully");
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("(debug) Download cancelled by user");

                // Kill processes kalau masih running
                try { psYtDlp?.Kill(); } catch { }
                try { psFfmpeg?.Kill(); } catch { }

                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"(debug) Download failed: {ex.Message}");
                throw new Exception("Failed to download: " + ex.Message);
            }
            finally
            {
                // Cleanup resources
                Debug.WriteLine("(debug) Cleaning up resources...");

                progressPipe?.Dispose();
                psYtDlp?.Dispose();
                psFfmpeg?.Dispose();

                // Delete temporary files
                try
                {
                    if (tmpFile != null && File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }
                    if (audioTempPath != null && File.Exists(audioTempPath))
                    {
                        File.Delete(audioTempPath);
                    }

                    // Remove partial file if cancelled or error
                    if (tempOutputFile != null && File.Exists(tempOutputFile) && controller?.IsCancelled == true)
                    {
                        File.Delete(tempOutputFile);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"(debug) Cleanup error: {ex.Message}");
                }
            }
        }
        else
        {
            // Handle single format download
            Format format = videoFormat ?? audioFormat!;
            await DownloadStandalone(
                ytDlp, ffmpeg,
                metadata,
                savePath,
                format,
                controller,
                progress
            );
        }
    }

    /// <summary>
    /// Downloads a video or audio using yt-dlp and ffmpeg with pause and cancel support.
    /// </summary>
    /// <param name="ytDlp"></param>
    /// <param name="ffmpeg"></param>
    /// <param name="metadata"></param>
    /// <param name="savePath"></param>
    /// <param name="format"></param>
    /// <param name="controller"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public static async Task DownloadStandalone(
        YtDlp ytDlp, Ffmpeg ffmpeg,
        object metadata,
        string savePath,
        Schema.Format format,
        DownloadController? controller = null,
        Action<YtDlpProgress?, FfmpegProgress?>? progress = null
    )
    {
        Process? psYtDlp = null;
        Process? psFfmpeg = null;
        NamedPipeServerStream? progressPipe = null;
        string? tmpFile = null;
        string? tempOutputFile = null;

        bool isAudio = format.Abr != 0.0 && format.Vbr == 0.0;

        try
        {
            controller ??= new DownloadController();

            // Check for pause/cancel
            await controller.CheckPauseAndCancel();

            // Create temporary info file
            string metaPath;
            if (metadata is string path)
                metaPath = path;
            else if (metadata is YoutubeMetadata m)
            {
                Debug.WriteLine("(debug) Creating temporary info file...");
                tmpFile = Path.Combine(tmpPath, $"{Guid.NewGuid()}.json");
                await File.WriteAllTextAsync(tmpFile, m._RawJson);
                metaPath = tmpFile;
            }
            else
                throw new ArgumentException("metadata must be string or Metadata");

            // Create temporary output file untuk support pause/resume
            string ext = Path.GetExtension(savePath)[1..];
            tempOutputFile = Path.ChangeExtension(savePath, $"partial.{ext}");

            // Create progress named pipe
            Debug.WriteLine("(debug) Creating progress named pipe...");
            string progressPipeName = $"yt-dlp-progress-{Guid.NewGuid()}";
            progressPipe = new NamedPipeServerStream(
                progressPipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous
            );

            await controller.CheckPauseAndCancel();

            // Build and start ffmpeg process
            Debug.WriteLine("(debug) Starting ffmpeg process...");
            (string codec, string? mux) = isAudio
                ? Ffmpeg.CreateAudioArgs(format, ext)
                : Ffmpeg.CreateVideoArgs(format, ext);
            Debug.WriteLine($"codec={codec}; mux={mux}");

            psFfmpeg = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpeg.ExePath,
                    Arguments = $"-i pipe: -progress \\\\.\\pipe\\{progressPipeName} " +
                        (isAudio ? $"-c:a {codec} -vn " : $"-c:v {codec} ") +
                        (mux != null ? $"-f {mux} " : "") + $"\"{tempOutputFile}\"",
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

            await controller.CheckPauseAndCancel();

            // Run yt-dlp process
            Debug.WriteLine("(debug) Starting yt-dlp process...");
            psYtDlp = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytDlp.Path,
                    Arguments = $"-f {format.FormatId} --load-info-json {metaPath} -o -",
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
                    prog.Type = isAudio ? "audio" : "video";
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
                    await controller.CheckPauseAndCancel();

                    string line = await reader.ReadLineAsync() ?? "";
                    string key = line.Split("=", 2)[0];

                    lines += line + "\n";

                    if (key == "progress")
                    {
                        var prog = FfmpegProgress.Parse(lines.Trim());
                        lines = "";
                        prog.Type = isAudio ? "audio" : "video";
                        progress?.Invoke(null, prog);
                    }
                }
            });

            // Pipe yt-dlp output to ffmpeg input dengan pause/cancel support
            Debug.WriteLine("(debug) Piping yt-dlp output to ffmpeg input...");
            await CopyStreamWithControl(
                psYtDlp.StandardOutput.BaseStream,
                psFfmpeg.StandardInput.BaseStream,
                controller
            );

            // Cleanup
            Debug.WriteLine("(debug) Waiting for yt-dlp to exit...");
            await psYtDlp.WaitForExitAsync();
            psFfmpeg.StandardInput.Close();
            Debug.WriteLine("(debug) Waiting for ffmpeg to exit...");
            await psFfmpeg.WaitForExitAsync();
            await progressTask;

            // Move file dari temporary ke final jika berhasil
            if (File.Exists(tempOutputFile))
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
                File.Move(tempOutputFile, savePath);
            }

            Debug.WriteLine("(debug) Download completed successfully");
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("(debug) Download cancelled by user");

            // Kill processes kalau masih running
            try { psYtDlp?.Kill(); } catch { }
            try { psFfmpeg?.Kill(); } catch { }

            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"(debug) Download failed: {ex.Message}");
            throw new Exception("Failed to download: " + ex.Message);
        }
        finally
        {
            // Cleanup resources
            Debug.WriteLine("(debug) Cleaning up resources...");

            progressPipe?.Dispose();
            psYtDlp?.Dispose();
            psFfmpeg?.Dispose();

            // Delete temporary files
            try
            {
                if (tmpFile != null && File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                }

                // Hapus partial file jika dibatalkan atau error
                if (tempOutputFile != null && File.Exists(tempOutputFile) && controller?.IsCancelled == true)
                {
                    File.Delete(tempOutputFile);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"(debug) Cleanup error: {ex.Message}");
            }
        }
    }
}
