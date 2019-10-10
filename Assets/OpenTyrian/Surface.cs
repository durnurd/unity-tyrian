using UnityEngine;

public static class SurfaceC
{
    public class Surface
    {
        public byte[] pixels;
        public readonly int w;
        public readonly int h;

        public Surface(int width, int height)
        {
            w = width;
            h = height;
            pixels = new byte[width * height];
        }
    }
}