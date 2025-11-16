using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using TubeDL.Schema;
using TubeDL.Tools;
using static System.Windows.Forms.DataFormats;

namespace TubeDL;

public partial class MainForm : Form
{
    readonly YtDlp ytDlp;
    readonly Ffmpeg ffmpeg;

    private YoutubeMetadata? _metadata;
    private DownloadController? _controller;
    private bool _isSearching = false;

    public MainForm(YtDlp ytDlp, Ffmpeg ffmpeg)
    {
        this.ytDlp = ytDlp;
        this.ffmpeg = ffmpeg;
        InitializeComponent();
    }

    /// <summary>
    /// Handles the <see cref="Form.Load"/> event for the main form, initializing the UI components and setting their
    /// default visibility states.
    /// </summary>
    /// <remarks>This method sets the initial visibility of various UI elements to ensure a clean and
    /// user-friendly interface upon application startup. It also configures the default directory for the save file
    /// dialog to the user's "Downloads" folder.</remarks>
    /// <param name="sender">The source of the event, typically the main form itself.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void MainForm_Load(object sender, EventArgs e)
    {
        // Clear any existing error messages
        labelError.Text = "";

        // Toggle visibility of UI components
        pictureBoxLoading.Visible = false;
        tabControlFormat.Visible = false;
        groupBoxFormat.Visible = false;

        pictureBoxThumbnail.Visible = false;
        labelTitle.Visible = false;
        labelDetails.Visible = false;
        labelAuthor.Visible = false;
        labelDur.Visible = false;
        buttonDownload.Visible = false;
        buttonCancel.Visible = false;
        buttonPause.Visible = false;
        tableLayoutPanelFormat.Visible = false;

        progressBarDownload.Visible = false;
        labelProgress.Visible = false;
        labelProgressSpeed.Visible = false;
        labelProgressSize.Visible = false;

        labelSelected.Visible = false;
        labelSelectedTitle.Visible = false;


        // Set default directory for save file dialog to user's "Downloads" folder
        saveFileDialog.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Download");
    }

    /// <summary>
    /// Handles the text changed event for the search input field.
    /// </summary>
    /// <remarks>This method updates the UI based on the current state of the input text. If the input is
    /// empty, a placeholder label is displayed. If the input contains a valid URL, the search button is enabled and
    /// styled for interaction. Otherwise, the search button is disabled, and an error message is displayed if the input
    /// is not empty.</remarks>
    /// <param name="sender">The source of the event, typically the input text box.</param>
    /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
    private void SearchInput_TextChanged(object sender, EventArgs e)
    {
        string text = textBoxInput.Text;
        // Toggle placeholder visibility based on input text
        if (text == "")
            labelPlaceholder.Visible = true;
        else
            labelPlaceholder.Visible = false;

        // Validate the URL format using a regular expression and update the search button state accordingly
        bool isValidUrl = Regex.IsMatch(text, YtDlp.regexUrl);
        if (isValidUrl)
        {
            buttonSearch.Enabled = true;
            buttonSearch.BackColor = Color.White;
            buttonSearch.Cursor = Cursors.Hand;
            this.AcceptButton = buttonSearch;
            labelError.Text = "";
        }
        else
        {
            buttonSearch.Enabled = false;
            buttonSearch.BackColor = Color.LightGray;
            buttonSearch.Cursor = Cursors.Default;
            this.AcceptButton = null;
            labelError.Text = text == "" ? "" : "URL tidak valid!";
        }
    }

    /// <summary>
    /// Handles the click event for the search placeholder.
    /// </summary>
    /// <remarks>This method sets focus to the input text box when the placeholder is clicked.</remarks>
    /// <param name="sender">The source of the event, typically a label. This parameter can be <see langword="null"/>.</param>
    /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
    private void SearchPlaceholder_Click(object? sender, EventArgs e)
    {
        textBoxInput.Focus();
    }

    // -- MAIN METHODS --

