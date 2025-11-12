using System.Diagnostics;

namespace Project
{
    public partial class StartForm : Form
    {
        /// <summary>
        /// Class contructor
        /// </summary>
        public StartForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for when the Start Form is loaded
        /// </summary>
        /// <remarks>
        /// Setting up and checking the tools, if isn't available then
        /// download or if it corrupt then redownload
        /// </remarks>
        private async void StartForm_Load(object sender, EventArgs e)
        {
            // Mark as initializing and delay it 1 sec
            labelText.Text = "Initializing...";
            await Task.Delay(1000);

            // Check for internet connection
            while (!await Utils.IsInternetAvailable())
            {
                labelText.Text = "No internet connection.\nPlease connect to the internet!";
            }

            // Check for ffmpeg
            labelText.Text = "Checking ffmpeg...";
            await Task.Delay(500);

            var ffmpeg = await Utils.GetFfmpeg();
            if (ffmpeg == null)
            {
                // Download ffmpeg if it is not found
                labelText.Text = "FFmpeg not found.\nDownloading...";
                var progress = new Progress<double>(value =>
                {
                    labelText.Text = $"FFmpeg not found.\nDownloading... ({(value / 100):P1})";
                });
                ffmpeg = await Ffmpeg.Download(
                    AppDomain.CurrentDomain.BaseDirectory,
                    progress
                );

            }
            else if (!await ffmpeg.IsExecutableWorking())
            {
                // Re-download ffmpeg if it is corrupted
                labelText.Text = "FFmpeg is corrupted.\nRe-downloading...";
                var progress = new Progress<double>(value =>
                {
                    labelText.Text = $"FFmpeg is corrupted.\nRe-downloading... ({(value / 100):P1})";
                });
                ffmpeg = await Ffmpeg.Download(
                    AppDomain.CurrentDomain.BaseDirectory,
                    progress
                );
            }
            Debug.WriteLine($"FFmpeg is ready. ({ffmpeg!.ExePath})");

            await Task.Yield();

            // Check for yt-dlp
            labelText.Text = "Checking yt-dlp...";
            await Task.Delay(500);

            var ytDlp = await Utils.GetYtDlp(ffmpeg);
            if (ytDlp == null)
            {
                // Download yt-dlp if it is not found
                labelText.Text = "yt-dlp not found.\nDownloading...";
                var progress = new Progress<double>(value =>
                {
                    labelText.Text = $"yt-dlp not found.\nDownloading... ({(value / 100):P1})";
                });
                ytDlp = await YtDlp.Download(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe"),
                    ffmpeg, progress
                );

            }
            else if (!await ytDlp.IsExecutableWorking())
            {
                // Re-download yt-dlp if it is corrupted
                labelText.Text = "yt-dlp is corrupted.\nRe-downloading...";
                var progress = new Progress<double>(value =>
                {
                    labelText.Text = $"yt-dlp is corrupted.\nRe-downloading... ({(value / 100):P1})";
                });
                ytDlp = await YtDlp.Download(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe"),
                    ffmpeg, progress
                );
            }
            Debug.WriteLine($"yt-dlp is ready. ({ytDlp!.ExePath})");

            await Task.Yield();

            // Mark as starting and delay it 2 sec
            labelText.Text = "Starting the app...";
            await Task.Delay(2000);

            // Show main form
            var main = new MainForm(ytDlp);
            main.FormClosed += (s, args) => this.Close();
            main.Show();
            this.Hide();
        }
    }
}
