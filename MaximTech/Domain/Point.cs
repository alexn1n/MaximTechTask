namespace MaximTech.Domain;

public readonly record struct Point(int X, int Y)
{
    public static long DistanceSquared(Point a, Point b)
    {
        long dx = a.X - b.X;
        long dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }

    public override string ToString() => $"({X}, {Y})";
}
