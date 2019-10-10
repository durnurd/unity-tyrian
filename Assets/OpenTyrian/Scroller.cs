using static FontC;
using static JoystickC;
using static JukeboxC;
using static KeyboardC;
using static LoudnessC;
using static LibC;
using static NortsongC;
using static NortVarsC;
using static OpenTyrC;
using static PaletteC;
using static ScrollerC;
using static SpriteC;
using static VarzC;
using static VGA256dC;
using static VideoC;
using static CoroutineRunner;
using static FontC.Font;
using static FontC.FontAlignment;
using static System.Math;

using System.Collections;
using UnityEngine;

public static class ScrollerC
{
    const int LINE_HEIGHT = 15;

    const int MAX_BEER = 5;
    const int BEER_SHAPE = 241;

    public struct about_text_type
    {
        public int effect;
        public string text;
    }

    private static about_text_type a(int e, string t) => new about_text_type { effect = e, text = t };

    public static about_text_type[] about_text = new[]
    {
        a(0x30, "----- ~OpenTyrian~ -----"),
        a(0x00, ""),
        a(0x0b, "...eliminating Microsol,"),
        a(0x0b, "one planet at a time..."),
        a(0x00, ""),
        a(0x00, ""),
        a(0x30, "----- ~Developers~ -----"),
        a(0x00, ""),
        a(0x03, "Carl Reinke // Mindless"),
        a(0x07, "Yuri Schlesner // yuriks"),
        a(0x04, "Casey McCann // syntaxglitch"),
        a(0x00, ""),
        a(0x00, ""),
        a(0x30, "----- ~Thanks~ -----"),
        a(0x00, ""),
        a(0x0e, "Thanks to everyone who has"),
        a(0x0e, "assisted the developers by testing"),
        a(0x0e, "the game and reporting bugs."),
        a(0x00, ""),
        a(0x00, ""),
        a(0x05, "Thanks to ~DOSBox~ for the"),
        a(0x05, "FM-Synthesis emulator and"),
        a(0x05, "~AdPlug~ for the Loudness player."),
        a(0x00, ""),
        a(0x00, ""),
        a(0x32, "And special thanks to ~Jason Emery~"),
        a(0x32, "for making all this possible"),
        a(0x32, "by giving Tyrian to its fans."),
        a(0x00, ""),
        a(0x00, ""),
    /*	a(0x00, "This is line color test ~0~."),
	    a(0x01, "This is line color test ~1~."),
	    a(0x02, "This is line color test ~2~."),
	    a(0x03, "This is line color test ~3~."),
	    a(0x04, "This is line color test ~4~."),
	    a(0x05, "This is line color test ~5~."),
	    a(0x06, "This is line color test ~6~."),
	    a(0x07, "This is line color test ~7~."),
	    a(0x08, "This is line color test ~8~."),
	    a(0x09, "This is line color test ~9~."),
	    a(0x0a, "This is line color test ~A~."),
	    a(0x0b, "This is line color test ~B~."),
	    a(0x0c, "This is line color test ~C~."),
	    a(0x0d, "This is line color test ~D~."),
	    a(0x0e, "This is line color test ~E~."),
	    a(0x0f, "This is line color test ~F~."),*/
	    a(0x00, ""),
        a(0x00, ""),
        a(0x00, ""),
        a(0x00, ""),
        a(0x00, ""),
        a(0x00, ""),
        a(0x00, "Press a key to leave."),
        a(0x00, null)
    };

    struct coin_def_type
    {
        public int shape_num;
        public int frame_count;
        public bool reverse_anim;
    }

    private static coin_def_type c(int s, int f, bool r = false) => new coin_def_type { shape_num = s, frame_count = f, reverse_anim = r };

    const int MAX_COINS = 20;
    private static readonly coin_def_type[] coin_defs =
    {
        c(1, 6), c(7, 6), c(20, 6), c(26, 6), // Coins
	    c(14, 5, true), c(32, 5, true), c(51, 5, true) // Gems
    };

    struct coin_type { public int x, y, vel, type, cur_frame; public bool backwards; }
    struct beer_type { public int x, y, ay, vx, vy; };

