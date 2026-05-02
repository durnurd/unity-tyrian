using static ConfigC;
using static FontC;
using static FontC.Font;
using static FontC.FontAlignment;
using static DestructC;
using static EditShipC;
using static EpisodesC;
using static FileIO;
using static HelpTextC;
//using static hg_revisionC;
using static JoystickC;
using static JukeboxC;
using static KeyboardC;
using static LoudnessC;
using static MainIntC;
using static MusMastC;
using static NetworkC;
using static NortsongC;
using static ParamsC;
using static PicLoadC;
using static ScrollerC;
using static SetupC;
using static SpriteC;
using static Tyrian2C;
using static XmasC;
using static VarzC;
using static VGA256dC;
using static VideoC;
//using static video_scaleC;
using static PaletteC;
using static SndMastC;

using static CoroutineRunner;

using System.Collections;
using UnityEngine;

public static class OpenTyrC {
public const string opentyrian_str = "OpenTyrian",
             opentyrian_version = "unknown revision";

    const int MENU_ABOUT = 0,
          //MENU_FULLSCREEN = 1,
          //MENU_SCALER = 2,
          // MENU_DESTRUCT,
          MENU_JUKEBOX = 1,
          MENU_RETURN = 2,
          MenuOptions_MAX = 3;


    static readonly string[] menu_items =
    {
        "About OpenTyrian",
        // "Toggle Fullscreen",
        // "Scaler: None",
		// "Play Destruct",
		"Jukebox",
        "Return to Main Menu",
    };

    static bool[] menu_items_disabled =
    {
        false,
        // !can_init_any_scaler(false) || !can_init_any_scaler(true),
        // false,
		// false,
		false,
        false,
    };


    public static IEnumerator e_opentyrian_menu()
    { UnityEngine.Debug.Log("e_opentyrian_menu");
        yield return Run(e_fade_black(10));
        JE_loadPic(VGAScreen, 13, false);

        draw_font_hv(VGAScreen, VGAScreen.w / 2, 5, opentyrian_str, large_font, centered, 15, -3);

        System.Array.Copy(VGAScreen.pixels, VGAScreen2.pixels, VGAScreen2.pixels.Length);

        JE_showVGA();

        play_song(36); // A Field for Mag

        int sel = 0;

        bool fade_in = true, quit = false;
        do
        {
            System.Array.Copy(VGAScreen2.pixels, VGAScreen.pixels, VGAScreen.pixels.Length);

            for (int i = 0; i < MenuOptions_MAX; i++)
            {
                string text = menu_items[i];

                //			if (i == MENU_SCALER)
                //			{
                //                    string buffer = "Scaler: " + scalers[temp_scaler].name);
                //text = buffer;
                //			}

                int y = i != MENU_RETURN ? i * 16 + 32 : 118;
                draw_font_hv(VGAScreen, VGAScreen.w / 2, y, text, normal_font, centered, 15, (sbyte)(menu_items_disabled[i] ? -8 : i != sel ? -4 : -2));
            }

            JE_showVGA();

            if (fade_in)
            {
                fade_in = false;
                yield return Run(e_fade_palette(colors, 20, 0, 255));
                yield return coroutine_wait_noinput(true, false, false);
            }

            yield return Run(e_JE_textMenuWait(null, false));

            if (newkey)
            {
                switch (lastkey_sym)
                {
                    case KeyCode.UpArrow:
                        do
                        {
                            if (sel-- == 0)
                                sel = MenuOptions_MAX - 1;
                        }
                        while (menu_items_disabled[sel]);

                        JE_playSampleNum(S_CURSOR);
                        break;
                    case KeyCode.DownArrow:
                        do
                        {
                            if (++sel >= MenuOptions_MAX)
                                sel = 0;
                        }
                        while (menu_items_disabled[sel]);

                        JE_playSampleNum(S_CURSOR);
                        break;

                    //case KeyCode.LeftArrow:
                    //	if (sel == MENU_SCALER)
                    //	{
                    //		do
                    //		{
                    //			if (temp_scaler == 0)
                    //				temp_scaler = scalers_count;
                    //			temp_scaler--;
                    //		}
                    //		while (!can_init_scaler(temp_scaler, fullscreen_enabled));

                    //		JE_playSampleNum(S_CURSOR);
                    //	}
                    //	break;
                    //case SDLK_RIGHT:
                    //	if (sel == MENU_SCALER)
                    //	{
                    //		do
                    //		{
                    //			temp_scaler++;
                    //			if (temp_scaler == scalers_count)
                    //				temp_scaler = 0;
                    //		}
                    //		while (!can_init_scaler(temp_scaler, fullscreen_enabled));

                    //		JE_playSampleNum(S_CURSOR);
                    //	}
                    //	break;

                    case KeyCode.Return:
                        switch (sel)
                        {
                            case MENU_ABOUT:
                                JE_playSampleNum(S_SELECT);

                                yield return Run(e_scroller_sine(about_text));

                                System.Array.Copy(VGAScreen2.pixels, VGAScreen.pixels, VGAScreen.pixels.Length);
                                JE_showVGA();
                                fade_in = true;
                                break;

                            //case MENU_FULLSCREEN:
                            //	JE_playSampleNum(S_SELECT);

                            //	if (!init_scaler(scaler, !fullscreen_enabled) && // try new fullscreen state
                            //		!init_any_scaler(!fullscreen_enabled) &&     // try any scaler in new fullscreen state
                            //		!init_scaler(scaler, fullscreen_enabled))    // revert on fail
                            //	{
                            //		exit(EXIT_FAILURE);
                            //	}
                            //	set_palette(colors, 0, 255); // for switching between 8 bpp scalers
                            //	break;

                            //case MENU_SCALER:
                            //	JE_playSampleNum(S_SELECT);

                            //	if (scaler != temp_scaler)
                            //	{
                            //		if (!init_scaler(temp_scaler, fullscreen_enabled) &&   // try new scaler
                            //			!init_scaler(temp_scaler, !fullscreen_enabled) &&  // try other fullscreen state
                            //			!init_scaler(scaler, fullscreen_enabled))          // revert on fail
                            //		{
                            //			exit(EXIT_FAILURE);
                            //		}
                            //		set_palette(colors, 0, 255); // for switching between 8 bpp scalers
                            //	}
                            //	break;

                            case MENU_JUKEBOX:
                                JE_playSampleNum(S_SELECT);

                                yield return Run(e_fade_black(10));
                                yield return Run(e_jukebox());

                                System.Array.Copy(VGAScreen2.pixels, VGAScreen.pixels, VGAScreen.pixels.Length);
                                JE_showVGA();
                                fade_in = true;
                                break;

                            case MENU_RETURN:
                                quit = true;
                                JE_playSampleNum(S_SPRING);
                                break;

                            case MenuOptions_MAX:
                                break;
                        }
                        break;

                    case KeyCode.Escape:
                        quit = true;
                        JE_playSampleNum(S_SPRING);
                        break;

                    default:
                        break;
                }
            }
        } while (!quit);
    }

