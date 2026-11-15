namespace PetriObj;

public sealed class PetriObject(string name)
{
    private readonly List<Transition> _transitions = [];

    public string Name { get; } = name;
    public IReadOnlyList<Transition> Transitions => _transitions;
    public IEnumerable<Place> Places => _transitions.SelectMany(t => t.Places).Distinct();

    public Transition this[string transitionName] => _transitions.Single(t => t.Name == transitionName);

    public Transition Add(Transition transition)
    {
        _transitions.Add(transition);
        return transition;
    }

    public double NextTime => _transitions.Count == 0 ? double.PositiveInfinity : _transitions.Min(t => t.NextTime);

    internal void Output(double time)
    {
        foreach (var t in _transitions)
            while (t.NextTime == time)
                t.Exit(time);
    }

    internal bool Input(double time, Random random)
    {
        var fired = false;
        while (true)
        {
            var enabled = _transitions.Where(t => t.CanFire).ToList();
            if (enabled.Count == 0) return fired;

            var top = enabled.Max(t => t.Priority);
            enabled.RemoveAll(t => t.Priority != top);
            enabled[random.Next(enabled.Count)].Enter(time);
            fired = true;
        }
    }
}
