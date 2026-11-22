namespace CG
{
    static class RandomManager
    {
        static Random rnd = new Random();

        public static int Integer(int maxExclusive) => Integer(0,maxExclusive);
        public static int Integer(int minInclusive, int maxExclusive)
        {
            return rnd.Next(minInclusive,maxExclusive);
        }
    }
}