    public static IEnumerator e_scroller_sine(about_text_type[] text)
    { UnityEngine.Debug.Log("e_scroller_sine");
        bool ale = (mt_rand() % 2) == 1;

        int visible_lines = 200 / LINE_HEIGHT + 1;
        int current_line = -visible_lines;
        int y = 0;
        bool fade_in = true;


        coin_type[] coins = new coin_type[MAX_COINS];
        beer_type[] beer = new beer_type[MAX_BEER];


        if (!ale)
            for (int i = 0; i < MAX_COINS; i++)
            {
                coins[i].x = (int)(mt_rand() % (vga_width - 12));
                coins[i].y = (int)(mt_rand() % (vga_height - 20 - 14));

                coins[i].vel = (int)((mt_rand() % 4) + 1);
                coins[i].type = (int)(mt_rand() % coin_defs.Length);
                coins[i].cur_frame = (int)(mt_rand() % coin_defs[coins[i].type].frame_count);
                coins[i].backwards = false;
            }

        yield return Run(e_fade_black(10));

        yield return coroutine_wait_noinput(true, true, true);

        play_song(40); // BEER

        while (!JE_anyButton())
        {
            setdelay(3);

            JE_clr256(VGAScreen);

            if (!ale)
            {
                for (int i = 0; i < MAX_COINS / 2; i++)
                {
                    coin_type coin = coins[i];
                    blit_sprite2(VGAScreen, coin.x, coin.y, eShapes[4], coin_defs[coin.type].shape_num + coin.cur_frame);
                }
            }

            for (int i = 0; i < visible_lines; i++)
            {
                if (current_line + i >= 0)
                {
                    if (current_line + i >= text.Length || text[current_line + i].text == null)
                    {
                        break;
                    }

                    int line_x = VGAScreen.w / 2;
                    int line_y = i * LINE_HEIGHT - y;

                    // smooths edges on sine-wave text
                    if ((text[i + current_line].effect & 0x20) != 0)
                    {
                        draw_font_hv(VGAScreen, line_x + 1, line_y, text[i + current_line].text, normal_font, centered, (byte)(text[i + current_line].effect & 0x0f), -10);
                        draw_font_hv(VGAScreen, line_x - 1, line_y, text[i + current_line].text, normal_font, centered, (byte)(text[i + current_line].effect & 0x0f), -10);
                    }

                    draw_font_hv(VGAScreen, line_x, line_y, text[i + current_line].text, normal_font, centered, (byte)(text[i + current_line].effect & 0x0f), -4);

                    if ((text[i + current_line].effect & 0x10) != 0)
                    {
                        for (int j = 0; j < LINE_HEIGHT; j++)
                        {
                            if (line_y + j >= 10 && line_y + j <= vga_height - 10)
                            {
                                int waver = (int)(Sin((((line_y + j) / 2) % 10) / 5.0f * PI) * 3);
                                System.Array.Copy(VGAScreen.pixels, VGAScreen.w * (line_y + j), VGAScreen.pixels, VGAScreen.w * (line_y + j) + waver, VGAScreen.w);
                            }
                        }
                    }
                }
            }

            if (++y == LINE_HEIGHT)
            {
                y = 0;

                if (current_line < 0 || text[current_line].text != null)
                    ++current_line;
                else
                    current_line = -visible_lines;
            }

            if (!ale)
            {
                for (int i = MAX_COINS / 2; i < MAX_COINS; i++)
                {
                    coin_type coin = coins[i];
                    blit_sprite2(VGAScreen, coin.x, coin.y, eShapes[4], coin_defs[coin.type].shape_num + coin.cur_frame);
                }
            }

            fill_rectangle_xy(VGAScreen, 0, 0, vga_width - 1, 14, 0);
            fill_rectangle_xy(VGAScreen, 0, vga_height - 14, vga_width - 1, vga_height - 1, 0);

            if (!ale)
            {
                for (int i = 0; i < MAX_COINS; i++)
                {
                    coin_type coin = coins[i];

                    if (coin.backwards)
                    {
                        coin.cur_frame--;
                    }
                    else
                    {
                        coin.cur_frame++;
                    }
                    if (coin.cur_frame == coin_defs[coin.type].frame_count)
                    {
                        if (coin_defs[coin.type].reverse_anim)
                        {
                            coin.backwards = true;
                            coin.cur_frame -= 2;
                        }
                        else
                        {
                            coin.cur_frame = 0;
                        }
                    }
                    if (coin.cur_frame == -1)
                    {
                        coin.cur_frame = 1;
                        coin.backwards = false;
                    }

                    coin.y += coin.vel;
                    if (coin.y > vga_height - 14)
                    {
                        coin.x = (int)(mt_rand() % (vga_width - 12));
                        coin.y = 0;

                        coin.vel = (int)((mt_rand() % 4) + 1);
                        coin.type = (int)(mt_rand() % coin_defs.Length);
                        coin.cur_frame = (int)(mt_rand() % coin_defs[coin.type].frame_count);
                    }

                    coins[i] = coin;
                }
            }
            else
            {
                for (uint i = 0; i < beer.Length; i++)
                {
                    while (beer[i].vx == 0)
                    {
                        beer[i].x = (int)mt_rand() % (vga_width - 24);
                        beer[i].y = (int)mt_rand() % (vga_height - 28 - 50);

                        beer[i].vx = ((int)mt_rand() % 5) - 2;
                    }

                    beer[i].vy++;

                    if (beer[i].x + beer[i].vx > vga_width - 24 || beer[i].x + beer[i].vx < 0) // check if the beer hit the sides
                    {
                        beer[i].vx = -beer[i].vx;
                    }
                    beer[i].x += beer[i].vx;

                    if (beer[i].y + beer[i].vy > vga_height - 28) // check if the beer hit the bottom
                    {
                        if ((beer[i].vy) < 8) // make sure the beer bounces!
                        {
                            beer[i].vy += (int)mt_rand() % 2;
                        }
                        else if (beer[i].vy > 16)
                        { // make sure the beer doesn't bounce too high
                            beer[i].vy = 16;
                        }
                        beer[i].vy = -beer[i].vy + ((int)mt_rand() % 3 - 1);

                        beer[i].x += (beer[i].vx > 0 ? 1 : -1) * (i % 2 != 0 ? 1 : -1);
                    }
                    beer[i].y += beer[i].vy;

                    blit_sprite2x2(VGAScreen, beer[i].x, beer[i].y, eShapes[4], BEER_SHAPE);
                }
            }

            JE_showVGA();

            if (fade_in)
            {
                fade_in = false;
                yield return Run(e_fade_palette(colors, 10, 0, 255));

                set_colors(new Color32(255, 255, 255, 254), 254, 254);
            }

            yield return coroutine_wait_delay();
        }

        yield return Run(e_fade_black(10));
    }
}