namespace imlatool;

class Camera
{
    private Vec2D m_offset = new Vec2D(0, 0);
    private float scale = 1.0f;

    private const float zoomSpeed = 1.1f;
    private const int viewBorder = 5;

    public bool wasFitted = false;

    public Vec2D offset
    {
        get { return m_offset; }
        set
        {
            m_offset = value;
            wasFitted = false;
        }
    }

    private void AdjustOffset(Vec2D center, float ds)
    {
        m_offset.x += center.x * scale * (1 - ds);
        m_offset.y += center.y * scale * (1 - ds);
    }

    public void Zoom(int step, Vec2D center)
    {
        float ds;
        if (step > 0)
        {
            ds = zoomSpeed;
        }
        else
        {
            ds = 1 / zoomSpeed;
        }
        AdjustOffset(center, ds);
        scale *= ds;
        wasFitted = false;
    }

    public void Fit(Rect bounds, int width, int height)
    {
        width -= viewBorder * 2;
        height -= viewBorder * 2;
        float boundsWidth = bounds.p2.x - bounds.p1.x;
        float boundsHeight = bounds.p2.y - bounds.p1.y;
        float newScaleX = width / boundsWidth;
        float newScaleY = height / boundsHeight;
        scale = float.Min(newScaleX, newScaleY);
        m_offset.x = width / 2.0f - (bounds.p1.x + boundsWidth / 2) * scale + viewBorder;
        m_offset.y = height / 2.0f - (bounds.p1.y + boundsHeight / 2) * scale + viewBorder;
        wasFitted = true;
    }

    public void Resize(int oldWidth, int oldHeight, int newWidth, int newHeight)
    {
        m_offset.x += (newWidth - oldWidth) / 2.0f;
        m_offset.y += (newHeight - oldHeight) / 2.0f;
        wasFitted = false;
    }

    public Vec2D WorldToScreen(Vec2D point)
    {
        return new Vec2D(point.x * scale + m_offset.x, point.y * scale + m_offset.y);
    }

    public Vec2D ScreenToWorld(Vec2D point)
    {
        return new Vec2D((point.x - m_offset.x) / scale, (point.y - m_offset.y) / scale);
    }
}