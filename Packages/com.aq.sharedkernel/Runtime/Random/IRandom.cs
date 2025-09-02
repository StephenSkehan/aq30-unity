namespace AQ.SharedKernel
{
    public interface IRandom
    {
        int Next();                         // >= 0
        int Next(int maxExclusive);         // [0, max)
        int Next(int minInclusive, int maxExclusive); // [min, max)
        double NextDouble();                // [0,1)
    }
}
