namespace PetriObj;

public sealed class Transition(string name, Func<double> delay, int priority = 0)
{
    private readonly List<(Place Place, int K)> _inputs = [];
    private readonly List<(Place Place, int K)> _informational = [];
    private readonly List<(Place Place, int K)> _outputs = [];
    private readonly List<double> _exitTimes = [];

    private double _area;
    private double _period;

    public string Name { get; } = name;
    public int Priority { get; } = priority;

    public int Buffer => _exitTimes.Count;
    public double NextTime => _exitTimes.Count == 0 ? double.PositiveInfinity : _exitTimes.Min();

    public double MeanBuffer => _period > 0 ? _area / _period : Buffer;
    public int Entered { get; private set; }
    public int Exited { get; private set; }

    public event Action<double>? OnEnter;
    public event Action<double>? OnExit;

    public Transition In(Place place, int k = 1) { _inputs.Add((place, k)); return this; }

    public Transition Info(Place place, int k = 1) { _informational.Add((place, k)); return this; }

    public Transition Out(Place place, int k = 1) { _outputs.Add((place, k)); return this; }

    internal IEnumerable<Place> Places =>
        _inputs.Concat(_informational).Concat(_outputs).Select(a => a.Place);

    public bool CanFire =>
        _inputs.All(a => a.Place.Marks >= a.K) && _informational.All(a => a.Place.Marks >= a.K);

    internal void Enter(double time)
    {
        foreach (var (place, k) in _inputs) place.Remove(k);
        _exitTimes.Add(time + delay());
        Entered++;
        OnEnter?.Invoke(time);
    }

    internal void Exit(double time)
    {
        _exitTimes.Remove(NextTime);
        foreach (var (place, k) in _outputs) place.Add(k);
        Exited++;
        OnExit?.Invoke(time);
    }

    internal void Accumulate(double dt)
    {
        _area += Buffer * dt;
        _period += dt;
    }

    internal void ResetStatistics()
    {
        _area = _period = 0;
        Entered = Exited = 0;
    }

    public override string ToString() => Name;
}
