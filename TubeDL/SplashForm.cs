using TubeDL.Tools;

namespace TubeDL;

public partial class SplashForm : Form
{
    /// <summary>
    /// Class contructor
    /// </summary>
    public SplashForm()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handles the <see cref="Form.Load"/> event for the splash screen.
    /// </summary>
    /// <param name="sender">The source of the event, typically the splash screen form.</param>
    /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
    private async void SplashForm_Load(object sender, EventArgs e)
    {
        // Mark as initializing and delay for visibility
        labelText.Text = "Initializing...";
        if (!Directory.Exists(Utils.tmpPath))
            Directory.CreateDirectory(Utils.tmpPath);
        await Task.Delay(500);

        // Check for internet connection
        while (!await Utils.IsInternetAvailable())
        {
            labelText.Text = "No internet connection.";
        }

        // Check dependencies
        labelText.Text = "Loading dependencies...";
        if (!Directory.Exists(Utils.depPath))
            Directory.CreateDirectory(Utils.depPath);

        string? ytDlpPath = await YtDlp.GetPath();
        string? ffmpegPath = await Ffmpeg.GetPath();

        // Download dependencies if not present
        if (ytDlpPath is null)
        {
            labelText.Text = "Loading dependencies...\nDownloading yt-dlp...";
            await YtDlp.DownloadLatest((received, total) =>
            {
                labelText.Text = $"Loading dependencies...\nDownloading yt-dlp... {received / (1024.0 * 1024.0):F2}/{total / (1024.0 * 1024.0):F2} MB";
            });
            ytDlpPath = YtDlp.localPath;
        }
        if (ffmpegPath is null)
        {
            labelText.Text = "Downloading ffmpeg...";
            await Ffmpeg.DownloadLatest((received, total) =>
            {
                labelText.Text = $"Loading dependencies...\nDownloading ffmpeg... {received / (1024.0 * 1024.0):F2}/{total / (1024.0 * 1024.0):F2} MB";
            });
            ffmpegPath = Ffmpeg.localPath;
        }

        labelText.Text = "Loading dependencies...";

        YtDlp ytDlp = new(ytDlpPath!);
        Ffmpeg ffmpeg = new(ffmpegPath!);

        // Check for updates
        if (await ytDlp.IsUpdateAvailable())
        {
            labelText.Text = "Loading dependencies...\nUpdating yt-dlp...";
            await YtDlp.DownloadLatest((received, total) =>
            {
                labelText.Text = $"Loading dependencies...\nUpdating yt-dlp... {received / (1024.0 * 1024.0):F2}/{total / (1024.0 * 1024.0):F2} MB";
            });
        }

        labelText.Text = "Loading dependencies...";

        // Validate dependencies
        if (!await ytDlp.IsValid())
        {
            labelText.Text = "Loading dependencies...\nRe-downloading yt-dlp...";
            await YtDlp.DownloadLatest((received, total) =>
            {
                labelText.Text = $"Loading dependencies...\nRe-downloading yt-dlp... {received / (1024.0 * 1024.0):F2}/{total / (1024.0 * 1024.0):F2} MB";
            });
            ytDlpPath = YtDlp.localPath;
            ytDlp = new(ytDlpPath!);
        }
        if (!await ffmpeg.IsValid())
        {
            labelText.Text = "Loading dependencies...\nRe-downloading ffmpeg...";
            await Ffmpeg.DownloadLatest((received, total) =>
            {
                labelText.Text = $"Loading dependencies...\nRe-downloading ffmpeg... {received / (1024.0 * 1024.0):F2}/{total / (1024.0 * 1024.0):F2} MB";
            });
            ffmpegPath = Ffmpeg.localPath;
            ffmpeg = new(ffmpegPath!);
        }

        // Launch main form
        labelText.Text = "Starting the app...";
        await Task.Delay(500);
        
        MainForm mainForm = new(ytDlp, ffmpeg);
        mainForm.FormClosed += (_, _) =>
        {
            Directory.Delete(Utils.tmpPath, true);
            this.Close();
        };
        mainForm.Show();
        this.Hide();
    }
}
