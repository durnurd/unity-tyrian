using System;

public static class LibC {
    private static Random rng = new Random();
    public static uint mt_rand()
    {
        return (uint)rng.Next();
    }
    public static int mt_rand_i()
    {
        return rng.Next();
    }
    public static float mt_rand_1()
    {
        return (float)rng.NextDouble();
    }
}