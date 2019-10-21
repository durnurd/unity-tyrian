using System.Linq;
using Unity.Collections;
using UnityEngine;
using static SurfaceC;

public static class VideoC
{

    public const int vga_width = 320;
    public const int vga_height = 200;

    public static Surface VGAScreen = new Surface(vga_width, vga_height);
    public static Surface VGAScreen2 = new Surface(vga_width, vga_height);
    public static Surface game_screen = new Surface(vga_width, vga_height);

    public static Surface VGAScreenSeg = VGAScreen;

    public static Texture2D ScreenTexture;

    private static Color32[] internalPalette = new Color32[256];
    public static Texture2D PaletteTexture;

    public static void JE_showVGA()
    {
        ScreenTexture.LoadRawTextureData(VGAScreen.pixels);
        ScreenTexture.Apply();
    }

    public static void SDL_SetColors(Color32[] colors, uint min, uint len)
    {
        System.Array.Copy(colors, min, internalPalette, min, len);
        PaletteTexture.SetPixels32(internalPalette);
        PaletteTexture.Apply();
    }

    private static Surface blackScreen = new Surface(vga_width, vga_height);
    public static void JE_clr256(Surface screen)
    {
        System.Array.Copy(blackScreen.pixels, screen.pixels, screen.pixels.Length);
    }

    public static void JE_clr256Pixels(byte[] pixels)
    {
        System.Array.Copy(blackScreen.pixels, pixels, pixels.Length);
    }

    public static Vector2 scaleToVGA(Vector2 v)
    {
        v.x = v.x / Screen.width * 320;
        v.y = v.y / Screen.height * 200;
        return v;
    }
}
