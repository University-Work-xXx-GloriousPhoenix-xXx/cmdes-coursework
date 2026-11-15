namespace PetriObj;

public sealed class Place(string name, int marks = 0)
{
    private double _area;
    private double _period;

    public string Name { get; } = name;
    public int Marks { get; private set; } = marks;

    public double Mean => _period > 0 ? _area / _period : Marks;
    public int Max { get; private set; } = marks;

    internal void Add(int k)
    {
        Marks += k;
        if (Marks > Max) Max = Marks;
    }

    internal void Remove(int k) => Marks -= k;

    internal void Accumulate(double dt)
    {
        _area += Marks * dt;
        _period += dt;
    }

    internal void ResetStatistics()
    {
        _area = _period = 0;
        Max = Marks;
    }

    public override string ToString() => $"{Name}={Marks}";
}
