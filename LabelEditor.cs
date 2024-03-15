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
}

class LabelEditor : Control
{
    private Pen pen = new(Color.Red);
    private SolidBrush brush = new(Color.FromArgb(75, Color.Red));
    private Rect rc;
    //dragging
    private bool isDragging = false;
    private int oldX;
    private int oldY;
    private CornerMask mask;
    private Rect tempRect;

    public LabelEditor()
    {
        DoubleBuffered = true;
        rc.p1.x = 100.0f;
        rc.p1.y = 50.0f;
        rc.p2.x = 400.0f;
        rc.p2.y = 250.0f;
    }

    private void DrawRect(Rect rect, PaintEventArgs e)
    {
        e.Graphics.FillRectangle(brush, rect.p1.x, rect.p1.y, rect.p2.x - rect.p1.x, rect.p2.y - rect.p1.y);
        e.Graphics.DrawRectangle(pen, rect.p1.x, rect.p1.y, rect.p2.x - rect.p1.x, rect.p2.y - rect.p1.y);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(Color.Black);
        if (isDragging)
        {
            DrawRect(tempRect, e);
        }
        else
        {
            DrawRect(rc, e);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
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

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
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
        if (e.Button == MouseButtons.Left)
        {
            isDragging = false;
            rc = tempRect;
        }
    }
}