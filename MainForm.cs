namespace imlatool;

public partial class MainForm : Form
{
    private string filename = "";

    private static readonly string[] supportedExtentions = {
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
        ".gif",
        ".tif",
        ".tiff"
    };

    static (List<string>, int) GetFilelist(string name)
    {
        List<string> files = Directory.GetFiles(Path.GetDirectoryName(name) ?? "", "*.*")
                .Where(file => supportedExtentions.Contains(Path.GetExtension(file))).ToList();
        int pos = files.FindIndex(item => item == name);
        return (files, pos);
    }


    public MainForm()
    {
        InitializeComponent();
        KeyPreview = true;
        Application.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    }

    public void DebugData(string data)
    {
        Text = data;
    }

    private void LoadFile(string name)
    {
        filename = name;
        if (!llList.isLoaded)
        {
            string classesFile = Path.Combine(Path.GetDirectoryName(filename) ?? "", "classes.txt");
            if (Path.Exists(classesFile))
            {
                llList.LoadFromFile(classesFile);
            }
        }
        leEditor.LoadImage(filename);
        leEditor.LoadFromFile(Path.ChangeExtension(filename, ".txt"));
        leEditor.ChangeMode(EditorMode.Hover);
        leEditor.ShowAll();
        llList.ClearSelection();
        Text = Path.GetFileName(filename) + " - ImLa tool";
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
        if ((ModifierKeys & Keys.Shift) == Keys.Shift && selectedClass >= 0)
        {
            selectedClass += 10;
        }
        selectedClass--;

        if ((ModifierKeys & Keys.Alt) == Keys.Alt)
        {
            llList.ToggleClass(selectedClass);
        }
        else
        {
            if (selectedClass >= 0)
            {
                llList.SelectClass(selectedClass);
            }
        }
    }
}
