using MaximTech.Domain;

namespace MaximTech.Algorithms;

public sealed class BruteForceSortLocator(int width, int height) : DriverLocatorBase(width, height)
{
    protected override IReadOnlyList<Driver> FindNearestCore(Point order, int count)
    {
        var keys = new DistanceKey[Count];
        var drivers = new Driver[Count];
        var i = 0;
        foreach (var (id, location) in Locations)
        {
            keys[i] = new DistanceKey(Point.DistanceSquared(order, location), id);
            drivers[i] = new Driver(id, location);
            i++;
        }

        Array.Sort(keys, drivers);
        return drivers.AsSpan(0, count).ToArray();
    }

    private readonly record struct DistanceKey(long Distance, int Id) : IComparable<DistanceKey>
    {
        public int CompareTo(DistanceKey other)
        {
            var byDistance = Distance.CompareTo(other.Distance);
            return byDistance != 0 ? byDistance : Id.CompareTo(other.Id);
        }
    }
}
