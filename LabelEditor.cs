
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

    public Vec2D(float x, float y)
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

    public readonly CornerMask GetCornerMask(int x, int y, Camera cam)
    {
        CornerMask result;
        Vec2D sp1 = cam.WorldToScreen(p1);
        Vec2D sp2 = cam.WorldToScreen(p2);
        result.x1 = (x >= sp1.x - border) && (x <= sp1.x + border) && (y >= sp1.y - border) && (y <= sp2.y + border);
        result.x2 = (x >= sp2.x - border) && (x <= sp2.x + border) && (y >= sp1.y - border) && (y <= sp2.y + border);
        result.y1 = (y >= sp1.y - border) && (y <= sp1.y + border) && (x >= sp1.x - border) && (x <= sp2.x + border);
        result.y2 = (y >= sp2.y - border) && (y <= sp2.y + border) && (x >= sp1.x - border) && (x <= sp2.x + border);
        if ((x > sp1.x + border) && (x < sp2.x - border) && (y > sp1.y + border) && (y < sp2.y - border))
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
    public int labelClass;

    public LabelRect(Rect rect, int labelClass)
    {
        this.rect = rect;
        this.labelClass = labelClass;
    }
}

enum EditorMode
{
    Create,
    Hover,
    Edit,
    Idle
}

class LabelEditor : Control
{
    private Pen pen = new(Color.Red);
    private SolidBrush brush = new(Color.FromArgb(75, Color.Red));
    private EditorMode mode = EditorMode.Idle;
    private Vec2D cur;
    //Edit
    private bool isDragging = false;
    private int oldX;
    private int oldY;
    private Vec2D oldPos = new();
    private CornerMask mask;
    private Rect tempRect;
    private int tempClass;
    private int selected;
    //Create
    private bool isFirstPoint = true;
    private bool showCross = true;
    private Vec2D p1;
    private Vec2D p2;
    //Hover
    private int highlighted = -1;
    private List<int> hoverlist = new();
    private bool fixHighlight = false;
    //Camera
    private Camera camera = new();
    private int camOldX;
    private int camOldY;
    private Vec2D oldOffset;
    private bool isPanning;
    private bool initialResize = true;
    private Size oldSize = Size.Empty;
    //Image
    private Image? image;
    private Vec2D imageSize;
    //LabelList
    private LabelList labelList;

    private List<LabelRect> rects = new();

    public LabelEditor(LabelList list)
    {
        labelList = list;
        labelList.ClassChanged += ClassSelected;
        labelList.ClassesUpdated += ClassesUpdated;
        DoubleBuffered = true;
    }

    public void CreateNew()
    {
        if (mode == EditorMode.Hover)
        {
            ChangeMode(EditorMode.Create);
        }
    }

    public void ExitToHover()
    {
        if (mode != EditorMode.Idle && mode != EditorMode.Hover)
        {
            ChangeMode(EditorMode.Hover);
        }
    }

    public void LoadImage(string filename)
    {
        if (image != null)
        {
            image.Dispose();
        }
        image = Image.FromFile(filename);
        imageSize.x = image.Width;
        imageSize.y = image.Height;
    }

    public void ShowAll()
    {
        if (mode == EditorMode.Idle) return;
        camera.Fit(GetBounds(), Size.Width, Size.Height);
        Invalidate();
    }

    public void DeleteRect()
    {
        if (mode == EditorMode.Edit)
        {
            rects.RemoveAt(selected);
            ChangeMode(EditorMode.Hover);
        }
    }

    public void SaveToFile(string filename)
    {
        if (rects.Count == 0) return;
        using (StreamWriter outfile = new StreamWriter(filename))
        {
            foreach (LabelRect lr in rects)
            {
                int classIndex = lr.labelClass;
                float centerX = (lr.rect.p1.x + lr.rect.p2.x) / 2.0f / imageSize.x;
                float centerY = (lr.rect.p1.y + lr.rect.p2.y) / 2.0f / imageSize.y;
                float width = (lr.rect.p2.x - lr.rect.p1.x) / imageSize.x;
                float height = (lr.rect.p2.y - lr.rect.p1.y) / imageSize.y;
                outfile.WriteLine("{0} {1:F6} {2:F6} {3:F6} {4:F6}", classIndex, centerX, centerY, width, height);
            }
        }
    }

