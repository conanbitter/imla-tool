namespace imlatool;

class Camera
{
    public Vec2D offset = new Vec2D(0, 0);
    public float scale = 1.0f;

    private const float zoomSpeed = 1.1f;

    private void AdjustOffset(Vec2D center, float k)
    {
        offset.x = center.x * (1.0f - k) + offset.x * k;
        offset.y = center.y * (1.0f - k) + offset.y * k;
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
        scale *= ds;
        AdjustOffset(center, ds);
    }

    public Vec2D WorldToScreen(Vec2D point)
    {
        return new Vec2D(point.x * scale + offset.x, point.y * scale + offset.y);
    }

    public Vec2D ScreenToWorld(Vec2D point)
    {
        return new Vec2D((point.x - offset.x) / scale, (point.y - offset.y) / scale);
    }
}