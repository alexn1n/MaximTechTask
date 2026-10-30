using MaximTech.Domain;

namespace MaximTech.Algorithms;

public abstract class DriverLocatorBase : IDriverLocator
{
    public const int DefaultCount = 5;

    private readonly Dictionary<int, Point> _locations = new();
    private readonly Dictionary<Point, int> _occupants = new();

    protected DriverLocatorBase(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public int Count => _locations.Count;

    protected Dictionary<int, Point> Locations => _locations;

    public void AddOrUpdate(int driverId, Point location)
    {
        EnsureInside(location);

        if (_occupants.TryGetValue(location, out var occupant))
        {
            if (occupant == driverId)
                return;

            throw new InvalidOperationException($"Клетка {location} уже занята водителем {occupant}.");
        }

        if (_locations.TryGetValue(driverId, out var previous))
        {
            _occupants.Remove(previous);
            _occupants.Add(location, driverId);
            _locations[driverId] = location;
            OnMoved(driverId, previous, location);
        }
        else
        {
            _occupants.Add(location, driverId);
            _locations.Add(driverId, location);
            OnAdded(driverId, location);
        }
    }

    public bool IsOccupied(Point location) => _occupants.ContainsKey(location);

    public IReadOnlyList<Driver> FindNearest(Point order, int count = DefaultCount)
    {
        EnsureInside(order);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        if (Count == 0)
            return [];

        return FindNearestCore(order, Math.Min(count, Count));
    }

    protected abstract IReadOnlyList<Driver> FindNearestCore(Point order, int count);

    protected virtual void OnAdded(int driverId, Point location)
    {
    }

    protected virtual void OnMoved(int driverId, Point from, Point to)
    {
    }

    private void EnsureInside(Point point)
    {
        if ((uint)point.X >= (uint)Width || (uint)point.Y >= (uint)Height)
            throw new ArgumentOutOfRangeException(nameof(point), point,
                $"Координаты должны удовлетворять 0 <= X < {Width}, 0 <= Y < {Height}.");
    }
}
