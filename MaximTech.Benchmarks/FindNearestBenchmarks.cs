using BenchmarkDotNet.Attributes;
using MaximTech.Algorithms;
using MaximTech.Domain;

namespace MaximTech.Benchmarks;

[MemoryDiagnoser]
public class FindNearestBenchmarks
{
    private const int OrdersCount = 1024;

    private IDriverLocator _locator = null!;
    private Point[] _orders = null!;
    private int _next;

    [Params(1_000, 100_000, 1_000_000)]
    public int Drivers { get; set; }

    [ParamsAllValues]
    public LocatorAlgorithm Algorithm { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _locator = MapFactory.CreateFilled(Algorithm, Drivers, random);
        _orders = Enumerable.Range(0, OrdersCount).Select(_ => MapFactory.RandomPoint(random)).ToArray();
        _locator.FindNearest(_orders[0]);
    }

    [Benchmark]
    public IReadOnlyList<Driver> FindNearest() => _locator.FindNearest(_orders[_next++ & (OrdersCount - 1)]);
}
