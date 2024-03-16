
namespace imlatool;

struct CornerMask
{
    public bool x1;
    public bool x2;
    public bool y1;
    public bool y2;

    public readonly Cursor GetCursor()
    {
        if (x1 && x2 && y1 && y2) return Cursors.SizeAll;
        if ((x1 && y1) || (x2 && y2)) return Cursors.SizeNWSE;
        if ((x1 && y2) || (x2 && y1)) return Cursors.SizeNESW;
        if (x1 || x2) return Cursors.SizeWE;
        if (y1 || y2) return Cursors.SizeNS;
        return Cursors.Default;
    }

    public readonly bool IsNotEmpty()
    {
        return x1 || x2 || y1 || y2;
    }
}

struct Vec2D
{
    public float x;
    public float y;

    public Vec2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

struct Rect
{
    public Vec2D p1;
    public Vec2D p2;

    private const int border = 6;

    public readonly CornerMask GetCornerMask(int x, int y)
    {
        CornerMask result;
        result.x1 = (x >= p1.x - border) && (x <= p1.x + border) && (y >= p1.y - border) && (y <= p2.y + border);
        result.x2 = (x >= p2.x - border) && (x <= p2.x + border) && (y >= p1.y - border) && (y <= p2.y + border);
        result.y1 = (y >= p1.y - border) && (y <= p1.y + border) && (x >= p1.x - border) && (x <= p2.x + border);
        result.y2 = (y >= p2.y - border) && (y <= p2.y + border) && (x >= p1.x - border) && (x <= p2.x + border);
        if ((x > p1.x + border) && (x < p2.x - border) && (y > p1.y + border) && (y < p2.y - border))
        {
            result.x1 = true;
            result.x2 = true;
            result.y1 = true;
            result.y2 = true;
        }

        return result;
    }

    public readonly Rect Move(Vec2D offset, CornerMask mask)
    {
        Rect result;
        result.p1.x = mask.x1 ? p1.x + offset.x : p1.x;
        result.p1.y = mask.y1 ? p1.y + offset.y : p1.y;
        result.p2.x = mask.x2 ? p2.x + offset.x : p2.x;
        result.p2.y = mask.y2 ? p2.y + offset.y : p2.y;
        result.FixCorners();
        return result;
    }

    public void FixCorners()
    {
        if (p1.x > p2.x) (p1.x, p2.x) = (p2.x, p1.x);
        if (p1.y > p2.y) (p1.y, p2.y) = (p2.y, p1.y);
    }

    public bool IsInside(float x, float y)
    {
        return (x >= p1.x) && (x <= p2.x) && (y >= p1.y) && (y <= p2.y);
    }
}

class LabelRect
{
    public Rect rect;

    public LabelRect(Rect rect)
    {
        this.rect = rect;
    }
}

enum EditorMode
{
    Create,
    Hover,
    Edit
}

class LabelEditor : Control
{
    private Pen pen = new(Color.Red);
    private SolidBrush brush = new(Color.FromArgb(75, Color.Red));
    private Rect rc;
    private EditorMode mode = EditorMode.Hover;
    private Vec2D cur;
    //Edit:Dragging
    private bool isDragging = false;
    private int oldX;
    private int oldY;
    private CornerMask mask;
    private Rect tempRect;
    //Create
    private bool isFirstPoint = true;
    private bool showCross = true;
    private Vec2D p1;
    private Vec2D p2;
    //Hover
    private int highlighted = -1;
    private List<int> hoverlist = new();
    private bool fixHighlight = false;

    private List<LabelRect> rects = new();

    public LabelEditor()
    {
        DoubleBuffered = true;
        rc.p1.x = 100.0f;
        rc.p1.y = 50.0f;
        rc.p2.x = 400.0f;
        rc.p2.y = 250.0f;
    }

    public void CreateNew()
    {
        mode = EditorMode.Create;
        isFirstPoint = true;
    }

    private void DrawRect(Rect rect, PaintEventArgs e)
    {
        e.Graphics.FillRectangle(brush, rect.p1.x, rect.p1.y, rect.p2.x - rect.p1.x, rect.p2.y - rect.p1.y);
        e.Graphics.DrawRectangle(pen, rect.p1.x, rect.p1.y, rect.p2.x - rect.p1.x, rect.p2.y - rect.p1.y);
    }

