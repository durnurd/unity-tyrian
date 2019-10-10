using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static SurfaceC;
using static VGA256dC;
using static KeyboardC;
using static JoystickC;

using UnityEngine;

public static class NortVarsC
{
    public static JE_boolean inputDetected;

    public static JE_boolean JE_buttonPressed()
    {
        if (Input.anyKeyDown)
            return true;

        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
                return true;
        }

        //TODO: Joystick

        return false;
    }

    public static JE_boolean JE_anyButton()
    {
        poll_joysticks();
        service_SDL_events(true);
        return newkey || mousedown || joydown;
    }

    public static void JE_dBar3(Surface surface, int x, int y, int num, JE_byte col)
    {
        JE_byte z;
        JE_byte zWait = 2;

        col += 2;

        for (z = 0; z <= num; z++)
        {
            JE_rectangle(surface, x, y - 1, x + 8, y, col); /* <MXD> SEGa000 */
            if (zWait > 0)
            {
                zWait--;
            }
            else
            {
                col++;
                zWait = 1;
            }
            y -= 2;
        }
    }

    public static void JE_barDrawShadow(Surface surface, int x, int y, int res, int col, int amt, int xsize, int ysize)
    {
        xsize--;
        ysize--;

        for (int z = 1; z <= amt / res; z++)
        {
            JE_barShade(surface, x + 2, y + 2, x + xsize + 2, y + ysize + 2);
            fill_rectangle_xy(surface, x, y, x + xsize, y + ysize, (byte)(col + 12));
            fill_rectangle_xy(surface, x, y, x + xsize, y, (byte)(col + 13));
            JE_pix(surface, x, y, (byte)(col + 15));
            fill_rectangle_xy(surface, x, y + ysize, x + xsize, y + ysize, (byte)(col + 11));
            x += (byte)(xsize + 2);
        }

        amt %= res;
        if (amt > 0)
        {
            JE_barShade(surface, x + 2, y + 2, x + xsize + 2, y + ysize + 2);
            fill_rectangle_xy(surface, x, y, x + xsize, y + ysize, (byte)(col + (12 / res * amt)));
        }
    }

    public static void JE_wipeKey()
    {
        // /!\ Doesn't seems to affect anything.
    }
}