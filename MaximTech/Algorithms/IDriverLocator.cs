using MaximTech.Domain;

namespace MaximTech.Algorithms;

public interface IDriverLocator
{
    int Width { get; }

    int Height { get; }

    int Count { get; }

    bool AddOrUpdate(int driverId, Point location);

    bool Remove(int driverId);

    bool IsInside(Point location);

    bool IsOccupied(Point location);

    IReadOnlyList<Driver> FindNearest(Point order, int count = DriverLocatorBase.DefaultCount);
}
