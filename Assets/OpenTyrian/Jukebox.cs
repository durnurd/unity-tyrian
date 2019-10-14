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


using static FontC;
using static FontC.Font;
using static FontC.FontAlignment;
using static JoystickC;
using static KeyboardC;
using static LdsPlayC;
using static LoudnessC;
using static LibC;
using static NortsongC;
using static OpenTyrC;
using static PaletteC;
using static SpriteC;
using static StarLibC;
using static VideoC;
using static VarzC;
using static MusMastC;
using static SndMastC;

using System.Collections;
using UnityEngine;

public static class JukeboxC {
    public static IEnumerator e_jukebox()
    {
        bool trigger_quit = false,  // true when user wants to quit
             quitting = false;

        bool hide_text = false;

        bool fade_looped_songs = true, fading_song = false;
        bool stopped = false;

        bool fx = false;
        int fx_num = 0;

        int palette_fade_steps = 15;

        int[][] diff = DoubleEmptyArray<int>(256, 3, 0);
        init_step_fade_palette(diff, generatePalette(), 0, 255);

        JE_starlib_init();

        int fade_volume = tyrMusicVolume;

        for (; ; )
        {
            if (!stopped && !audio_disabled)
            {
                if (songlooped && fade_looped_songs)
                    fading_song = true;

                if (fading_song)
                {
                    if (fade_volume > 5)
                    {
                        fade_volume -= 2;
                    }
                    else
                    {
                        fade_volume = tyrMusicVolume;

                        fading_song = false;
                    }

                    set_volume(fade_volume, fxVolume);
                }

                if (!playing || (songlooped && fade_looped_songs && !fading_song))
                    play_song(mt_rand_i() % MUSIC_NUM);
            }

            setdelay(1);

            JE_clr256(VGAScreenSeg);

            // starlib input needs to be rewritten
            yield return e_JE_starlib_main();

            push_joysticks_as_keyboard();
            service_SDL_events(true);

            ushort x, y;

            if (!hide_text)
            {
                string buffer;

                if (fx)
                    buffer = (fx_num + 1) + " " + soundTitle[fx_num];
                else
                    buffer = (song_playing + 1) + " " + musicTitle[song_playing];

                x = (ushort)(VGAScreen.w / 2);

                draw_font_hv(VGAScreen, x, 170, "Press ESC to quit the jukebox.", small_font, centered, 1, 0);
                draw_font_hv(VGAScreen, x, 180, "Arrow keys change the song being played.", small_font, centered, 1, 0);
                draw_font_hv(VGAScreen, x, 190, buffer, small_font, centered, 1, 4);
            }

            if (palette_fade_steps > 0)
                step_fade_palette(diff, palette_fade_steps--, 0, 255);

            JE_showVGA();

            yield return coroutine_wait_delay();

            // quit on mouse click
            if (JE_mousePosition(out x, out y) > 0)
                trigger_quit = true;

            if (newkey)
            {
                switch (lastkey_sym)
                {
                    case KeyCode.Escape: // quit jukebox
                    case KeyCode.Q:
                        trigger_quit = true;
                        break;

                    case KeyCode.Space:
                        hide_text = !hide_text;
                        break;

                    case KeyCode.F:
                        fading_song = !fading_song;
                        break;
                    case KeyCode.N:
                        fade_looped_songs = !fade_looped_songs;
                        break;

                    case KeyCode.Slash: // switch to sfx mode
                        fx = !fx;
                        break;
                    case KeyCode.Comma:
                        if (fx && --fx_num < 0)

                            fx_num = SAMPLE_COUNT - 1;
                        break;
                    case KeyCode.Period:
                        if (fx && ++fx_num >= SAMPLE_COUNT)
                            fx_num = 0;
                        break;
                    case KeyCode.Semicolon:
                        if (fx)
                            JE_playSampleNum(fx_num + 1);
                        break;

                    case KeyCode.LeftArrow:
                    case KeyCode.UpArrow:
                        play_song((song_playing > 0 ? song_playing : MUSIC_NUM) - 1);
                        stopped = false;
                        break;
                    case KeyCode.Return:
                    case KeyCode.RightArrow:
                    case KeyCode.DownArrow:
                        play_song((song_playing + 1) % MUSIC_NUM);
                        stopped = false;
                        break;
                    case KeyCode.S: // stop song
                        stop_song();
                        stopped = true;
                        break;
                    case KeyCode.R: // restart song
                        restart_song();
                        stopped = false;
                        break;

                    default:
                        break;
                }
            }

            // user wants to quit, start fade-out
            if (trigger_quit && !quitting)
            {
                palette_fade_steps = 15;

                Color32 black = new Color32(0, 0, 0, 0);
                init_step_fade_solid(diff, black, 0, 255);

                quitting = true;
            }

            // if fade-out finished, we can finally quit
            if (quitting && palette_fade_steps == 0)
                break;
        }

        set_volume(tyrMusicVolume, fxVolume);
    }

