using MaximTech.Algorithms;
using MaximTech.Domain;

namespace MaximTech.Benchmarks;

public static class MapFactory
{
    public const int Width = 2000;
    public const int Height = 2000;

    public static IDriverLocator CreateFilled(LocatorAlgorithm algorithm, int drivers, Random random)
    {
        var locator = DriverLocatorFactory.Create(algorithm, Width, Height);
        for (var id = 0; id < drivers; id++)
        {
            Point point;
            do
                point = RandomPoint(random);
            while (locator.IsOccupied(point));

            locator.AddOrUpdate(id, point);
        }

        return locator;
    }

    public static Point RandomPoint(Random random) => new(random.Next(Width), random.Next(Height));
}
