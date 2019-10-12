/*
 * OpenTyrian: A modern cross-platform port of Tyrian
 * Copyright (C) 2007-2009  The OpenTyrian Development Team
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static SurfaceC;
using static SpriteC;
using static VGA256dC;
using static NortVarsC;
using static VideoC;
using static NortsongC;
using System.Collections;

public static class FontHandC
{
    public static short[] font_ascii =
    {
         -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,
         -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,
         -1,  26,  33,  60,  61,  62,  -1,  32,  64,  65,  63,  84,  29,  83,  28,  80, //  !"#$%&'()*+,-./
	     79,  70,  71,  72,  73,  74,  75,  76,  77,  78,  31,  30,  -1,  85,  -1,  27, // 0123456789:;<=>?
	     -1,   0,   1,   2,   3,   4,   5,   6,   7,   8,   9,  10,  11,  12,  13,  14, // @ABCDEFGHIJKLMNO
	     15,  16,  17,  18,  19,  20,  21,  22,  23,  24,  25,  68,  82,  69,  -1,  -1, // PQRSTUVWXYZ[\]^_
	     -1,  34,  35,  36,  37,  38,  39,  40,  41,  42,  43,  44,  45,  46,  47,  48, // `abcdefghijklmno
	     49,  50,  51,  52,  53,  54,  55,  56,  57,  58,  59,  66,  81,  67,  -1,  -1, // pqrstuvwxyz{|}~⌂

	     86,  87,  88,  89,  90,  91,  92,  93,  94,  95,  96,  97,  98,  99, 100, 101, // ÇüéâäàåçêëèïîìÄÅ
	    102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, // ÉæÆôöòûùÿÖÜ¢£¥₧ƒ
	    118, 119, 120, 121, 122, 123, 124, 125, 126,  -1,  -1,  -1,  -1,  -1,  -1,  -1, // áíóúñÑªº¿
	     -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,
         -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,
         -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,
         -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,
         -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,  -1,
    };

    public const int PART_SHADE = 0;
    public const int FULL_SHADE = 1;
    public const int DARKEN = 2;
    public const int TRICK = 3;
    public const int NO_SHADE = 255;

    public static JE_integer defaultBrightness = -3;
    public static JE_byte textGlowFont, textGlowBrightness = 6;
    public static JE_boolean levelWarningDisplay;
    public static JE_byte levelWarningLines;
    public static string[] levelWarningText = new string[10];
    public static JE_boolean warningRed;
    public static JE_byte warningSoundDelay;
    public static JE_word armorShipDelay;
    public static JE_byte warningCol;
    public static JE_shortint warningColChange;

    public static void JE_dString(Surface screen, int x, int y, string s, ushort font)
    {
        int bright = 0;

        for (int i = 0; i < s.Length && s[i] != '\0'; ++i)
        {
            int sprite_id = font_ascii[s[i]];

            switch (s[i])
            {
                case ' ':
                    x += 6;
                    break;

                case '~':
                    bright = (bright == 0) ? 2 : 0;
                    break;

                default:
                    if (sprite_id != -1)
                    {
                        blit_sprite_dark(screen, x + 2, y + 2, font, sprite_id, false);
                        blit_sprite_hv_unsafe(screen, x, y, font, sprite_id, 0xf, (sbyte)(defaultBrightness + bright));

                        x += sprite(font, sprite_id).width + 1;
                    }
                    break;
            }
        }
    }

    public static int JE_fontCenter(string s, ushort font)
    {
        return 160 - (JE_textWidth(s, font) / 2);
    }
    public static int JE_textWidth(string s, ushort font)
    {
        int x = 0;

        for (int i = 0; i < s.Length && s[i] != '\0'; ++i)
        {
            int sprite_id = font_ascii[s[i]];

            if (s[i] == ' ')
                x += 6;
            else if (sprite_id != -1)
                x += sprite(font, sprite_id).width + 1;
        }

        return x;
    }
    public static void JE_textShade(Surface screen, int x, int y, string s, int colorbankI, int brightnessI, ushort shadetype)
    {
        byte colorbank = (byte)colorbankI;
        sbyte brightness = (sbyte)brightnessI;
        switch (shadetype)
        {
            case PART_SHADE:
                JE_outText(screen, x + 1, y + 1, s, 0, -1);
                JE_outText(screen, x, y, s, colorbank, brightness);
                break;
            case FULL_SHADE:
                JE_outText(screen, x - 1, y, s, 0, -1);
                JE_outText(screen, x + 1, y, s, 0, -1);
                JE_outText(screen, x, y - 1, s, 0, -1);
                JE_outText(screen, x, y + 1, s, 0, -1);
                JE_outText(screen, x, y, s, colorbank, brightness);
                break;
            case DARKEN:
                JE_outTextAndDarken(screen, x + 1, y + 1, s, colorbank, brightness, TINY_FONT);
                break;
            case TRICK:
                JE_outTextModify(screen, x, y, s, colorbank, brightness, TINY_FONT);
                break;
        }
    }
    public static void JE_outText(Surface screen, int x, int y, string s, int colorbankI, int brightnessI)
    {
        byte colorbank = (byte)colorbankI;
        sbyte brightness = (sbyte)brightnessI;

        int bright = 0;

        for (int i = 0; i < s.Length && s[i] != '\0'; ++i)
        {
            int sprite_id = font_ascii[s[i]];

            switch (s[i])
            {
                case ' ':
                    x += 6;
                    break;

                case '~':
                    bright = (bright == 0) ? 4 : 0;
                    break;

                default:
                    if (sprite_id != -1 && sprite(TINY_FONT, sprite_id) != null)
                    {
                        if (brightness >= 0)
                            blit_sprite_hv_unsafe(screen, x, y, TINY_FONT, sprite_id, colorbank, (sbyte)(brightness + bright));
                        else
                            blit_sprite_dark(screen, x, y, TINY_FONT, sprite_id, true);

                        x += sprite(TINY_FONT, sprite_id).width + 1;
                    }
                    break;
            }
        }
    }
    public static void JE_outTextModify(Surface screen, int x, int y, string s, int filterI, int brightnessI, ushort font)
    {
        byte filter = (byte)filterI;
        sbyte brightness = (sbyte)brightnessI;

        for (int i = 0; i < s.Length && s[i] != '\0'; ++i)
        {
            int sprite_id = font_ascii[s[i]];

            if (s[i] == ' ')
            {
                x += 6;
            }
            else if (sprite_id != -1)
            {
                blit_sprite_hv_blend(screen, x, y, font, sprite_id, filter, brightness);

                x += sprite(font, sprite_id).width + 1;
            }
        }
    }
    public static void JE_outTextAdjust(Surface screen, int x, int y, string s, int filterI, int brightnessI, ushort font, bool shadow)
    {
        byte filter = (byte)filterI;
        sbyte brightness = (sbyte)brightnessI;
        int bright = 0;

        for (int i = 0; i < s.Length && s[i] != '\0'; ++i)
        {
            int sprite_id = font_ascii[s[i]];

            switch (s[i])
            {
                case ' ':
                    x += 6;
                    break;

                case '~':
                    bright = (bright == 0) ? 4 : 0;
                    break;

                default:
                    if (sprite_id != -1 && sprite(TINY_FONT, sprite_id) != null)
                    {
                        if (shadow)
                            blit_sprite_dark(screen, x + 2, y + 2, font, sprite_id, false);
                        blit_sprite_hv(screen, x, y, font, sprite_id, filter, (sbyte)(brightness + bright));

                        x += sprite(font, sprite_id).width + 1;
                    }
                    break;
            }
        }
    }
    public static void JE_outTextAndDarken(Surface screen, int x, int y, string s, int colorbankI, int brightnessI, ushort font)
    {
        if (s == null)
            return;

        byte colorbank = (byte)colorbankI;
        sbyte brightness = (sbyte)brightnessI;
        int bright = 0;

        for (int i = 0; i < s.Length && s[i] != '\0'; ++i)
        {
            int sprite_id = font_ascii[s[i]];

            switch (s[i])
            {
                case ' ':
                    x += 6;
                    break;

                case '~':
                    bright = (bright == 0) ? 4 : 0;
                    break;

                default:
                    if (sprite_id != -1 && sprite(TINY_FONT, sprite_id) != null)
                    {
                        blit_sprite_dark(screen, x + 1, y + 1, font, sprite_id, false);
                        blit_sprite_hv_unsafe(screen, x, y, font, sprite_id, colorbank, (sbyte)(brightness + bright));

                        x += sprite(font, sprite_id).width + 1;
                    }
                    break;
            }
        }
    }

    public static void JE_updateWarning(Surface screen)
    {
        if (delaycount2() == 0)
        { /*Update Color Bars*/

            warningCol += (byte)warningColChange;
            if (warningCol > 14 * 16 + 10 || warningCol < 14 * 16 + 4)
            {
                warningColChange = (sbyte)(-warningColChange);
            }
            fill_rectangle_xy(screen, 0, 0, 319, 5, warningCol);
            fill_rectangle_xy(screen, 0, 194, 319, 199, warningCol);
            JE_showVGA();

            setjasondelay2(6);

            if (warningSoundDelay > 0)
            {
                warningSoundDelay--;
            }
            else
            {
                warningSoundDelay = 14;
                JE_playSampleNum(17);
            }
        }
    }

    public static IEnumerator e_JE_outTextGlow(Surface screen, int x, int y, string s)
    {
        int z;
        JE_byte c = 15;

        if (warningRed)
        {
            c = 7;
        }

        JE_outTextAdjust(screen, x - 1, y, s, 0, -12, textGlowFont, false);
        JE_outTextAdjust(screen, x, y - 1, s, 0, -12, textGlowFont, false);
        JE_outTextAdjust(screen, x + 1, y, s, 0, -12, textGlowFont, false);
        JE_outTextAdjust(screen, x, y + 1, s, 0, -12, textGlowFont, false);
        if (frameCountMax > 0)
            for (z = 1; z <= 12; z++)
            {
                setjasondelay(frameCountMax);
                JE_outTextAdjust(screen, x, y, s, c, z - 10, textGlowFont, false);
                if (JE_anyButton())
                {
                    frameCountMax = 0;
                }

                //NETWORK_KEEP_ALIVE();

                JE_showVGA();

                yield return coroutine_wait_delay();
            }
        for (z = (frameCountMax == 0) ? 6 : 12; z >= textGlowBrightness; z--)
        {
            setjasondelay(frameCountMax);
            JE_outTextAdjust(screen, x, y, s, c, z - 10, textGlowFont, false);
            if (JE_anyButton())
            {
                frameCountMax = 0;
            }

            //NETWORK_KEEP_ALIVE();

            JE_showVGA();

            yield return coroutine_wait_delay();
        }
        textGlowBrightness = 6;
    }
}