    static readonly byte[] vga_palette = {
      0,   0,   0,   0,   0, 168,   0, 168,   0,   0, 168, 168,
    168,   0,   0, 168,   0, 168, 168,  84,   0, 168, 168, 168,
     84,  84,  84,  84,  84, 252,  84, 252,  84,  84, 252, 252,
    252,  84,  84, 252,  84, 252, 252, 252,  84, 252, 252, 252,
      0,   0,   0,  20,  20,  20,  32,  32,  32,  44,  44,  44,
     56,  56,  56,  68,  68,  68,  80,  80,  80,  96,  96,  96,
    112, 112, 112, 128, 128, 128, 144, 144, 144, 160, 160, 160,
    180, 180, 180, 200, 200, 200, 224, 224, 224, 252, 252, 252,
      0,   0, 252,  64,   0, 252, 124,   0, 252, 188,   0, 252,
    252,   0, 252, 252,   0, 188, 252,   0, 124, 252,   0,  64,
    252,   0,   0, 252,  64,   0, 252, 124,   0, 252, 188,   0,
    252, 252,   0, 188, 252,   0, 124, 252,   0,  64, 252,   0,
      0, 252,   0,   0, 252,  64,   0, 252, 124,   0, 252, 188,
      0, 252, 252,   0, 188, 252,   0, 124, 252,   0,  64, 252,
    124, 124, 252, 156, 124, 252, 188, 124, 252, 220, 124, 252,
    252, 124, 252, 252, 124, 220, 252, 124, 188, 252, 124, 156,
    252, 124, 124, 252, 156, 124, 252, 188, 124, 252, 220, 124,
    252, 252, 124, 220, 252, 124, 188, 252, 124, 156, 252, 124,
    124, 252, 124, 124, 252, 156, 124, 252, 188, 124, 252, 220,
    124, 252, 252, 124, 220, 252, 124, 188, 252, 124, 156, 252,
    180, 180, 252, 196, 180, 252, 216, 180, 252, 232, 180, 252,
    252, 180, 252, 252, 180, 232, 252, 180, 216, 252, 180, 196,
    252, 180, 180, 252, 196, 180, 252, 216, 180, 252, 232, 180,
    252, 252, 180, 232, 252, 180, 216, 252, 180, 196, 252, 180,
    180, 252, 180, 180, 252, 196, 180, 252, 216, 180, 252, 232,
    180, 252, 252, 180, 232, 252, 180, 216, 252, 180, 196, 252,
      0,   0, 112,  28,   0, 112,  56,   0, 112,  84,   0, 112,
    112,   0, 112, 112,   0,  84, 112,   0,  56, 112,   0,  28,
    112,   0,   0, 112,  28,   0, 112,  56,   0, 112,  84,   0,
    112, 112,   0,  84, 112,   0,  56, 112,   0,  28, 112,   0,
      0, 112,   0,   0, 112,  28,   0, 112,  56,   0, 112,  84,
      0, 112, 112,   0,  84, 112,   0,  56, 112,   0,  28, 112,
     56,  56, 112,  68,  56, 112,  84,  56, 112,  96,  56, 112,
    112,  56, 112, 112,  56,  96, 112,  56,  84, 112,  56,  68,
    112,  56,  56, 112,  68,  56, 112,  84,  56, 112,  96,  56,
    112, 112,  56,  96, 112,  56,  84, 112,  56,  68, 112,  56,
     56, 112,  56,  56, 112,  68,  56, 112,  84,  56, 112,  96,
     56, 112, 112,  56,  96, 112,  56,  84, 112,  56,  68, 112,
     80,  80, 112,  88,  80, 112,  96,  80, 112, 104,  80, 112,
    112,  80, 112, 112,  80, 104, 112,  80,  96, 112,  80,  88,
    112,  80,  80, 112,  88,  80, 112,  96,  80, 112, 104,  80,
    112, 112,  80, 104, 112,  80,  96, 112,  80,  88, 112,  80,
     80, 112,  80,  80, 112,  88,  80, 112,  96,  80, 112, 104,
     80, 112, 112,  80, 104, 112,  80,  96, 112,  80,  88, 112,
      0,   0,  64,  16,   0,  64,  32,   0,  64,  48,   0,  64,
     64,   0,  64,  64,   0,  48,  64,   0,  32,  64,   0,  16,
     64,   0,   0,  64,  16,   0,  64,  32,   0,  64,  48,   0,
     64,  64,   0,  48,  64,   0,  32,  64,   0,  16,  64,   0,
      0,  64,   0,   0,  64,  16,   0,  64,  32,   0,  64,  48,
      0,  64,  64,   0,  48,  64,   0,  32,  64,   0,  16,  64,
     32,  32,  64,  40,  32,  64,  48,  32,  64,  56,  32,  64,
     64,  32,  64,  64,  32,  56,  64,  32,  48,  64,  32,  40,
     64,  32,  32,  64,  40,  32,  64,  48,  32,  64,  56,  32,
     64,  64,  32,  56,  64,  32,  48,  64,  32,  40,  64,  32,
     32,  64,  32,  32,  64,  40,  32,  64,  48,  32,  64,  56,
     32,  64,  64,  32,  56,  64,  32,  48,  64,  32,  40,  64,
     44,  44,  64,  48,  44,  64,  52,  44,  64,  60,  44,  64,
     64,  44,  64,  64,  44,  60,  64,  44,  52,  64,  44,  48,
     64,  44,  44,  64,  48,  44,  64,  52,  44,  64,  60,  44,
     64,  64,  44,  60,  64,  44,  52,  64,  44,  48,  64,  44,
     44,  64,  44,  44,  64,  48,  44,  64,  52,  44,  64,  60,
     44,  64,  64,  44,  60,  64,  44,  52,  64,  44,  48,  64,
      0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
      0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0
    };

    static Color32[] generatePalette()
    {
        var palette = new Color32[vga_palette.Length / 3];
        for (int i = 0; i < palette.Length; ++i)
        {
            palette[i] = new Color32(vga_palette[i * 3 + 0], vga_palette[i * 3 + 1], vga_palette[i * 3 + 2], (byte)i);
        }
        return palette;
    }

}