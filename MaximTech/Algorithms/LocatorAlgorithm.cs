namespace MaximTech.Algorithms;

public enum LocatorAlgorithm
{
    BruteForceSort,
    BruteForceHeap,
    GridSpiral,
    KdTree
}

public static class DriverLocatorFactory
{
    public static IDriverLocator Create(LocatorAlgorithm algorithm, int width, int height) => algorithm switch
    {
        LocatorAlgorithm.BruteForceSort => new BruteForceSortLocator(width, height),
        LocatorAlgorithm.BruteForceHeap => new BruteForceHeapLocator(width, height),
        LocatorAlgorithm.GridSpiral => new GridSpiralLocator(width, height),
        LocatorAlgorithm.KdTree => new KdTreeLocator(width, height),
        _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
    };
}
