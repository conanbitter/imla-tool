namespace imlatool;

class LabelEditor : Control
{
    public LabelEditor()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(Color.Black);
    }
}