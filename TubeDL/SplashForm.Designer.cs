namespace TubeDL;

partial class SplashForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
        pictureBox1 = new PictureBox();
        labelText = new Label();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        SuspendLayout();
        // 
        // pictureBox1
        // 
        pictureBox1.BackColor = Color.Transparent;
        pictureBox1.BackgroundImage = (Image)resources.GetObject("pictureBox1.BackgroundImage");
        pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
        pictureBox1.Location = new Point(141, 28);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(366, 161);
        pictureBox1.TabIndex = 0;
        pictureBox1.TabStop = false;
        // 
        // labelText
        // 
        labelText.Font = new Font("Trebuchet MS", 10.8F);
        labelText.ForeColor = SystemColors.ControlLightLight;
        labelText.Location = new Point(12, 196);
        labelText.Name = "labelText";
        labelText.Size = new Size(624, 55);
        labelText.TabIndex = 1;
        labelText.Text = "%labelText%";
        labelText.TextAlign = ContentAlignment.TopCenter;
        // 
        // StartForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Brown;
        ClientSize = new Size(648, 291);
        Controls.Add(labelText);
        Controls.Add(pictureBox1);
        FormBorderStyle = FormBorderStyle.None;
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "StartForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Youtube Downloader Startup";
        Load += SplashForm_Load;
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private PictureBox pictureBox1;
    private Label labelText;
}