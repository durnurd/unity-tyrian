using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static KeyboardC;
using static VideoC;
using static JoystickC;
using static MouseC;
using static NortVarsC;
using static MainIntC;
using static NetworkC;

using static System.Math;

using UnityEngine;
using System.Collections;

public static class SetupC
{

    public static IEnumerator e_JE_textMenuWait(JE_word[] ref_waitTime, JE_boolean doGamma)
    { UnityEngine.Debug.Log("e_JE_textMenuWait");
        set_mouse_position(160, 100);

        JE_showVGA();

        do
        {

            push_joysticks_as_keyboard();
            service_SDL_events(true);

            if (doGamma)
                JE_gammaCheck();

            inputDetected = newkey | mousedown;

            if (lastkey_sym == KeyCode.Space)
            {
                lastkey_sym = KeyCode.Return;
            }

            if (mousedown)
            {
                newkey = true;
                lastkey_sym = KeyCode.Return;
            }

            if (has_mouse && input_grab_enabled)
            {
                if (Abs(mouse_y - 100) > 10)
                {
                    inputDetected = true;
                    if (mouse_y - 100 < 0)
                    {
                        lastkey_sym = KeyCode.UpArrow;
                    }
                    else
                    {
                        lastkey_sym = KeyCode.DownArrow;
                    }
                    newkey = true;
                }
                if (Abs(mouse_x - 160) > 10)
                {
                    inputDetected = true;
                    if (mouse_x - 160 < 0)
                    {
                        lastkey_sym = KeyCode.LeftArrow;
                    }
                    else
                    {
                        lastkey_sym = KeyCode.RightArrow;
                    }
                    newkey = true;
                }
            }

            //NETWORK_KEEP_ALIVE();

            yield return null;

            if (ref_waitTime != null && ref_waitTime[0] > 0) {
                (ref_waitTime[0])--;
            }
        } while (!(inputDetected || (ref_waitTime != null && ref_waitTime[0] == 1) || haltGame));
    }
}