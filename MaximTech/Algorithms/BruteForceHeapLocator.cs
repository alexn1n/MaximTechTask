using MaximTech.Domain;

namespace MaximTech.Algorithms;

public sealed class BruteForceHeapLocator(int width, int height) : DriverLocatorBase(width, height)
{
    protected override IReadOnlyList<Driver> FindNearestCore(Point order, int count)
    {
        var collector = new NearestCollector(count);
        foreach (var (id, location) in Locations)
            collector.Offer(id, location, Point.DistanceSquared(order, location));

        return collector.ToArray();
    }
}