    public void LoadFromFile(string filename)
    {
        rects.Clear();
        if (Path.Exists(filename))
        {
            var lines = File.ReadLines(filename);
            foreach (var line in lines)
            {
                string[] numbers = line.Split();
                if (numbers.Length != 5) continue;
                int classIndex;
                float centerX;
                float centerY;
                float width;
                float height;
                try
                {
                    classIndex = int.Parse(numbers[0]);
                    centerX = float.Parse(numbers[1]);
                    centerY = float.Parse(numbers[2]);
                    width = float.Parse(numbers[3]);
                    height = float.Parse(numbers[4]);
                }
                catch
                {
                    continue;
                }
                Vec2D p1 = new((centerX - width / 2.0f) * imageSize.x, (centerY - height / 2.0f) * imageSize.y);
                Vec2D p2 = new((centerX + width / 2.0f) * imageSize.x, (centerY + height / 2.0f) * imageSize.y);
                rects.Add(new LabelRect(new Rect() { p1 = p1, p2 = p2 }, classIndex));
            }
        }
        Invalidate();
    }

    private void ClassSelected(object sender, int newIndex)
    {
        switch (mode)
        {
            case EditorMode.Hover:
                ChangeMode(EditorMode.Create);
                break;
            case EditorMode.Create:
                tempClass = newIndex;
                Invalidate();
                break;
            case EditorMode.Edit:
                rects[selected].labelClass = newIndex;
                Invalidate();
                break;
        }
    }

    private void ClassesUpdated(object sender)
    {
        if (
                (mode == EditorMode.Create && (!labelList[tempClass].enabled)) ||
                (mode == EditorMode.Edit && (!labelList[rects[selected].labelClass].enabled))
                )
        {
            ExitToHover();
            return;
        }

        if (mode == EditorMode.Hover && highlighted >= 0 && (!labelList[rects[highlighted].labelClass].enabled))
        {
            highlighted = -1;
            fixHighlight = false;
            hoverlist.Clear();
            return;
        }

        Invalidate();
    }

    private void DrawRect(Rect rect, PaintEventArgs e, Color color)
    {
        Vec2D p1 = camera.WorldToScreen(rect.p1);
        Vec2D p2 = camera.WorldToScreen(rect.p2);
        pen.Color = color;
        e.Graphics.DrawRectangle(pen, p1.x, p1.y, p2.x - p1.x, p2.y - p1.y);
    }

    private void FillRect(Rect rect, PaintEventArgs e, Color color, Color fillColor)
    {
        Vec2D p1 = camera.WorldToScreen(rect.p1);
        Vec2D p2 = camera.WorldToScreen(rect.p2);
        pen.Color = color;
        brush.Color = fillColor;
        e.Graphics.FillRectangle(brush, p1.x, p1.y, p2.x - p1.x, p2.y - p1.y);
        e.Graphics.DrawRectangle(pen, p1.x, p1.y, p2.x - p1.x, p2.y - p1.y);
    }

    private void DrawCross(Vec2D point, PaintEventArgs e, Color color)
    {
        point = camera.WorldToScreen(point);
        pen.Color = color;
        e.Graphics.DrawLine(pen, point.x, e.ClipRectangle.Top, point.x, e.ClipRectangle.Bottom);
        e.Graphics.DrawLine(pen, e.ClipRectangle.Left, point.y, e.ClipRectangle.Right, point.y);
        e.Graphics.DrawEllipse(pen, point.x - 3, point.y - 3, 6, 6);
    }

    public void ChangeMode(EditorMode newMode)
    {
        switch (newMode)
        {
            case EditorMode.Create:
                isFirstPoint = true;
                p1 = cur;
                tempClass = labelList.selected;
                break;
            case EditorMode.Hover:
                highlighted = -1;
                hoverlist.Clear();
                fixHighlight = false;
                Cursor = Cursors.Default;
                labelList.ClearSelection();
                break;
            case EditorMode.Edit:
                if (highlighted < 0 && highlighted >= rects.Count)
                {
                    MessageBox.Show("Wrong edit");
                    return;
                }
                isDragging = false;
                selected = highlighted;
                labelList.SetSelected(rects[selected].labelClass);
                break;
        }
        mode = newMode;
        Invalidate();
    }

