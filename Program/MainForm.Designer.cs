namespace Project
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            pictureBox1 = new PictureBox();
            label1 = new Label();
            textBoxInput = new TextBox();
            buttonSearch = new Button();
            labelPlaceholder = new Label();
            labelError = new Label();
            pictureBoxLoading = new PictureBox();
            groupBoxFormat = new GroupBox();
            tableLayoutPanelFormat = new TableLayoutPanel();
            labelFormatDetailsKey = new Label();
            labelFormatDetails = new Label();
            labelFormatDetailsEmpty = new Label();
            tabControlFormat = new TabControl();
            tabPageVideo = new TabPage();
            listViewVideo = new ListView();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            columnHeader6 = new ColumnHeader();
            columnHeader7 = new ColumnHeader();
            tabPageAudio = new TabPage();
            listViewAudio = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            labelTitle = new Label();
            labelAuthor = new Label();
            panel2 = new Panel();
            labelDur = new Label();
            pictureBoxThumbnail = new PictureBox();
            labelDetails = new Label();
            panel1 = new Panel();
            buttonDownload = new Button();
            progressBarDownload = new ProgressBar();
            saveFileDialog = new SaveFileDialog();
            labelProgress = new Label();
            labelProgressSize = new Label();
            labelProgressSpeed = new Label();
            labelSelectedTitle = new Label();
            labelSelected = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLoading).BeginInit();
            groupBoxFormat.SuspendLayout();
            tableLayoutPanelFormat.SuspendLayout();
            tabControlFormat.SuspendLayout();
            tabPageVideo.SuspendLayout();
            tabPageAudio.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxThumbnail).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.BackgroundImage = (Image)resources.GetObject("pictureBox1.BackgroundImage");
            pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox1.Location = new Point(320, 22);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(255, 72);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.Font = new Font("Trebuchet MS", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(284, 97);
            label1.Name = "label1";
            label1.Size = new Size(327, 25);
            label1.TabIndex = 3;
            label1.Text = "Download video dan audio dari Youtube";
            // 
            // textBoxInput
            // 
            textBoxInput.Font = new Font("Trebuchet MS", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBoxInput.Location = new Point(244, 143);
            textBoxInput.Name = "textBoxInput";
            textBoxInput.Size = new Size(299, 28);
            textBoxInput.TabIndex = 4;
            textBoxInput.TextChanged += textBoxInput_TextChanged;
            // 
            // buttonSearch
            // 
            buttonSearch.BackColor = Color.LightGray;
            buttonSearch.Font = new Font("Trebuchet MS", 9F);
            buttonSearch.Location = new Point(549, 142);
            buttonSearch.Name = "buttonSearch";
            buttonSearch.Size = new Size(94, 30);
            buttonSearch.TabIndex = 5;
            buttonSearch.Text = "Cari";
            buttonSearch.UseVisualStyleBackColor = false;
            buttonSearch.Click += buttonSearch_Click;
            // 
            // labelPlaceholder
            // 
            labelPlaceholder.AutoSize = true;
            labelPlaceholder.BackColor = Color.Transparent;
            labelPlaceholder.Cursor = Cursors.IBeam;
            labelPlaceholder.Font = new Font("Trebuchet MS", 9F);
            labelPlaceholder.ForeColor = SystemColors.GrayText;
            labelPlaceholder.Location = new Point(248, 147);
            labelPlaceholder.Name = "labelPlaceholder";
            labelPlaceholder.Size = new Size(124, 20);
            labelPlaceholder.TabIndex = 6;
            labelPlaceholder.Text = "Masukkan URL...";
            labelPlaceholder.Click += labelPlaceholder_Click;
            // 
            // labelError
            // 
            labelError.Font = new Font("Trebuchet MS", 7.8F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            labelError.ForeColor = Color.Red;
            labelError.Location = new Point(244, 172);
            labelError.Name = "labelError";
            labelError.Size = new Size(299, 23);
            labelError.TabIndex = 7;
            labelError.Text = "%labelError%";
            // 
            // pictureBoxLoading
            // 
            pictureBoxLoading.Image = (Image)resources.GetObject("pictureBoxLoading.Image");
            pictureBoxLoading.Location = new Point(417, 253);
            pictureBoxLoading.Name = "pictureBoxLoading";
            pictureBoxLoading.Size = new Size(60, 60);
            pictureBoxLoading.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLoading.TabIndex = 10;
            pictureBoxLoading.TabStop = false;
            // 
            // groupBoxFormat
            // 
            groupBoxFormat.Controls.Add(tableLayoutPanelFormat);
            groupBoxFormat.Controls.Add(labelFormatDetailsEmpty);
            groupBoxFormat.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBoxFormat.Location = new Point(596, 205);
            groupBoxFormat.Name = "groupBoxFormat";
            groupBoxFormat.Size = new Size(250, 239);
            groupBoxFormat.TabIndex = 9;
            groupBoxFormat.TabStop = false;
            groupBoxFormat.Text = "Format Details";
            // 
            // tableLayoutPanelFormat
            // 
            tableLayoutPanelFormat.ColumnCount = 2;
            tableLayoutPanelFormat.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36.8F));
            tableLayoutPanelFormat.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63.2F));
            tableLayoutPanelFormat.Controls.Add(labelFormatDetailsKey, 0, 0);
            tableLayoutPanelFormat.Controls.Add(labelFormatDetails, 1, 0);
            tableLayoutPanelFormat.Location = new Point(0, 29);
            tableLayoutPanelFormat.Name = "tableLayoutPanelFormat";
            tableLayoutPanelFormat.RowCount = 1;
            tableLayoutPanelFormat.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelFormat.Size = new Size(250, 207);
            tableLayoutPanelFormat.TabIndex = 0;
            // 
            // labelFormatDetailsKey
            // 
            labelFormatDetailsKey.AutoSize = true;
            labelFormatDetailsKey.Dock = DockStyle.Fill;
            labelFormatDetailsKey.Location = new Point(3, 0);
            labelFormatDetailsKey.Name = "labelFormatDetailsKey";
            labelFormatDetailsKey.Size = new Size(86, 207);
            labelFormatDetailsKey.TabIndex = 0;
            labelFormatDetailsKey.Text = "%labelFormatDetailsKey%";
            // 
            // labelFormatDetails
            // 
            labelFormatDetails.AutoSize = true;
            labelFormatDetails.Dock = DockStyle.Fill;
            labelFormatDetails.Location = new Point(95, 0);
            labelFormatDetails.Name = "labelFormatDetails";
            labelFormatDetails.Size = new Size(152, 207);
            labelFormatDetails.TabIndex = 1;
            labelFormatDetails.Text = "%labelFormatDetails%";
            // 
            // labelFormatDetailsEmpty
            // 
            labelFormatDetailsEmpty.Dock = DockStyle.Fill;
            labelFormatDetailsEmpty.Location = new Point(3, 23);
            labelFormatDetailsEmpty.Name = "labelFormatDetailsEmpty";
            labelFormatDetailsEmpty.Size = new Size(244, 213);
            labelFormatDetailsEmpty.TabIndex = 0;
            labelFormatDetailsEmpty.Text = "Select a format to view\r\nits details.\r\n";
            labelFormatDetailsEmpty.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tabControlFormat
            // 
            tabControlFormat.Controls.Add(tabPageVideo);
            tabControlFormat.Controls.Add(tabPageAudio);
            tabControlFormat.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabControlFormat.Location = new Point(0, 205);
            tabControlFormat.Name = "tabControlFormat";
            tabControlFormat.SelectedIndex = 0;
            tabControlFormat.Size = new Size(563, 239);
            tabControlFormat.TabIndex = 8;
            tabControlFormat.SelectedIndexChanged += UpdateFormatDetails;
            // 
            // tabPageVideo
            // 
            tabPageVideo.Controls.Add(listViewVideo);
            tabPageVideo.Location = new Point(4, 29);
            tabPageVideo.Name = "tabPageVideo";
            tabPageVideo.Padding = new Padding(3);
            tabPageVideo.Size = new Size(555, 206);
            tabPageVideo.TabIndex = 0;
            tabPageVideo.Text = "Video";
            tabPageVideo.UseVisualStyleBackColor = true;
            // 
            // listViewVideo
            // 
            listViewVideo.Columns.AddRange(new ColumnHeader[] { columnHeader4, columnHeader5, columnHeader6, columnHeader7 });
            listViewVideo.FullRowSelect = true;
            listViewVideo.Location = new Point(0, 0);
            listViewVideo.MultiSelect = false;
            listViewVideo.Name = "listViewVideo";
            listViewVideo.Size = new Size(555, 207);
            listViewVideo.TabIndex = 1;
            listViewVideo.UseCompatibleStateImageBehavior = false;
            listViewVideo.View = View.Details;
            listViewVideo.SelectedIndexChanged += UpdateFormatDetails;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "File type";
            columnHeader4.Width = 100;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Quality";
            columnHeader5.Width = 110;
            // 
            // columnHeader6
            // 
            columnHeader6.Text = "Video bitrate";
            columnHeader6.Width = 130;
            // 
            // columnHeader7
            // 
            columnHeader7.Text = "File size";
            columnHeader7.Width = 150;
            // 
            // tabPageAudio
            // 
            tabPageAudio.Controls.Add(listViewAudio);
            tabPageAudio.Location = new Point(4, 29);
            tabPageAudio.Name = "tabPageAudio";
            tabPageAudio.Padding = new Padding(3);
            tabPageAudio.Size = new Size(555, 206);
            tabPageAudio.TabIndex = 1;
            tabPageAudio.Text = "Audio";
            tabPageAudio.UseVisualStyleBackColor = true;
            // 
            // listViewAudio
            // 
            listViewAudio.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3 });
            listViewAudio.FullRowSelect = true;
            listViewAudio.Location = new Point(0, 0);
            listViewAudio.MultiSelect = false;
            listViewAudio.Name = "listViewAudio";
            listViewAudio.Size = new Size(555, 210);
            listViewAudio.TabIndex = 0;
            listViewAudio.UseCompatibleStateImageBehavior = false;
            listViewAudio.View = View.Details;
            listViewAudio.SelectedIndexChanged += UpdateFormatDetails;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "File type";
            columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Audio bitrate";
            columnHeader2.Width = 130;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "File size";
            columnHeader3.Width = 150;
            // 
            // labelTitle
            // 
            labelTitle.AutoEllipsis = true;
            labelTitle.Font = new Font("Trebuchet MS", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTitle.Location = new Point(266, 17);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(580, 48);
            labelTitle.TabIndex = 11;
            labelTitle.Tag = "";
            labelTitle.Text = "%labelTItle%";
            // 
            // labelAuthor
            // 
            labelAuthor.AutoEllipsis = true;
            labelAuthor.Font = new Font("Trebuchet MS", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelAuthor.ForeColor = SystemColors.GrayText;
            labelAuthor.Location = new Point(266, 135);
            labelAuthor.MaximumSize = new Size(580, 26);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(580, 26);
            labelAuthor.TabIndex = 12;
            labelAuthor.Tag = "";
            labelAuthor.Text = "%labelAuthor%";
            // 
            // panel2
            // 
            panel2.Controls.Add(labelDur);
            panel2.Controls.Add(pictureBoxThumbnail);
            panel2.Location = new Point(4, 17);
            panel2.Name = "panel2";
            panel2.Size = new Size(256, 144);
            panel2.TabIndex = 14;
            // 
            // labelDur
            // 
            labelDur.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            labelDur.AutoSize = true;
            labelDur.BackColor = Color.DimGray;
            labelDur.Font = new Font("Trebuchet MS", 7.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelDur.ForeColor = SystemColors.ControlLightLight;
            labelDur.Location = new Point(212, 120);
            labelDur.Name = "labelDur";
            labelDur.Size = new Size(41, 18);
            labelDur.TabIndex = 13;
            labelDur.Tag = "";
            labelDur.Text = "00:00";
            labelDur.TextAlign = ContentAlignment.TopRight;
            // 
            // pictureBoxThumbnail
            // 
            pictureBoxThumbnail.BackColor = Color.Black;
            pictureBoxThumbnail.Dock = DockStyle.Fill;
            pictureBoxThumbnail.Location = new Point(0, 0);
            pictureBoxThumbnail.Name = "pictureBoxThumbnail";
            pictureBoxThumbnail.Size = new Size(256, 144);
            pictureBoxThumbnail.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxThumbnail.TabIndex = 10;
            pictureBoxThumbnail.TabStop = false;
            // 
            // labelDetails
            // 
            labelDetails.AutoEllipsis = true;
            labelDetails.Font = new Font("Trebuchet MS", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelDetails.ForeColor = SystemColors.GrayText;
            labelDetails.Location = new Point(266, 105);
            labelDetails.MaximumSize = new Size(580, 26);
            labelDetails.Name = "labelDetails";
            labelDetails.Size = new Size(580, 26);
            labelDetails.TabIndex = 15;
            labelDetails.Tag = "";
            labelDetails.Text = "%labelDetails%";
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Controls.Add(labelDetails);
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(labelAuthor);
            panel1.Controls.Add(labelTitle);
            panel1.Controls.Add(tabControlFormat);
            panel1.Controls.Add(groupBoxFormat);
            panel1.Location = new Point(12, 198);
            panel1.Name = "panel1";
            panel1.Size = new Size(870, 458);
            panel1.TabIndex = 10;
            // 
            // buttonDownload
            // 
            buttonDownload.Font = new Font("Trebuchet MS", 9F);
            buttonDownload.Location = new Point(742, 662);
            buttonDownload.Name = "buttonDownload";
            buttonDownload.Size = new Size(116, 40);
            buttonDownload.TabIndex = 16;
            buttonDownload.Text = "Download";
            buttonDownload.UseVisualStyleBackColor = true;
            buttonDownload.Click += buttonDownload_Click;
            // 
            // progressBarDownload
            // 
            progressBarDownload.Location = new Point(12, 694);
            progressBarDownload.Maximum = 10000;
            progressBarDownload.Name = "progressBarDownload";
            progressBarDownload.Size = new Size(563, 8);
            progressBarDownload.Style = ProgressBarStyle.Continuous;
            progressBarDownload.TabIndex = 17;
            // 
            // saveFileDialog
            // 
            saveFileDialog.Title = "Save As";
            // 
            // labelProgress
            // 
            labelProgress.AutoEllipsis = true;
            labelProgress.Location = new Point(12, 671);
            labelProgress.Name = "labelProgress";
            labelProgress.Size = new Size(563, 20);
            labelProgress.TabIndex = 18;
            labelProgress.Text = "%labelProgress%";
            labelProgress.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelProgressSize
            // 
            labelProgressSize.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelProgressSize.Location = new Point(405, 671);
            labelProgressSize.Name = "labelProgressSize";
            labelProgressSize.Size = new Size(170, 20);
            labelProgressSize.TabIndex = 19;
            labelProgressSize.Text = "%labelProgressSize%";
            labelProgressSize.TextAlign = ContentAlignment.BottomRight;
            // 
            // labelProgressSpeed
            // 
            labelProgressSpeed.Location = new Point(278, 671);
            labelProgressSpeed.Name = "labelProgressSpeed";
            labelProgressSpeed.Size = new Size(121, 20);
            labelProgressSpeed.TabIndex = 20;
            labelProgressSpeed.Text = "%labelProgressSpeed%";
            labelProgressSpeed.TextAlign = ContentAlignment.BottomRight;
            // 
            // labelSelectedTitle
            // 
            labelSelectedTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelSelectedTitle.Location = new Point(657, 662);
            labelSelectedTitle.Name = "labelSelectedTitle";
            labelSelectedTitle.Size = new Size(79, 19);
            labelSelectedTitle.TabIndex = 21;
            labelSelectedTitle.Text = "Selected:";
            labelSelectedTitle.TextAlign = ContentAlignment.BottomRight;
            // 
            // labelSelected
            // 
            labelSelected.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelSelected.Location = new Point(581, 681);
            labelSelected.Name = "labelSelected";
            labelSelected.Size = new Size(155, 20);
            labelSelected.TabIndex = 22;
            labelSelected.Text = "No format selected";
            labelSelected.TextAlign = ContentAlignment.BottomRight;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(894, 714);
            Controls.Add(labelSelected);
            Controls.Add(labelSelectedTitle);
            Controls.Add(labelProgressSpeed);
            Controls.Add(labelProgressSize);
            Controls.Add(labelProgress);
            Controls.Add(buttonDownload);
            Controls.Add(progressBarDownload);
            Controls.Add(pictureBoxLoading);
            Controls.Add(panel1);
            Controls.Add(labelError);
            Controls.Add(labelPlaceholder);
            Controls.Add(buttonSearch);
            Controls.Add(textBoxInput);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Youtube Downloader";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLoading).EndInit();
            groupBoxFormat.ResumeLayout(false);
            tableLayoutPanelFormat.ResumeLayout(false);
            tableLayoutPanelFormat.PerformLayout();
            tabControlFormat.ResumeLayout(false);
            tabPageVideo.ResumeLayout(false);
            tabPageAudio.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxThumbnail).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private PictureBox pictureBox1;
        private Label label1;
        private TextBox textBoxInput;
        private Button buttonSearch;
        private Label labelPlaceholder;
        private Label labelError;
        private PictureBox pictureBoxLoading;
        private GroupBox groupBoxFormat;
        private TabControl tabControlFormat;
        private TabPage tabPageVideo;
        private ListView listViewVideo;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
        private ColumnHeader columnHeader6;
        private ColumnHeader columnHeader7;
        private TabPage tabPageAudio;
        private ListView listViewAudio;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private Label labelTitle;
        private Label labelAuthor;
        private Panel panel2;
        private Label labelDur;
        private PictureBox pictureBoxThumbnail;
        private Label labelDetails;
        private Panel panel1;
        private Button buttonDownload;
        private SaveFileDialog saveFileDialog;
        private TableLayoutPanel tableLayoutPanelFormat;
        private Label labelFormatDetailsKey;
        private Label labelFormatDetails;
        private Label labelFormatDetailsEmpty;
        private ProgressBar progressBarDownload;
        private Label labelProgress;
        private Label labelProgressSize;
        private Label labelProgressSpeed;
        private Label labelSelectedTitle;
        private Label labelSelected;
    }
}
