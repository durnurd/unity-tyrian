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
using static MouseC;

using UnityEngine;
using System;
using System.Linq;
using System.Collections;

public static class KeyboardC
{
    public static JE_boolean ESCPressed;

    public static JE_boolean newkey, newmouse, keydown, mousedown;
    public static byte lastmouse_but;
    public static int lastmouse_x, lastmouse_y;

    public static bool[] mouse_pressed = new bool[4];
    public static int mouse_x;// => Mathf.Clamp((int)Input.mousePosition.x, 0, vga_width);
    public static int mouse_y;// => Screen.height - Mathf.Clamp((int)Input.mousePosition.y, 0, vga_height);

    private static readonly KeyCode[] SupportedKeys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().Where(e => e < KeyCode.Mouse0).ToArray();     //Mouse0 is the first entry we don't support
    public static bool[] keysactive = new bool[(int)SupportedKeys.Max() + 1];

    public static KeyCode lastkey_sym;
    public static char lastkey_char;

    public static bool input_grab_enabled;

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

    private static Coroutine mouseMotionListener;
    private static float accumulatedMouseX, accumulatedMouseY;
    private static bool[] accumulatedMouseDowns = new bool[3];
    private static bool[] accumulatedMouseUps = new bool[3];
    public static void input_grab(bool enable)
    {
        if (!has_mouse)
            return;
        input_grab_enabled = enable;
        Cursor.visible = !enable;
        Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        if (mouseMotionListener != null)
        {
            CoroutineRunner.Instance.StopCoroutine(mouseMotionListener);
            mouseMotionListener = null;
        }
        if (enable)
            mouseMotionListener = CoroutineRunner.Run(listenForMouseMotion());
    }

    private static IEnumerator listenForMouseMotion()
    {
        accumulatedMouseX = 0;
        accumulatedMouseY = 0;
        while (true)
        {
            accumulatedMouseX += Input.GetAxis("Mouse X");
            accumulatedMouseY -= Input.GetAxis("Mouse Y");
            for (int i = 0; i < 3; ++i)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    accumulatedMouseDowns[i] = true;
                }
                else if (Input.GetMouseButtonUp(i))
                {
                    accumulatedMouseUps[i] = true;
                }
            }
            yield return null;
        }
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
            mouse_x = x;
            mouse_y = y;
        }
    }

    public static void service_SDL_events(JE_boolean clear_new)
    {
        if (clear_new)
            newkey = newmouse = false;

        if (!Application.isFocused)
            input_grab(false);

        mouse_x += (int)accumulatedMouseX;
        mouse_y += (int)accumulatedMouseY;
        mouse_x = Mathf.Clamp(mouse_x, 0, vga_width);
        mouse_y = Mathf.Clamp(mouse_y, 0, vga_height);

        accumulatedMouseX = 0;
        accumulatedMouseY = 0;

        for (byte i = 0; i < 3; ++i)
        {
            if (!input_grab_enabled && has_mouse && Input.GetMouseButton(i))
            {
                input_grab(true);
                break;
            }

            if (accumulatedMouseDowns[i])
            {
                newmouse = true;
                lastmouse_x = mouse_x;
                lastmouse_y = mouse_y;
                lastmouse_but = (byte)(i + 1);
                mousedown = true;

                mouse_pressed[i] = true;
                accumulatedMouseDowns[i] = false;
            }
            else if (accumulatedMouseUps[i])
            {
                mouse_pressed[i] = false;
                accumulatedMouseUps[i] = false;
                mousedown = false;
            }
        }

        keydown = false;
        for (int i = 0; i < SupportedKeys.Length; ++i)
        {
            KeyCode k = SupportedKeys[i];
            int idx = (int)k;
            bool active = Input.GetKey(k);
            if (active && !keysactive[idx])
            {
                if (k == KeyCode.F10)
                {
                    input_grab(!input_grab_enabled);
                }
                newkey = true;
                lastkey_sym = k;
                lastkey_char = (char)lastkey_sym;
            }
            keydown |= keysactive[idx] = active;
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