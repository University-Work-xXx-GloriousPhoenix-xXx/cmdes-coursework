namespace PetriObj;

/// <summary>
/// Перехід стохастичної мережі Петрі з часовою затримкою (багатоканальний).
/// Кожен вхід маркерів відкриває новий канал з моментом виходу t + delay().
/// </summary>
public sealed class Transition(string name, Func<double> delay, int priority = 0)
{
    private readonly List<(Place Place, int K)> _inputs = [];
    private readonly List<(Place Place, int K)> _informational = [];
    private readonly List<(Place Place, int K)> _outputs = [];
    private readonly List<double> _exitTimes = [];

    private double _area;   // інтеграл кількості зайнятих каналів за часом
    private double _period;

    public string Name { get; } = name;
    public int Priority { get; } = priority;

    /// <summary>Кількість маркерів, що зараз перебувають у переході (зайняті канали).</summary>
    public int Buffer => _exitTimes.Count;
    public double NextTime => _exitTimes.Count == 0 ? double.PositiveInfinity : _exitTimes.Min();

    /// <summary>Середня за часом кількість зайнятих каналів (для одноканального — завантаження).</summary>
    public double MeanBuffer => _period > 0 ? _area / _period : Buffer;
    public int Entered { get; private set; }
    public int Exited { get; private set; }

    /// <summary>Викликається після входу маркерів у перехід (аргумент — поточний час).</summary>
    public event Action<double>? OnEnter;
    /// <summary>Викликається після виходу маркерів з переходу (аргумент — поточний час).</summary>
    public event Action<double>? OnExit;

    /// <summary>Вхідна дуга кратності k.</summary>
    public Transition In(Place place, int k = 1) { _inputs.Add((place, k)); return this; }

    /// <summary>Інформаційна дуга: маркери перевіряються, але не віднімаються.</summary>
    public Transition Info(Place place, int k = 1) { _informational.Add((place, k)); return this; }

    /// <summary>Вихідна дуга кратності k.</summary>
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
