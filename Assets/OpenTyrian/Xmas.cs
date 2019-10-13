using static PaletteC;
using static VideoC;
using static FontC;
using static FontC.Font;
using static FontC.FontAlignment;
using static SetupC;
using static CoroutineRunner;
using static KeyboardC;

using System.Collections;
using UnityEngine;

public static class XmasC
{
    public static bool xmas;

    public static bool xmasOverride;

    public static bool xmas_time()
    {
        return xmasOverride || System.DateTime.Now.Month == 12;
    }

    public static IEnumerator e_xmas_prompt(bool[] outRet)
    {
        string[] prompt =
        {
            "Christmas has been detected.",
            "Activate Christmas?",
        };
        string[] choice =
        {
            "Yes",
            "No",
        };

        set_palette(palettes[0], 0, 255);

        for (int i = 0; i < prompt.Length; ++i)
            draw_font_hv(VGAScreen, 320 / 2, 85 + 15 * i, prompt[i], normal_font, centered, (byte)(((i % 2) != 0) ? 2 : 4), -2);

        uint selection = 0;

        bool decided = false, quit = false;
        while (!decided)
        {
            for (int i = 0; i < choice.Length; ++i)
                draw_font_hv(VGAScreen, 320 / 2 - 20 + 40 * i, 120, choice[i], normal_font, centered, 15, (sbyte)((selection == i) ? -2 : -4));

            JE_showVGA();

            yield return Run(e_JE_textMenuWait(null, false));

            if (newkey)
            {
                switch (lastkey_sym)
                {
                    case KeyCode.LeftArrow:
                        if (selection == 0)
                            selection = 2;
                        selection--;
                        break;
                    case KeyCode.RightArrow:
                        selection++;
                        selection %= 2;
                        break;

                    case KeyCode.Return:
                        decided = true;
                        break;
                    case KeyCode.Escape:
                        decided = true;
                        quit = true;
                        break;
                    default:
                        break;
                }
            }
        }

        yield return Run(e_fade_black(10));

        outRet[0] = selection == 0 && quit == false;
        yield break;
    }
}