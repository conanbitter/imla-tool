namespace imlatool;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        KeyPreview = true;
    }

    public void DebugData(string data)
    {
        Text = data;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.D1)
        {
            leEditor.CreateNew();
        }
    }
}
