using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static JoystickC;
using static VideoC;

using UnityEngine;
using System;
using System.Linq;
public static class KeyboardC
{
    public static JE_boolean ESCPressed;

    public static JE_boolean newkey, newmouse, keydown, mousedown;
    public static byte lastmouse_but;
    public static int lastmouse_x, lastmouse_y;

    public static bool[] mouse_pressed = new bool[4];
    public static int mouse_x => Mathf.Clamp((int)Input.mousePosition.x, 0, vga_width);
    public static int mouse_y => Screen.height - Mathf.Clamp((int)Input.mousePosition.y, 0, vga_height);

    public class MouseActiveChecker {
        public bool this[int idx] => Input.GetMouseButton(idx - 1);
    }


    private static readonly KeyCode[] SupportedKeys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().Where(e => e < KeyCode.Mouse0).ToArray();     //Mouse0 is the first entry we don't support
    public static bool[] keysactive = new bool[(int)SupportedKeys.Max() + 1];

    public static KeyCode lastkey_sym;
    public static char lastkey_char;

    public static bool input_grab_enabled;

    public static void flush_events_buffer()
    {

    }
    public static WaitWhile coroutine_wait_input(JE_boolean keyboard, JE_boolean mouse, JE_boolean joystick)
    {
        service_SDL_events(false);
        return new WaitWhile(() => {
            push_joysticks_as_keyboard();
            service_SDL_events(false);
#if WITH_NETWORK
            if (isNetworkGame)
                network_check();
#endif
            return !((keyboard && keydown) || (mouse && mousedown) || (joystick && joydown));
        });
    }
    public static WaitWhile coroutine_wait_noinput(JE_boolean keyboard, JE_boolean mouse, JE_boolean joystick)
    {
        service_SDL_events(false);
        return new WaitWhile(() => {
            push_joysticks_as_keyboard();
            service_SDL_events(false);
#if WITH_NETWORK
            if (isNetworkGame)
                network_check();
#endif
            return ((keyboard && keydown) || (mouse && mousedown) || (joystick && joydown));
        });
    }

    public static void input_grab(bool enable)
    {
        //Nah...
        //input_grab_enabled = enable;
        //Cursor.visible = !enable;
    }
    public static byte JE_mousePosition(out JE_word mouseX, out JE_word mouseY)
    {
        service_SDL_events(false);

        mouseX = (JE_word)mouse_x;
        mouseY = (JE_word)mouse_y;

        return mousedown ? lastmouse_but : (byte)0;
    }
    public static void set_mouse_position(int x, int y)
    {
        if (input_grab_enabled)
        {
            //nah...
            //SDL_WarpMouse(x * scalers[scaler].width / vga_width, y * scalers[scaler].height / vga_height);
            //mouse_x = x;
            //mouse_y = y;
        }
    }

    public static void service_SDL_events(JE_boolean clear_new)
    {
        if (clear_new)
            newkey = newmouse = false;

        lastmouse_x = mouse_x;
        lastmouse_y = mouse_y;

        if (!Application.isFocused)
            input_grab(false);

        mousedown = false;
        for (byte i = 1; i < 4; ++i)
        {
            bool active = Input.GetMouseButton(i - 1);
            if (active && !mouse_pressed[i])
            {
                newmouse = true;
                lastmouse_but = (byte)(i);
            }
            mousedown |= mouse_pressed[i] = Input.GetMouseButton(i - 1);
        }

        keydown = false;
        for (int i = 0; i < SupportedKeys.Length; ++i)
        {
            bool active = Input.GetKey(SupportedKeys[i]);
            if (active && !keysactive[i])
            {
                newkey = true;
                lastkey_sym = SupportedKeys[i];
                lastkey_char = (char)lastkey_sym;
            }
            keydown |= keysactive[i] = active;
        }
        ESCPressed = keysactive[(int)KeyCode.Escape];
    }
    
    public static void sleep_game()
    {

    }
    
    public static void JE_clearKeyboard()
    {
        // /!\ Doesn't seems important. I think. D:
    }

}