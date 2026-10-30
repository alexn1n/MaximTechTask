using MaximTech.Domain;

namespace MaximTech.Algorithms;

public sealed class KdTreeLocator(int width, int height) : DriverLocatorBase(width, height)
{
    private static readonly Comparison<Driver> ByX = (a, b) => a.Location.X.CompareTo(b.Location.X);
    private static readonly Comparison<Driver> ByY = (a, b) => a.Location.Y.CompareTo(b.Location.Y);

    private Driver[] _nodes = [];
    private bool _isDirty;

    protected override IReadOnlyList<Driver> FindNearestCore(Point order, int count)
    {
        if (_isDirty)
            Rebuild();

        var collector = new NearestCollector(count);
        Search(0, _nodes.Length, 0, order, collector);
        return collector.ToArray();
    }

    protected override void OnAdded(int driverId, Point location) => _isDirty = true;

    protected override void OnMoved(int driverId, Point from, Point to) => _isDirty = true;

    private void Rebuild()
    {
        var nodes = new Driver[Count];
        var i = 0;
        foreach (var (id, location) in Locations)
            nodes[i++] = new Driver(id, location);

        Build(nodes, 0, nodes.Length, 0);
        _nodes = nodes;
        _isDirty = false;
    }

    private static void Build(Driver[] nodes, int lo, int hi, int depth)
    {
        if (hi - lo <= 1)
            return;

        nodes.AsSpan(lo, hi - lo).Sort(depth % 2 == 0 ? ByX : ByY);
        var mid = (lo + hi) >>> 1;
        Build(nodes, lo, mid, depth + 1);
        Build(nodes, mid + 1, hi, depth + 1);
    }

    private void Search(int lo, int hi, int depth, Point order, NearestCollector collector)
    {
        if (lo >= hi)
            return;

        var mid = (lo + hi) >>> 1;
        var node = _nodes[mid];
        collector.Offer(node.Id, node.Location, Point.DistanceSquared(order, node.Location));

        long diff = depth % 2 == 0 ? order.X - node.Location.X : order.Y - node.Location.Y;
        var nearLeft = diff < 0;

        if (nearLeft)
            Search(lo, mid, depth + 1, order, collector);
        else
            Search(mid + 1, hi, depth + 1, order, collector);

        if (collector.IsFull && diff * diff > collector.WorstDistance)
            return;

        if (nearLeft)
            Search(mid + 1, hi, depth + 1, order, collector);
        else
            Search(lo, mid, depth + 1, order, collector);
    }
}
