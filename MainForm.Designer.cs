namespace imlatool;

partial class MainForm
{
    private LabelEditor leEditor;
    private Panel pToolBox;
    private Button bCameraReset;
    private Button bOpen;
    private OpenFileDialog ofdOpen;

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1280, 720);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "ImLa Tool";
        this.Name = "ImLa Tool";

        leEditor = new LabelEditor();
        leEditor.Dock = DockStyle.Fill;
        this.Controls.Add(leEditor);

        pToolBox = new Panel();
        pToolBox.Size = new Size(200, 100);
        pToolBox.Dock = DockStyle.Right;
        this.Controls.Add(pToolBox);

        bCameraReset = new Button();
        bCameraReset.Size = new Size(100, 40);
        bCameraReset.Location = new Point(30, 30);
        bCameraReset.Text = "Reset View";
        bCameraReset.Click += (object sender, System.EventArgs e) => leEditor.CreateNew();
        pToolBox.Controls.Add(bCameraReset);

        ofdOpen = new OpenFileDialog()
        {

            Filter = "All image fomats|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|PNG image (*.png)|*.png|JPEG image (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP image (*.bmp)|*.bmp|GIF image (*.gif)|*.gif|TIFF image (*.tif;*.tiff)|*.tif;*.tiff",
            Title = "Load image",
            CheckFileExists = true,
            CheckPathExists = true,
        };

        bOpen = new Button();
        bOpen.Size = new Size(100, 40);
        bOpen.Location = new Point(30, 70);
        bOpen.Text = "Open ...";
        bOpen.Click += (object sender, System.EventArgs e) =>
        {
            if (ofdOpen.ShowDialog() == DialogResult.OK)
            {
                leEditor.LoadImage(ofdOpen.FileName);
                leEditor.ChangeMode(EditorMode.Hover);
            }
        };
        pToolBox.Controls.Add(bOpen);

        this.ResumeLayout();
    }
}
