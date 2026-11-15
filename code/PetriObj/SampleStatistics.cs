namespace PetriObj;

/// <summary>Статистика вибірки (алгоритм Велфорда): кількість, середнє, СКВ, мінімум, максимум.</summary>
public sealed class SampleStatistics
{
    private double _m2;

    public int Count { get; private set; }
    public double Mean { get; private set; }
    public double StdDev => Count > 1 ? Math.Sqrt(_m2 / (Count - 1)) : 0;
    public double Min { get; private set; } = double.PositiveInfinity;
    public double Max { get; private set; } = double.NegativeInfinity;

    public void Add(double x)
    {
        Count++;
        var delta = x - Mean;
        Mean += delta / Count;
        _m2 += delta * (x - Mean);
        if (x < Min) Min = x;
        if (x > Max) Max = x;
    }

    public void Reset()
    {
        Count = 0;
        Mean = _m2 = 0;
        Min = double.PositiveInfinity;
        Max = double.NegativeInfinity;
    }
}
