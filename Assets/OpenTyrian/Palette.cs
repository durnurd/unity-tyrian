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

using System;
using System.Collections;
using System.IO;
using UnityEngine;

using static FileIO;
using static VarzC;
using static NortsongC;
using static VideoC;

class PaletteC
{
#if TYRIAN2000
    public const int PALETTE_COUNT = 24;
#else
    public const int PALETTE_COUNT = 23;
#endif

    public static Color32[][] palettes = new Color32[PALETTE_COUNT][];
    private static Color32[] palette = new Color32[256];

    public static Color32[] colors = new Color32[256]; //TODO: get rid of this

    public static void JE_loadPals()
    {
        BinaryReader f = open("palette.dat");

        int palette_count = (int)f.BaseStream.Length / (256 * 3);

        for (int p = 0; p < palette_count; ++p)
        {
            palettes[p] = new Color32[256];
            for (int i = 0; i < 256; ++i)
            {
                // The VGA hardware palette used only 6 bits per component, so the values need to be rescaled to
                // 8 bits. The naive way to do this is to simply do (c << 2), padding it with 0's, however this
                // makes the maximum value 252 instead of the proper 255. A trick to fix this is to use the upper 2
                // bits of the original value instead. This ensures that the value goes to 255 as the original goes
                // to 63.

                palettes[p][i].a = (byte)i;
                int c = f.ReadByte();
                palettes[p][i].r = (byte)((c << 2) | (c >> 4));
                c = f.ReadByte();
                palettes[p][i].g = (byte)((c << 2) | (c >> 4));
                c = f.ReadByte();
                palettes[p][i].b = (byte)((c << 2) | (c >> 4));
            }
        }
        Array.Copy(palettes[0], palette, palette.Length);
    }

    public static void set_palette(Color32[] colors, uint first_color, uint last_color)
    { UnityEngine.Debug.Log("set_palette");
        Array.Copy(colors, first_color, palette, first_color, last_color - first_color + 1);
        
        SDL_SetColors(palette, first_color, last_color - first_color + 1);
    }

    public static void set_colors(Color32 color, uint first_color, uint last_color)
    {
        for (uint i = first_color; i <= last_color; ++i) {
            palette[i] = color;
        }

        SDL_SetColors(palette, first_color, last_color - first_color + 1);
    }

    private static void init_step_fade_palette(int[][] diff, Color32[] colors, int first_color, int last_color)
    {
        for (int i = first_color; i <= last_color; i++)
        {
            diff[i][0] = (int)colors[i].r - palette[i].r;
            diff[i][1] = (int)colors[i].g - palette[i].g;
            diff[i][2] = (int)colors[i].b - palette[i].b;
        }
    }

    private static void init_step_fade_solid(int[][] diff, Color32 color, int first_color, int last_color)
    {
        for (int i = first_color; i <= last_color; i++)
        {
            diff[i][0] = (int)color.r - palette[i].r;
            diff[i][1] = (int)color.g - palette[i].g;
            diff[i][2] = (int)color.b - palette[i].b;
        }
    }

    private static void step_fade_palette(int[][] diff, int steps, int first_color, int last_color)
    {
        for (int i = first_color; i <= last_color; i++)
        {
            int[] delta = { (int)(diff[i][0] / steps), (int)(diff[i][1] / steps), (int)(diff[i][2] / steps) };

            diff[i][0] -= delta[0];
            diff[i][1] -= delta[1];
            diff[i][2] -= delta[2];

            palette[i].r = (byte)(palette[i].r + delta[0]);
            palette[i].g = (byte)(palette[i].g + delta[1]);
            palette[i].b = (byte)(palette[i].b + delta[2]);
        }


        SDL_SetColors(palette, 0, 256);
    }

    public static IEnumerator e_fade_palette(Color32[] colors, int steps, int first_color, int last_color)
    { UnityEngine.Debug.Log("e_fade_palette");
        int[][] diff = DoubleEmptyArray(256, 3, 0);
        init_step_fade_palette(diff, colors, first_color, last_color);

        for (; steps > 0; steps--)
        {
            setdelay(1);

            step_fade_palette(diff, steps, first_color, last_color);

            yield return coroutine_wait_delay();
        }
    }

    public static IEnumerator e_fade_solid(Color32 color, int steps, int first_color, int last_color)
    { UnityEngine.Debug.Log("e_fade_solid");
        int[][] diff = DoubleEmptyArray(256, 3, 0);
        init_step_fade_solid(diff, color, first_color, last_color);

    	for (; steps > 0; steps--)
    	{
    		setdelay(1);

            step_fade_palette(diff, steps, first_color, last_color);

    		yield return coroutine_wait_delay();
    	}
    }

    public static IEnumerator e_fade_black(int steps)
    { UnityEngine.Debug.Log("e_fade_black");
        return e_fade_solid(new Color32(0, 0, 0, 255), steps, 0, 255);
    }

    public static IEnumerator e_fade_white(int steps)
    { UnityEngine.Debug.Log("e_fade_white");
        return e_fade_solid(new Color32(255, 255, 255, 255), steps, 0, 255);
    }

}