    private void DrawCross(Vec2D point, PaintEventArgs e)
    {
        e.Graphics.DrawLine(pen, point.x, e.ClipRectangle.Top, point.x, e.ClipRectangle.Bottom);
        e.Graphics.DrawLine(pen, e.ClipRectangle.Left, point.y, e.ClipRectangle.Right, point.y);
        e.Graphics.DrawEllipse(pen, point.x - 3, point.y - 3, 6, 6);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(Color.Black);
        switch (mode)
        {
            case EditorMode.Create:
                {
                    foreach (LabelRect r in rects)
                    {
                        DrawRect(r.rect, e);
                    }
                    if (isFirstPoint)
                    {
                        if (showCross) DrawCross(p1, e);
                    }
                    else
                    {
                        if (showCross) DrawCross(p2, e);
                        DrawRect(tempRect, e);
                    }
                    break;
                }
            case EditorMode.Hover:
                {
                    for (int i = 0; i < rects.Count; i++)
                    {
                        /*if (i == highlighted)
                        {
                            pen.Width = 3;
                        }
                        else
                        {
                            pen.Width = 1;
                        }*/
                        DrawRect(rects[i].rect, e);
                    }
                    if (highlighted >= 0)
                    {
                        pen.Width = 3;
                        DrawRect(rects[highlighted].rect, e);
                        pen.Width = 1;
                    }
                    /*pen.Width = 3;
                    foreach (int index in hoverlist)
                    {
                        DrawRect(rects[index].rect, e);
                    }
                    pen.Width = 1;*/
                }
                break;
            case EditorMode.Edit:
                {
                    if (isDragging)
                    {
                        DrawRect(tempRect, e);
                    }
                    else
                    {
                        DrawRect(rc, e);
                    }
                }
                break;
        }

    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        switch (mode)
        {
            case EditorMode.Create:
                {
                    if (isFirstPoint)
                    {
                        p1.x = e.X;
                        p1.y = e.Y;
                    }
                    else
                    {
                        p2.x = e.X;
                        p2.y = e.Y;

                        tempRect.p1 = p1;
                        tempRect.p2 = p2;
                        tempRect.FixCorners();
                    }
                    Invalidate();
                }
                break;
            case EditorMode.Hover:
                {
                    cur.x = e.X;
                    cur.y = e.Y;
                    hoverlist.Clear();
                    for (int i = 0; i < rects.Count; i++)
                    {
                        if (rects[i].rect.IsInside(e.X, e.Y))
                        {
                            //highlighted = i;
                            //break;
                            hoverlist.Add(i);
                        }
                    }
                    /*if(!fixHighlight){
                        highlighted = -1;
                    }*/
                    if (fixHighlight && highlighted >= 0 && !rects[highlighted].rect.IsInside(e.X, e.Y))
                    {
                        fixHighlight = false;
                    }
                    if (hoverlist.Count > 0)
                    {
                        if (!fixHighlight)
                        {
                            highlighted = hoverlist[hoverlist.Count - 1];
                        }
                    }
                    else
                    {
                        highlighted = -1;
                    }
                    Invalidate();
                }
                break;
            case EditorMode.Edit:
                {
                    if (isDragging)
                    {
                        tempRect = rc.Move(new Vec2D(e.X - oldX, e.Y - oldY), mask);
                        Invalidate();
                    }
                    else
                    {
                        Cursor = rc.GetCornerMask(e.X, e.Y).GetCursor();
                    }
                }
                break;

        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && mode == EditorMode.Edit)
        {
            mask = rc.GetCornerMask(e.X, e.Y);
            if (mask.IsNotEmpty())
            {
                isDragging = true;
                oldX = e.X;
                oldY = e.Y;
                tempRect = rc;
            }
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left && mode == EditorMode.Edit)
        {
            isDragging = false;
            rc = tempRect;
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Left && mode == EditorMode.Create)
        {
            if (isFirstPoint)
            {
                isFirstPoint = false;
            }
            else
            {
                rects.Add(new LabelRect(tempRect));
                mode = EditorMode.Hover;
                Invalidate();
            }
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (mode == EditorMode.Create)
        {
            showCross = false;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (mode == EditorMode.Create)
        {
            showCross = true;
            Invalidate();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (mode == EditorMode.Hover
            && ((ModifierKeys & Keys.Control) == Keys.Control)
            && highlighted >= 0
            && hoverlist.Count > 0)
        {
            int currentIndex = hoverlist.FindIndex(item => item == highlighted);
            int found = currentIndex;
            if (e.Delta > 0)
            {
                currentIndex = (currentIndex + 1) % hoverlist.Count;
            }
            else
            {
                currentIndex--;
                if (currentIndex < 0) currentIndex += hoverlist.Count;
            }
            fixHighlight = true;
            highlighted = hoverlist[currentIndex];
            if (Parent != null)
            {
                ((MainForm)Parent).DebugData(string.Format("Found:{0} Next:{1}", found, currentIndex));
            }
            Invalidate();
        }
    }
}