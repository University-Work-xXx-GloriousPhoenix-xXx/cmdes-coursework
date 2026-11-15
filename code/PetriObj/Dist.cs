namespace PetriObj;

/// <summary>Генератори часових затримок у позначеннях посібника.</summary>
public static class Dist
{
    /// <summary>d — детерміноване значення.</summary>
    public static Func<double> Const(double d) => () => d;

    /// <summary>unif(d, s) — рівномірний розподіл на інтервалі (d − s, d + s).</summary>
    public static Func<double> Unif(Random random, double d, double s) =>
        () => d - s + 2 * s * random.NextDouble();
}
