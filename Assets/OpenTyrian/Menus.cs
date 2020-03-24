using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static VarzC;
using static PicLoadC;
using static VideoC;
using static FontHandC;
using static SpriteC;
using static PaletteC;
using static CoroutineRunner;
using static KeyboardC;
using static SetupC;
using static SndMastC;
using static NortsongC;
using static ConfigC;
using static EpisodesC;

using System.Collections;
using UnityEngine;

public static class MenusC
{
#if TYRIAN2000
    public const int GAMEPLAY_NAME_COUNT = 6;
#else
    public const int GAMEPLAY_NAME_COUNT = 5;
#endif

    public static string[] episode_name = EmptyArray(6, ""), difficulty_name = EmptyArray(7, ""), gameplay_name = EmptyArray(GAMEPLAY_NAME_COUNT, "");

    public static IEnumerator e_select_gameplay(bool[] refResult)
    { UnityEngine.Debug.Log("e_select_gameplay");
        refResult[0] = false;

        JE_loadPic(VGAScreen, 2, false);
        JE_dString(VGAScreen, JE_fontCenter(gameplay_name[0], FONT_SHAPES), 20, gameplay_name[0], FONT_SHAPES);

        int gameplay = 1,
            gameplay_max = GAMEPLAY_NAME_COUNT - 1;

        bool fade_in = true;
        for (; ; )
        {
            for (int i = 1; i <= gameplay_max; i++)
            {
                JE_outTextAdjust(VGAScreen, JE_fontCenter(gameplay_name[i], SMALL_FONT_SHAPES), i * 24 + 30, gameplay_name[i], 15, -4 + (i == gameplay ? 2 : 0) - (i == (GAMEPLAY_NAME_COUNT - 1) ? 4 : 0), SMALL_FONT_SHAPES, true);
            }
            JE_showVGA();

            if (fade_in)
            {
                yield return Run(e_fade_palette(colors, 10, 0, 255));
                fade_in = false;
            }

            yield return Run(e_JE_textMenuWait(null, false));

            if (newkey)
            {
                switch (lastkey_sym)
                {
                    case KeyCode.UpArrow:
                        if (--gameplay < 1)
                            gameplay = gameplay_max;
                        JE_playSampleNum(S_CURSOR);
                        break;
                    case KeyCode.DownArrow:
                        if (++gameplay > gameplay_max)
                            gameplay = 1;
                        JE_playSampleNum(S_CURSOR);
                        break;

                    case KeyCode.Return:
                        if (gameplay == GAMEPLAY_NAME_COUNT - 1)
                        {
                            JE_playSampleNum(S_SPRING);
                            /* TODO: NETWORK */
                            //fprintf(stderr, "error: networking via menu not implemented\n");
                            break;
                        }
                        JE_playSampleNum(S_SELECT);
                        yield return Run(e_fade_black(10));

                        onePlayerAction = (gameplay == 2);
                        twoPlayerMode = (gameplay == GAMEPLAY_NAME_COUNT - 2);
                        refResult[0] = true;
                        yield break;

                    case KeyCode.Escape:
                        JE_playSampleNum(S_SPRING);
                        /* fading handled elsewhere
                        fade_black(10); */

                        refResult[0] = false;
                        yield break;

                    default:
                        break;
                }
            }
        }
    }

