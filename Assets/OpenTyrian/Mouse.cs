using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;
using UnityEngine;

using static VideoC;
using static KeyboardC;
using static PaletteC;
using static SpriteC;

using static System.Math;

public static class MouseC
{
    public const int LEFT_MOUSE_BUTTON = 1;
    public const int RIGHT_MOUSE_BUTTON = 2;
    public const int MIDDLE_MOUSE_BUTTON = 3;

    public static bool touchscreen = Input.touchSupported;

    public static bool has_mouse => true;//Input.mousePresent && !touchscreen;
    public static bool mouse_has_three_buttons = true;  //Probably

    public static JE_word lastMouseX, lastMouseY;
    public static JE_byte mouseCursor;
    public static JE_word mouseX, mouseY, mouseButton;
    public static JE_word mouseXB, mouseYB;

    public static JE_byte[] mouseGrabShape = new JE_byte[24 * 28];

    public static void JE_drawShapeTypeOne(JE_word x, JE_word y, JE_byte[] shape)
    {
        JE_word xloop = 0, yloop = 0;
        JE_byte[] p = shape; /* shape pointer */
        byte[] s;   /* screen pointer, 8-bit specific */
        int pIdx = 0, sIdx;

        s = VGAScreen.pixels;
        sIdx = y * VGAScreen.w + x;

        for (yloop = 0; yloop < 28; yloop++)
        {
            for (xloop = 0; xloop < 24; xloop++)
            {
                if (sIdx >= s.Length) return;
                s[sIdx] = p[pIdx];
                sIdx++; pIdx++;
            }
            sIdx -= 24;
            sIdx += VGAScreen.w;
        }
    }

    public static void JE_grabShapeTypeOne(JE_word x, JE_word y, JE_byte[] shape)
    {
        JE_word xloop = 0, yloop = 0;
        JE_byte[] p = shape; /* shape pointer */
        byte[] s;   /* screen pointer, 8-bit specific */
        int pIdx = 0, sIdx;

        s = VGAScreen.pixels;
        sIdx = y * VGAScreen.w+ x;

        for (yloop = 0; yloop < 28; yloop++)
        {
            for (xloop = 0; xloop < 24; xloop++)
            {
                if (sIdx >= s.Length) return;
                p[pIdx] = s[sIdx];
                sIdx++; pIdx++;
            }
            sIdx -= 24;
            sIdx += VGAScreen.w;
        }
    }

    static readonly JE_word[] mouseCursorGr /* [1..3] */ = { 273, 275, 277 };
    public static void JE_mouseStart()
    {
        if (has_mouse)
        {
            service_SDL_events(false);
            mouseButton = mousedown ? lastmouse_but : (byte)0; /* incorrect, possibly unimportant */
            lastMouseX = (JE_word)Min(mouse_x, 320 - 13);
            lastMouseY = (JE_word)Min(mouse_y, 200 - 16);

            JE_grabShapeTypeOne(lastMouseX, lastMouseY, mouseGrabShape);

            blit_sprite2x2(VGAScreen, lastMouseX, lastMouseY, shapes6, mouseCursorGr[mouseCursor]);
        }
    }

    public static void JE_mouseReplace()
    {
        if (has_mouse)
            JE_drawShapeTypeOne(lastMouseX, lastMouseY, mouseGrabShape);
    }
}