    private static IEnumerator e_downloadAll()
    {
        yield return Run(e_download("cubetxt1.dat"));
        yield return Run(e_download("cubetxt2.dat"));
        yield return Run(e_download("cubetxt3.dat"));
        yield return Run(e_download("demo.1"));
        yield return Run(e_download("demo.2"));
        yield return Run(e_download("demo.3"));
        yield return Run(e_download("demo.4"));
        yield return Run(e_download("demo.5"));
        yield return Run(e_download("dpmi16bi.ovl"));
        yield return Run(e_download("estsc.shp"));
        yield return Run(e_download("exitmsg.bin"));
        yield return Run(e_download("file_id.diz"));
        yield return Run(e_download("levels1.dat"));
        yield return Run(e_download("levels2.dat"));
        yield return Run(e_download("levels3.dat"));
        yield return Run(e_download("levels4.dat"));
        yield return Run(e_download("loudness.awe"));
        yield return Run(e_download("music.mus"));
        yield return Run(e_download("netterm.int"));
        yield return Run(e_download("newshPOUND.shp"));
        yield return Run(e_download("newsh^.shp"));
        yield return Run(e_download("newsh~.shp"));
        yield return Run(e_download("newsh0.shp"));
        yield return Run(e_download("newsh1.shp"));
        yield return Run(e_download("newsh2.shp"));
        yield return Run(e_download("newsh3.shp"));
        yield return Run(e_download("newsh4.shp"));
        yield return Run(e_download("newsh5.shp"));
        yield return Run(e_download("newsh6.shp"));
        yield return Run(e_download("newsh7.shp"));
        yield return Run(e_download("newsh8.shp"));
        yield return Run(e_download("newsh9.shp"));
        yield return Run(e_download("newsha.shp"));
        yield return Run(e_download("newshb.shp"));
        yield return Run(e_download("newshc.shp"));
        yield return Run(e_download("newshd.shp"));
        yield return Run(e_download("newshe.shp"));
        yield return Run(e_download("newshf.shp"));
        yield return Run(e_download("newshg.shp"));
        yield return Run(e_download("newshh.shp"));
        yield return Run(e_download("newshi.shp"));
        yield return Run(e_download("newshj.shp"));
        yield return Run(e_download("newshk.shp"));
        yield return Run(e_download("newshl.shp"));
        yield return Run(e_download("newshm.shp"));
        yield return Run(e_download("newshn.shp"));
        yield return Run(e_download("newsho.shp"));
        yield return Run(e_download("newshp.shp"));
        yield return Run(e_download("newshr.shp"));
        yield return Run(e_download("newshs.shp"));
        yield return Run(e_download("newsht.shp"));
        yield return Run(e_download("newshu.shp"));
        yield return Run(e_download("newshv.shp"));
        yield return Run(e_download("palette.dat"));
        yield return Run(e_download("setup.ini"));
        yield return Run(e_download("shapes).dat"));
        yield return Run(e_download("shapesw.dat"));
        yield return Run(e_download("shapesx.dat"));
        yield return Run(e_download("shapesy.dat"));
        yield return Run(e_download("shapesz.dat"));
        yield return Run(e_download("shipedit.pcx"));
        yield return Run(e_download("tshp2.pcx"));
        yield return Run(e_download("tyrian.cdt"));
        yield return Run(e_download("tyrian.hdt"));
        yield return Run(e_download("tyrian.pic"));
        yield return Run(e_download("tyrian.shp"));
        yield return Run(e_download("tyrian.snd"));
        yield return Run(e_download("tyrian1.lvl"));
        yield return Run(e_download("tyrian2.lvl"));
        yield return Run(e_download("tyrian3.lvl"));
        yield return Run(e_download("tyrian4.lvl"));
        yield return Run(e_download("tyrianc.shp"));
        yield return Run(e_download("tyrset.pcx"));
        yield return Run(e_download("user1.shp"));
        yield return Run(e_download("user2.shp"));
        yield return Run(e_download("voices.snd"));
        yield return Run(e_download("voicesc.snd"));
    }

