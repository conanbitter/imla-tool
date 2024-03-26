namespace imlatool;

partial class MainForm
{
    private LabelEditor leEditor;
    private Panel pToolBox;
    private Button bCameraReset;
    private Button bOpen;
    private Button bSave;
    private Button bNext;
    private Button bPrev;
    private Button bNextUnlabeled;
    private Button bPrevUnlabeled;
    private Button bLoadClasses;
    private OpenFileDialog ofdOpenImage;
    private OpenFileDialog ofdOpenClasses;
    private LabelList llList;

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1280, 720);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "ImLa Tool";
        this.Name = "ImLa Tool";

        llList = new LabelList();
        llList.Dock = DockStyle.Top;
        llList.AutoSize = true;
        this.Controls.Add(llList);
        llList.AddLabel("Cat", Color.FromArgb(0xFF9800));
        llList.AddLabel("Dog little", Color.FromArgb(0x43A047));
        llList.AddLabel("Dog big", Color.FromArgb(0x673AB7));
        llList.AddLabel("Bird", Color.FromArgb(0xA1887F));

        leEditor = new LabelEditor(llList);
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
        bCameraReset.Click += (object sender, System.EventArgs e) => leEditor.ShowAll();
        pToolBox.Controls.Add(bCameraReset);

        ofdOpenImage = new OpenFileDialog()
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
            if (ofdOpenImage.ShowDialog() == DialogResult.OK)
            {
                LoadFile(ofdOpenImage.FileName);
            }
        };
        pToolBox.Controls.Add(bOpen);

        ofdOpenClasses = new OpenFileDialog()
        {

            Filter = "Classes list (*.txt)|*.txt",
            Title = "Load classes",
            CheckFileExists = true,
            CheckPathExists = true,
        };

        bLoadClasses = new Button();
        bLoadClasses.Size = new Size(100, 40);
        bLoadClasses.Location = new Point(30, 110);
        bLoadClasses.Text = "Load classes ...";
        bLoadClasses.Click += (object sender, System.EventArgs e) =>
        {
            if (ofdOpenClasses.ShowDialog() == DialogResult.OK)
            {
                llList.LoadFromFile(ofdOpenClasses.FileName);
                leEditor.ExitToHover();
                llList.ClearSelection();
            }
        };
        pToolBox.Controls.Add(bLoadClasses);

        bSave = new Button();
        bSave.Size = new Size(100, 40);
        bSave.Location = new Point(30, 150);
        bSave.Text = "Save";
        bSave.Click += (object sender, System.EventArgs e) =>
        {
            leEditor.SaveToFile(Path.ChangeExtension(filename, ".txt"));
        };
        pToolBox.Controls.Add(bSave);

        bNext = new Button();
        bNext.Size = new Size(50, 40);
        bNext.Location = new Point(80, 190);
        bNext.Text = "Next";
        bNext.Click += (object sender, System.EventArgs e) =>
        {
            (List<string> files, int pos) = GetFilelist(filename);
            if (files.Count > 1)
            {
                pos++;
                if (pos >= files.Count)
                {
                    pos = 0;
                }
                LoadFile(files[pos]);
            }
        };
        pToolBox.Controls.Add(bNext);

        bPrev = new Button();
        bPrev.Size = new Size(50, 40);
        bPrev.Location = new Point(30, 190);
        bPrev.Text = "Prev";
        bPrev.Click += (object sender, System.EventArgs e) =>
        {
            (List<string> files, int pos) = GetFilelist(filename);
            if (files.Count > 1)
            {
                pos--;
                if (pos < 0)
                {
                    pos = files.Count - 1;
                }
                LoadFile(files[pos]);
            }
        };
        pToolBox.Controls.Add(bPrev);

        bNextUnlabeled = new Button();
        bNextUnlabeled.Size = new Size(50, 40);
        bNextUnlabeled.Location = new Point(80, 230);
        bNextUnlabeled.Text = "NextU";
        bNextUnlabeled.Click += (object sender, System.EventArgs e) =>
        {
            (List<string> files, int pos) = GetFilelist(filename);
            if (files.Count > 1)
            {
                int oldpos = pos;
                pos++;
                while (pos != oldpos)
                {
                    if (pos >= files.Count)
                    {
                        pos = 0;
                    }
                    if (!Path.Exists(Path.ChangeExtension(files[pos], ".txt")))
                    {
                        LoadFile(files[pos]);
                        break;
                    }
                    pos++;

                }
            }
        };
        pToolBox.Controls.Add(bNextUnlabeled);

        bPrevUnlabeled = new Button();
        bPrevUnlabeled.Size = new Size(50, 40);
        bPrevUnlabeled.Location = new Point(30, 230);
        bPrevUnlabeled.Text = "PrevU";
        bPrevUnlabeled.Click += (object sender, System.EventArgs e) =>
        {
            (List<string> files, int pos) = GetFilelist(filename);
            if (files.Count > 1)
            {
                int oldpos = pos;
                pos--;
                while (pos != oldpos)
                {
                    if (pos < 0)
                    {
                        pos = files.Count - 1;
                    }
                    if (!Path.Exists(Path.ChangeExtension(files[pos], ".txt")))
                    {
                        LoadFile(files[pos]);
                        break;
                    }
                    pos--;

                }
            }
        };
        pToolBox.Controls.Add(bPrevUnlabeled);




        this.ResumeLayout();
    }
}