    /// <summary>
    /// Initiates a search operation based on the input provided in the search box.
    /// </summary>
    /// <remarks>This method performs the following actions: <list type="bullet"> <item>Prevents multiple
    /// concurrent searches by ensuring only one search is active at a time.</item> <item>Updates the UI to indicate the
    /// search is in progress, including disabling input fields and showing a loading indicator.</item> <item>Fetches
    /// metadata for the provided input using yt-dlp and updates the UI with the retrieved information, such as title,
    /// uploader, duration, and available formats.</item> <item>Populates the audio and video format lists for user
    /// selection.</item> <item>Restores the UI to its default state after the search is complete.</item> </list> The
    /// method is asynchronous and should be awaited to ensure the search operation completes before proceeding with
    /// other tasks.</remarks>
    /// <param name="sender">The source of the event that triggered the search.</param>
    /// <param name="e">The event data associated with the triggering event.</param>
    private async void Search(object sender, EventArgs e)
    {
        // Block the action when it's still searching
        if (_isSearching)
            return;

        // Enable the state of searching and disable the default accept button
        _isSearching = true;
        this.AcceptButton = null;

        // Styling the search box and button
        textBoxInput.ReadOnly = true;
        textBoxInput.Cursor = Cursors.No;
        textBoxInput.BackColor = Color.LightGray;
        buttonSearch.Cursor = Cursors.No;
        buttonSearch.BackColor = Color.LightGray;
        pictureBoxLoading.Visible = true;

        // Cleanup last content
        pictureBoxThumbnail.Image = null;
        pictureBoxThumbnail.Visible = false;
        labelTitle.Visible = false;
        labelDetails.Visible = false;
        labelAuthor.Visible = false;
        labelDur.Visible = false;
        buttonDownload.Visible = false;
        tabControlFormat.Visible = false;
        groupBoxFormat.Visible = false;
        labelSelected.Visible = false;
        labelSelectedTitle.Visible = false;
        listViewAudio.Items.Clear();
        listViewVideo.Items.Clear();

        // Search or fetch using yt-dlp and save it result to jsonInfo
        Debug.WriteLine("(yt-dlp) Searching for URL: " + textBoxInput.Text);
        _metadata = await ytDlp.GetMetadata(textBoxInput.Text);

        // Store the info to variables
        string title = _metadata.Title ?? "Unknown Title";
        string uploader = _metadata.Uploader ?? "Unknown Uploader";
        string duration = _metadata.DurationString ?? "x:xx";
        long viewCount = _metadata.ViewCount;
        long uploadAt = _metadata.Timestamp;
        string thumbnail = _metadata.Thumbnail?.Replace("_webp", "").Replace(".webp", ".jpg") ?? "";

        // Assign the info into the GUI components
        pictureBoxThumbnail.LoadAsync(thumbnail);
        labelTitle.Text = title;
        labelDetails.Text =
            $"{viewCount:N0} x ditonton • " +
            $"{Formatter.RelativeDuration((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - uploadAt) * 1000)} yg lalu";
        labelAuthor.Text = uploader;
        labelDur.Text = duration;

        // Parse the formats
        List<Schema.Format> formats = _metadata.Formats;
        var videoFormats = formats.Where(item => item.Vbr != 0.0 && item.Abr == 0.0).Reverse();
        var audioFormats = formats.Where(item => item.Vbr == 0.0 && item.Abr != 0.0).Reverse();

        // Loop through formats and add it to the listView (for selection)
        foreach (var item in audioFormats)
        {
            listViewAudio.Items.Add(new ListViewItem([
                item.Ext,
                $"{item.Abr:N0} kbps",
                $"{item.Filesize:N0} B"
            ])
            {
                Tag = item
            });
        }
        foreach (var item in videoFormats)
        {
            listViewVideo.Items.Add(new ListViewItem([
                item.Ext,
                item.FormatNote?.Split(", ")[0]!,
                $"{item.Vbr:N0} kbps",
                $"{item.Filesize:N0} B"
            ])
            {
                Tag = item
            });
        }

        // Showing the components
        pictureBoxThumbnail.Visible = true;
        labelTitle.Visible = true;
        labelDetails.Visible = true;
        labelAuthor.Visible = true;
        labelDur.Visible = true;
        buttonDownload.Visible = true;
        labelSelected.Visible = true;
        labelSelectedTitle.Visible = true;

        // Showing the formats to the listView and refresh the 
        listViewAudio.Size = new Size(listViewAudio.Width, tabControlFormat.Height - 40);
        tabControlFormat.Visible = true;
        tabControlFormat.SelectedIndex = 0;
        groupBoxFormat.Visible = true;
        UpdateFormatDetails(this, EventArgs.Empty);

        // Styling back the search box and button
        textBoxInput.ReadOnly = false;
        textBoxInput.Cursor = Cursors.IBeam;
        textBoxInput.BackColor = Color.White;
        buttonSearch.Cursor = Cursors.Hand;
        buttonSearch.BackColor = Color.White;
        pictureBoxLoading.Visible = false;

        // Disable the state of searching
        _isSearching = false;
        this.AcceptButton = buttonSearch;
    }

