namespace imlatool;

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
    public List<LabelClass> labels = new List<LabelClass>();

    public void Clear()
    {
        Controls.Clear();
        labels.Clear();
    }

    public void SelectClass(int index)
    {
        MessageBox.Show("Selected " + index.ToString());
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
            Image = icon
        };
        Controls.Add(newRb);
        newRb.CheckedChanged += (object? sender, EventArgs e) =>
        {
            if (((RadioButton?)sender)?.Checked ?? false)
            {
                SelectClass(index);
            }
        };

        LabelClass newClass = new LabelClass(newRb, icon)
        {
            name = name,
            mainColor = color,
            fadedColor = Color.FromArgb(100, color),
            index = index
        };

        labels.Add(newClass);
    }

    public void ClearSelection()
    {
        foreach (LabelClass lc in labels)
        {
            lc.button.Checked = false;
        }
    }
}