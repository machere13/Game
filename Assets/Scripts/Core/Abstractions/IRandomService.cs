namespace IdlePancake.Core.Abstractions
{
    public interface IRandomService
    {
        int NextInt(int minInclusive, int maxExclusive);
        double NextDouble();
    }
}