    /// <summary>
    /// Updates the format details displayed in the user interface based on the currently selected tab  and the selected
    /// items in the video or audio lists.
    /// </summary>
    /// <remarks>This method updates the "selected format" label, the state of the download button, and the 
    /// format details section depending on the user's selection. If no format is selected, the  format details section
    /// is hidden, and a placeholder message is displayed. If a format is  selected, detailed information about the
    /// format is shown, including properties such as  resolution, bitrate, codec, and file size for video formats, or
    /// bitrate, channels, codec,  and file size for audio formats.</remarks>
    /// <param name="sender">The source of the event that triggered the update.</param>
    /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
    private void UpdateFormatDetails(object sender, EventArgs e)
    {
        // Declare the selected tab index (Video=0; Audio=1)
        int tab = tabControlFormat.SelectedIndex;

        // Update the "selected format" label
        labelSelected.Text = listViewVideo.SelectedItems.Count == 1 && listViewAudio.SelectedItems.Count == 1
            ? "Video + Audio"
            : listViewVideo.SelectedItems.Count == 1
                ? "Video only"
                : listViewAudio.SelectedItems.Count == 1
                    ? "Audio only"
                    : "No format selected";

        if (labelSelected.Text == "No format selected")
        {
            buttonDownload.BackColor = Color.LightGray;
            buttonDownload.Enabled = false;
        }
        else
        {
            buttonDownload.BackColor = Color.White;
            buttonDownload.Enabled = true;
        }

        // Showing labelFormatDetailsEmpty if there's no selected format
        if ((tab == 0 ? listViewVideo.SelectedItems.Count : listViewAudio.SelectedItems.Count) == 0)
        {
            tableLayoutPanelFormat.Visible = false;
            labelFormatDetailsEmpty.Visible = true;
            groupBoxFormat.Text = "Format Details";
            return;
        }

        // Declaring and showing the format details if there's selected format
        Schema.Format format = (Schema.Format)(tab == 0 ? listViewVideo : listViewAudio).SelectedItems[0].Tag!;
        groupBoxFormat.Text = $"Format Details ({format.FormatId})";
        if (tab == 0)
        {
            labelFormatDetailsKey.Text =
                """
                File type
                Resolution
                Bitrate
                FPS
                Dyn. range
                Codec
                File size
                """;
            labelFormatDetails.Text =
                $"""
                : {format.Ext}
                : {format.Resolution}
                : {Formatter.MetricNumber(format.Vbr ?? 0, 1, " ", 1000, ["kbps", "Mbps", "Gbps", "Tbps", "Pbps"])}
                : {format.Fps}
                : {format.DynamicRange}
                : {format.Vcodec}
                : {Formatter.MetricNumber(format.Filesize ?? 0, 1, " ", 1024, ["B", "KB", "MB", "GB", "TB", "PB"])}
                """;
        }
        else if (tab == 1)
        {
            string audioChannels = format.AudioChannels == 2 ? "Stereo (2)" : "Mono (1)";
            labelFormatDetailsKey.Text =
                """
                File type
                Bitrate
                Channels
                Codec
                File size
                """;
            labelFormatDetails.Text =
                $"""
                : {format.Ext}
                : {Formatter.MetricNumber(format.Abr ?? 0, 1, " ", 1000, ["kbps", "Mbps", "Gbps", "Tbps", "Pbps"])}
                : {audioChannels}
                : {format.Acodec}
                : {Formatter.MetricNumber(format.Filesize ?? 0, 1, " ", 1024, ["B", "KB", "MB", "GB", "TB", "PB"])}
                """;
        }
        tableLayoutPanelFormat.Visible = true;
        labelFormatDetailsEmpty.Visible = false;
    }

