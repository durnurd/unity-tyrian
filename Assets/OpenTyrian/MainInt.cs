using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using UnityEngine;
using static ConfigC;
using static PaletteC;
using static PlayerC;
using static SpriteC;
using static EpisodesC;
using static VarzC;
using static HelpTextC;
using static SurfaceC;
using static CoroutineRunner;
using static PicLoadC;
using static VideoC;
using static LoudnessC;
using static FontHandC;
using static KeyboardC;
using static SetupC;
using static NortsongC;
using static SndMastC;
using static MusMastC;
using static JoystickC;
using static NortVarsC;
using static FileIO;
using static PcxMastC;
using static NetworkC;
using static ParamsC;
using static LibC;
using static MenusC;
using static VGA256dC;
using static MouseC;
using static BackgrndC;

using static System.Math;

using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.Text;

public static class MainIntC
{
    public static bool[] button = new bool[4]; // fire, left fire, right fire, mode swap

    public static JE_shortint constantLastX;
    public static JE_word textErase;
    public static JE_word upgradeCost;
    public static JE_word downgradeCost;
    public static JE_boolean performSave;
    public static JE_boolean jumpSection;
    public static JE_boolean useLastBank;

    private const int MAX_PAGE = 8;
    private const int TOPICS = 6;
    private static readonly JE_byte[] topicStart = { 0, 1, 2, 3, 7, 255 };

    /*
extern bool pause_pressed, ingamemenu_pressed;

    void JE_drawTextWindow( const char* text );
    void JE_initPlayerData(void );
    void JE_highScoreScreen(void );
    void JE_loadOrderingInfo(void );
    */
    public static IEnumerator e_JE_nextEpisode()
    { UnityEngine.Debug.Log("e_JE_nextEpisode");
        lastLevelName = "Completed";

        if (episodeNum == initial_episode_num && !gameHasRepeated && episodeNum != EPISODE_AVAILABLE &&
            !isNetworkGame && !constantPlay)
        {
            yield return Run(e_JE_highScoreCheck());
        }

        int newEpisode = JE_findNextEpisode();

        if (jumpBackToEpisode1)
        {
            // shareware version check
            if (episodeNum == 1 &&
                !isNetworkGame && !constantPlay)
            {
                // JE_loadOrderingInfo();
            }

            if (episodeNum > 2 &&
                !constantPlay)
            {
                yield return Run(e_JE_playCredits());
            }

            // randomly give player the SuperCarrot
            if ((mt_rand() % 6) == 0)
            {
                player[0].items.ship = 2;                      // SuperCarrot
                player[0].items.weapon[FRONT_WEAPON].id = 23;  // Banana Blast
                player[0].items.weapon[REAR_WEAPON].id = 24;   // Banana Blast Rear

                for (uint i = 0; i < player[0].items.weapon.Length; ++i)
                    player[0].items.weapon[i].power = 1;

                player[1].items.weapon[REAR_WEAPON].id = 24;   // Banana Blast Rear

                player[0].last_items = player[0].items;
            }
        }

        if (newEpisode != episodeNum)
            JE_initEpisode(newEpisode);

        gameLoaded = true;
        mainLevel = FIRST_LEVEL;
        saveLevel = FIRST_LEVEL;

        play_song(26);

        JE_clr256(VGAScreen);
        System.Array.Copy(palettes[6 - 1], colors, colors.Length);

        JE_dString(VGAScreen, JE_fontCenter(episode_name[episodeNum], SMALL_FONT_SHAPES), 130, episode_name[episodeNum], SMALL_FONT_SHAPES);
        JE_dString(VGAScreen, JE_fontCenter(miscText[5 - 1], SMALL_FONT_SHAPES), 185, miscText[5 - 1], SMALL_FONT_SHAPES);

        JE_showVGA();
        yield return Run(e_fade_palette(colors, 15, 0, 255));

        JE_wipeKey();
        if (!constantPlay)
        {
            do
            {
                //NETWORK_KEEP_ALIVE();

                yield return null;
            } while (!JE_anyButton());
        }

        yield return Run(e_fade_black(15));
    }
    /*
    void JE_doInGameSetup(void );
    JE_boolean JE_inGameSetup(void );
    void JE_inGameHelp(void );
    */

    public static bool load_next_demo()
    {
        if (++demo_num > 5)
            demo_num = 1;

        string demo_filename = "demo." + demo_num;
        demo_file = open(demo_filename); // TODO: only play demos from existing file (instead of dying)

        difficultyLevel = 2;
        bonusLevelCurrent = false;

        byte temp = demo_file.ReadByte();
        JE_initEpisode(temp);
        byte[] levelNameBytes = demo_file.ReadBytes(10);
        levelNameBytes[10] = 0;
        System.Text.Encoding.ASCII.GetString(levelNameBytes, 0, 10);
        lvlFileNum = demo_file.ReadByte();

        player[0].items.weapon[FRONT_WEAPON].id = demo_file.ReadByte();
        player[0].items.weapon[REAR_WEAPON].id = demo_file.ReadByte();
        player[0].items.super_arcade_mode = demo_file.ReadByte();
        player[0].items.sidekick[LEFT_SIDEKICK] = demo_file.ReadByte();
        player[0].items.sidekick[RIGHT_SIDEKICK] = demo_file.ReadByte();
        player[0].items.generator = demo_file.ReadByte();

        player[0].items.sidekick_level = demo_file.ReadByte(); // could probably ignore
        player[0].items.sidekick_series = demo_file.ReadByte(); // could probably ignore

        initial_episode_num = demo_file.ReadByte(); // could probably ignore

        player[0].items.shield = demo_file.ReadByte();
        player[0].items.special = demo_file.ReadByte();
        player[0].items.ship = demo_file.ReadByte();

        for (uint i = 0; i < 2; ++i)
            player[0].items.weapon[i].power = demo_file.ReadByte();

        demo_file.BaseStream.Seek(3, System.IO.SeekOrigin.Current);

        levelSong = demo_file.ReadByte();

        demo_keys_wait = 0;
        demo_keys = next_demo_keys = 0;

        return true;
    }