    public static IEnumerator e_select_episode(bool[] refResult)
    { UnityEngine.Debug.Log("e_select_episode");
        refResult[0] = false;

        JE_loadPic(VGAScreen, 2, false);
        JE_dString(VGAScreen, JE_fontCenter(episode_name[0], FONT_SHAPES), 20, episode_name[0], FONT_SHAPES);

        int episode = 1, episode_max = EPISODE_AVAILABLE;

        bool fade_in = true;
        for (; ; )
        {
            for (int i = 1; i <= episode_max; i++)
            {
                JE_outTextAdjust(VGAScreen, 20, i * 30 + 20, episode_name[i], 15, -4 + (i == episode ? 2 : 0) - (!episodeAvail[i - 1] ? 4 : 0), SMALL_FONT_SHAPES, true);
            }
            JE_showVGA();

            if (fade_in)
            {
                yield return Run(e_fade_palette(colors, 10, 0, 255));
                fade_in = false;
            }

            yield return Run(e_JE_textMenuWait(null, false));

            if (newkey)
            {
                switch (lastkey_sym)
                {
                    case KeyCode.UpArrow:
                        episode--;
                        if (episode < 1)
                        {
                            episode = episode_max;
                        }
                        JE_playSampleNum(S_CURSOR);
                        break;
                    case KeyCode.DownArrow:
                        episode++;
                        if (episode > episode_max)
                        {
                            episode = 1;
                        }
                        JE_playSampleNum(S_CURSOR);
                        break;

                    case KeyCode.Return:
                        if (!episodeAvail[episode - 1])
                        {
                            JE_playSampleNum(S_SPRING);
                            break;
                        }
                        JE_playSampleNum(S_SELECT);
                        yield return Run(e_fade_black(10));

                        JE_initEpisode(episode);
                        initial_episode_num = episodeNum;

                        refResult[0] = true;
                        yield break;

                    case KeyCode.Escape:
                        JE_playSampleNum(S_SPRING);
                        /* fading handled elsewhere
                        fade_black(10); */

                        refResult[0] = false;
                        yield break;

                    default:
                        break;
                }
            }
        }
    }

    public static IEnumerator e_select_difficulty(bool[] refResult)
    { UnityEngine.Debug.Log("e_select_difficulty");
        refResult[0] = false;

        JE_loadPic(VGAScreen, 2, false);
        JE_dString(VGAScreen, JE_fontCenter(difficulty_name[0], FONT_SHAPES), 20, difficulty_name[0], FONT_SHAPES);

        difficultyLevel = 2;
        int difficulty_max = 3;

        bool fade_in = true;
        for (; ; )
        {
            for (int i = 1; i <= difficulty_max; i++)
            {
                JE_outTextAdjust(VGAScreen, JE_fontCenter(difficulty_name[i], SMALL_FONT_SHAPES), i * 24 + 30, difficulty_name[i], 15, -4 + (i == difficultyLevel ? 2 : 0), SMALL_FONT_SHAPES, true);
            }
            JE_showVGA();

            if (fade_in)
            {
                yield return Run(e_fade_palette(colors, 10, 0, 255));
                fade_in = false;
            }

            yield return Run(e_JE_textMenuWait(null, false));

            if (keysactive[(int)KeyCode.LeftShift] || keysactive[(int)KeyCode.RightShift])
            {
                if ((difficulty_max < 4 && keysactive[(int)KeyCode.G]) ||
                    (difficulty_max == 4 && keysactive[(int)KeyCode.RightBracket]))
                {
                    difficulty_max++;
                }
            }
            else if (difficulty_max == 5 && keysactive[(int)KeyCode.L] && keysactive[(int)KeyCode.O] && keysactive[(int)KeyCode.R] && keysactive[(int)KeyCode.D])
            {
                difficulty_max++;
            }

            if (newkey)
            {
                switch (lastkey_sym)
                {
                    case KeyCode.UpArrow:
                        difficultyLevel--;
                        if (difficultyLevel < 1)
                        {
                            difficultyLevel = difficulty_max;
                        }
                        JE_playSampleNum(S_CURSOR);
                        break;
                    case KeyCode.DownArrow:
                        difficultyLevel++;
                        if (difficultyLevel > difficulty_max)
                        {
                            difficultyLevel = 1;
                        }
                        JE_playSampleNum(S_CURSOR);
                        break;

                    case KeyCode.Return:
                        JE_playSampleNum(S_SELECT);
                        /* fading handled elsewhere
                        fade_black(10); */

                        if (difficultyLevel == 6)
                        {
                            difficultyLevel = 8;
                        }
                        else if (difficultyLevel == 5)
                        {
                            difficultyLevel = 6;
                        }
                        refResult[0] = true;
                        yield break;

                    case KeyCode.Escape:
                        JE_playSampleNum(S_SPRING);
                        /* fading handled elsewhere
                        fade_black(10); */

                        refResult[0] = false;
                        yield break;

                    default:
                        break;
                }
            }
        }
    }
}