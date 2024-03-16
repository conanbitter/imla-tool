namespace imlatool;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    public void DebugData(string data)
    {
        Text = data;
    }
}
