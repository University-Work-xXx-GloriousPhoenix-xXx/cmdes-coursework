namespace PetriObj;

/// <summary>
/// Позиція мережі Петрі.
/// Ототожнення позицій різних Петрі-об'єктів = використання одного й того самого екземпляра.
/// </summary>
public sealed class Place(string name, int marks = 0)
{
    private double _area;   // інтеграл кількості маркерів за часом
    private double _period; // тривалість спостереження

    public string Name { get; } = name;
    public int Marks { get; private set; } = marks;

    /// <summary>Середня за часом кількість маркерів.</summary>
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
