namespace PetriObj;

public static class Dist
{
    public static Func<double> Const(double d) => () => d;

    public static Func<double> Unif(Random random, double d, double s) =>
        () => d - s + 2 * s * random.NextDouble();
}
