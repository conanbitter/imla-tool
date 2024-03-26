namespace imlatool;

public delegate void ClassChangedEventHandler(object sender, int newIndex);

class LabelClass
{
    public string name = "";
    public Color mainColor = Color.Blue;
    public Color fadedColor = Color.FromArgb(100, Color.Blue);

    public RadioButton button;
    public Image icon;
    public int index;

    public LabelClass(RadioButton rb, Image ic)
    {
        button = rb;
        icon = ic;
    }
}

class LabelList : FlowLayoutPanel
{
    private List<LabelClass> labels = new List<LabelClass>();
    private readonly LabelClass unknown = new LabelClass(new RadioButton(), new Bitmap(1, 1))
    {
        name = "unknown",
        mainColor = Color.FromArgb(255, 0, 0, 0),
        fadedColor = Color.FromArgb(100, 0, 0, 0),
        index = -1
    };
    public int selected;
    private bool internalSelection = false;
    public bool isLoaded = false;

    public event ClassChangedEventHandler? ClassChanged;

    public LabelClass this[int index]
    {
        get
        {
            if (index >= 0 && index < labels.Count)
            {
                return labels[index];
            }
            else
            {
                return unknown;
            }
        }
    }

    public void Clear()
    {
        Controls.Clear();
        labels.Clear();
    }

    public void ClassSelected(int index)
    {
        if (internalSelection) return;
        ClassChanged?.Invoke(this, index);
    }

    public void SelectClass(int index)
    {
        if (index < 0 || index >= labels.Count) return;
        SetSelected(index);
        ClassSelected(index);
    }

    public void SetSelected(int index)
    {
        if (index < 0 || index >= labels.Count) return;
        internalSelection = true;
        for (int i = 0; i < labels.Count; i++)
        {
            labels[i].button.Checked = i == index;
        }
        labels[index].button.Focus();
        internalSelection = false;
        selected = index;
    }

    public void AddLabel(string name, Color color)
    {
        int index = labels.Count;

        Bitmap icon = new(16, 16);
        Graphics iconGraphics = Graphics.FromImage(icon);
        iconGraphics.FillRectangle(new SolidBrush(Color.FromArgb(255, color)), 1, 1, 15, 15);

        RadioButton newRb = new()
        {
            Text = string.Format("[{0}] {1}", index + 1, name),
            Appearance = Appearance.Button,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            AutoSize = true,
            Image = icon,
            TabStop = false
        };
        Controls.Add(newRb);
        newRb.CheckedChanged += (object? sender, EventArgs e) =>
        {
            if (((RadioButton?)sender)?.Checked ?? false)
            {
                selected = index;
                ClassSelected(index);
            }
        };

        LabelClass newClass = new LabelClass(newRb, icon)
        {
            name = name,
            mainColor = Color.FromArgb(255, color),
            fadedColor = Color.FromArgb(100, color),
            index = index
        };

        labels.Add(newClass);
    }

    public void LoadFromFile(string filename)
    {
        Clear();
        var lines = File.ReadLines(filename);
        foreach (var line in lines)
        {
            if (line == "") continue;
            string[] components = line.Split('#');
            if (components.Length >= 1)
            {
                string name = components[0].Trim();
                Color color = Color.Red;
                if (components.Length > 1)
                {
                    string colorString = components[1].Trim();
                    try
                    {
                        int colorInt = int.Parse(colorString, System.Globalization.NumberStyles.HexNumber);
                        color = Color.FromArgb(colorInt);
                    }
                    catch
                    {

                    }
                }
                AddLabel(name, color);
            }
        }
        isLoaded = true;
    }

    public void ClearSelection()
    {
        foreach (LabelClass lc in labels)
        {
            lc.button.Checked = false;
        }
        if (Parent != null)
        {
            ((MainForm)Parent).ActiveControl = null;
        }
    }
}