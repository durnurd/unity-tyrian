using System;

public static class LibC {
    private static Random rng = new Random();

    public static int rand()
    {
        return rng.Next();
    }
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

    public static float mt_rand_lt1()
    {
        float ret;
        do
        {
            ret = mt_rand_1();
        } while (ret >= 1);
        return ret;
    }
}