    public static IEnumerator e_main(int argc, string[] argv)
    { UnityEngine.Debug.Log("e_main");
#if !UNITY_WEBGL
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
#endif

        yield return Run(e_downloadAll());

        //mt_srand(time(NULL));

        //printf("\nWelcome to... >> %s %s <<\n\n", opentyrian_str, opentyrian_version);

        //printf("Copyright (C) 2007-2013 The OpenTyrian Development Team\n\n");

        //printf("This program comes with ABSOLUTELY NO WARRANTY.\n");
        //printf("This is free software, and you are welcome to redistribute it\n");
        //printf("under certain conditions.  See the file GPL.txt for details.\n\n");

        //if (SDL_Init(0))
        //{
        //    printf("Failed to initialize SDL: %s\n", SDL_GetError());
        //    return -1;
        //}

        JE_loadConfiguration();

        xmas = xmas_time();  // arg handler may override

        JE_paramCheck(argc, argv);

        JE_scanForEpisodes();

        input_grab(true);

        if (xmas && (!fileExists("tyrianc.shp") || !fileExists("voicesc.snd")))
        {
            xmas = false;

            //fprintf(stderr, "warning: Christmas is missing.\n");
        }

        JE_loadPals();
        JE_loadMainShapeTables(xmas ? "tyrianc.shp" : "tyrian.shp");

        if (xmas)
        {
            bool[] outXmas = new bool[1];
            yield return Run(e_xmas_prompt(outXmas));
            if (!outXmas[0])
            {
                xmas = outXmas[0];
                JE_loadMainShapeTables("tyrian.shp");
            }
        }


        /* Default Options */
        youAreCheating = false;
        smoothScroll = true;
        loadDestruct = false;

        if (!audio_disabled)
        {
            init_audio();

            load_music();

            JE_loadSndFile("tyrian.snd", xmas ? "voicesc.snd" : "voices.snd");
        }
        //else
        //{
        //    printf("audio disabled\n");
        //}

        //if (record_demo)
        //    printf("demo recording enabled (input limited to keyboard)\n");

        JE_loadExtraShapes();  /*Editship*/

        JE_loadHelpText();
        /*debuginfo("Help text complete");*/

        if (isNetworkGame)
        {
#if WITH_NETWORK
        if (network_init())
        {
            network_tyrian_halt(3, false);
        }
#else
            //fprintf(stderr, "OpenTyrian was compiled without networking support.");
            JE_tyrianHalt(5);
#endif
        }

        if (!isNetworkGame)
            yield return Run(e_intro_logos());

        for (; ; )
        {
            JE_initPlayerData();
            JE_sortHighScores();

            bool[] refResult = { false };
            yield return Run(e_JE_titleScreen(true, refResult));
            if (refResult[0])
                break;  // user quit from title screen

            if (loadDestruct)
            {
                yield return Run(e_JE_destructGame());
                loadDestruct = false;
            }
            else
            {
                yield return Run(e_JE_main());
            }
        }

        JE_tyrianHalt(0);
    }
}