    /*
    bool replay_demo_keys(void );
    bool read_demo_keys(void );

    void JE_SFCodes(JE_byte playerNum_, JE_integer PX_, JE_integer PY_, JE_integer mouseX_, JE_integer mouseY_);
    void JE_sort(void );

    */
    public static IEnumerator e_JE_helpSystem(JE_byte startTopic)
    { UnityEngine.Debug.Log("e_JE_helpSystem");
        JE_integer page, lastPage = 0;
        JE_byte menu;

        page = topicStart[startTopic - 1];

        yield return Run(e_fade_black(10));

        JE_loadPic(VGAScreen, 2, false);

        play_song(SONG_MAPVIEW);

        JE_showVGA();
        yield return Run(e_fade_palette(colors, 10, 0, 255));

        System.Array.Copy(VGAScreen.pixels, VGAScreen2.pixels, VGAScreen2.pixels.Length);

        do
        {
            System.Array.Copy(VGAScreen2.pixels, VGAScreen.pixels, VGAScreen.pixels.Length);

            temp2 = 0;

            for (temp = 0; temp < TOPICS; temp++)
            {
                if (topicStart[temp] <= page)
                {
                    temp2 = temp;
                }
            }

            if (page > 0)
            {
                string buf = miscText[24] + " " + (page - topicStart[temp2] + 1);
                JE_outText(VGAScreen, 10, 192, buf, 13, 5);

                buf = miscText[25] + " " + page + " of " + MAX_PAGE;
                JE_outText(VGAScreen, 220, 192, buf, 13, 5);

                JE_dString(VGAScreen, JE_fontCenter(topicName[temp2], SMALL_FONT_SHAPES), 1, topicName[temp2], SMALL_FONT_SHAPES);
            }

            menu = 0;

            helpBoxBrightness = 3;
            verticalHeight = 8;

            switch (page)
            {
                case 0:
                    menu = 2;
                    if (lastPage == MAX_PAGE)
                    {
                        menu = TOPICS;
                    }
                    JE_dString(VGAScreen, JE_fontCenter(topicName[0], FONT_SHAPES), 30, topicName[0], FONT_SHAPES);

                    do
                    {
                        for (temp = 1; temp <= TOPICS; temp++)
                        {
                            string buf;

                            if (temp == menu - 1)
                            {
                                buf = "~" + topicName[temp];
                            }
                            else
                            {
                                buf = topicName[temp];
                            }

                            JE_dString(VGAScreen, JE_fontCenter(topicName[temp], SMALL_FONT_SHAPES), temp * 20 + 40, buf, SMALL_FONT_SHAPES);
                        }

                        //JE_waitRetrace();  didn't do anything anyway?
                        JE_showVGA();

                        tempW = 0;
                        yield return Run(e_JE_textMenuWait(null, false));
                        if (newkey)
                        {
                            switch (lastkey_sym)
                            {
                                case KeyCode.UpArrow:
                                    menu--;
                                    if (menu < 2)
                                    {
                                        menu = TOPICS;
                                    }
                                    JE_playSampleNum(S_CURSOR);
                                    break;
                                case KeyCode.DownArrow:
                                    menu++;
                                    if (menu > TOPICS)
                                    {
                                        menu = 2;
                                    }
                                    JE_playSampleNum(S_CURSOR);
                                    break;
                                default:
                                    break;
                            }
                        }
                    } while (!(lastkey_sym == KeyCode.Escape || lastkey_sym == KeyCode.Return));

                    if (lastkey_sym == KeyCode.Return)
                    {
                        page = topicStart[menu - 1];
                        JE_playSampleNum(S_CLICK);
                    }

                    break;
                case 1: /* One-Player Menu */
                    JE_HBox(VGAScreen, 10, 20, 2, 60);
                    JE_HBox(VGAScreen, 10, 50, 5, 60);
                    JE_HBox(VGAScreen, 10, 80, 21, 60);
                    JE_HBox(VGAScreen, 10, 110, 1, 60);
                    JE_HBox(VGAScreen, 10, 140, 28, 60);
                    break;
                case 2: /* Two-Player Menu */
                    JE_HBox(VGAScreen, 10, 20, 1, 60);
                    JE_HBox(VGAScreen, 10, 60, 2, 60);
                    JE_HBox(VGAScreen, 10, 100, 21, 60);
                    JE_HBox(VGAScreen, 10, 140, 28, 60);
                    break;
                case 3: /* Upgrade Ship */
                    JE_HBox(VGAScreen, 10, 20, 5, 60);
                    JE_HBox(VGAScreen, 10, 70, 6, 60);
                    JE_HBox(VGAScreen, 10, 110, 7, 60);
                    break;
                case 4:
                    JE_HBox(VGAScreen, 10, 20, 8, 60);
                    JE_HBox(VGAScreen, 10, 55, 9, 60);
                    JE_HBox(VGAScreen, 10, 87, 10, 60);
                    JE_HBox(VGAScreen, 10, 120, 11, 60);
                    JE_HBox(VGAScreen, 10, 170, 13, 60);
                    break;
                case 5:
                    JE_HBox(VGAScreen, 10, 20, 14, 60);
                    JE_HBox(VGAScreen, 10, 80, 15, 60);
                    JE_HBox(VGAScreen, 10, 120, 16, 60);
                    break;
                case 6:
                    JE_HBox(VGAScreen, 10, 20, 17, 60);
                    JE_HBox(VGAScreen, 10, 40, 18, 60);
                    JE_HBox(VGAScreen, 10, 130, 20, 60);
                    break;
                case 7: /* Options */
                    JE_HBox(VGAScreen, 10, 20, 21, 60);
                    JE_HBox(VGAScreen, 10, 70, 22, 60);
                    JE_HBox(VGAScreen, 10, 110, 23, 60);
                    JE_HBox(VGAScreen, 10, 140, 24, 60);
                    break;
                case 8:
                    JE_HBox(VGAScreen, 10, 20, 25, 60);
                    JE_HBox(VGAScreen, 10, 60, 26, 60);
                    JE_HBox(VGAScreen, 10, 100, 27, 60);
                    JE_HBox(VGAScreen, 10, 140, 28, 60);
                    JE_HBox(VGAScreen, 10, 170, 29, 60);
                    break;
            }

            helpBoxBrightness = 1;
            verticalHeight = 7;

            lastPage = page;

            if (menu == 0)
            {
                do
                {
                    setjasondelay(3);

                    push_joysticks_as_keyboard();
                    service_SDL_events(true);

                    JE_showVGA();

                    yield return coroutine_wait_delay();
                } while (!newkey && !newmouse);

                yield return coroutine_wait_noinput(false, true, false);

                if (newmouse)
                {
                    switch (lastmouse_but)
                    {
                        case 0:
                            lastkey_sym = KeyCode.RightArrow;
                            break;
                        case 1:
                            lastkey_sym = KeyCode.LeftArrow;
                            break;
                        case 2:
                            lastkey_sym = KeyCode.Escape;
                            break;
                    }
                    do
                    {
                        service_SDL_events(false);
                        yield return null;
                    } while (mousedown);
                    newkey = true;
                }

                if (newkey)
                {
                    switch (lastkey_sym)
                    {
                        case KeyCode.LeftArrow:
                        case KeyCode.UpArrow:
                        case KeyCode.PageUp:
                            page--;
                            JE_playSampleNum(S_CURSOR);
                            break;
                        case KeyCode.RightArrow:
                        case KeyCode.DownArrow:
                        case KeyCode.PageDown:
                        case KeyCode.Return:
                        case KeyCode.Space:
                            if (page == MAX_PAGE)
                            {
                                page = 0;
                            }
                            else
                            {
                                page++;
                            }
                            JE_playSampleNum(S_CURSOR);
                            break;
                        case KeyCode.F1:
                            page = 0;
                            JE_playSampleNum(S_CURSOR);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (page == 255)
            {
                lastkey_sym = KeyCode.Escape;
            }
        } while (lastkey_sym != KeyCode.Escape);
    }

    // cost to upgrade a weapon power from power-1 (where power == 0 indicates an unupgraded weapon)
    public static int weapon_upgrade_cost(int base_cost, int power)
    {
        int temp = 0;

        // 0 1 3 6 10 15 21 29 ...
        for (; power > 0; power--)
            temp += power;

        return base_cost * temp;
    }

    public static int JE_getCost(int itemType, int itemNum)
    {
        int cost = 0;

        switch (itemType)
        {
            case 2:
                cost = (itemNum > 90) ? 100 : ships[itemNum].cost;
                break;
            case 3:
            case 4:
                cost = weaponPort[itemNum].cost;

                ushort port = (ushort)(itemType - 3),
                           item_power = (ushort)(player[0].items.weapon[port].power - 1);

                downgradeCost = (ushort)weapon_upgrade_cost(cost, item_power);
                upgradeCost = (ushort)weapon_upgrade_cost(cost, item_power + 1);
                break;
            case 5:
                cost = shields[itemNum].cost;
                break;
            case 6:
                cost = powerSys[itemNum].cost;
                break;
            case 7:
            case 8:
                cost = options[itemNum].cost;
                break;
        }

        return cost;
    }

    public static JE_longint JE_getValue(JE_byte itemType, JE_word itemNum)
    {
        int value = 0;

        switch (itemType)
        {
            case 2:
                value = ships[itemNum].cost;
                break;
            case 3:
            case 4:
                ;
                int base_value = weaponPort[itemNum].cost;

                // if two-player, use first player's front and second player's rear weapon
                int port = itemType - 3;
                int item_power = player[twoPlayerMode ? port : 0].items.weapon[port].power - 1;

                value = base_value;
                for (int i = 1; i <= item_power; ++i)
                    value += weapon_upgrade_cost(base_value, i);
                break;
            case 5:
                value = shields[itemNum].cost;
                break;
            case 6:
                value = powerSys[itemNum].cost;
                break;
            case 7:
            case 8:
                value = options[itemNum].cost;
                break;
        }

        return value;
    }

    public static int JE_totalScore(Player this_player)
    {
        int temp = this_player.cash;

        temp += JE_getValue(2, this_player.items.ship);
        temp += JE_getValue(3, this_player.items.weapon[FRONT_WEAPON].id);
        temp += JE_getValue(4, this_player.items.weapon[REAR_WEAPON].id);
        temp += JE_getValue(5, this_player.items.shield);
        temp += JE_getValue(6, this_player.items.generator);
        temp += JE_getValue(7, this_player.items.sidekick[LEFT_SIDEKICK]);
        temp += JE_getValue(8, this_player.items.sidekick[RIGHT_SIDEKICK]);

        return temp;
    }

    public static void JE_drawTextWindow(string text)
    {
        if (textErase > 0) // erase current text
            blit_sprite(VGAScreenSeg, 16, 189, OPTION_SHAPES, 36);  // in-game text area

        textErase = 100;
        JE_outText(VGAScreenSeg, 20, 190, text, 0, 4);
    }

    public static IEnumerator e_JE_outCharGlow(int x, int y, string s)
    { UnityEngine.Debug.Log("e_JE_outCharGlow");
        int maxloc, loc, z;
        JE_shortint[] glowcol = new JE_shortint[60]; /* [1..60] */
        JE_shortint[] glowcolc = new JE_shortint[60]; /* [1..60] */
        int[] textloc = new int[60]; /* [1..60] */
        int bank;

        setjasondelay2(1);

        bank = (warningRed) ? 7 : ((useLastBank) ? 15 : 14);

        if (s == null || s.Length == 0)
            yield break;

        if (frameCountMax == 0)
        {
            JE_textShade(VGAScreen, x, y, s, bank, 0, PART_SHADE);
            JE_showVGA();
        }
        else
        {
            maxloc = s.Length;
            for (z = 0; z < 60; z++)
            {
                glowcol[z] = -8;
                glowcolc[z] = 1;
            }

            loc = x;
            for (z = 0; z < maxloc; z++)
            {
                textloc[z] = loc;

                int sprite_id = font_ascii[s[z]];

                if (s[z] == ' ')
                    loc += 6;
                else if (sprite_id != -1)
                    loc += sprite(TINY_FONT, sprite_id).width + 1;
            }

            for (loc = 0; loc < s.Length + 28; loc++)
            {
                if (!ESCPressed)
                {
                    setjasondelay(frameCountMax);

                    //NETWORK_KEEP_ALIVE();

                    int sprite_id = -1;

                    for (z = loc - 28; z <= loc; z++)
                    {
                        if (z >= 0 && z < maxloc)
                        {
                            sprite_id = font_ascii[s[z]];

                            if (sprite_id != -1)
                            {
                                blit_sprite_hv(VGAScreen, textloc[z], y, TINY_FONT, sprite_id, (byte)bank, glowcol[z]);

                                glowcol[z] += glowcolc[z];
                                if (glowcol[z] > 9)
                                    glowcolc[z] = -1;
                            }
                        }
                    }
                    if (sprite_id != -1 && --z < maxloc)
                        blit_sprite_dark(VGAScreen, textloc[z] + 1, y + 1, TINY_FONT, sprite_id, true);

                    if (JE_anyButton())
                        frameCountMax = 0;

                    do
                    {
                        if (levelWarningDisplay)
                            JE_updateWarning(VGAScreen);

                        yield return new WaitForSeconds(.016f);
                    }
                    while (!(delaycount() == 0 || ESCPressed));

                    JE_showVGA();
                }
            }
        }
    }

    public static void JE_drawPortConfigButtons() // rear weapon pattern indicator
    {
        if (twoPlayerMode)
            return;

        if (player[0].weapon_mode == 1)
        {
            blit_sprite(VGAScreenSeg, 285, 44, OPTION_SHAPES, 18);  // lit
            blit_sprite(VGAScreenSeg, 302, 44, OPTION_SHAPES, 19);  // unlit
        }
        else // == 2
        {
            blit_sprite(VGAScreenSeg, 285, 44, OPTION_SHAPES, 19);  // unlit
            blit_sprite(VGAScreenSeg, 302, 44, OPTION_SHAPES, 18);  // lit
        }
    }

    public static IEnumerator e_JE_endLevelAni()
    {
        JE_word x, y;
        JE_byte temp;
        string tempStr;

        if (!constantPlay)
        {
            // grant shipedit privileges

            // special
            if (player[0].items.special < 21)
                saveTemp[SAVE_FILES_SIZE + 81 + player[0].items.special] = 1;

            for (int p = 0; p < player.Length; ++p)
            {
                // front, rear
                for (int i = 0; i < player[p].items.weapon.Length; ++i)
                    saveTemp[SAVE_FILES_SIZE + player[p].items.weapon[i].id] = 1;

                // options
                for (int i = 0; i < player[p].items.sidekick.Length; ++i)
                    saveTemp[SAVE_FILES_SIZE + 51 + player[p].items.sidekick[i]] = 1;
            }
        }

        adjust_difficulty();

        player[0].last_items = player[0].items;
        lastLevelName = levelName;

        JE_wipeKey();
        frameCountMax = 4;
        textGlowFont = SMALL_FONT_SHAPES;

        set_colors(new Color32(255,255,255,255), 254, 254);

        if (!levelTimer || levelTimerCountdown > 0 || !(episodeNum == 4))
            JE_playSampleNum(V_LEVEL_END);
        else
            play_song(21);

        if (bonusLevel)
        {
            yield return Run(e_JE_outTextGlow(VGAScreenSeg, 20, 20, miscText[17 - 1]));
        }
        else if (all_players_alive())
        {
            tempStr = miscText[27 - 1] + " " + levelName; // "Completed"
            yield return Run(e_JE_outTextGlow(VGAScreenSeg, 20, 20, tempStr));
        }
        else
        {
            tempStr = miscText[62 - 1] + " " + levelName; // "Exiting"
            yield return Run(e_JE_outTextGlow(VGAScreenSeg, 20, 20, tempStr));
        }

        if (twoPlayerMode)
        {
            for (int i = 0; i < 2; ++i)
            {
                tempStr = miscText[40 + i] + " " + player[i].cash;
                yield return Run(e_JE_outTextGlow(VGAScreenSeg, 30, 50 + 20 * i, tempStr));
            }
        }
        else
        {
            tempStr = miscText[28 - 1] + " " + player[0].cash;
            yield return Run(e_JE_outTextGlow(VGAScreenSeg, 30, 50, tempStr));
        }

        temp = (totalEnemy == 0) ? (byte)0 : (byte)Round(enemyKilled * 100.0 / totalEnemy);
        tempStr =miscText[63 - 1] + " " + temp + "%";
        yield return Run(e_JE_outTextGlow(VGAScreenSeg, 40, 90, tempStr));

        if (!constantPlay)
            editorLevel += (ushort)(temp / 5);

        if (!onePlayerAction && !twoPlayerMode)
        {
            yield return Run(e_JE_outTextGlow(VGAScreenSeg, 30, 120, miscText[4 - 1]));   /*Cubes*/

            if (cubeMax > 0)
            {
                if (cubeMax > 4)
                    cubeMax = 4;

                if (frameCountMax != 0)
                    frameCountMax = 1;

                for (temp = 1; temp <= cubeMax; temp++)
                {
                    //NETWORK_KEEP_ALIVE();

                    JE_playSampleNum(18);
                    x = (ushort)(20 + 30 * temp);
                    y = 135;
                    JE_drawCube(VGAScreenSeg, x, y, 9, 0);
                    JE_showVGA();

                    for (sbyte i = -15; i <= 10; i++)
                    {
                        setjasondelay(frameCountMax);

                        blit_sprite_hv(VGAScreenSeg, x, y, OPTION_SHAPES, 25, 0x9, i);

                        if (JE_anyButton())
                            frameCountMax = 0;

                        JE_showVGA();

                        yield return coroutine_wait_delay();
                    }
                    for (sbyte i = 10; i >= 0; i--)
                    {
                        setjasondelay(frameCountMax);

                        blit_sprite_hv(VGAScreenSeg, x, y, OPTION_SHAPES, 25, 0x9, i);

                        if (JE_anyButton())
                            frameCountMax = 0;

                        JE_showVGA();

                        yield return coroutine_wait_delay();
                    }
                }
            }
            else
            {
                yield return Run(e_JE_outTextGlow(VGAScreenSeg, 50, 135, miscText[15 - 1]));
            }

        }

        if (frameCountMax != 0)
        {
            frameCountMax = 6;
            temp = 1;
        }
        else
        {
            temp = 0;
        }
        temp2 = twoPlayerMode ? 150 : 160;
        yield return Run(e_JE_outTextGlow(VGAScreenSeg, 90, temp2, miscText[5 - 1]));

        if (!constantPlay)
        {
            do
            {
                setjasondelay(1);

                //NETWORK_KEEP_ALIVE();

                yield return coroutine_wait_delay();
            } while (!(JE_anyButton() || (frameCountMax == 0 && temp == 1)));
        }

        yield return coroutine_wait_noinput(false, false, true); // TODO: should up the joystick repeat temporarily instead

        yield return Run(e_fade_black(15));
        JE_clr256(VGAScreen);
    }

    public static void JE_drawCube(Surface screen, JE_word x, JE_word y, JE_byte filter, JE_byte brightness)
    {
        blit_sprite_dark(screen, x + 4, y + 4, OPTION_SHAPES, 25, false);
        blit_sprite_dark(screen, x + 3, y + 3, OPTION_SHAPES, 25, false);
        blit_sprite_hv(screen, x, y, OPTION_SHAPES, 25, filter, (sbyte)brightness);
    }


    public static void JE_handleChat()
    {
        //Nope
    }
    public static bool str_pop_int(ref string str, int startPos, out int val)
    {
        if (startPos >= str.Length)
        {
            val = 0;
            return false;
        }
        string ignoreBeginning = str.Substring(0, startPos);
        string toParse = str.Substring(startPos);
        var match = Regex.Match(toParse, "^ *([0-9]+)");
        if (match.Groups.Count == 2)
        {
            var capture = match.Groups[1].Captures[0];
            int parsed;
            if (int.TryParse(capture.Value, out parsed))
            {
                val = parsed;
                str = ignoreBeginning + toParse.Substring(toParse.IndexOf(capture.Value) + capture.Value.Length);
                return true;
            }
        }
        val = 0;
        return false;
    }

    public static IEnumerator e_JE_operation(JE_byte slot)
    {

        if (!performSave)
        {
            if (saveFiles[slot - 1].level > 0)
            {
                gameJustLoaded = true;
                JE_loadGame(slot);
                gameLoaded = true;
            }
        }
        else if (slot % 11 != 0)
        {
            byte flash;
            string tempStr;

            tempStr = saveFiles[slot - 1].name.TrimEnd(' ');
            temp = tempStr.Length;

            StringBuilder stemp = new StringBuilder(tempStr, 14);
            stemp.Append(' ', 14 - temp);

            flash = 8 * 16 + 10;

            yield return coroutine_wait_noinput(false, true, false);

            JE_barShade(VGAScreen, 65, 55, 255, 155);

            bool quit = false;
            while (!quit)
            {
                service_SDL_events(true);

                blit_sprite(VGAScreen, 50, 50, OPTION_SHAPES, 35);  // message box

                JE_textShade(VGAScreen, 60, 55, miscText[1 - 1], 11, 4, DARKEN);
                JE_textShade(VGAScreen, 70, 70, levelName, 11, 4, DARKEN);

                do
                {
                    flash = (byte)((flash == 8 * 16 + 10) ? 8 * 16 + 2 : 8 * 16 + 10);
                    temp3 = (temp3 == 6) ? 2 : 6;

                    tempStr = stemp.ToString().TrimEnd();
                    JE_outText(VGAScreen, 65, 89, tempStr, 8, 3);
                    tempW = (JE_word)(65 + JE_textWidth(tempStr, TINY_FONT));
                    JE_barShade(VGAScreen, tempW + 2, 90, tempW + 6, 95);
                    fill_rectangle_xy(VGAScreen, tempW + 1, 89, tempW + 5, 94, flash);

                    for (int i = 0; i < 14; i++)
                    {
                        setjasondelay(1);

                        JE_mouseStart();
                        JE_showVGA();
                        JE_mouseReplace();

                        push_joysticks_as_keyboard();
                        yield return coroutine_service_wait_delay();

                        if (newkey || newmouse)
                            break;
                    }

                }
                while (!newkey && !newmouse);

                if (mouseButton > 0)
                {
                    if (mouseX > 56 && mouseX < 142 && mouseY > 123 && mouseY < 149)
                    {
                        quit = true;
                        JE_saveGame(slot, stemp.ToString());
                        JE_playSampleNum(S_SELECT);
                    }
                    else if (mouseX > 151 && mouseX < 237 && mouseY > 123 && mouseY < 149)
                    {
                        quit = true;
                        JE_playSampleNum(S_SPRING);
                    }
                }
                else if (newkey)
                {
                    bool validkey = false;
                    lastkey_char = char.ToUpper(lastkey_char);
                    switch (lastkey_char)
                    {
                        case ' ':
                        case '-':
                        case '.':
                        case ',':
                        case ':':
                        case '!':
                        case '?':
                        case '#':
                        case '@':
                        case '$':
                        case '%':
                        case '*':
                        case '(':
                        case ')':
                        case '/':
                        case '=':
                        case '+':
                        case '<':
                        case '>':
                        case ';':
                        case '"':
                        case '\'':
                            validkey = true;
                            goto default;
                        default:
                            switch (lastkey_sym)
                            {
                                default:
                                    if (temp < 14 && (validkey || (lastkey_char >= 'A' && lastkey_char <= 'Z') || (lastkey_char >= '0' && lastkey_char <= '9')))
                                    {
                                        JE_playSampleNum(S_CURSOR);
                                        stemp[temp] = lastkey_char;
                                        temp++;
                                    }
                                    break;
                                case KeyCode.Backspace:
                                case KeyCode.Delete:
                                    if (temp > 0)
                                    {
                                        temp--;
                                        stemp[temp] = ' ';
                                        JE_playSampleNum(S_CLICK);
                                    }
                                    break;
                                case KeyCode.Escape:
                                    quit = true;
                                    JE_playSampleNum(S_SPRING);
                                    break;
                                case KeyCode.Return:
                                    quit = true;
                                    JE_saveGame(slot, stemp.ToString());
                                    JE_playSampleNum(S_SELECT);
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        yield return coroutine_wait_noinput(false, true, false);
    }

    /*
    void JE_loadScreen(void );
    void JE_inGameDisplays(void );
    void JE_mainKeyboardInput(void );
    void JE_pauseGame(void );

    void JE_playerMovement(Player* this_player, JE_byte inputDevice, JE_byte playerNum, JE_word shipGr, Sprite2_array* shapes9ptr_, JE_word* mouseX, JE_word* mouseY);
    */
    public static void JE_mainGamePlayerFunctions()
    {
        /*PLAYER MOVEMENT/MOUSE ROUTINES*/

        if (endLevel && levelEnd > 0)
        {
            levelEnd--;
            levelEndWarp++;
        }

        /*Reset Street-Fighter commands*/
        FillByteArrayWithZeros(SFExecuted);

        portConfigChange = false;

        //if (twoPlayerMode)
        //{
        //    JE_playerMovement(player[0],
        //                      !galagaMode ? inputDevice[0] : 0, 1, shipGr, shipGrPtr,
        //                      ref mouseX, ref mouseY);
        //    JE_playerMovement(player[1],
        //                      !galagaMode ? inputDevice[1] : 0, 2, shipGr2, shipGr2ptr,
        //                      ref mouseXB, ref mouseYB);
        //}
        //else
        //{
        //    JE_playerMovement(player[0],
        //                      0, 1, shipGr, shipGrPtr,
        //                      ref mouseX, ref mouseY);
        //}

        /* == Parallax Map Scrolling == */
        if (twoPlayerMode)
        {
            tempX = (ushort)((player[0].x + player[1].x) / 2);
        }
        else
        {
            tempX = (ushort)player[0].x;
        }

        tempW = (ushort)Floor((260.0f - (tempX - 36.0f)) / (260.0f - 36.0f) * (24.0f * 3.0f) - 1.0f);
        mapX3Ofs = tempW;
        mapX3Pos = (ushort)(mapX3Ofs % 24);
        mapX3bpPos = 1 - (mapX3Ofs / 24);

        mapX2Ofs = (ushort)((tempW * 2) / 3);
        mapX2Pos = (ushort)(mapX2Ofs % 24);
        mapX2bpPos = 1 - (mapX2Ofs / 24);

        oldMapXOfs = mapXOfs;
        mapXOfs = (ushort)(mapX2Ofs / 2);
        mapXPos = (ushort)(mapXOfs % 24);
        mapXbpPos = 1 - (mapXOfs / 24);

        if (background3x1)
        {
            mapX3Ofs = mapXOfs;
            mapX3Pos = mapXPos;
            mapX3bpPos = mapXbpPos - 1;
        }
    }
    /*
    const char* JE_getName( JE_byte pnum );

    void JE_playerCollide(Player* this_player, JE_byte playerNum);

    */


    public static void JE_initPlayerData()
    {
        /* JE: New Game Items/Data */

        player[0].items.ship = 1;                     // USP Talon
        player[0].items.weapon[FRONT_WEAPON].id = 1;  // Pulse Cannon
        player[0].items.weapon[REAR_WEAPON].id = 0;   // None
        player[0].items.shield = 4;                   // Gencore High Energy Shield
        player[0].items.generator = 2;                // Advanced MR-12
        for (uint i = 0; i < player[0].items.sidekick.Length; ++i)
            player[0].items.sidekick[i] = 0;          // None
        player[0].items.special = 0;                  // None

        player[0].last_items = player[0].items;

        player[1].items = player[0].items;
        player[1].items.weapon[REAR_WEAPON].id = 15;  // Vulcan Cannon
        player[1].items.sidekick_level = 101;         // 101, 102, 103
        player[1].items.sidekick_series = 0;          // None

        gameHasRepeated = false;
        onePlayerAction = false;
        superArcadeMode = SA_NONE;
        superTyrian = false;
        twoPlayerMode = false;

        secretHint = (byte)Random.Range(1, 4);

        for (uint p = 0; p < player.Length; ++p)
        {
            for (uint i = 0; i < player[p].items.weapon.Length; ++i)
            {
                player[p].items.weapon[i].power = 1;
            }

            player[p].weapon_mode = 1;
            player[p].armor = ships[player[p].items.ship].dmg;

            player[p].is_dragonwing = (p == 1);
        }

        mainLevel = FIRST_LEVEL;
        saveLevel = FIRST_LEVEL;

        lastLevelName = miscText[19];
    }

    public static void JE_sortHighScores()
    {
        JE_byte x;

        temp = 0;
        for (x = 0; x < 6; x++)
        {
            JE_sort();
            temp += 3;
        }
    }

    private static void JE_sort()
    {
        //Maybe another time...


        //int a, b;

        //for (a = 0; a < 2; a++)
        //{
        //    for (b = a + 1; b < 3; b++)
        //    {
        //        if (saveFiles[temp + a].highScore1 < saveFiles[temp + b].highScore1)
        //        {
        //            JE_longint tempLI;
        //            string tempStr;
        //            JE_byte tempByte;

        //            tempLI = saveFiles[temp + a].highScore1;
        //            saveFiles[temp + a].highScore1 = saveFiles[temp + b].highScore1;
        //            saveFiles[temp + b].highScore1 = tempLI;

        //            strcpy(tempStr, saveFiles[temp + a].highScoreName);
        //            strcpy(saveFiles[temp + a].highScoreName, saveFiles[temp + b].highScoreName);
        //            strcpy(saveFiles[temp + b].highScoreName, tempStr);

        //            tempByte = saveFiles[temp + a].highScoreDiff;
        //            saveFiles[temp + a].highScoreDiff = saveFiles[temp + b].highScoreDiff;
        //            saveFiles[temp + b].highScoreDiff = tempByte;
        //        }
        //    }
        //}
    }

    public static IEnumerator e_JE_playCredits()
    { UnityEngine.Debug.Log("e_JE_playCredits");
        const int lines_max = 132;

        string[] credstr = new string[lines_max];

        int lines = 0;

        JE_byte currentpic = 0, fade = 0;
        JE_shortint fadechg = 1;
        JE_byte currentship = 0;
        JE_integer shipx = 0, shipxwait = 0;
        JE_shortint shipxc = 0, shipxca = 0;

        load_sprites_file(EXTRA_SHAPES, "estsc.shp");

        setjasondelay2(1000);

        play_song(8);

        // load credits text
        BinaryReader f = open("tyrian.cdt");
        for (lines = 0; f.PeekChar() >= 0 && lines < lines_max; ++lines)
        {
            credstr[lines] = read_encrypted_pascal_string(f);
        }

        if (lines == lines_max)
            --lines;
        f.Close();

        System.Array.Copy(palettes[6 - 1], colors, colors.Length);

        JE_clr256(VGAScreen);
        JE_showVGA();
        yield return Run(e_fade_palette(colors, 2, 0, 255));

        //tempScreenSeg = VGAScreenSeg;

        int ticks_max = lines * 20 * 3;
        for (int ticks = 0; ticks < ticks_max; ++ticks)
        {
            setjasondelay(1);
            JE_clr256(VGAScreen);

            blit_sprite_hv(VGAScreenSeg, 319 - sprite(EXTRA_SHAPES, currentpic).width, 100 - (sprite(EXTRA_SHAPES, currentpic).height / 2), EXTRA_SHAPES, currentpic, 0x0, (sbyte)(fade - 15));

            fade += (byte)fadechg;
            if (fade == 0 && fadechg == -1)
            {
                fadechg = 1;
                ++currentpic;
                if (currentpic >= sprite_table[EXTRA_SHAPES].Length)
                    currentpic = 0;
            }
            if (fade == 15)
                fadechg = 0;

            if (delaycount2() == 0)
            {
                fadechg = -1;
                setjasondelay2(900);
            }

            if (ticks % 200 == 0)
            {
                currentship = (byte)((mt_rand() % 11) + 1);
                shipxwait = (byte)((mt_rand() % 80) + 10);
                if ((mt_rand() % 2) == 1)
                {
                    shipx = 1;
                    shipxc = 0;
                    shipxca = 1;
                }
                else
                {
                    shipx = 900;
                    shipxc = 0;
                    shipxca = -1;
                }
            }

            shipxwait--;
            if (shipxwait == 0)
            {
                if (shipx == 1 || shipx == 900)
                    shipxc = 0;
                shipxca = (sbyte)-shipxca;
                shipxwait = (short)((mt_rand() % 40) + 15);
            }
            shipxc += shipxca;
            shipx += shipxc;
            if (shipx < 1)
            {
                shipx = 1;
                shipxwait = 1;
            }
            if (shipx > 900)
            {
                shipx = 900;
                shipxwait = 1;
            }
            int tmp_unknown = shipxc * shipxc;
            if (450 + tmp_unknown < 0 || 450 + tmp_unknown > 900)
            {
                if (shipxca < 0 && shipxc < 0)

                    shipxwait = 1;
                if (shipxca > 0 && shipxc > 0)
                    shipxwait = 1;
            }

            int ship_sprite = ships[currentship].shipgraphic;
            if (shipxc < -10)

                ship_sprite -= (shipxc < -20) ? 4 : 2;
            else if (shipxc > 10)
                ship_sprite += (shipxc > 20) ? 4 : 2;

            blit_sprite2x2(VGAScreen, shipx / 40, 184 - (ticks % 200), shapes9, ship_sprite);

            int bottom_line = (ticks / 3) / 20;
            int y = 20 - ((ticks / 3) % 20);

            for (int line = bottom_line - 10; line < bottom_line; ++line)
            {
                if (line >= 0 && line < lines_max)
                {
                    if (credstr[line].Length != 1 || credstr[line][0] != '.')
                    {
                        byte color = (byte)(credstr[line][0] - 65);
                        string text = credstr[line].Substring(1);

                        int x = 110 - JE_textWidth(text, SMALL_FONT_SHAPES) / 2;

                        JE_outTextAdjust(VGAScreen, x + Abs((y / 18) % 4 - 2) - 1, y - 1, text, color, -8, SMALL_FONT_SHAPES, false);
                        JE_outTextAdjust(VGAScreen, x, y, text, color, -2, SMALL_FONT_SHAPES, false);
                    }
                }

                y += 20;
            }

            fill_rectangle_xy(VGAScreen, 0, 0, 319, 10, 0);
            fill_rectangle_xy(VGAScreen, 0, 190, 319, 199, 0);

            if (currentpic == sprite_table[EXTRA_SHAPES].Length - 1)
                JE_outTextAdjust(VGAScreen, 5, 180, miscText[54], 2, -2, SMALL_FONT_SHAPES, false);  // levels-in-episode

            if (bottom_line == lines_max - 8)
                fade_song();

            if (ticks == ticks_max - 1)
            {
                --ticks;
                play_song(9);
            }

            //NETWORK_KEEP_ALIVE();

            JE_showVGA();

            yield return coroutine_wait_delay();

            if (JE_anyButton())
                break;
        }

        yield return Run(e_fade_black(10));
    }

    private static void JE_gammaCorrect_func(ref JE_byte col, JE_real r)
    {
        int temp = (int)Round(col * r);
        if (temp > 255)
        {
            temp = 255;
        }
        col = (byte)temp;
    }

    public static void JE_gammaCorrect(Color32[] colorBuffer, JE_byte gamma)
    {
        int x;
        JE_real r = 1 + (JE_real)gamma / 10;

        for (x = 0; x < 256; x++)
        {
            JE_gammaCorrect_func(ref colorBuffer[x].r, r);
            JE_gammaCorrect_func(ref colorBuffer[x].g, r);
            JE_gammaCorrect_func(ref colorBuffer[x].b, r);
        }
    }

    public static JE_boolean JE_gammaCheck()
    {
        bool temp = keysactive[(int)KeyCode.F11];
        if (temp)
        {
            keysactive[(int)KeyCode.F11] = false;
            newkey = false;
            gammaCorrection = (byte)((gammaCorrection + 1) % 4);
            System.Array.Copy(palettes[pcxpal[3 - 1]], colors, colors.Length);
            JE_gammaCorrect(colors, gammaCorrection);
            set_palette(colors, 0, 255);
        }
        return temp;
    }

    public static IEnumerator e_JE_highScoreCheck()
    { UnityEngine.Debug.Log("e_JE_highScoreCheck");
        //Some day...
        yield break;
    }

    // increases game difficulty based on player's total score / total of players' scores
    private static void adjust_difficulty()
    {
        float[] score_multiplier =
        {
            0,     // Wimp  (doesn't exist)
		    0.4f,  // Easy
		    0.8f,  // Normal
		    1.3f,  // Hard
		    1.6f,  // Impossible
		    2,     // Insanity
		    2,     // Suicide
		    3,     // Maniacal
		    3,     // Zinglon
		    3,     // Nortaneous
        };

        int score = twoPlayerMode ? (player[0].cash + player[1].cash) : JE_totalScore(player[0]),
            adjusted_score = (int)Round(score * score_multiplier[initialDifficulty]);

        int new_difficulty = 0;

        if (twoPlayerMode)
        {
            if (adjusted_score < 10000)
                new_difficulty = 1;  // Easy
            else if (adjusted_score < 20000)
                new_difficulty = 2;  // Normal
            else if (adjusted_score < 50000)
                new_difficulty = 3;  // Hard
            else if (adjusted_score < 80000)
                new_difficulty = 4;  // Impossible
            else if (adjusted_score < 125000)
                new_difficulty = 5;  // Insanity
            else if (adjusted_score < 200000)
                new_difficulty = 6;  // Suicide
            else if (adjusted_score < 400000)
                new_difficulty = 7;  // Maniacal
            else if (adjusted_score < 600000)
                new_difficulty = 8;  // Zinglon
            else
                new_difficulty = 9;  // Nortaneous
        }
        else
        {
            if (adjusted_score < 40000)
                new_difficulty = 1;  // Easy
            else if (adjusted_score < 70000)
                new_difficulty = 2;  // Normal
            else if (adjusted_score < 150000)
                new_difficulty = 3;  // Hard
            else if (adjusted_score < 300000)
                new_difficulty = 4;  // Impossible
            else if (adjusted_score < 600000)
                new_difficulty = 5;  // Insanity
            else if (adjusted_score < 1000000)
                new_difficulty = 6;  // Suicide
            else if (adjusted_score < 2000000)
                new_difficulty = 7;  // Maniacal
            else if (adjusted_score < 3000000)
                new_difficulty = 8;  // Zinglon
            else
                new_difficulty = 9;  // Nortaneous
        }

        difficultyLevel = (int)Max((uint)difficultyLevel, new_difficulty);
    }
}