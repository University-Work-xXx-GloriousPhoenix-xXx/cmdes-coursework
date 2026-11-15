namespace PetriObj;

public sealed class Model
{
    private readonly IReadOnlyList<PetriObject> _objects;
    private readonly Place[] _places;
    private readonly Transition[] _transitions;
    private readonly Random _random;

    public Model(IReadOnlyList<PetriObject> objects, Random random)
    {
        _objects = objects;
        _random = random;
        _places = objects.SelectMany(o => o.Places).Distinct().ToArray();
        _transitions = objects.SelectMany(o => o.Transitions).ToArray();
    }

    public double Time { get; private set; }
    public IReadOnlyList<PetriObject> Objects => _objects;

    public TextWriter? Trace { get; set; }

    public event Action? StatisticsReset;

    public void Run(double endTime, double warmup = 0)
    {
        Time = 0;
        var warmedUp = warmup <= 0;
        if (Trace is not null) AttachTrace(Trace);

        InputAll();
        TraceMarks();

        while (true)
        {
            var next = _objects.Min(o => o.NextTime);
            var until = Math.Min(next, endTime);

            if (!warmedUp && until >= warmup)
            {
                Accumulate(warmup - Time);
                Time = warmup;
                ResetStatistics();
                warmedUp = true;
            }
            Accumulate(until - Time);
            Time = until;
            if (next > endTime) break;

            foreach (var obj in Shuffled())
                if (obj.NextTime == Time)
                    obj.Output(Time);
            InputAll();
            TraceMarks();
        }

        if (Trace is not null) DetachTrace();
    }

    private void InputAll()
    {
        bool fired;
        do
        {
            fired = false;
            foreach (var obj in Shuffled())
                fired |= obj.Input(Time, _random);
        } while (fired);
    }

    private PetriObject[] Shuffled()
    {
        var copy = _objects.ToArray();
        _random.Shuffle(copy);
        return copy;
    }

    private void Accumulate(double dt)
    {
        if (dt <= 0) return;
        foreach (var p in _places) p.Accumulate(dt);
        foreach (var t in _transitions) t.Accumulate(dt);
    }

    private void ResetStatistics()
    {
        foreach (var p in _places) p.ResetStatistics();
        foreach (var t in _transitions) t.ResetStatistics();
        StatisticsReset?.Invoke();
    }

    private readonly List<Action> _detach = [];

    private void AttachTrace(TextWriter w)
    {
        foreach (var obj in _objects)
            foreach (var t in obj.Transitions)
            {
                Action<double> enter = time => w.WriteLine($"{time,12:F3}  вхід   {obj.Name}.{t.Name}");
                Action<double> exit = time => w.WriteLine($"{time,12:F3}  вихід  {obj.Name}.{t.Name}");
                t.OnEnter += enter;
                t.OnExit += exit;
                _detach.Add(() => { t.OnEnter -= enter; t.OnExit -= exit; });
            }
    }

    private void DetachTrace()
    {
        _detach.ForEach(d => d());
        _detach.Clear();
    }

    private void TraceMarks() =>
        Trace?.WriteLine($"{"",12}  M: {string.Join(", ", _places.Select(p => p.ToString()))}");
}