    /// <summary>
    /// Handles the download process for selected audio and/or video formats, allowing the user to save the file to a
    /// specified location.
    /// </summary>
    /// <remarks>This method is triggered by a user action and performs the following steps: <list
    /// type="bullet"> <item>Validates that the required metadata and selected formats are available.</item>
    /// <item>Prompts the user to specify a save location and file name using a save file dialog.</item> <item>Initiates
    /// the download process, displaying progress updates to the user.</item> <item>Handles post-processing tasks, such
    /// as combining audio and video streams if necessary.</item> </list> The method updates the UI to reflect the
    /// download progress and provides feedback upon completion.</remarks>
    /// <param name="sender">The source of the event that triggered the download.</param>
    /// <param name="e">The event data associated with the triggering action.</param>
    private async void Download(object sender, EventArgs e)
    {
        // Block the action when the variable formats dosen't have any stored value in it
        if (_metadata is null) return;

        // Check if the user selected video and/or audio format
        bool hasAudio = listViewAudio.SelectedItems.Count != 0;
        bool hasVideo = listViewVideo.SelectedItems.Count != 0;
        if (!hasAudio && !hasVideo) return;

        // Declare the selected formats
        Schema.Format? audio = (Schema.Format?)(hasAudio ? listViewAudio.SelectedItems[0].Tag : null);
        Schema.Format? video = (Schema.Format?)(hasVideo ? listViewVideo.SelectedItems[0].Tag : null);

        // Preparing the saveFileDialog by setting the filename.ext and the filter
        string? ext = hasVideo ? video?.Ext : audio?.Ext;
        saveFileDialog.FileName = Regex.Replace(_metadata.RequestedDownloads[0].FileName, @"\.([a-zA-Z0-9]+)(?:\?|$)", $".{ext}");
        saveFileDialog.Filter = (hasVideo && hasAudio ? Utils.VideoFormat + "|" + Utils.AudioFormat : hasVideo ? Utils.VideoFormat : Utils.AudioFormat) + "|WEBM File (*.webm)|*.webm";
        if (hasVideo)
            saveFileDialog.FilterIndex =
                ext == "mp4" ? 1 :
                ext == "mkv" ? 2 :
                ext == "avi" ? 3 :
                ext == "mov" ? 4 : hasAudio ? 11 : 5;
        else
            saveFileDialog.FilterIndex =
                ext == "mp3" ? 1 :
                ext == "m4a" ? 2 :
                ext == "wav" ? 3 :
                ext == "flac" ? 4 :
                ext == "ogg" ? 5 :
                ext == "opus" ? 6 : 7;

        // Showing the dialog and continue if user confirm
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            // Declaring the selected path
            string savePath = saveFileDialog.FileName;
            string selectedExt = Path.GetExtension(savePath)[1..];

            string audioFormat = audio?.FormatId ?? "";
            string videoFormat = video?.FormatId ?? "";
            bool isAudio = new List<string>(["mp3", "m4a", "wav", "flac", "ogg", "opus"]).Contains(selectedExt) || (!hasVideo && selectedExt == "webm");
            string formatArg = (isAudio ? audioFormat : videoFormat + " " + audioFormat).Trim();
            Debug.WriteLine(
                $"""
                    (debug) Starting download...
                        format={formatArg};
                        savePath={savePath}
                    """
            );

            // Toggle visible of download progress and labels
            progressBarDownload.Value = 0;
            progressBarDownload.Visible = true;
            labelProgress.Visible = true;
            labelProgressSpeed.Visible = true;
            labelProgressSize.Visible = true;

            // Cleanup start progress
            labelProgress.Text = "Menyiapkan unduhan...";
            labelProgressSpeed.Text = "";
            labelProgressSize.Text = "";

            // Declaring the file sizes
            double aSize = audio?.Filesize ?? 0,
                vSize = video?.Filesize ?? 0,
                totalSize = aSize + vSize;

            // Start downloading and updating the progress
            _controller = new DownloadController();
            buttonPause.Text = "||";

            buttonDownload.Visible = false;
            buttonPause.Visible = true;
            buttonCancel.Visible = true;

            YtDlpProgress ytP = new(); FfmpegProgress ffmP = new();
            try
            {
                await Utils.Download(
                    ytDlp, ffmpeg,
                    _metadata,
                    savePath,
                    video, audio,
                    _controller,
                    (ytProg, ffmProg) =>
                    {
                        Debug.WriteLine($"(progress) {ytProg}{ffmProg}");
                        // Saving updated progress to its variable
                        ytP = ytProg ?? ytP;
                        ffmP = ffmProg ?? ffmP;

                        // Declaring variable for media type and sizes
                        string type = ytP.Type ?? ffmP.Type;
                        double size = hasAudio && hasVideo
                        ? type == "video"
                            ? aSize + (ytP.Percentage / 100 * vSize)
                            : ytP.Percentage / 100 * aSize
                        : ytP.Percentage / 100 * totalSize;
                        double percentage = size / totalSize * 100;

                        if (_controller.IsPaused)
                            return;

                        // Updating the progress
                        progressBarDownload.Invoke(() =>
                            progressBarDownload.Value = (int)(percentage * 100)
                        );
                        labelProgress.Invoke(() =>
                            labelProgress.Text = percentage < 100
                            ? hasVideo && hasAudio
                                ? $"Mengunduh {ytP.Type ?? ffmP.Type}..."
                                : "Mengunduh..."
                            : "Post-processing..."
                        );
                        labelProgressSpeed.Invoke(() =>
                            labelProgressSpeed.Text = percentage < 100
                            ? $"{ytP.Speed} {ytP.SpeedUnit}"
                            : $"{ffmP.Speed:0.00}x"
                        );
                        labelProgressSize.Invoke(() =>
                            labelProgressSize.Text = percentage < 100
                                ? $"{(size / totalSize):P2} dari {Formatter.MetricNumber(totalSize, 2, " ", 1024, ["Bi", "KiB", "MiB", "GiB", "TiB", "PiB"])}"
                                : $"{Formatter.MetricNumber(ffmP.TotalSize, 2, " ", 1024, ["Bi", "KiB", "MiB", "GiB", "TiB", "PiB"])}"
                        );
                    }
                );
                labelProgress.Text = $"Selesai! Tersimpan di {savePath}";
            }
            catch (OperationCanceledException)
            {
                labelProgress.Text = $"Download dibatalkan.";
            }

            // Cleanup
            progressBarDownload.Visible = false;
            labelProgressSpeed.Visible = false;
            labelProgressSize.Visible = false;

            buttonDownload.Visible = true;
            buttonPause.Visible = false;
            buttonCancel.Visible = false;

            _controller = null;

            await Task.Delay(15_000);
            labelProgress.Visible = false;
        }
    }

    private void PauseDownload(object sender, EventArgs e)
    {
        if (_controller is null)
            return;

        if (_controller.IsPaused)
        {
            _controller.Resume();
            buttonPause.Text = "||";
            labelProgress.Text = "Melanjutkan unduhan...";
        }
        else
        {
            _controller.Pause();
            buttonPause.Text = "▶";
            labelProgress.Text = "Unduhan dijeda.";
            labelProgressSpeed.Text = "";
        }
    }

    private void CancelDownload(object sender, EventArgs e)
    {
        if (_controller is null)
            return;
        _controller.Cancel();
    }
}
