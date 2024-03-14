namespace imlatool;

partial class MainForm
{
    private Panel pToolBox;
    private Button bCameraReset;

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1280, 720);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "ImLa Tool";
        this.Name = "ImLa Tool";

        pToolBox = new Panel();
        pToolBox.Size = new Size(200, 100);
        pToolBox.Dock = DockStyle.Right;
        this.Controls.Add(pToolBox);

        bCameraReset = new Button();
        bCameraReset.Size = new Size(100, 40);
        bCameraReset.Location = new Point(30, 30);
        bCameraReset.Text = "Reset View";
        pToolBox.Controls.Add(bCameraReset);

        this.ResumeLayout();
    }
}
