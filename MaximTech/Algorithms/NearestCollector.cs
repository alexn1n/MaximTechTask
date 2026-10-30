using MaximTech.Domain;

namespace MaximTech.Algorithms;

internal sealed class NearestCollector
{
    private readonly Candidate[] _heap;
    private int _count;

    public NearestCollector(int capacity)
    {
        _heap = new Candidate[capacity];
    }

    public bool IsFull => _count == _heap.Length;

    public long WorstDistance => IsFull ? _heap[0].Distance : long.MaxValue;

    public void Offer(int driverId, Point location, long distance)
    {
        var candidate = new Candidate(distance, new Driver(driverId, location));

        if (!IsFull)
        {
            SiftUp(_count++, candidate);
            return;
        }

        if (candidate.CompareTo(_heap[0]) < 0)
            SiftDown(0, candidate);
    }

    public Driver[] ToArray()
    {
        var candidates = _heap.AsSpan(0, _count).ToArray();
        Array.Sort(candidates);

        var result = new Driver[candidates.Length];
        for (var i = 0; i < candidates.Length; i++)
            result[i] = candidates[i].Driver;

        return result;
    }

    private void SiftUp(int index, Candidate candidate)
    {
        while (index > 0)
        {
            var parent = (index - 1) / 2;
            if (_heap[parent].CompareTo(candidate) >= 0)
                break;

            _heap[index] = _heap[parent];
            index = parent;
        }

        _heap[index] = candidate;
    }

    private void SiftDown(int index, Candidate candidate)
    {
        while (true)
        {
            var child = 2 * index + 1;
            if (child >= _count)
                break;

            if (child + 1 < _count && _heap[child + 1].CompareTo(_heap[child]) > 0)
                child++;

            if (_heap[child].CompareTo(candidate) <= 0)
                break;

            _heap[index] = _heap[child];
            index = child;
        }

        _heap[index] = candidate;
    }

    private readonly record struct Candidate(long Distance, Driver Driver) : IComparable<Candidate>
    {
        public int CompareTo(Candidate other)
        {
            var byDistance = Distance.CompareTo(other.Distance);
            return byDistance != 0 ? byDistance : Driver.Id.CompareTo(other.Driver.Id);
        }
    }
}
