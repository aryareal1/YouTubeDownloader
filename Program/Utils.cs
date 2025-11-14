using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;

namespace Project
{
    public class Utils
    {
        public static readonly string VideoFormat =
            "MP4 File (*.mp4)|*.mp4|" +
            "MKV File (*.mkv)|*.mkv|" +
            "AVI File (*.avi)|*.avi|" +
            "MOV File (*.mov)|*.mov";
        public static readonly string AudioFormat =
            "MP3 File (*.mp3)|*.mp3|" +
            "M4A File (*.m4a)|*.m4a|" +
            "WAV File (*.wav)|*.wav|" +
            "FLAC File (*.flac)|*.flac|" +
            "OGG File (*.ogg)|*.ogg|" +
            "OPUS FIle (*.opus)|*.opus";

        /// <summary>
        /// Downloads a file from the specified URL to the given save path with optional progress reporting.
        /// </summary>
        /// <param name="url">Source file URL to download</param>
        /// <param name="savePath">Path to save the file in</param>
        /// <param name="progress">Optional handler for progression</param>
        public static async Task DownloadFileAsync(string url, string savePath, IProgress<double>? progress = null)
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
                    progress?.Report((double)totalRead / totalBytes * 100);
            }
        }

        /// <summary>
        /// Runs an executable with specified arguments and captures its output.
        /// </summary>
        /// <param name="exePath">Path to the exe file to execute</param>
        /// <param name="arguments">Specify arguments for the command</param>
        /// <param name="handleStd">Optional handler for output and error returns</param>
        /// <returns>Output from begining to end (same as `ReadToEnd()`)</returns>
        public static async Task<string> RunExe(string exePath, string arguments, DataReceivedEventHandler? handleStd = null)
        {
            ProcessStartInfo psi = new()
            {
                FileName = exePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi)!;

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
        /// Asynchronously retrieves an instance of the <see cref="Ffmpeg"/> class if the ffmpeg executable is found.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="Ffmpeg"/> initialized with the path to the ffmpeg executable if found;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        public static async Task<Ffmpeg?> GetFfmpeg()
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "bin", "ffmpeg.exe");
            if (File.Exists(exePath))
                return new Ffmpeg(exePath);
            try
            {
                string output = (await RunExe("cmd.exe", "/c where ffmpeg")).Trim();
                if (
                    string.IsNullOrWhiteSpace(output) ||
                    output == "INFO: Could not find files for the given pattern(s)."
                )
                    return null;
                exePath = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0];
                return new Ffmpeg(exePath);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Asynchronously retrieves an instance of the <see cref="YtDlp"/> class if the yt-dlp executable is found.
        /// </summary>
        /// <remarks>
        /// This method first checks for the presence of the yt-dlp executable in the
        /// application's base directory. If not found, it attempts to locate the executable using the system's PATH
        /// environment variable.
        /// </remarks>
        /// <returns>
        /// An instance of <see cref="YtDlp"/> initialized with the path to the yt-dlp executable if found;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        public static async Task<YtDlp?> GetYtDlp(Ffmpeg ffmpeg)
        {
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
            if (File.Exists(exePath))
                return new YtDlp(exePath, ffmpeg);

            try
            {
                string output = (await RunExe("cmd.exe", "/c where yt-dlp")).Trim();
                if (
                    string.IsNullOrWhiteSpace(output) ||
                    output == "INFO: Could not find files for the given pattern(s)."
                )
                    return null;

                exePath = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0];
                return new YtDlp(exePath, ffmpeg);
            }
            catch
            {
                return null;
            }
        }
    }

    public class Formatter
    {
        /// <summary>
        /// Formats a number into a metric representation (e.g., 1.5k, 2.3M).
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <param name="fractionDigits">How many decimal point to include</param>
        /// <param name="separator">A string to separate between the value and the types ("2.5{separator}M")</param>
        /// <param name="divider">The amount to divide the value</param>
        /// <param name="types">Type for the format</param>
        /// <returns>A string with number formatted with the types</returns>
        public static string MetricNumber(
            double value,
            int fractionDigits = 1,
            string separator = "",
            double divider = 1000,
            string[]? types = null
        )
        {
            types ??= ["", "k", "M", "G", "T", "P", "E", "Z", "Y"];

            if (value == 0)
                return "0";

            int index = (int)Math.Floor(Math.Log(Math.Abs(value), divider));
            index = Math.Clamp(index, 0, types.Length - 1);

            double scaled = value / Math.Pow(divider, index);
            return $"{scaled.ToString($"F{fractionDigits}")}{separator}{types[index]}";
        }

        /// <summary>
        /// Convert milliseconds into a human readeble relative duration
        /// </summary>
        /// <param name="milliseconds">Duration long in milliseconds</param>
        public static string RelativeDuration(long milliseconds)
        {
            if (milliseconds < 1000)
                return $"{milliseconds} milidetik";

            long seconds = milliseconds / 1000;
            if (seconds < 60)
                return $"{seconds} detik";

            long minutes = seconds / 60;
            if (minutes < 60)
                return $"{minutes} menit";

            long hours = minutes / 60;
            if (hours < 24)
                return $"{hours} jam";

            long days = hours / 24;
            if (days < 30)
                return $"{days} hari";

            long months = days / 30;
            if (months < 12)
                return $"{months} bulan";

            long years = months / 12;
            return $"{years} tahun";
        }
    }

    public class JsonUtils
    {
        /// <summary>
        /// Tries to get a string property from a JsonElement.
        /// </summary>
        /// <param name="el"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string? TryGetString(JsonElement el, string prop)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var p) && p.ValueKind == JsonValueKind.String)
                return p.GetString();
            return null;
        }
    }
}