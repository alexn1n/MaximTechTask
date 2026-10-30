using MaximTech.Domain;

namespace MaximTech.Algorithms;

public interface IDriverLocator
{
    int Width { get; }

    int Height { get; }

    int Count { get; }

    void AddOrUpdate(int driverId, Point location);

    bool IsOccupied(Point location);

    IReadOnlyList<Driver> FindNearest(Point order, int count = DriverLocatorBase.DefaultCount);
}
