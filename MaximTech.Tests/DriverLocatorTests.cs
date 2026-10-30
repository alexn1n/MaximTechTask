using MaximTech.Algorithms;
using MaximTech.Domain;

namespace MaximTech.Tests;

[TestFixtureSource(nameof(Algorithms))]
public class DriverLocatorTests(LocatorAlgorithm algorithm)
{
    public static readonly LocatorAlgorithm[] Algorithms = Enum.GetValues<LocatorAlgorithm>();

    private IDriverLocator Create(int width, int height) => DriverLocatorFactory.Create(algorithm, width, height);

    [Test]
    public void FindNearest_EmptyMap_ReturnsEmpty()
    {
        var locator = Create(10, 10);

        Assert.That(locator.FindNearest(new Point(5, 5)), Is.Empty);
    }

    [Test]
    public void FindNearest_FewerThanFiveDrivers_ReturnsAllSorted()
    {
        var locator = Create(10, 10);
        locator.AddOrUpdate(1, new Point(9, 9));
        locator.AddOrUpdate(2, new Point(1, 1));
        locator.AddOrUpdate(3, new Point(4, 4));

        var result = locator.FindNearest(new Point(0, 0));

        Assert.That(result.Select(d => d.Id), Is.EqualTo(new[] { 2, 3, 1 }));
    }

    [Test]
    public void FindNearest_ReturnsFiveNearest()
    {
        var locator = Create(20, 20);
        locator.AddOrUpdate(1, new Point(10, 10));
        locator.AddOrUpdate(2, new Point(10, 11));
        locator.AddOrUpdate(3, new Point(12, 10));
        locator.AddOrUpdate(4, new Point(13, 13));
        locator.AddOrUpdate(5, new Point(7, 10));
        locator.AddOrUpdate(6, new Point(0, 0));
        locator.AddOrUpdate(7, new Point(19, 19));
        locator.AddOrUpdate(8, new Point(10, 15));

        var result = locator.FindNearest(new Point(10, 10));

        Assert.That(result.Select(d => d.Id), Is.EqualTo(new[] { 1, 2, 3, 5, 4 }));
    }

    [Test]
    public void FindNearest_UsesEuclideanDistance()
    {
        var locator = Create(20, 20);
        locator.AddOrUpdate(1, new Point(4, 4));
        locator.AddOrUpdate(2, new Point(6, 0));

        var result = locator.FindNearest(new Point(0, 0), 1);

        Assert.That(result.Single().Id, Is.EqualTo(1));
    }

    [Test]
    public void FindNearest_EqualDistances_OrderedById()
    {
        var locator = Create(11, 11);
        locator.AddOrUpdate(40, new Point(5, 6));
        locator.AddOrUpdate(10, new Point(6, 5));
        locator.AddOrUpdate(30, new Point(5, 4));
        locator.AddOrUpdate(20, new Point(4, 5));

        var result = locator.FindNearest(new Point(5, 5), 3);

        Assert.That(result.Select(d => d.Id), Is.EqualTo(new[] { 10, 20, 30 }));
    }

    [Test]
    public void AddOrUpdate_MovedDriver_FoundAtNewLocation()
    {
        var locator = Create(10, 10);
        locator.AddOrUpdate(1, new Point(0, 0));
        locator.AddOrUpdate(2, new Point(5, 5));
        locator.FindNearest(new Point(0, 0));

        locator.AddOrUpdate(2, new Point(1, 0));
        locator.AddOrUpdate(1, new Point(9, 9));

        var result = locator.FindNearest(new Point(0, 0));

        Assert.That(result, Is.EqualTo(new[] { new Driver(2, new Point(1, 0)), new Driver(1, new Point(9, 9)) }));
    }

    [Test]
    public void AddOrUpdate_InvalidLocation_Throws()
    {
        var locator = Create(10, 10);
        locator.AddOrUpdate(1, new Point(2, 2));

        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => locator.AddOrUpdate(2, new Point(2, 2)));
            Assert.Throws<ArgumentOutOfRangeException>(() => locator.AddOrUpdate(3, new Point(10, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => locator.AddOrUpdate(3, new Point(0, -1)));
        });
    }

    [TestCase(1, 64, 64, 50)]
    [TestCase(2, 200, 100, 1000)]
    [TestCase(3, 30, 30, 900)]
    public void FindNearest_RandomScenario_MatchesReference(int seed, int width, int height, int drivers)
    {
        var random = new Random(seed);
        var locator = Create(width, height);
        var reference = new Dictionary<int, Point>();

        for (var step = 0; step < drivers + 300; step++)
        {
            var point = new Point(random.Next(width), random.Next(height));

            if (step >= drivers && step % 2 == 0)
            {
                var expected = reference
                    .Select(p => new Driver(p.Key, p.Value))
                    .OrderBy(d => Point.DistanceSquared(point, d.Location))
                    .ThenBy(d => d.Id)
                    .Take(DriverLocatorBase.DefaultCount);

                Assert.That(locator.FindNearest(point), Is.EqualTo(expected), $"step {step}, order {point}");
                continue;
            }

            if (reference.ContainsValue(point))
                continue;

            var id = random.Next(drivers);
            locator.AddOrUpdate(id, point);
            reference[id] = point;
        }
    }
}
