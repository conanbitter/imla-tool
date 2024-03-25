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
        int selectedClass = -1;
        switch (e.KeyCode)
        {
            case Keys.D1:
                selectedClass = 1;
                break;
            case Keys.D2:
                selectedClass = 2;
                break;
            case Keys.D3:
                selectedClass = 3;
                break;
            case Keys.D4:
                selectedClass = 4;
                break;
            case Keys.D5:
                selectedClass = 5;
                break;
            case Keys.D6:
                selectedClass = 6;
                break;
            case Keys.D7:
                selectedClass = 7;
                break;
            case Keys.D8:
                selectedClass = 8;
                break;
            case Keys.D9:
                selectedClass = 9;
                break;
            case Keys.D0:
                selectedClass = 0;
                break;
            case Keys.Delete:
                leEditor.DeleteRect();
                break;
            case Keys.Escape:
                leEditor.ExitToHover();
                break;
        }
        if ((ModifierKeys & Keys.Shift) == Keys.Shift)
        {
            selectedClass += 10;
        }
        selectedClass--;
        if (selectedClass >= 0)
        {
            llList.SelectClass(selectedClass);
        }
    }
}
