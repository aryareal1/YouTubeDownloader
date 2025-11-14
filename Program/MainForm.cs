/*
 * MAIN FORM CODE
 * This code perform the main function of this project
 * - Showing and styling the main form
 * - Fetch and download content from Youtube
 * 
 * AryaReal © 2025
 */

using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Project
{
    public partial class MainForm : Form
    {
        // Declaring first variables

        /// <summary>
        /// RegEx for matching youtube URLs
        /// </summary>
        private readonly string regexUrl = @"^(?:(?:https?:)?\/\/)?(?:(?:(?:www|m(?:usic)?)\.)?youtu(?:\.be|be\.com)\/(?:shorts\/|live\/|v\/|e(?:mbed)?\/|watch(?:\/|\?(?:\S+=\S+&)*v=)|oembed\?url=https?%3A\/\/(?:www|m(?:usic)?)\.youtube\.com\/watch\?(?:\S+=\S+&)*v%3D|attribution_link\?(?:\S+=\S+&)*u=(?:\/|%2F)watch(?:\?|%3F)v(?:=|%3D))?|www\.youtube-nocookie\.com\/embed\/)([\w-]{11})[\?&#]?\S*$";
        /// <summary>
        /// Variable to stores ytDlp executable object
        /// </summary>
        private readonly YtDlp ytDlp;
        /// <summary>
        /// Variable to stores json information after fetching a video
        /// </summary>
        private JsonDocument? jsonInfo;
        /// <summary>
        /// Variable to stores list of formats after fetching a video
        /// </summary>
        private List<JsonElement>? formats;

        /// <summary>
        /// State of searching or not searching
        /// </summary>
        private bool isSearching = false;

        // Class contructor
        public MainForm(YtDlp ytDlp)
        {
            // Stores ytDlp to the variable and init components
            this.ytDlp = ytDlp;
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for when MainForm is loaded
        /// </summary>
        /// <remarks>Clean up text and make some components invisible</remarks>
        private void Form1_Load(object sender, EventArgs e)
        {
            // Clean up text
            labelError.Text = "";

            // Make some components invisible
            pictureBoxLoading.Visible = false;
            tabControlFormat.Visible = false;
            groupBoxFormat.Visible = false;

            pictureBoxThumbnail.Visible = false;
            labelTitle.Visible = false;
            labelDetails.Visible = false;
            labelAuthor.Visible = false;
            labelDur.Visible = false;
            buttonDownload.Visible = false;
            tableLayoutPanelFormat.Visible = false;

            progressBarDownload.Visible = false;
            labelProgress.Visible = false;
            labelProgressSpeed.Visible = false;
            labelProgressSize.Visible = false;
            
            labelSelected.Visible = false;
            labelSelectedTitle.Visible = false;

            // Set the default directory for Download
            saveFileDialog.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Download");
        }

        /// <summary>
        /// Event handler for when the Search Box's text is changed
        /// </summary>
        /// <remarks>
        /// Check if the inputed value is correct and when it is, enable the Search button.<br/>
        /// Toggle placeholder visible and invisible when there's text on it
        /// </remarks>
        private void textBoxInput_TextChanged(object sender, EventArgs e)
        {
            string text = textBoxInput.Text;
            // Toggle placeholder
            if (text == "")
                labelPlaceholder.Visible = true;
            else
                labelPlaceholder.Visible = false;

            // Check if input is a valid Youtube URL (and toggle Search button)
            bool isValidUrl = Regex.IsMatch(text, regexUrl);
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
        /// Event handler for when the search placeholder is clicked
        /// </summary>
        /// <remarks>When clicked, it'll focus to the search textbox</remarks>
        private void labelPlaceholder_Click(object sender, EventArgs e)
        {
            // Focus to the search text box
            textBoxInput.Focus();
        }

        /// <summary>
        /// Event handler for when the search button is clicked
        /// </summary>
        /// <remarks>
        /// This is the main function to search video and fetching the content from youtube
        /// using <see cref="YtDlp"/>.
        /// </remarks>
        private async void buttonSearch_Click(object sender, EventArgs e)
        {
            // Block the action when it's still searching
            if (isSearching)
                return;

            // Enable the state of searching and disable the default accept button
            isSearching = true;
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
            jsonInfo = await ytDlp.GetJsonInfo(textBoxInput.Text);

            // Store the info to variables
            string title = jsonInfo?.RootElement.GetProperty("title").GetString() ?? "Unknown Title";
            string uploader = jsonInfo?.RootElement.GetProperty("uploader").GetString() ?? "Unknown Uploader";
            string duration = jsonInfo?.RootElement.GetProperty("duration_string").GetString() ?? "x:xx";
            int viewCount = jsonInfo?.RootElement.GetProperty("view_count").GetInt32() ?? 0;
            long uploadAt = 0;
            if (jsonInfo?.RootElement.TryGetProperty("timestamp", out var timestampElem) == true && timestampElem.ValueKind == JsonValueKind.Number)
                uploadAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (timestampElem.GetInt64() * 1000);
            string thumbnail = jsonInfo?.RootElement.GetProperty("thumbnail").GetString()?.Replace("_webp", "").Replace(".webp", ".jpg") ?? "";

            // Assign the info into the GUI components
            pictureBoxThumbnail.LoadAsync(thumbnail);
            labelTitle.Text = title;
            labelDetails.Text =
                $"{viewCount:N0} x ditonton • " +
                $"{Formatter.RelativeDuration(uploadAt)} yg lalu";
            labelAuthor.Text = uploader;
            labelDur.Text = duration;

            // Parse the formats
            formats = jsonInfo?.RootElement.GetProperty("formats").EnumerateArray().ToList()!;
            var videoFormats = formats.Where(item =>
            {
                double vbr = 0, abr = 0;
                if (item.TryGetProperty("vbr", out var vbrElem) && vbrElem.ValueKind == JsonValueKind.Number)
                    vbr = vbrElem.GetDouble();
                if (item.TryGetProperty("abr", out var abrElem) && abrElem.ValueKind == JsonValueKind.Number)
                    abr = abrElem.GetDouble();

                return vbr != 0.0 && abr == 0.0;
            }).Reverse();
            var audioFormats = formats.Where(item =>
            {
                double vbr = 0, abr = 0;
                if (item.TryGetProperty("vbr", out var vbrElem) && vbrElem.ValueKind == JsonValueKind.Number)
                    vbr = vbrElem.GetDouble();
                if (item.TryGetProperty("abr", out var abrElem) && abrElem.ValueKind == JsonValueKind.Number)
                    abr = abrElem.GetDouble();

                return vbr == 0.0 && abr != 0.0;
            }).Reverse();

            // Loop through formats and add it to the listView (for selection)
            foreach (var item in audioFormats)
            {
                listViewAudio.Items.Add(new ListViewItem([
                    item.GetProperty("ext").GetString()!,
                    $"{item.GetProperty("abr").GetDouble():N0} kbps",
                    $"{item.GetProperty("filesize").GetDouble():N0} B"
                ])
                {
                    Tag = item
                });
            }
            foreach (var item in videoFormats)
            {
                listViewVideo.Items.Add(new ListViewItem([
                    item.GetProperty("ext").GetString()!,
                    item.GetProperty("format_note").GetString()?.Split(", ")[0]!,
                    $"{item.GetProperty("vbr").GetDouble():N0} kbps",
                    $"{item.GetProperty("filesize").GetDouble():N0} B"
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
            isSearching = false;
            this.AcceptButton = buttonSearch;
        }

        /// <summary>
        /// Event handler for when the Download button is clicked
        /// </summary>
        /// <remarks>
        /// The main function to download the video or audio using <see cref="YtDlp"/>
        /// and manage it with <see cref="Ffmpeg"/>.
        /// </remarks>
        private async void buttonDownload_Click(object sender, EventArgs e)
        {
            // Block the action when the variable formats dosen't have any stored value in it
            if (formats == null) return;

            // Check if the user selected video and/or audio format
            bool hasAudio = listViewAudio.SelectedItems.Count != 0;
            bool hasVideo = listViewVideo.SelectedItems.Count != 0;
            if (!hasAudio && !hasVideo) return;

            // Declare the selected formats
            JsonElement? audio = (JsonElement?)(hasAudio ? listViewAudio.SelectedItems[0].Tag : null);
            JsonElement? video = (JsonElement?)(hasVideo ? listViewVideo.SelectedItems[0].Tag : null);

            // Preparing the saveFileDialog by setting the filename.ext and the filter
            string? ext = hasVideo ? video?.GetProperty("ext").GetString() : audio?.GetProperty("ext").GetString();
            saveFileDialog.FileName = Regex.Replace(jsonInfo?.RootElement
                .GetProperty("requested_downloads")
                .EnumerateArray().First()
                .GetProperty("filename").GetString()
            ?? "", @"\.([a-zA-Z0-9]+)(?:\?|$)", $".{ext}");
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

                string audioFormat = audio?.GetProperty("format_id").GetString() ?? "";
                string videoFormat = video?.GetProperty("format_id").GetString() ?? "";
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
                double aSize = audio?.GetProperty("filesize").GetDouble() ?? 0,
                    vSize = video?.GetProperty("filesize").GetDouble() ?? 0,
                    totalSize = aSize + vSize;

                // Start downloading and updating the progress
                YtDlpProgress ytP = new(); FfmpegProgress ffmP = new();
                await ytDlp.Download(
                    jsonInfo!,
                    savePath,
                    video, audio,
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

                // Cleanup
                progressBarDownload.Visible = false;
                labelProgress.Text = $"Selesai! Tersimpan di {savePath}";
                labelProgressSpeed.Visible = false;
                labelProgressSize.Visible = false;

                await Task.Delay(15_000);
                labelProgress.Visible = false;
            }
        }

        // === Other Handlers ===

        /// <summary>
        /// Event handler to update format details when a format is selected in the list view.
        /// </summary>
        /// <remarks>
        /// Used by: listViewVideo.SelectedIndexChanged, listViewAudio.SelectedIndexChanged, tabControlFormat.SelectedIndexChanged
        /// </remarks>
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

            // Showing labelFormatDetailsEmpty if there's no selected format
            if ((tab == 0 ? listViewVideo.SelectedItems.Count : listViewAudio.SelectedItems.Count) == 0)
            {
                tableLayoutPanelFormat.Visible = false;
                labelFormatDetailsEmpty.Visible = true;
                groupBoxFormat.Text = "Format Details";
                return;
            }

            // Declaring and showing the format details if there's selected format
            JsonElement format = (JsonElement)(tab == 0 ? listViewVideo : listViewAudio).SelectedItems[0].Tag!;
            groupBoxFormat.Text = $"Format Details ({format.GetProperty("format_id").GetString()})";
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
                    : {format.GetProperty("ext").GetString()}
                    : {format.GetProperty("resolution").GetString()}
                    : {Formatter.MetricNumber(format.GetProperty("vbr").GetDouble()!, 1, " ", 1000, ["kbps", "Mbps", "Gbps", "Tbps", "Pbps"])}
                    : {format.GetProperty("fps").GetDouble()}
                    : {format.GetProperty("dynamic_range").GetString()}
                    : {format.GetProperty("vcodec").GetString()}
                    : {Formatter.MetricNumber(format.GetProperty("filesize").GetDouble()!, 1, " ", 1024, ["B", "KB", "MB", "GB", "TB", "PB"])}
                    """;
            }
            else if (tab == 1)
            {
                string audioChannels = format.GetProperty("audio_channels").GetInt32() == 2 ? "Stereo (2)" : "Mono (1)";
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
                    : {format.GetProperty("ext").GetString()}
                    : {Formatter.MetricNumber(format.GetProperty("abr").GetDouble()!, 1, " ", 1000, ["kbps", "Mbps", "Gbps", "Tbps", "Pbps"])}
                    : {audioChannels}
                    : {format.GetProperty("acodec").GetString()}
                    : {Formatter.MetricNumber(format.GetProperty("filesize").GetDouble()!, 1, " ", 1024, ["B", "KB", "MB", "GB", "TB", "PB"])}
                    """;
            }
            tableLayoutPanelFormat.Visible = true;
            labelFormatDetailsEmpty.Visible = false;
        }
    }
}
