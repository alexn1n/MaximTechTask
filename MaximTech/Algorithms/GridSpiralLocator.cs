using MaximTech.Domain;

namespace MaximTech.Algorithms;

public sealed class GridSpiralLocator : DriverLocatorBase
{
    private readonly int[] _ids;
    private readonly bool[] _occupied;

    public GridSpiralLocator(int width, int height) : base(width, height)
    {
        var cells = checked(width * height);
        _ids = new int[cells];
        _occupied = new bool[cells];
    }

    protected override IReadOnlyList<Driver> FindNearestCore(Point order, int count)
    {
        var collector = new NearestCollector(count);
        var maxRadius = Math.Max(
            Math.Max(order.X, Width - 1 - order.X),
            Math.Max(order.Y, Height - 1 - order.Y));
        var found = 0;

        for (var r = 0; r <= maxRadius && found < Count; r++)
        {
            if (collector.IsFull && (long)r * r > collector.WorstDistance)
                break;

            found += ScanRing(order, r, collector);
        }

        return collector.ToArray();
    }

    protected override void OnAdded(int driverId, Point location) => Occupy(location, driverId);

    protected override void OnMoved(int driverId, Point from, Point to)
    {
        _occupied[IndexOf(from.X, from.Y)] = false;
        Occupy(to, driverId);
    }

    private int ScanRing(Point order, int radius, NearestCollector collector)
    {
        if (radius == 0)
            return ScanCell(order.X, order.Y, order, collector);

        var found = 0;
        var xMin = Math.Max(order.X - radius, 0);
        var xMax = Math.Min(order.X + radius, Width - 1);
        var top = order.Y - radius;
        var bottom = order.Y + radius;

        if (top >= 0)
            for (var x = xMin; x <= xMax; x++)
                found += ScanCell(x, top, order, collector);

        if (bottom < Height)
            for (var x = xMin; x <= xMax; x++)
                found += ScanCell(x, bottom, order, collector);

        var yMin = Math.Max(top + 1, 0);
        var yMax = Math.Min(bottom - 1, Height - 1);
        var left = order.X - radius;
        var right = order.X + radius;

        if (left >= 0)
            for (var y = yMin; y <= yMax; y++)
                found += ScanCell(left, y, order, collector);

        if (right < Width)
            for (var y = yMin; y <= yMax; y++)
                found += ScanCell(right, y, order, collector);

        return found;
    }

    private int ScanCell(int x, int y, Point order, NearestCollector collector)
    {
        var index = IndexOf(x, y);
        if (!_occupied[index])
            return 0;

        var location = new Point(x, y);
        collector.Offer(_ids[index], location, Point.DistanceSquared(order, location));
        return 1;
    }

    private void Occupy(Point location, int driverId)
    {
        var index = IndexOf(location.X, location.Y);
        _ids[index] = driverId;
        _occupied[index] = true;
    }

    private int IndexOf(int x, int y) => y * Width + x;
}