    private Rect GetBounds()
    {
        if (rects.Count == 0 && image == null) return new Rect() { p1 = new Vec2D(0.0f, 0.0f), p2 = new Vec2D(0.0f, 0.0f) };
        Rect results = new()
        {
            p1 = new Vec2D(float.MaxValue, float.MaxValue),
            p2 = new Vec2D(float.MinValue, float.MinValue),
        };
        foreach (LabelRect lr in rects)
        {
            if (lr.rect.p1.x < results.p1.x) results.p1.x = lr.rect.p1.x;
            if (lr.rect.p1.y < results.p1.y) results.p1.y = lr.rect.p1.y;
            if (lr.rect.p2.x > results.p2.x) results.p2.x = lr.rect.p2.x;
            if (lr.rect.p2.y > results.p2.y) results.p2.y = lr.rect.p2.y;
        }
        if (image != null)
        {
            if (0.0f < results.p1.x) results.p1.x = 0.0f;
            if (0.0f < results.p1.y) results.p1.y = 0.0f;
            if (imageSize.x > results.p2.x) results.p2.x = imageSize.x;
            if (imageSize.y > results.p2.y) results.p2.y = imageSize.y;
        }
        return results;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        if (mode == EditorMode.Idle)
        {
            e.Graphics.Clear(SystemColors.Control);
            return;
        }
        e.Graphics.Clear(Color.Black);
        if (image != null)
        {
            Vec2D p1 = camera.WorldToScreen(new Vec2D(0.0f, 0.0f));
            Vec2D p2 = camera.WorldToScreen(imageSize);
            RectangleF dest = new RectangleF(p1.x, p1.y, p2.x - p1.x, p2.y - p1.y);
            e.Graphics.DrawImage(image, dest);
        }
        switch (mode)
        {
            case EditorMode.Create:
                {
                    foreach (LabelRect r in rects)
                    {
                        if (labelList[r.labelClass].enabled)
                        {
                            FillRect(r.rect, e, labelList[r.labelClass].mainColor, labelList[r.labelClass].fadedColor);
                        }
                    }
                    if (isFirstPoint)
                    {
                        if (showCross) DrawCross(p1, e, labelList[tempClass].mainColor);
                    }
                    else
                    {
                        if (showCross) DrawCross(p2, e, labelList[tempClass].mainColor);
                        FillRect(tempRect, e, labelList[tempClass].mainColor, labelList[tempClass].fadedColor);
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
                        if (labelList[rects[i].labelClass].enabled)
                        {
                            FillRect(rects[i].rect, e, labelList[rects[i].labelClass].mainColor, labelList[rects[i].labelClass].fadedColor);
                        }
                    }
                    if (highlighted >= 0 && labelList[rects[highlighted].labelClass].enabled)
                    {
                        pen.Width = 3;
                        FillRect(rects[highlighted].rect, e, labelList[rects[highlighted].labelClass].mainColor, labelList[rects[highlighted].labelClass].fadedColor);
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
                    for (int i = 0; i < rects.Count; i++)
                    {
                        if (i != selected && labelList[rects[i].labelClass].enabled)
                        {
                            FillRect(rects[i].rect, e, labelList[rects[i].labelClass].mainColor, labelList[rects[i].labelClass].fadedColor);
                        }

                    }
                    if (isDragging)
                    {
                        DrawRect(tempRect, e, labelList[rects[selected].labelClass].mainColor);
                    }
                    else
                    {
                        DrawRect(rects[selected].rect, e, labelList[rects[selected].labelClass].mainColor);
                    }
                }
                break;
        }

    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (mode == EditorMode.Idle) return;
        bool needsInvalidation = false;
        cur = camera.ScreenToWorld(new Vec2D(e.X, e.Y));
        switch (mode)
        {
            case EditorMode.Create:
                {
                    if (isFirstPoint)
                    {
                        p1 = camera.ScreenToWorld(new Vec2D(e.X, e.Y));

                    }
                    else
                    {
                        p2 = camera.ScreenToWorld(new Vec2D(e.X, e.Y));

                        tempRect.p1 = p1;
                        tempRect.p2 = p2;
                        tempRect.FixCorners();
                    }
                    needsInvalidation = true;
                }
                break;
            case EditorMode.Hover:
                {
                    hoverlist.Clear();
                    for (int i = 0; i < rects.Count; i++)
                    {
                        if (labelList[rects[i].labelClass].enabled && rects[i].rect.IsInside(cur.x, cur.y))
                        {
                            //highlighted = i;
                            //break;
                            hoverlist.Add(i);
                        }
                    }
                    /*if(!fixHighlight){
                        highlighted = -1;
                    }*/
                    if (fixHighlight && highlighted >= 0 && !rects[highlighted].rect.IsInside(cur.x, cur.y))
                    {
                        fixHighlight = false;
                    }
                    if (hoverlist.Count > 0)
                    {
                        if (!fixHighlight)
                        {
                            highlighted = hoverlist[^1];
                        }
                    }
                    else
                    {
                        highlighted = -1;
                    }
                    needsInvalidation = true;
                }
                break;
            case EditorMode.Edit:
                {
                    Vec2D newPos = camera.ScreenToWorld(new Vec2D(e.X, e.Y));
                    if (isDragging)
                    {
                        Vec2D move = new(newPos.x - oldPos.x, newPos.y - oldPos.y);//new Vec2D((e.X - oldX) * camera.scale, (e.Y - oldY) * camera.scale);
                        tempRect = rects[selected].rect.Move(move, mask);
                        needsInvalidation = true;
                    }
                    else
                    {
                        Cursor = rects[selected].rect.GetCornerMask(e.X, e.Y, camera).GetCursor();
                    }
                }
                break;

        }
        if (isPanning)
        {
            camera.offset = new Vec2D(
                oldOffset.x + e.X - camOldX,
                oldOffset.y + e.Y - camOldY
            );
            needsInvalidation = true;
            //MessageBox.Show("drf");
        }
        if (needsInvalidation)
        {
            Invalidate();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (mode == EditorMode.Idle) return;
        if (e.Button == MouseButtons.Left)
        {
            switch (mode)
            {
                case EditorMode.Create:
                    {
                        if (isFirstPoint)
                        {
                            isFirstPoint = false;
                        }
                        else
                        {
                            rects.Add(new LabelRect(tempRect, labelList.selected));
                            ChangeMode(EditorMode.Hover);
                        }
                    }
                    break;
                case EditorMode.Hover:
                    {
                        if (highlighted >= 0)
                        {
                            ChangeMode(EditorMode.Edit);
                        }
                    }
                    break;
                case EditorMode.Edit:
                    {
                        mask = rects[selected].rect.GetCornerMask(e.X, e.Y, camera);
                        if (mask.IsNotEmpty())
                        {
                            isDragging = true;
                            oldPos = camera.ScreenToWorld(new Vec2D(e.X, e.Y));
                            oldX = e.X;
                            oldY = e.Y;
                            tempRect = rects[selected].rect;
                        }
                        else
                        {
                            ChangeMode(EditorMode.Hover);
                        }
                    }
                    break;
            }
        }
        if (e.Button == MouseButtons.Right)
        {
            isPanning = true;
            oldOffset = camera.offset;
            camOldX = e.X;
            camOldY = e.Y;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (mode == EditorMode.Idle) return;
        if (e.Button == MouseButtons.Left && mode == EditorMode.Edit && isDragging)
        {
            isDragging = false;
            rects[selected].rect = tempRect;
        }
        if (e.Button == MouseButtons.Right)
        {
            isPanning = false;
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (mode == EditorMode.Idle) return;
        if (mode == EditorMode.Create)
        {
            showCross = false;
            Invalidate();
        }
        if (mode == EditorMode.Hover)
        {
            highlighted = -1;
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        if (mode == EditorMode.Idle) return;
        if (mode == EditorMode.Create)
        {
            showCross = true;
            Invalidate();
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (mode == EditorMode.Idle) return;
        if ((ModifierKeys & Keys.Control) == Keys.Control)
        {
            if (mode == EditorMode.Hover && highlighted >= 0 && hoverlist.Count > 0)
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
        else
        {
            camera.Zoom(e.Delta, cur);
            Invalidate();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (initialResize)
        {
            initialResize = false;
            oldSize = Size;
        }
        else
        {
            if (camera.wasFitted)
            {
                ShowAll();
            }
            else
            {
                camera.Resize(oldSize.Width, oldSize.Height, Size.Width, Size.Height);
                Invalidate();
            }
            oldSize = Size;
        }
    }
}