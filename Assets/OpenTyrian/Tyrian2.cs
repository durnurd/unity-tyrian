using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static VarzC;
using static FileIO;
using static EpisodesC;
using static HelpTextC;
using static MainIntC;
using static GameMenuC;
using static ConfigC;
using static PlayerC;
using static ParamsC;
using static MusMastC;
using static NetworkC;
using static KeyboardC;
using static LibC;
using static FontHandC;
using static PaletteC;
using static CoroutineRunner;
using static PicLoadC;
using static VideoC;
using static SpriteC;
using static NortVarsC;
using static NortsongC;
using static PcxMastC;
using static PcxLoadC;
using static LoudnessC;
using static LvlLibC;
using static BackgrndC;
using static LvlMastC;
using static FontC;
using static FontC.Font;
using static FontC.FontAlignment;
using static OpenTyrC;
using static SetupC;
using static SndMastC;
using static MenusC;
using static VGA256dC;
using static ShotsC;
using static LdsPlayC;
using static MouseC;
using static JoystickC;
using static System.Math;

using System.IO;
using System.Collections;
using UnityEngine;
using static SurfaceC;

public static class Tyrian2C
{
    public struct boss_bar_t
    {
        public int link_num;
        public int armor;
        public int color;
    }

    public static boss_bar_t[] boss_bar = new boss_bar_t[2];

    /* Level Event Data */
    static JE_boolean quit, loadLevelOk;

    private static JE_EventRecType[] eventRec = new JE_EventRecType[EVENT_MAXIMUM]; /* [1..eventMaximum] */
    public static JE_word levelEnemyMax;
    public static JE_word levelEnemyFrequency;
    public static JE_word[] levelEnemy = new JE_word[40]; /* [1..40] */

    public static JE_byte[][] itemAvail = DoubleEmptyArray<JE_byte>(9, 10, 0);
    public static readonly JE_byte[] itemAvailMax = new JE_byte[9];

    private static WaitForSeconds coroutine_JE_starShowVGA()
    {
        WaitForSeconds ret = null;

        byte[] src;
        byte[] s; /* screen pointer, 8-bit specific */
        int srcIdx = 0, sIdx = 0;

        int x, y, lightx, lighty, lightdist;

        if (!playerEndLevel && !skipStarShowVGA)
        {

            s = VGAScreenSeg.pixels;

            src = game_screen.pixels;
            srcIdx += 24;

            if (smoothScroll /*&& thisPlayerNum != 2*/)
            {
                ret = coroutine_wait_delay();
                setjasondelay(frameCountMax);
            }

            if (starShowVGASpecialCode == 1)
            {
                srcIdx += game_screen.w * 183;
                for (y = 0; y < 184; y++)
                {
                    System.Array.Copy(src, s, 264);
                    sIdx += VGAScreenSeg.w;
                    srcIdx -= game_screen.w;
                }
            }
            else if (starShowVGASpecialCode == 2 && processorType >= 2)
            {
                lighty = 172 - player[0].y;
                lightx = 281 - player[0].x;

                for (y = 184; y > 0; y--)
                {
                    if (lighty > y)
                    {
                        for (x = 320 - 56; x > 0; x--)
                        {
                            s[sIdx] = (byte)((src[srcIdx] & 0xf0) | ((src[srcIdx] >> 2) & 0x03));
                            sIdx++;
                            srcIdx++;
                        }
                    }
                    else
                    {
                        for (x = 320 - 56; x > 0; x--)
                        {
                            lightdist = Abs(lightx - x) + lighty;
                            if (lightdist < y)
                                s[sIdx] = src[srcIdx];
                            else if (lightdist - y <= 5)
                                s[sIdx] = (byte)((src[srcIdx] & 0xf0) | (((src[srcIdx] & 0x0f) + (3 * (5 - (lightdist - y)))) / 4));
                            else
                                s[sIdx] = (byte)((src[srcIdx] & 0xf0) | ((src[srcIdx] & 0x0f) >> 2));
                            sIdx++;
                            srcIdx++;
                        }
                    }
                    sIdx += 56 + VGAScreenSeg.w - 320;
                    srcIdx += 56 + VGAScreenSeg.w - 320;
                }
            }
            else
            {
                for (y = 0; y < 184; y++)
                {
                    System.Array.Copy(src, srcIdx, s, sIdx, 264);
                    sIdx += VGAScreenSeg.w;
                    srcIdx += game_screen.w;
                }
            }
            JE_showVGA();
        }

        quitRequested = false;
        skipStarShowVGA = false;

        return ret;
    }

    private static void blit_enemy(Surface surface, int i, int x_offset, int y_offset, int sprite_offset)
    {
        if (enemy[i].sprite2s == null)
        {
            //fprintf(stderr, "warning: enemy %d sprite missing\n", i);
            return;
        }

        int x = enemy[i].ex + x_offset + tempMapXOfs,
            y = enemy[i].ey + y_offset;
        int index = enemy[i].egr[enemy[i].enemycycle - 1] + sprite_offset;

        if (enemy[i].filter != 0)
            blit_sprite2_filter(surface, x, y, enemy[i].sprite2s, index, enemy[i].filter);
        else
            blit_sprite2(surface, x, y, enemy[i].sprite2s, index);
    }

    private static void JE_drawEnemy(int enemyOffset) // actually does a whole lot more than just drawing
    {
        player[0].x -= 25;

        for (int i = enemyOffset - 25; i < enemyOffset; i++)
        {
            if (enemyAvail[i] != 1)
            {
                enemy[i].mapoffset = tempMapXOfs;

                if (enemy[i].xaccel != 0 && enemy[i].xaccel - 89u > mt_rand() % 11)
                {
                    if (player[0].x > enemy[i].ex)
                    {
                        if (enemy[i].exc < enemy[i].xaccel - 89)
                            enemy[i].exc++;
                    }
                    else
                    {
                        if (enemy[i].exc >= 0 || -enemy[i].exc < enemy[i].xaccel - 89)
                            enemy[i].exc--;
                    }
                }

                if (enemy[i].yaccel != 0 && enemy[i].yaccel - 89u > mt_rand() % 11)
                {
                    if (player[0].y > enemy[i].ey)
                    {
                        if (enemy[i].eyc < enemy[i].yaccel - 89)
                            enemy[i].eyc++;
                    }
                    else
                    {
                        if (enemy[i].eyc >= 0 || -enemy[i].eyc < enemy[i].yaccel - 89)
                            enemy[i].eyc--;
                    }
                }

                if (enemy[i].ex + tempMapXOfs > -29 && enemy[i].ex + tempMapXOfs < 300)
                {
                    if (enemy[i].aniactive == 1)
                    {
                        enemy[i].enemycycle++;

                        if (enemy[i].enemycycle == enemy[i].animax)
                            enemy[i].aniactive = enemy[i].aniwhenfire;
                        else if (enemy[i].enemycycle > enemy[i].ani)
                            enemy[i].enemycycle = enemy[i].animin;
                    }

                    if (enemy[i].egr[enemy[i].enemycycle - 1] == 999)
                        goto enemy_gone;

                    if (enemy[i].size == 1) // 2x2 enemy
                    {
                        if (enemy[i].ey > -13)
                        {
                            blit_enemy(VGAScreen, i, -6, -7, 0);
                            blit_enemy(VGAScreen, i, 6, -7, 1);
                        }
                        if (enemy[i].ey > -26 && enemy[i].ey < 182)
                        {
                            blit_enemy(VGAScreen, i, -6, 7, 19);
                            blit_enemy(VGAScreen, i, 6, 7, 20);
                        }
                    }
                    else
                    {
                        if (enemy[i].ey > -13)
                            blit_enemy(VGAScreen, i, 0, 0, 0);
                    }

                    enemy[i].filter = 0;
                }

                if (enemy[i].excc != 0)
                {
                    if (--enemy[i].exccw <= 0)
                    {
                        if (enemy[i].exc == enemy[i].exrev)
                        {
                            enemy[i].excc = (sbyte)-enemy[i].excc;
                            enemy[i].exrev = (sbyte)-enemy[i].exrev;
                            enemy[i].exccadd = (short)-enemy[i].exccadd;
                        }
                        else
                        {
                            enemy[i].exc += (sbyte)enemy[i].exccadd;
                            enemy[i].exccw = (sbyte)enemy[i].exccwmax;
                            if (enemy[i].exc == enemy[i].exrev)
                            {
                                enemy[i].excc = (sbyte)-enemy[i].excc;
                                enemy[i].exrev = (sbyte)-enemy[i].exrev;
                                enemy[i].exccadd = (short)-enemy[i].exccadd;
                            }
                        }
                    }
                }

                if (enemy[i].eycc != 0)
                {
                    if (--enemy[i].eyccw <= 0)
                    {
                        if (enemy[i].eyc == enemy[i].eyrev)
                        {
                            enemy[i].eycc = (sbyte)-enemy[i].eycc;
                            enemy[i].eyrev = (sbyte)-enemy[i].eyrev;
                            enemy[i].eyccadd = (short)-enemy[i].eyccadd;
                        }
                        else
                        {
                            enemy[i].eyc += (sbyte)enemy[i].eyccadd;
                            enemy[i].eyccw = (sbyte)enemy[i].eyccwmax;
                            if (enemy[i].eyc == enemy[i].eyrev)
                            {
                                enemy[i].eycc = (sbyte)-enemy[i].eycc;
                                enemy[i].eyrev = (sbyte)-enemy[i].eyrev;
                                enemy[i].eyccadd = (short)-enemy[i].eyccadd;
                            }
                        }
                    }
                }

                enemy[i].ey += enemy[i].fixedmovey;

                enemy[i].ex += enemy[i].exc;
                if (enemy[i].ex < -80 || enemy[i].ex > 340)
                    goto enemy_gone;

                enemy[i].ey += enemy[i].eyc;
                if (enemy[i].ey < -112 || enemy[i].ey > 190)
                    goto enemy_gone;

                goto enemy_still_exists;

            enemy_gone:
                /* enemy[i].egr[10] &= 0x00ff; <MXD> madness? */
                enemyAvail[i] = 1;
                goto draw_enemy_end;

            enemy_still_exists:

                /*X bounce*/
                if (enemy[i].ex <= enemy[i].xminbounce || enemy[i].ex >= enemy[i].xmaxbounce)
                    enemy[i].exc = (sbyte)-enemy[i].exc;

                /*Y bounce*/
                if (enemy[i].ey <= enemy[i].yminbounce || enemy[i].ey >= enemy[i].ymaxbounce)
                    enemy[i].eyc = (sbyte)-enemy[i].eyc;

                /* Evalue != 0 - score item at boundary */
                if (enemy[i].scoreitem)
                {
                    if (enemy[i].ex < -5)
                        enemy[i].ex++;
                    if (enemy[i].ex > 245)
                        enemy[i].ex--;
                }

                enemy[i].ey += (short)tempBackMove;

                if (enemy[i].ex <= -24 || enemy[i].ex >= 296)
                    goto draw_enemy_end;

                tempX = (ushort)enemy[i].ex;
                tempY = (ushort)enemy[i].ey;

                temp = enemy[i].enemytype;

                /* Enemy Shots */
                if (enemy[i].edamaged)
                    goto draw_enemy_end;

                enemyOnScreen++;

                if (enemy[i].iced != 0)
                {
                    enemy[i].iced--;
                    if (enemy[i].enemyground)
                    {
                        enemy[i].filter = 0x09;
                    }
                    goto draw_enemy_end;
                }

                for (int j = 3; j > 0; j--)
                {
                    if (enemy[i].freq[j - 1] != 0)
                    {
                        temp3 = enemy[i].tur[j - 1];

                        if (--enemy[i].eshotwait[j - 1] == 0 && temp3 != 0)
                        {
                            enemy[i].eshotwait[j - 1] = enemy[i].freq[j - 1];
                            if (difficultyLevel > 2)
                            {
                                enemy[i].eshotwait[j - 1] = (byte)((enemy[i].eshotwait[j - 1] / 2) + 1);
                                if (difficultyLevel > 7)
                                    enemy[i].eshotwait[j - 1] = (byte)((enemy[i].eshotwait[j - 1] / 2) + 1);
                            }

                            if (galagaMode && (enemy[i].eyc == 0 || (mt_rand() % 400) >= galagaShotFreq))
                                goto draw_enemy_end;

                            switch (temp3)
                            {
                                case 252: /* Savara Boss DualMissile */
                                    if (enemy[i].ey > 20)
                                    {
                                        JE_setupExplosion(tempX - 8 + tempMapXOfs, tempY - 20 - backMove * 8, -2, 6, false, false);
                                        JE_setupExplosion(tempX + 4 + tempMapXOfs, tempY - 20 - backMove * 8, -2, 6, false, false);
                                    }
                                    break;
                                case 251:
                                    ; /* Suck-O-Magnet */
                                    int attractivity = 4 - (Abs(player[0].x - tempX) + Abs(player[0].y - tempY)) / 100;
                                    player[0].x_velocity += (player[0].x > tempX) ? -attractivity : attractivity;
                                    break;
                                case 253: /* Left ShortRange Magnet */
                                    if (Abs(player[0].x + 25 - 14 - tempX) < 24 && Abs(player[0].y - tempY) < 28)
                                    {
                                        player[0].x_velocity += 2;
                                    }
                                    if (twoPlayerMode &&
                                       (Abs(player[1].x - 14 - tempX) < 24 && Abs(player[1].y - tempY) < 28))
                                    {
                                        player[1].x_velocity += 2;
                                    }
                                    break;
                                case 254: /* Left ShortRange Magnet */
                                    if (Abs(player[0].x + 25 - 14 - tempX) < 24 && Abs(player[0].y - tempY) < 28)
                                    {
                                        player[0].x_velocity -= 2;
                                    }
                                    if (twoPlayerMode &&
                                       (Abs(player[1].x - 14 - tempX) < 24 && Abs(player[1].y - tempY) < 28))
                                    {
                                        player[1].x_velocity -= 2;
                                    }
                                    break;
                                case 255: /* Magneto RePulse!! */
                                    if (difficultyLevel != 1) /*DIF*/
                                    {
                                        if (j == 3)
                                        {
                                            enemy[i].filter = 0x70;
                                        }
                                        else
                                        {
                                            int repulsivity = 4 - (Abs(player[0].x - tempX) + Abs(player[0].y - tempY)) / 20;
                                            if (repulsivity > 0)
                                                player[0].x_velocity += (player[0].x > tempX) ? repulsivity : -repulsivity;
                                        }
                                    }
                                    break;
                                default:
                                    /*Rot*/
                                    for (int tempCount = weapons[temp3].multi; tempCount > 0; tempCount--)
                                    {
                                        for (b = 0; b < ENEMY_SHOT_MAX; b++)
                                        {
                                            if (enemyShotAvail[b])
                                                break;
                                        }
                                        if (b == ENEMY_SHOT_MAX)
                                            goto draw_enemy_end;

                                        enemyShotAvail[b] = !enemyShotAvail[b];

                                        if (weapons[temp3].sound > 0)
                                        {
                                            do
                                                temp = (int)(mt_rand() % 8);
                                            while (temp == 3);
                                            soundQueue[temp] = weapons[temp3].sound;
                                        }

                                        if (enemy[i].aniactive == 2)
                                            enemy[i].aniactive = 1;

                                        if (++enemy[i].eshotmultipos[j - 1] > weapons[temp3].max)
                                            enemy[i].eshotmultipos[j - 1] = 1;

                                        int tempPos = enemy[i].eshotmultipos[j - 1] - 1;

                                        if (j == 1)
                                            temp2 = 4;

                                        enemyShot[b].sx = (short)(tempX + weapons[temp3].bx[tempPos] + tempMapXOfs);
                                        enemyShot[b].sy = (short)(tempY + weapons[temp3].by[tempPos]);
                                        enemyShot[b].sdmg = weapons[temp3].attack[tempPos];
                                        enemyShot[b].tx = weapons[temp3].tx;
                                        enemyShot[b].ty = weapons[temp3].ty;
                                        enemyShot[b].duration = weapons[temp3].del[tempPos];
                                        enemyShot[b].animate = 0;
                                        enemyShot[b].animax = weapons[temp3].weapani;

                                        enemyShot[b].sgr = weapons[temp3].sg[tempPos];
                                        switch (j)
                                        {
                                            case 1:
                                                enemyShot[b].syc = weapons[temp3].acceleration;
                                                enemyShot[b].sxc = weapons[temp3].accelerationx;

                                                enemyShot[b].sxm = weapons[temp3].sx[tempPos];
                                                enemyShot[b].sym = weapons[temp3].sy[tempPos];
                                                break;
                                            case 3:
                                                enemyShot[b].sxc = (sbyte)-weapons[temp3].acceleration;
                                                enemyShot[b].syc = (sbyte)weapons[temp3].accelerationx;

                                                enemyShot[b].sxm = (short)-weapons[temp3].sy[tempPos];
                                                enemyShot[b].sym = (short)-weapons[temp3].sx[tempPos];
                                                break;
                                            case 2:
                                                enemyShot[b].sxc = (sbyte)weapons[temp3].acceleration;
                                                enemyShot[b].syc = (sbyte)-weapons[temp3].acceleration;

                                                enemyShot[b].sxm = (short)weapons[temp3].sy[tempPos];
                                                enemyShot[b].sym = (short)-weapons[temp3].sx[tempPos];
                                                break;
                                        }

                                        if (weapons[temp3].aim > 0)
                                        {
                                            int aim = weapons[temp3].aim;

                                            /*DIF*/
                                            if (difficultyLevel > 2)
                                            {
                                                aim += difficultyLevel - 2;
                                            }

                                            int target_x = player[0].x;
                                            int target_y = player[0].y;

                                            if (twoPlayerMode)
                                            {
                                                // fire at live player(s)
                                                if (player[0].is_alive && !player[1].is_alive)
                                                    temp = 0;
                                                else if (player[1].is_alive && !player[0].is_alive)
                                                    temp = 1;
                                                else
                                                    temp = (int)(mt_rand() % 2);

                                                if (temp == 1)
                                                {
                                                    target_x = player[1].x - 25;
                                                    target_y = player[1].y;
                                                }
                                            }

                                            int relative_x = (target_x + 25) - tempX - tempMapXOfs - 4;
                                            if (relative_x == 0)
                                                relative_x = 1;
                                            int relative_y = target_y - tempY;
                                            if (relative_y == 0)
                                                relative_y = 1;
                                            int longest_side = Max(Abs(relative_x), Abs(relative_y));
                                            enemyShot[b].sxm = (short)Round((float)relative_x / longest_side * aim);
                                            enemyShot[b].sym = (short)Round((float)relative_y / longest_side * aim);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }

                /* Enemy Launch Routine */
                if (enemy[i].launchfreq != 0)
                {
                    if (--enemy[i].launchwait == 0)
                    {
                        enemy[i].launchwait = enemy[i].launchfreq;

                        if (enemy[i].launchspecial != 0)
                        {
                            /*Type  1 : Must be inline with player*/
                            if (Abs(enemy[i].ey - player[0].y) > 5)
                                goto draw_enemy_end;
                        }

                        if (enemy[i].aniactive == 2)
                        {
                            enemy[i].aniactive = 1;
                        }

                        if (enemy[i].launchtype == 0)
                            goto draw_enemy_end;

                        tempW = enemy[i].launchtype;
                        b = JE_newEnemy(enemyOffset == 50 ? 75 : enemyOffset - 25, tempW, 0);

                        /*Launch Enemy Placement*/
                        if (b > 0)
                        {

                            JE_SingleEnemyType e = enemy[b - 1];

                            e.ex = (short)tempX;
                            e.ey = (short)(tempY + enemyDat[e.enemytype].startyc);
                            if (e.size == 0)
                                e.ey -= 7;

                            if (e.launchtype > 0 && e.launchfreq == 0)
                            {
                                if (e.launchtype > 90)
                                {
                                    e.ex += (short)(mt_rand() % ((e.launchtype - 90) * 4) - (e.launchtype - 90) * 2);
                                }
                                else
                                {
                                    int target_x = (player[0].x + 25) - tempX - tempMapXOfs - 4;
                                    if (target_x == 0)
                                        target_x = 1;
                                    int tempI5 = player[0].y - tempY;
                                    if (tempI5 == 0)
                                        tempI5 = 1;
                                    int longest_side = Max(Abs(target_x), Abs(tempI5));
                                    e.exc = (sbyte)Round(((float)target_x / longest_side) * e.launchtype);
                                    e.eyc = (sbyte)Round(((float)tempI5 / longest_side) * e.launchtype);
                                }
                            }

                            do
                                temp = (int)(mt_rand() % 8);
                            while (temp == 3);
                            soundQueue[temp] = randomEnemyLaunchSounds[(mt_rand() % 3)];

                            if (enemy[i].launchspecial == 1
                                && enemy[i].linknum < 100)
                            {
                                e.linknum = enemy[i].linknum;
                            }
                        }
                    }
                }
            }
        draw_enemy_end:
            ;
        }

        player[0].x += 25;
    }

    public static IEnumerator e_JE_main()
    {
        UnityEngine.Debug.Log("e_JE_main");
        string buffer;

        int lastEnemyOnScreen;

        /* NOTE: BEGIN MAIN PROGRAM HERE AFTER LOADING A GAME OR STARTING A NEW ONE */

        /* ----------- GAME ROUTINES ------------------------------------- */
        /* We need to jump to the beginning to make space for the routines */
        /* --------------------------------------------------------------- */
        goto start_level_first;


    /*------------------------------GAME LOOP-----------------------------------*/


    /* Startlevel is called after a previous level is over.  If the first level
	   is started for a gaming session, startlevelfirst is called instead and
	   this code is skipped.  The code here finishes the level and prepares for
	   the loadmap function. */

    start_level:
        Application.targetFrameRate = 60;

        if (galagaMode)
            twoPlayerMode = false;

        JE_clearKeyboard();

        /* Normal speed */
        if (fastPlay != 0)
        {
            smoothScroll = true;
            speed = 0x4300;
            JE_resetTimerInt();
            JE_setTimerInt();
        }

        if (play_demo || record_demo)
        {
            if (demo_file != null)
            {
                demo_file.Close();
                demo_file = null;
            }

            if (play_demo)
            {
                stop_song();
                yield return Run(e_fade_black(10));

                yield return coroutine_wait_noinput(true, true, true);
            }
        }

        difficultyLevel = oldDifficultyLevel;   /*Return difficulty to normal*/

        if (!play_demo)
        {
            if ((!all_players_dead() || normalBonusLevelCurrent || bonusLevelCurrent) && !playerEndLevel)
            {
                mainLevel = nextLevel;
                yield return Run(e_JE_endLevelAni());

                fade_song();
            }
            else
            {
                fade_song();
                yield return Run(e_fade_black(10));

                JE_loadGame((byte)(twoPlayerMode ? 22 : 11));
                if (doNotSaveBackup)
                {
                    superTyrian = false;
                    onePlayerAction = false;
                    player[0].items.super_arcade_mode = SA_NONE;
                }
                if (bonusLevelCurrent && !playerEndLevel)
                {
                    mainLevel = nextLevel;
                }
            }
        }
        doNotSaveBackup = false;

        if (play_demo)
            yield break;

        start_level_first:

        set_volume(tyrMusicVolume, fxVolume);

        endLevel = false;
        reallyEndLevel = false;
        playerEndLevel = false;
        extraGame = false;

        doNotSaveBackup = false;
        yield return Run(e_JE_loadMap());

        if (mainLevel == 0)  // if quit itemscreen
            yield break;          // back to titlescreen

        fade_song();

        for (uint i = 0; i < player.Length; ++i)
            player[i].is_alive = true;

        oldDifficultyLevel = difficultyLevel;
        if (episodeNum == EPISODE_AVAILABLE)
            difficultyLevel--;
        if (difficultyLevel < 1)
            difficultyLevel = 1;

        player[0].x = 100;
        player[0].y = 180;

        player[1].x = 190;
        player[1].y = 180;

        for (uint i = 0; i < player.Length; ++i)
        {
            for (int j = 0; j < player[i].old_x.Length; ++j)
            {
                player[i].old_x[j] = player[i].x - (19 - j);
                player[i].old_y[j] = player[i].y - 18;
            }

            player[i].last_x_shot_move = player[i].x;
            player[i].last_y_shot_move = player[i].y;
        }

        JE_loadPic(VGAScreen, twoPlayerMode ? 6 : 3, false);

        JE_drawOptions();

        JE_outText(VGAScreen, 268, twoPlayerMode ? 76 : 118, levelName, 12, 4);

        JE_showVGA();

        JE_gammaCorrect(colors, gammaCorrection);

        yield return Run(e_fade_palette(colors, 50, 0, 255));

        JE_loadCompShapes(out shapes6, '6'); // explosion sprites

        /* MAPX will already be set correctly */
        mapY = 300 - 8;
        mapY2 = 600 - 8;
        mapY3 = 600 - 8;
        mapYPos = mapY;
        mapY2Pos = mapY2;
        mapY3Pos = mapY3;
        mapXPos = 0;
        mapXOfs = 0;
        mapX2Pos = 0;
        mapX3Pos = 0;
        mapX3Ofs = 0;
        mapXbpPos = 0;
        mapX2bpPos = 0;
        mapX3bpPos = 0;

        map1YDelay = 1;
        map1YDelayMax = 1;
        map2YDelay = 1;
        map2YDelayMax = 1;

        musicFade = false;

        backPos = 0;
        backPos2 = 0;
        backPos3 = 0;
        power = 0;
        starfield_speed = 1;

        /* Setup player ship graphics */
        JE_getShipInfo();

        for (uint i = 0; i < player.Length; ++i)
        {
            player[i].x_velocity = 0;
            player[i].y_velocity = 0;

            player[i].invulnerable_ticks = 100;
        }

        newkey = newmouse = false;

        /* Initialize Level Data and Debug Mode */
        levelEnd = 255;
        levelEndWarp = -4;
        levelEndFxWait = 0;
        warningCol = 120;
        warningColChange = 1;
        warningSoundDelay = 0;
        armorShipDelay = 50;

        bonusLevel = false;
        readyToEndLevel = false;
        firstGameOver = true;
        eventLoc = 1;
        curLoc = 0;
        backMove = 1;
        backMove2 = 2;
        backMove3 = 3;
        explodeMove = 2;
        enemiesActive = true;
        for (temp = 0; temp < 3; temp++)
        {
            button[temp] = false;
        }
        stopBackgrounds = false;
        stopBackgroundNum = 0;
        background3x1 = false;
        background3x1b = false;
        background3over = 0;
        background2over = 1;
        topEnemyOver = false;
        skyEnemyOverAll = false;
        smallEnemyAdjust = false;
        starActive = true;
        enemyContinualDamage = false;
        levelEnemyFrequency = 96;
        quitRequested = false;

        for (int i = 0; i < boss_bar.Length; i++)
            boss_bar[i].link_num = 0;

        forceEvents = false;  /*Force events to continue if background movement = 0*/

        superEnemy254Jump = 0;   /*When Enemy with PL 254 dies*/

        /* Filter Status */
        filterActive = true;
        filterFade = true;
        filterFadeStart = false;
        levelFilter = -99;
        levelBrightness = -14;
        levelBrightnessChg = 1;

        background2notTransparent = false;

        uint[] old_weapon_bar = { 0, 0 };  // only redrawn when they change

        /* Initially erase power bars */
        lastPower = power / 10;

        /* Initial Text */
        JE_drawTextWindow(miscText[20]);

        /* Setup Armor/Shield Data */
        shieldWait = 1;
        shieldT = (byte)(shields[player[0].items.shield].tpwr * 20);

        for (uint i = 0; i < player.Length; ++i)
        {
            player[i].shield = shields[player[i].items.shield].mpwr;
            player[i].shield_max = player[i].shield * 2;
        }

        JE_drawShield();
        JE_drawArmor();

        for (uint i = 0; i < player.Length; ++i)
            player[i].superbombs = 0;

        /* Set cubes to 0 */
        cubeMax = 0;

        /* Secret Level Display */
        flash = 0;
        flashChange = 1;
        displayTime = 0;

        play_song(levelSong - 1);

        JE_drawPortConfigButtons();

        /* --- MAIN LOOP --- */

        newkey = false;

#if WITH_NETWORK
        if (isNetworkGame)
        {
            JE_clearSpecialRequests();
            mt_srand(32402394);
        }
#endif

        initialize_starfield();

        JE_setNewGameSpeed();

        /* JE_setVol(tyrMusicVolume, fxPlayVol >> 2); NOTE: MXD killed this because it was broken */

        /*Save backup game*/
        if (!play_demo && !doNotSaveBackup)
        {
            temp = twoPlayerMode ? 22 : 11;
            JE_saveGame((byte)temp, "LAST LEVEL    ");
        }

        //ED TODO: Demo support
        //if (!play_demo && record_demo)
        //{
        //    byte new_demo_num = 0;

        //    do
        //    {
        //        sprintf(tempStr, "demorec.%d", new_demo_num++);
        //    }
        //    while (dir_file_exists(get_user_directory(), tempStr)); // until file doesn't exist

        //    demo_file = dir_fopen_warn(get_user_directory(), tempStr, "wb");
        //    if (!demo_file)
        //        exit(1);

        //    efwrite(&episodeNum, 1, 1, demo_file);
        //    efwrite(levelName, 1, 10, demo_file);
        //    efwrite(&lvlFileNum, 1, 1, demo_file);

        //    fputc(player[0].items.weapon[FRONT_WEAPON].id, demo_file);
        //    fputc(player[0].items.weapon[REAR_WEAPON].id, demo_file);
        //    fputc(player[0].items.super_arcade_mode, demo_file);
        //    fputc(player[0].items.sidekick[LEFT_SIDEKICK], demo_file);
        //    fputc(player[0].items.sidekick[RIGHT_SIDEKICK], demo_file);
        //    fputc(player[0].items.generator, demo_file);

        //    fputc(player[0].items.sidekick_level, demo_file);
        //    fputc(player[0].items.sidekick_series, demo_file);

        //    fputc(initial_episode_num, demo_file);

        //    fputc(player[0].items.shield, demo_file);
        //    fputc(player[0].items.special, demo_file);
        //    fputc(player[0].items.ship, demo_file);

        //    for (uint i = 0; i < 2; ++i)
        //        fputc(player[0].items.weapon[i].power, demo_file);

        //    for (uint i = 0; i < 3; ++i)
        //        fputc(0, demo_file);

        //    efwrite(&levelSong, 1, 1, demo_file);

        //    demo_keys = 0;
        //    demo_keys_wait = 0;
        //}

        twoPlayerLinked = false;
        linkGunDirec = (float)PI;

        for (uint i = 0; i < player.Length; ++i)
            calc_purple_balls_needed(player[i]);

        damageRate = 2;  /*Normal Rate for Collision Damage*/

        chargeWait = 5;
        chargeLevel = 0;
        chargeMax = 5;
        chargeGr = 0;
        chargeGrWait = 3;

        portConfigChange = false;

        /*Destruction Ratio*/
        totalEnemy = 0;
        enemyKilled = 0;

        astralDuration = 0;

        superArcadePowerUp = 1;

        yourInGameMenuRequest = false;

        constantLastX = -1;

        for (uint i = 0; i < player.Length; ++i)
            player[i].exploding_ticks = 0;

        if (isNetworkGame)
        {
            JE_loadItemDat();
        }

        FillByteArrayWithOnes(enemyAvail);
        FillBoolArrayWithTrues(enemyShotAvail);

        /*Initialize Shots*/
        playerShotData = EmptyArray<PlayerShotDataType>(playerShotData.Length);
        FillByteArrayWithZeros(shotAvail);
        FillByteArrayWithZeros(shotMultiPos);
        FillByteArrayWithOnes(shotRepeat);

        FillBoolArrayWithFalses(button);
        FillBoolArrayWithFalses(globalFlags);

        explosions = new explosion_type[explosions.Length];
        rep_explosions = new rep_explosion_type[rep_explosions.Length];

        /* --- Clear Sound Queue --- */
        FillByteArrayWithZeros(soundQueue);
        soundQueue[3] = V_GOOD_LUCK;

        FillByteArrayWithZeros(enemyShapeTables);
        enemy = EmptyArray<JE_SingleEnemyType>(enemy.Length);

        for (int i = 0; i < SFCurrentCode.Length; ++i)
            FillByteArrayWithZeros(SFCurrentCode[i]);
        FillByteArrayWithZeros(SFExecuted);

        zinglonDuration = 0;
        specialWait = 0;
        nextSpecialWait = 0;
        optionAttachmentMove = 0;    /*Launch the Attachments!*/
        optionAttachmentLinked = true;

        editShip1 = false;
        editShip2 = false;

        FillBoolArrayWithFalses(smoothies);

        levelTimer = false;
        randomExplosions = false;

        last_superpixel = 0;
        superpixels = new superpixel_type[superpixels.Length];

        returnActive = false;

        galagaShotFreq = 0;

        if (galagaMode)
        {
            difficultyLevel = 2;
        }
        galagaLife = 10000;

        JE_drawOptionLevel();

        // keeps map from scrolling past the top
        BKwrap1 = BKwrap1to = 1;
        BKwrap2 = BKwrap2to = 1;
        BKwrap3 = BKwrap3to = 1;

    level_loop:
        if (isNetworkGame)
        {
            smoothies[9 - 1] = false;
            smoothies[6 - 1] = false;
        }
        else
        {
            starShowVGASpecialCode = (byte)((smoothies[9 - 1] ? 1 : 0) + (smoothies[6 - 1] ? 2 : 0));
        }


        /*Background Wrapping*/
        if (mapYPos <= BKwrap1)
        {
            mapYPos = BKwrap1to;
        }
        if (mapY2Pos <= BKwrap2)
        {
            mapY2Pos = BKwrap2to;
        }
        if (mapY3Pos <= BKwrap3)
        {
            mapY3Pos = BKwrap3to;
        }


        allPlayersGone = all_players_dead() &&
                         ((player[0].lives == 1 && player[0].exploding_ticks == 0) || (!onePlayerAction && !twoPlayerMode)) &&
                         ((player[1].lives == 1 && player[1].exploding_ticks == 0) || !twoPlayerMode);


        /*-----MUSIC FADE------*/
        if (musicFade)
        {
            if (tempVolume > 10)
            {
                tempVolume--;
                set_volume(tempVolume, fxVolume);
            }
            else
            {
                musicFade = false;
            }
        }

        if (!allPlayersGone && levelEnd > 0 && endLevel)
        {
            play_song(9);
            musicFade = false;
        }
        else if (!playing && firstGameOver)
        {
            play_song(levelSong - 1);
        }


        if (!endLevel) // draw HUD
        {
            VGAScreen = VGAScreenSeg; /* side-effect of game_screen */

            /*-----------------------Message Bar------------------------*/
            if (textErase > 0 && --textErase == 0)
                blit_sprite(VGAScreenSeg, 16, 189, OPTION_SHAPES, 36);  // in-game message area

            /*------------------------Shield Gen-------------------------*/
            if (galagaMode)
            {
                for (uint i = 0; i < player.Length; ++i)
                    player[i].shield = 0;

                // spawned dragonwing died :(
                if (player[1].lives == 0 || player[1].armor == 0)
                    twoPlayerMode = false;

                if (player[0].cash >= (uint)galagaLife)
                {
                    soundQueue[6] = S_EXPLOSION_11;
                    soundQueue[7] = S_SOUL_OF_ZINGLON;

                    if (player[0].lives < 11)
                        ++player[0].lives;
                    else
                        player[0].cash += 1000;

                    if (galagaLife == 10000)
                        galagaLife = 20000;
                    else
                        galagaLife += 25000;
                }
            }
            else // not galagaMode
            {
                if (twoPlayerMode)
                {
                    if (--shieldWait == 0)
                    {
                        shieldWait = 15;

                        for (uint i = 0; i < player.Length; ++i)
                        {
                            if (player[i].shield < player[i].shield_max && player[i].is_alive)
                                ++player[i].shield;
                        }

                        JE_drawShield();
                    }
                }
                else if (player[0].is_alive && player[0].shield < player[0].shield_max && power > shieldT)
                {
                    if (--shieldWait == 0)
                    {
                        shieldWait = 15;

                        power -= shieldT;

                        ++player[0].shield;
                        if (player[1].shield < player[0].shield_max)
                            ++player[1].shield;

                        JE_drawShield();
                    }
                }
            }

            /*---------------------Weapon Display-------------------------*/
            for (uint i = 0; i < 2; ++i)
            {
                uint item_power = player[twoPlayerMode ? i : 0].items.weapon[i].power;

                if (old_weapon_bar[i] != item_power)
                {
                    old_weapon_bar[i] = item_power;

                    int x = twoPlayerMode ? 286 : 289,
                        y = (i == 0) ? (twoPlayerMode ? 6 : 17) : (twoPlayerMode ? 100 : 38);

                    fill_rectangle_xy(VGAScreenSeg, x, y, x + 1 + 10 * 2, y + 2, 0);

                    for (int j = 1; j <= item_power; ++j)
                    {
                        JE_rectangle(VGAScreen, x, y, x + 1, y + 2, (byte)(115 + j)); /* SEGa000 */
                        x += 2;
                    }
                }
            }

            /*------------------------Power Bar-------------------------*/
            if (twoPlayerMode || onePlayerAction)
            {
                power = 900;
            }
            else
            {
                power += powerAdd;
                if (power > 900)
                    power = 900;

                temp = power / 10;

                if (temp != lastPower)
                {
                    if (temp > lastPower)
                        fill_rectangle_xy(VGAScreenSeg, 269, 113 - 11 - temp, 276, 114 - 11 - lastPower, (byte)(113 + temp / 7));
                    else
                        fill_rectangle_xy(VGAScreenSeg, 269, 113 - 11 - lastPower, 276, 114 - 11 - temp, 0);

                    lastPower = temp;
                }
            }

            oldMapX3Ofs = mapX3Ofs;

            enemyOnScreen = 0;
        }

        /* use game_screen for all the generic drawing functions */
        VGAScreen = game_screen;

        /*---------------------------EVENTS-------------------------*/
        while (eventRec[eventLoc - 1].eventtime <= curLoc && eventLoc <= maxEvent)
            JE_eventSystem();

        if (isNetworkGame && reallyEndLevel)
            goto start_level;

        /* SMOOTHIES! */
        JE_checkSmoothies();
        if (anySmoothies)
            VGAScreen = VGAScreen2;  // this makes things complicated, but we do it anyway :(

        /* --- BACKGROUNDS --- */
        /* --- BACKGROUND 1 --- */

        if (forceEvents && backMove == 0)
            curLoc++;

        if (map1YDelayMax > 1 && backMove < 2)
            backMove = (ushort)((map1YDelay == 1) ? 1 : 0);

        /*Draw background*/
        if (astralDuration == 0)
            draw_background_1(VGAScreen);
        else
            JE_clr256(VGAScreen);

        /*Set Movement of background 1*/
        if (--map1YDelay == 0)
        {
            map1YDelay = map1YDelayMax;

            curLoc += backMove;

            backPos += backMove;

            if (backPos > 27)
            {
                backPos -= 28;
                mapY--;
                mapYPos--;
            }
        }

        if (starActive || astralDuration > 0)
        {
            update_and_draw_starfield(VGAScreen, starfield_speed);
        }

        if (processorType > 1 && smoothies[5 - 1])
        {
            iced_blur_filter(game_screen, VGAScreen);
            VGAScreen = game_screen;
        }

        /*-----------------------BACKGROUNDS------------------------*/
        /*-----------------------BACKGROUND 2------------------------*/
        if (background2over == 3)
        {
            draw_background_2(VGAScreen);
            background2 = true;
        }

        if (background2over == 0)
        {
            if (!(smoothies[2 - 1] && processorType < 4) && !(smoothies[1 - 1] && processorType == 3))
            {
                if (wild && !background2notTransparent)
                    draw_background_2_blend(VGAScreen);
                else
                    draw_background_2(VGAScreen);
            }
        }

        if (smoothies[0] && processorType > 2 && smoothie_data[0] == 0)
        {
            lava_filter(game_screen, VGAScreen);
            VGAScreen = game_screen;
        }
        if (smoothies[2 - 1] && processorType > 2)
        {
            water_filter(game_screen, VGAScreen);
            VGAScreen = game_screen;
        }

        /*-----------------------Ground Enemy------------------------*/
        lastEnemyOnScreen = enemyOnScreen;

        tempMapXOfs = mapXOfs;
        tempBackMove = backMove;
        JE_drawEnemy(50);
        JE_drawEnemy(100);

        if (enemyOnScreen == 0 || enemyOnScreen == lastEnemyOnScreen)
        {
            if (stopBackgroundNum == 1)
                stopBackgroundNum = 9;
        }

        if (smoothies[0] && processorType > 2 && smoothie_data[0] > 0)
        {
            lava_filter(game_screen, VGAScreen);
            VGAScreen = game_screen;
        }

        if (superWild)
        {
            neat += 3;
            JE_darkenBackground(neat);
        }

        /*-----------------------BACKGROUNDS------------------------*/
        /*-----------------------BACKGROUND 2------------------------*/
        if (!(smoothies[2 - 1] && processorType < 4) &&
            !(smoothies[1 - 1] && processorType == 3))
        {
            if (background2over == 1)
            {
                if (wild && !background2notTransparent)
                    draw_background_2_blend(VGAScreen);
                else
                    draw_background_2(VGAScreen);
            }
        }

        if (superWild)
        {
            neat++;
            JE_darkenBackground(neat);
        }

        if (background3over == 2)
            draw_background_3(VGAScreen);


        /* New Enemy */
        if (enemiesActive && mt_rand() % 100 > levelEnemyFrequency)
        {
            tempW = levelEnemy[mt_rand() % levelEnemyMax];
            if (tempW == 2)
                soundQueue[3] = S_WEAPON_7;
            b = JE_newEnemy(0, tempW, 0);
        }

        if (processorType > 1 && smoothies[3 - 1])
        {
            iced_blur_filter(game_screen, VGAScreen);
            VGAScreen = game_screen;
        }
        if (processorType > 1 && smoothies[4 - 1])
        {
            blur_filter(game_screen, VGAScreen);
            VGAScreen = game_screen;
        }

        /* Draw Sky Enemy */
        if (!skyEnemyOverAll)
        {
            lastEnemyOnScreen = enemyOnScreen;

            tempMapXOfs = mapX2Ofs;
            tempBackMove = 0;
            JE_drawEnemy(25);

            if (enemyOnScreen == lastEnemyOnScreen)
            {
                if (stopBackgroundNum == 2)
                    stopBackgroundNum = 9;
            }
        }

        if (background3over == 0)
            draw_background_3(VGAScreen);

        /* Draw Top Enemy */
        if (!topEnemyOver)
        {
            tempMapXOfs = (!background3x1) ? oldMapX3Ofs : mapXOfs;
            tempBackMove = backMove3;
            JE_drawEnemy(75);
        }

        /* Player Shot Images */
        for (int z = 0; z < MAX_PWEAPON; z++)
        {
            if (shotAvail[z] != 0)
            {
                bool is_special = false;
                int tempShotX = 0, tempShotY = 0;
                JE_byte chain;
                JE_byte playerNum;
                JE_word tempX2, tempY2;
                JE_integer damage;
                JE_byte temp2;

                if (!player_shot_move_and_draw(z, out is_special, out tempShotX, out tempShotY, out damage, out temp2, out chain, out playerNum, out tempX2, out tempY2))
                {
                    goto draw_player_shot_loop_end;
                }

                for (b = 0; b < 100; b++)
                {
                    if (enemyAvail[b] == 0)
                    {
                        bool collided;

                        if (z == MAX_PWEAPON - 1)
                        {
                            temp = 25 - Abs(zinglonDuration - 25);
                            collided = Abs(enemy[b].ex + enemy[b].mapoffset - (player[0].x + 7)) < temp;
                            temp2 = 9;
                            chain = 0;
                            damage = 10;
                        }
                        else if (is_special)
                        {
                            collided = ((enemy[b].enemycycle == 0) &&
                                        (Abs(enemy[b].ex + enemy[b].mapoffset - tempShotX - tempX2) < (25 + tempX2)) &&
                                        (Abs(enemy[b].ey - tempShotY - 12 - tempY2) < (29 + tempY2))) ||
                                       ((enemy[b].enemycycle > 0) &&
                                        (Abs(enemy[b].ex + enemy[b].mapoffset - tempShotX - tempX2) < (13 + tempX2)) &&
                                        (Abs(enemy[b].ey - tempShotY - 6 - tempY2) < (15 + tempY2)));
                        }
                        else
                        {
                            collided = ((enemy[b].enemycycle == 0) &&
                                        (Abs(enemy[b].ex + enemy[b].mapoffset - tempShotX) < 25) && (Abs(enemy[b].ey - tempShotY - 12) < 29)) ||
                                       ((enemy[b].enemycycle > 0) &&
                                        (Abs(enemy[b].ex + enemy[b].mapoffset - tempShotX) < 13) && (Abs(enemy[b].ey - tempShotY - 6) < 15));
                        }

                        if (collided)
                        {
                            if (chain > 0)
                            {
                                shotMultiPos[SHOT_MISC] = 0;
                                b = player_shot_create(0, SHOT_MISC, tempShotX, tempShotY, mouseX, mouseY, chain, playerNum);
                                shotAvail[z] = 0;
                                goto draw_player_shot_loop_end;
                            }

                            infiniteShot = false;

                            if (damage == 99)
                            {
                                damage = 0;
                                doIced = 40;
                                enemy[b].iced = 40;
                            }
                            else
                            {
                                doIced = 0;
                                if (damage >= 250)
                                {
                                    damage = (short)(damage - 250);
                                    infiniteShot = true;
                                }
                            }

                            int armorleft = enemy[b].armorleft;

                            temp = enemy[b].linknum;
                            if (temp == 0)
                                temp = 255;

                            if (enemy[b].armorleft < 255)
                            {
                                for (int i = 0; i < boss_bar.Length; i++)
                                    if (temp == boss_bar[i].link_num)
                                        boss_bar[i].color = 6;

                                if (enemy[b].enemyground)
                                    enemy[b].filter = temp2;

                                for (int e = 0; e < enemy.Length; e++)
                                {
                                    if (enemy[e].linknum == temp &&
                                        enemyAvail[e] != 1 &&
                                        enemy[e].enemyground)
                                    {
                                        if (doIced != 0)
                                            enemy[e].iced = doIced;
                                        enemy[e].filter = temp2;
                                    }
                                }
                            }

                            if (armorleft > damage)
                            {
                                if (z != MAX_PWEAPON - 1)
                                {
                                    if (enemy[b].armorleft != 255)
                                    {
                                        enemy[b].armorleft -= (byte)damage;
                                        JE_setupExplosion(tempShotX, tempShotY, 0, 0, false, false);
                                    }
                                    else
                                    {
                                        JE_doSP(tempShotX + 6, tempShotY + 6, damage / 2 + 3, (byte)(damage / 4 + 2), temp2);
                                    }
                                }

                                soundQueue[5] = S_ENEMY_HIT;

                                if ((armorleft - damage <= enemy[b].edlevel) &&
                                    ((!enemy[b].edamaged) ^ (enemy[b].edani < 0)))
                                {

                                    for (temp3 = 0; temp3 < 100; temp3++)
                                    {
                                        if (enemyAvail[temp3] != 1)
                                        {
                                            int linknum = enemy[temp3].linknum;
                                            if (
                                                 (temp3 == b) ||
                                                 (
                                                   (temp != 255) &&
                                                   (
                                                     ((enemy[temp3].edlevel > 0) && (linknum == temp)) ||
                                                     (
                                                       (enemyContinualDamage && (temp - 100 == linknum)) ||
                                                       ((linknum > 40) && (linknum / 20 == temp / 20) && (linknum <= temp))
                                                     )
                                                   )
                                                 )
                                               )
                                            {
                                                enemy[temp3].enemycycle = 1;

                                                enemy[temp3].edamaged = !enemy[temp3].edamaged;

                                                if (enemy[temp3].edani != 0)
                                                {
                                                    enemy[temp3].ani = (byte)Abs(enemy[temp3].edani);
                                                    enemy[temp3].aniactive = 1;
                                                    enemy[temp3].animax = 0;
                                                    enemy[temp3].animin = (byte)enemy[temp3].edgr;
                                                    enemy[temp3].enemycycle = (byte)(enemy[temp3].animin - 1);

                                                }
                                                else if (enemy[temp3].edgr > 0)
                                                {
                                                    enemy[temp3].egr[1 - 1] = enemy[temp3].edgr;
                                                    enemy[temp3].ani = 1;
                                                    enemy[temp3].aniactive = 0;
                                                    enemy[temp3].animax = 0;
                                                    enemy[temp3].animin = 1;
                                                }
                                                else
                                                {
                                                    enemyAvail[temp3] = 1;
                                                    enemyKilled++;
                                                }

                                                enemy[temp3].aniwhenfire = 0;

                                                if (enemy[temp3].armorleft > (byte)enemy[temp3].edlevel)
                                                    enemy[temp3].armorleft = (byte)enemy[temp3].edlevel;

                                                tempX = (ushort)(enemy[temp3].ex + enemy[temp3].mapoffset);
                                                tempY = (ushort)(enemy[temp3].ey);

                                                if (enemyDat[enemy[temp3].enemytype].esize != 1)
                                                    JE_setupExplosion(tempX, tempY - 6, 0, 1, false, false);
                                                else
                                                    JE_setupExplosionLarge(enemy[temp3].enemyground, (byte)(enemy[temp3].explonum / 2), (short)tempX, (short)tempY);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {

                                if ((temp == 254) && (superEnemy254Jump > 0))
                                    JE_eventJump(superEnemy254Jump);

                                for (temp2 = 0; temp2 < 100; temp2++)
                                {
                                    if (enemyAvail[temp2] != 1)
                                    {
                                        temp3 = enemy[temp2].linknum;
                                        if ((temp2 == b) || (temp == 254) ||
                                            ((temp != 255) && ((temp == temp3) || (temp - 100 == temp3)
                                            || ((temp3 > 40) && (temp3 / 20 == temp / 20) && (temp3 <= temp)))))
                                        {

                                            int enemy_screen_x = enemy[temp2].ex + enemy[temp2].mapoffset;

                                            if (enemy[temp2].special)
                                            {
                                                globalFlags[enemy[temp2].flagnum - 1] = enemy[temp2].setto;
                                            }

                                            if ((enemy[temp2].enemydie > 0) &&
                                                !((superArcadeMode != SA_NONE) &&
                                                  (enemyDat[enemy[temp2].enemydie].value == 30000)))
                                            {
                                                int temp_b = b;
                                                tempW = enemy[temp2].enemydie;
                                                int enemy_offset = temp2 - (temp2 % 25);
                                                if (enemyDat[tempW].value > 30000)
                                                {
                                                    enemy_offset = 0;
                                                }
                                                b = JE_newEnemy(enemy_offset, tempW, 0);
                                                if (b != 0)
                                                {
                                                    if ((superArcadeMode != SA_NONE) && (enemy[b - 1].evalue > 30000))
                                                    {
                                                        superArcadePowerUp++;
                                                        if (superArcadePowerUp > 5)
                                                            superArcadePowerUp = 1;
                                                        enemy[b - 1].egr[1 - 1] = (ushort)(5 + superArcadePowerUp * 2);
                                                        enemy[b - 1].evalue = (short)(30000 + superArcadePowerUp);
                                                    }

                                                    if (enemy[b - 1].evalue != 0)
                                                        enemy[b - 1].scoreitem = true;
                                                    else
                                                        enemy[b - 1].scoreitem = false;

                                                    enemy[b - 1].ex = enemy[temp2].ex;
                                                    enemy[b - 1].ey = enemy[temp2].ey;
                                                }
                                                b = temp_b;
                                            }

                                            if ((enemy[temp2].evalue > 0) && (enemy[temp2].evalue < 10000))
                                            {
                                                if (enemy[temp2].evalue == 1)
                                                {
                                                    cubeMax++;
                                                }
                                                else
                                                {
                                                    // in galaga mode player 2 is sidekick, so give cash to player 1
                                                    player[galagaMode ? 0 : playerNum - 1].cash += enemy[temp2].evalue;
                                                }
                                            }

                                            if ((enemy[temp2].edlevel == -1) && (temp == temp3))
                                            {
                                                enemy[temp2].edlevel = 0;
                                                enemyAvail[temp2] = 2;
                                                enemy[temp2].egr[1 - 1] = enemy[temp2].edgr;
                                                enemy[temp2].ani = 1;
                                                enemy[temp2].aniactive = 0;
                                                enemy[temp2].animax = 0;
                                                enemy[temp2].animin = 1;
                                                enemy[temp2].edamaged = true;
                                                enemy[temp2].enemycycle = 1;
                                            }
                                            else
                                            {
                                                enemyAvail[temp2] = 1;
                                                enemyKilled++;
                                            }

                                            if (enemyDat[enemy[temp2].enemytype].esize == 1)
                                            {
                                                JE_setupExplosionLarge(enemy[temp2].enemyground, enemy[temp2].explonum, (short)enemy_screen_x, enemy[temp2].ey);
                                                soundQueue[6] = S_EXPLOSION_9;
                                            }
                                            else
                                            {
                                                JE_setupExplosion(enemy_screen_x, enemy[temp2].ey, 0, 1, false, false);
                                                soundQueue[6] = S_SELECT; // S_EXPLOSION_8
                                            }
                                        }
                                    }
                                }
                            }

                            if (infiniteShot)
                            {
                                damage += 250;
                            }
                            else if (z != MAX_PWEAPON - 1)
                            {
                                if (damage <= armorleft)
                                {
                                    shotAvail[z] = 0;
                                    goto draw_player_shot_loop_end;
                                }
                                else
                                {
                                    playerShotData[z].shotDmg -= (byte)armorleft;
                                }
                            }
                        }
                    }
                }

            draw_player_shot_loop_end:
                ;
            }
        }

        /* Player movement indicators for shots that track your ship */
        for (uint i = 0; i < player.Length; ++i)
        {
            player[i].last_x_shot_move = player[i].x;
            player[i].last_y_shot_move = player[i].y;
        }

        /*=================================*/
        /*=======Collisions Detection======*/
        /*=================================*/

        for (uint i = 0; i < (twoPlayerMode ? 2 : 1); ++i)
            if (player[i].is_alive && !endLevel)
                JE_playerCollide(player[i], (byte)(i + 1));

        if (firstGameOver)
            JE_mainGamePlayerFunctions();      /*--------PLAYER DRAW+MOVEMENT---------*/

        if (!endLevel)
        {    /*MAIN DRAWING IS STOPPED STARTING HERE*/

            /* Draw Enemy Shots */
            for (int z = 0; z < ENEMY_SHOT_MAX; z++)
            {
                if (!enemyShotAvail[z])
                {
                    enemyShot[z].sxm += enemyShot[z].sxc;
                    enemyShot[z].sx += enemyShot[z].sxm;

                    if (enemyShot[z].tx != 0)
                    {
                        if (enemyShot[z].sx > player[0].x)
                        {
                            if (enemyShot[z].sxm > -enemyShot[z].tx)
                            {
                                enemyShot[z].sxm--;
                            }
                        }
                        else
                        {
                            if (enemyShot[z].sxm < enemyShot[z].tx)
                            {
                                enemyShot[z].sxm++;
                            }
                        }
                    }

                    enemyShot[z].sym += enemyShot[z].syc;
                    enemyShot[z].sy += enemyShot[z].sym;

                    if (enemyShot[z].ty != 0)
                    {
                        if (enemyShot[z].sy > player[0].y)
                        {
                            if (enemyShot[z].sym > -enemyShot[z].ty)
                            {
                                enemyShot[z].sym--;
                            }
                        }
                        else
                        {
                            if (enemyShot[z].sym < enemyShot[z].ty)
                            {
                                enemyShot[z].sym++;
                            }
                        }
                    }

                    if (enemyShot[z].duration-- == 0 || enemyShot[z].sy > 190 || enemyShot[z].sy <= -14 || enemyShot[z].sx > 275 || enemyShot[z].sx <= 0)
                    {
                        enemyShotAvail[z] = true;
                    }
                    else  // check if shot collided with player
                    {
                        for (uint i = 0; i < (twoPlayerMode ? 2 : 1); ++i)
                        {
                            if (player[i].is_alive &&
                                enemyShot[z].sx > player[i].x - player[i].shot_hit_area_x &&
                                enemyShot[z].sx < player[i].x + player[i].shot_hit_area_x &&
                                enemyShot[z].sy > player[i].y - player[i].shot_hit_area_y &&
                                enemyShot[z].sy < player[i].y + player[i].shot_hit_area_y)
                            {
                                tempX = (ushort)enemyShot[z].sx;
                                tempY = (ushort)enemyShot[z].sy;
                                temp = enemyShot[z].sdmg;

                                enemyShotAvail[z] = true;

                                JE_setupExplosion(tempX, tempY, 0, 0, false, false);

                                if (player[i].invulnerable_ticks == 0)
                                {
                                    if ((temp = JE_playerDamage(temp, player[i])) > 0)
                                    {
                                        player[i].x_velocity += (enemyShot[z].sxm * temp) / 2;
                                        player[i].y_velocity += (enemyShot[z].sym * temp) / 2;
                                    }
                                }

                                break;
                            }
                        }

                        if (enemyShotAvail[z] == false)
                        {
                            if (enemyShot[z].animax != 0)
                            {
                                if (++enemyShot[z].animate >= enemyShot[z].animax)
                                    enemyShot[z].animate = 0;
                            }

                            if (enemyShot[z].sgr >= 500)
                                blit_sprite2(VGAScreen, enemyShot[z].sx, enemyShot[z].sy, shapesW2, enemyShot[z].sgr + enemyShot[z].animate - 500);
                            else
                                blit_sprite2(VGAScreen, enemyShot[z].sx, enemyShot[z].sy, shapesC1, enemyShot[z].sgr + enemyShot[z].animate);
                        }
                    }

                }
            }
        }

        if (background3over == 1)
            draw_background_3(VGAScreen);

        /* Draw Top Enemy */
        if (topEnemyOver)
        {
            tempMapXOfs = (!background3x1) ? oldMapX3Ofs : oldMapXOfs;
            tempBackMove = backMove3;
            JE_drawEnemy(75);
        }

        /* Draw Sky Enemy */
        if (skyEnemyOverAll)
        {
            lastEnemyOnScreen = enemyOnScreen;

            tempMapXOfs = mapX2Ofs;
            tempBackMove = 0;
            JE_drawEnemy(25);

            if (enemyOnScreen == lastEnemyOnScreen)
            {
                if (stopBackgroundNum == 2)
                    stopBackgroundNum = 9;
            }
        }

        /*-------------------------- Sequenced Explosions -------------------------*/
        enemyStillExploding = false;
        for (int i = 0; i < MAX_REPEATING_EXPLOSIONS; i++)
        {
            if (rep_explosions[i].ttl != 0)
            {
                enemyStillExploding = true;

                if (rep_explosions[i].delay > 0)
                {
                    rep_explosions[i].delay--;
                    continue;
                }

                rep_explosions[i].y += (uint)(backMove2 + 1);
                short x = (short)(rep_explosions[i].x + (mt_rand() % 24) - 12);
                short y = (short)(rep_explosions[i].y + (mt_rand() % 27) - 24);

                if (rep_explosions[i].big)
                {
                    JE_setupExplosionLarge(false, 2, x, y);

                    if (rep_explosions[i].ttl == 1 || mt_rand() % 5 == 1)
                        soundQueue[7] = S_EXPLOSION_11;
                    else
                        soundQueue[6] = S_EXPLOSION_9;

                    rep_explosions[i].delay = 4 + (mt_rand() % 3);
                }
                else
                {
                    JE_setupExplosion(x, y, 0, 1, false, false);

                    soundQueue[5] = S_EXPLOSION_4;

                    rep_explosions[i].delay = 3;
                }

                rep_explosions[i].ttl--;
            }
        }

        /*---------------------------- Draw Explosions ----------------------------*/
        for (int j = 0; j < MAX_EXPLOSIONS; j++)
        {
            if (explosions[j].ttl != 0)
            {
                if (explosions[j].fixed_position != true)
                {
                    explosions[j].sprite++;
                    explosions[j].y += explodeMove;
                }
                else if (explosions[j].follow_player == true)
                {
                    explosions[j].x += explosionFollowAmountX;
                    explosions[j].y += explosionFollowAmountY;
                }
                explosions[j].y += explosions[j].delta_y;
                explosions[j].x += explosions[j].delta_x;

                if (explosions[j].y > 200 - 14)
                {
                    explosions[j].ttl = 0;
                }
                else
                {
                    if (explosionTransparent)
                        blit_sprite2_blend(VGAScreen, explosions[j].x, explosions[j].y, shapes6, explosions[j].sprite + 1);
                    else
                        blit_sprite2(VGAScreen, explosions[j].x, explosions[j].y, shapes6, explosions[j].sprite + 1);

                    explosions[j].ttl--;
                }
            }
        }

        if (!portConfigChange)
            portConfigDone = true;

        /*-----------------------BACKGROUNDS------------------------*/
        /*-----------------------BACKGROUND 2------------------------*/
        if (!(smoothies[2 - 1] && processorType < 4) &&
            !(smoothies[1 - 1] && processorType == 3))
        {
            if (background2over == 2)
            {
                if (wild && !background2notTransparent)
                    draw_background_2_blend(VGAScreen);
                else
                    draw_background_2(VGAScreen);
            }
        }


        /*-------------------------Warning---------------------------*/
        if ((player[0].is_alive && player[0].armor < 6) ||
            (twoPlayerMode && !galagaMode && player[1].is_alive && player[1].armor < 6))
        {
            int armor_amount = (player[0].is_alive && player[0].armor < 6) ? player[0].armor : player[1].armor;

            if (armorShipDelay > 0)
            {
                armorShipDelay--;
            }
            else
            {
                tempW = 560;
                b = JE_newEnemy(50, tempW, 0);
                if (b > 0)
                {
                    enemy[b - 1].enemydie = (ushort)(560 + (mt_rand() % 3) + 1);
                    enemy[b - 1].eyc -= (sbyte)(backMove3);
                    enemy[b - 1].armorleft = 4;
                }
                armorShipDelay = 500;
            }

            if ((player[0].is_alive && player[0].armor < 6 && (!isNetworkGame || thisPlayerNum == 1)) ||
                (twoPlayerMode && player[1].is_alive && player[1].armor < 6 && (!isNetworkGame || thisPlayerNum == 2)))
            {

                tempW = (ushort)(armor_amount * 4 + 8);
                if (warningSoundDelay > tempW)
                    warningSoundDelay = (byte)tempW;

                if (warningSoundDelay > 1)
                {
                    warningSoundDelay--;
                }
                else
                {
                    soundQueue[7] = S_WARNING;
                    warningSoundDelay = (byte)tempW;
                }

                warningCol = (byte)(warningCol + warningColChange);
                if (warningCol > 113 + (14 - (armor_amount * 2)))
                {
                    warningColChange = (sbyte)-warningColChange;
                    warningCol = (byte)(113 + (14 - (armor_amount * 2)));
                }
                else if (warningCol < 113)
                {
                    warningColChange = (sbyte)-warningColChange;
                }
                fill_rectangle_xy(VGAScreen, 24, 181, 138, 183, warningCol);
                fill_rectangle_xy(VGAScreen, 175, 181, 287, 183, warningCol);
                fill_rectangle_xy(VGAScreen, 24, 0, 287, 3, warningCol);

                JE_outText(VGAScreen, 140, 178, "WARNING", 7, (warningCol % 16) / 2);

            }
        }

        /*------- Random Explosions --------*/
        if (randomExplosions && mt_rand() % 10 == 1)
            JE_setupExplosionLarge(false, 20, (short)(mt_rand() % 280), (short)(mt_rand() % 180));

        /*=================================*/
        /*=======The Sound Routine=========*/
        /*=================================*/
        if (firstGameOver)
        {
            temp = 0;
            for (temp2 = 0; temp2 < SFX_CHANNELS; temp2++)
            {
                if (soundQueue[temp2] != S_NONE)
                {
                    temp = soundQueue[temp2];
                    if (temp2 == 3)
                        temp3 = fxPlayVol;
                    else if (temp == 15)
                        temp3 = fxPlayVol / 4;
                    else   /*Lightning*/
                        temp3 = fxPlayVol / 2;

                    JE_multiSamplePlay(digiFx[temp - 1], fxSize[temp - 1], (byte)temp2, (byte)temp3);

                    soundQueue[temp2] = S_NONE;
                }
            }
        }

        if (returnActive && enemyOnScreen == 0)
        {
            JE_eventJump(65535);
            returnActive = false;
        }

        ///*-------      DEbug      ---------*/
        //debugTime = SDL_GetTicks();
        //tempW = lastmouse_but;
        //tempX = (ushort)mouse_x;
        //tempY = (ushort)mouse_y;

        //if (debug)
        //{
        //    string tempStr = "";
        //    for (temp = 0; temp < 9; temp++)
        //    {
        //        tempStr = tempStr + ((smoothies[temp] ? 1 : 0) + 48);
        //    }
        //    buffer = "SM = " + tempStr;
        //    JE_outText(VGAScreen, 30, 70, buffer, 4, 0);

        //    sprintf(buffer, "Memory left = %d", -1);
        //    JE_outText(VGAScreen, 30, 80, buffer, 4, 0);
        //    sprintf(buffer, "Enemies onscreen = %d", enemyOnScreen);
        //    JE_outText(VGAScreen, 30, 90, buffer, 6, 0);

        //    debugHist = debugHist + abs((JE_longint)debugTime - (JE_longint)lastDebugTime);
        //    debugHistCount++;
        //    sprintf(tempStr, "%2.3f", 1000.0f / roundf(debugHist / debugHistCount));
        //    sprintf(buffer, "X:%d Y:%-5d  %s FPS  %d %d %d %d", (mapX - 1) * 12 + player[0].x, curLoc, tempStr, player[0].x_velocity, player[0].y_velocity, player[0].x, player[0].y);
        //    JE_outText(VGAScreen, 45, 175, buffer, 15, 3);
        //    lastDebugTime = debugTime;
        //}


        if (displayTime > 0)
        {
            displayTime--;
            JE_outTextAndDarken(VGAScreen, 90, 10, miscText[59], 15, (JE_byte)flash - 8, FONT_SHAPES);
            flash = (byte)(flash + flashChange);
            if (flash > 4 || flash == 0)
                flashChange = (sbyte)-flashChange;
        }

        /*Pentium Speed Mode?*/
        if (pentiumMode)
        {
            frameCountMax = (ushort)((frameCountMax == 2) ? 3 : 2);
        }


        /*--------  Level Timer    ---------*/
        if (levelTimer && levelTimerCountdown > 0)
        {
            levelTimerCountdown--;
            if (levelTimerCountdown == 0)
                JE_eventJump(levelTimerJumpTo);

            if (levelTimerCountdown > 200)
            {
                if (levelTimerCountdown % 100 == 0)
                    soundQueue[7] = S_WARNING;

                if (levelTimerCountdown % 10 == 0)
                    soundQueue[6] = S_CLICK;
            }
            else if (levelTimerCountdown % 20 == 0)
            {
                soundQueue[7] = S_WARNING;
            }

            JE_textShade(VGAScreen, 140, 6, miscText[66], 7, (levelTimerCountdown % 20) / 3, FULL_SHADE);
            buffer = (levelTimerCountdown / 100.0f).ToString("0.#");
            JE_dString(VGAScreen, 100, 2, buffer, SMALL_FONT_SHAPES);
        }

        /*GAME OVER*/
        if (!constantPlay && !constantDie)
        {
            if (allPlayersGone)
            {
                if (player[0].exploding_ticks > 0 || player[1].exploding_ticks > 0)
                {
                    if (galagaMode)
                        player[1].exploding_ticks = 0;

                    musicFade = true;
                }
                else
                {
                    if (play_demo || normalBonusLevelCurrent || bonusLevelCurrent)
                        reallyEndLevel = true;
                    else
                        JE_dString(VGAScreen, 120, 60, miscText[21], FONT_SHAPES); // game over

                    set_mouse_position(159, 100);
                    if (firstGameOver)
                    {
                        if (!play_demo)
                        {
                            play_song(SONG_GAMEOVER);
                            set_volume(tyrMusicVolume, fxVolume);
                        }
                        firstGameOver = false;
                    }

                    if (!play_demo)
                    {
                        push_joysticks_as_keyboard();
                        service_SDL_events(true);
                        if ((newkey || button[0] || button[1] || button[2]) || newmouse)
                        {
                            reallyEndLevel = true;
                        }
                    }

                    if (isNetworkGame)
                        reallyEndLevel = true;
                }
            }
        }

        if (play_demo) // input kills demo
        {
            push_joysticks_as_keyboard();
            service_SDL_events(false);

            if (newkey || newmouse)
            {
                reallyEndLevel = true;

                stopped_demo = true;
            }
        }
        else // input handling for pausing, menu, cheats
        {
            service_SDL_events(false);

            if (newkey)
            {
                skipStarShowVGA = false;
                yield return Run(e_JE_mainKeyboardInput());
                newkey = false;
                if (skipStarShowVGA)
                    goto level_loop;
            }

            if (pause_pressed)
            {
                pause_pressed = false;

                if (isNetworkGame)
                    pauseRequest = true;
                else
                    yield return Run(e_JE_pauseGame());
            }

            if (ingamemenu_pressed)
            {
                ingamemenu_pressed = false;

                if (isNetworkGame)
                {
                    inGameMenuRequest = true;
                }
                else
                {
                    yourInGameMenuRequest = true;
                    yield return Run(e_JE_doInGameSetup());
                    skipStarShowVGA = true;
                }
            }
        }

        /*Network Update*/
#if WITH_NETWORK
        if (isNetworkGame)
        {
            if (!reallyEndLevel)
            {
                Uint16 requests = (pauseRequest == true) |
                                  (inGameMenuRequest == true) << 1 |
                                  (skipLevelRequest == true) << 2 |
                                  (nortShipRequest == true) << 3;
                SDLNet_Write16(requests, &packet_state_out[0]->data[14]);

                SDLNet_Write16(difficultyLevel, &packet_state_out[0]->data[16]);
                SDLNet_Write16(player[0].x, &packet_state_out[0]->data[18]);
                SDLNet_Write16(player[1].x, &packet_state_out[0]->data[20]);
                SDLNet_Write16(player[0].y, &packet_state_out[0]->data[22]);
                SDLNet_Write16(player[1].y, &packet_state_out[0]->data[24]);
                SDLNet_Write16(curLoc, &packet_state_out[0]->data[26]);

                network_state_send();

                if (network_state_update())
                {
                    assert(SDLNet_Read16(&packet_state_in[0]->data[26]) == SDLNet_Read16(&packet_state_out[network_delay]->data[26]));

                    requests = SDLNet_Read16(&packet_state_in[0]->data[14]) ^ SDLNet_Read16(&packet_state_out[network_delay]->data[14]);
                    if (requests & 1)
                    {
                        JE_pauseGame();
                    }
                    if (requests & 2)
                    {
                        yourInGameMenuRequest = SDLNet_Read16(&packet_state_out[network_delay]->data[14]) & 2;
                        JE_doInGameSetup();
                        yourInGameMenuRequest = false;
                        if (haltGame)
                            reallyEndLevel = true;
                    }
                    if (requests & 4)
                    {
                        levelTimer = true;
                        levelTimerCountdown = 0;
                        endLevel = true;
                        levelEnd = 40;
                    }
                    if (requests & 8) // nortship
                    {
                        player[0].items.ship = 12;                     // Nort Ship
                        player[0].items.special = 13;                  // Astral Zone
                        player[0].items.weapon[FRONT_WEAPON].id = 36;  // NortShip Super Pulse
                        player[0].items.weapon[REAR_WEAPON].id = 37;   // NortShip Spreader
                        shipGr = 1;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        if (SDLNet_Read16(&packet_state_in[0]->data[18 + i * 2]) != SDLNet_Read16(&packet_state_out[network_delay]->data[18 + i * 2]) || SDLNet_Read16(&packet_state_in[0]->data[20 + i * 2]) != SDLNet_Read16(&packet_state_out[network_delay]->data[20 + i * 2]))
                        {
                            char temp[64];
                            sprintf(temp, "Player %d is unsynchronized!", i + 1);

                            JE_textShade(game_screen, 40, 110 + i * 10, temp, 9, 2, FULL_SHADE);
                        }
                    }
                }
            }

            JE_clearSpecialRequests();
        }
#endif

        /** Test **/
        JE_drawSP();

        /*Filtration*/
        if (filterActive)
        {
            JE_filterScreen((sbyte)levelFilter, (sbyte)levelBrightness);
        }

        draw_boss_bar();

        JE_inGameDisplays();

        VGAScreen = VGAScreenSeg; /* side-effect of game_screen */

        yield return coroutine_JE_starShowVGA();

        /*Start backgrounds if no enemies on screen
          End level if number of enemies left to kill equals 0.*/
        if (stopBackgroundNum == 9 && backMove == 0 && !enemyStillExploding)
        {
            backMove = 1;
            backMove2 = 2;
            backMove3 = 3;
            explodeMove = 2;
            stopBackgroundNum = 0;
            stopBackgrounds = false;
            if (waitToEndLevel)
            {
                endLevel = true;
                levelEnd = 40;
            }
            if (allPlayersGone)
            {
                reallyEndLevel = true;
            }
        }

        if (!endLevel && enemyOnScreen == 0)
        {
            if (readyToEndLevel && !enemyStillExploding)
            {
                if (levelTimerCountdown > 0)
                {
                    levelTimer = false;
                }
                readyToEndLevel = false;
                endLevel = true;
                levelEnd = 40;
                if (allPlayersGone)
                {
                    reallyEndLevel = true;
                }
            }
            if (stopBackgrounds)
            {
                stopBackgrounds = false;
                backMove = 1;
                backMove2 = 2;
                backMove3 = 3;
                explodeMove = 2;
            }
        }

        /*Other Network Functions*/
        JE_handleChat();

        if (reallyEndLevel)
        {
            goto start_level;
        }
        goto level_loop;
    }

    public static IEnumerator e_JE_loadMap()
    { UnityEngine.Debug.Log("e_JE_loadMap");
        string buffer;
        byte[] pic_buffer = new byte[320 * 200]; /* screen buffer, 8-bit specific */
        byte[] vga, pic, vga2; /* screen pointer, 8-bit specific */

        lastCubeMax = cubeMax;

        /*Defaults*/
        songBuy = DEFAULT_SONG_BUY;  /*Item Screen default song*/

        /* Load LEVELS.DAT - Section = MAINLEVEL */
        saveLevel = mainLevel;

    new_game:
        galagaMode = false;
        useLastBank = false;
        extraGame = false;
        haltGame = false;

        gameLoaded = false;

        if (!play_demo)
        {
            do
            {
                BinaryReader ep_f = open(episode_file);
                string s;

                jumpSection = false;
                loadLevelOk = false;

                /* Seek Section # Mainlevel */
                int x = 0;
                while (x < mainLevel)
                {
                    s = read_encrypted_pascal_string(ep_f);
                    if (s.Length > 0 && s[0] == '*')
                    {
                        x++;
                    }
                }

                do
                {
                    s = read_encrypted_pascal_string(ep_f);

                    if (s.Length < 2 || s[0] != ']')
                        continue;
                    switch (s[1])
                    {
                        case 'A':
                            break;
                        case 'G':
                            {
                                str_pop_int(ref s, 4, out mapOrigin);
                                str_pop_int(ref s, 4, out mapPNum);
                                for (int i = 0; i < mapPNum; i++)
                                {
                                    str_pop_int(ref s, 4, out mapPlanet[i]);
                                    str_pop_int(ref s, 4, out mapSection[i]);
                                }
                                break;
                            }
                        case '?':
                            {
                                int temp;
                                str_pop_int(ref s, 4, out temp);
                                for (int i = 0; i < temp; i++)
                                {
                                    str_pop_int(ref s, 4, out cubeList[i]);
                                }
                                if (cubeMax > temp)
                                    cubeMax = temp;
                                break;
                            }
                        case '!':
                            str_pop_int(ref s, 4, out cubeMax);    /*Auto set CubeMax*/
                            break;
                        case '+':
                            str_pop_int(ref s, 4, out temp);
                            cubeMax += temp;
                            if (cubeMax > 4)
                                cubeMax = 4;
                            break;
                        case 'g':
                            galagaMode = true;   /*GALAGA mode*/

                            player[1].items = player[0].items;
                            player[1].items.weapon[REAR_WEAPON].id = 15;  // Vulcan Cannon
                            for (uint i = 0; i < player[1].items.sidekick.Length; ++i)
                                player[1].items.sidekick[i] = 0;          // None
                            break;

                        case 'x':
                            extraGame = true;
                            break;

                        case 'e': // ENGAGE mode, used for mini-games
                            doNotSaveBackup = true;
                            constantDie = false;
                            onePlayerAction = true;
                            superTyrian = true;
                            twoPlayerMode = false;

                            player[0].cash = 0;

                            player[0].items.ship = 13;                     // The Stalker 21.126
                            player[0].items.weapon[FRONT_WEAPON].id = 39;  // Atomic RailGun
                            player[0].items.weapon[REAR_WEAPON].id = 0;    // None
                            for (uint i = 0; i < player[0].items.sidekick.Length; ++i)
                                player[0].items.sidekick[i] = 0;           // None
                            player[0].items.generator = 2;                 // Advanced MR-12
                            player[0].items.shield = 4;                    // Advanced Integrity Field
                            player[0].items.special = 0;                   // None

                            player[0].items.weapon[FRONT_WEAPON].power = 3;
                            player[0].items.weapon[REAR_WEAPON].power = 1;
                            break;

                        case 'J':  // section jump
                            int tmp;
                            str_pop_int(ref s, 3, out tmp);
                            mainLevel = tmp;
                            jumpSection = true;
                            break;

                        case '2':  // two-player section jump
                            str_pop_int(ref s, 3, out temp);
                            if (twoPlayerMode || onePlayerAction)
                            {
                                mainLevel = temp;
                                jumpSection = true;
                            }
                            break;

                        case 'w':  // Stalker 21.126 section jump
                            str_pop_int(ref s, 3, out temp);   /*Allowed to go to Time War?*/
                            if (player[0].items.ship == 13)
                            {
                                mainLevel = temp;
                                jumpSection = true;
                            }
                            break;

                        case 't':
                            str_pop_int(ref s, 3, out temp);
                            if (levelTimer && levelTimerCountdown == 0)
                            {
                                mainLevel = temp;
                                jumpSection = true;
                            }
                            break;

                        case 'l':
                            str_pop_int(ref s, 3, out temp);
                            if (!all_players_alive())
                            {
                                mainLevel = temp;
                                jumpSection = true;
                            }
                            break;

                        case 's':
                            saveLevel = mainLevel;
                            break; /*store savepoint*/

                        case 'b':
                            if (twoPlayerMode)
                            {
                                temp = 22;
                            }
                            else
                            {
                                temp = 11;
                            }
                            JE_saveGame(11, "LAST LEVEL    ");
                            break;

                        case 'i':
                            str_pop_int(ref s, 3, out temp);
                            songBuy = temp - 1;
                            break;

                        case 'I':
                            {
                                itemAvail = DoubleEmptyArray<JE_byte>(9, 10, 0);


                                for (int i = 0; i < 9; ++i)
                                {
                                    s = read_encrypted_pascal_string(ep_f);

                                    string buf = s.Length > 8 ? s.Substring(8) : "";

                                    int j = 0, temp;
                                    while (str_pop_int(ref buf, 0, out temp))
                                        itemAvail[i][j++] = (byte)temp;
                                    itemAvailMax[i] = (byte)j;
                                }

                                yield return Run(e_JE_itemScreen());
                                break;
                            }

                        case 'L':
                            string tmpS = s;
                            str_pop_int(ref tmpS, 9, out nextLevel);
                            levelName = s.Substring(13, 9).Trim();
                            tmpS = s;
                            str_pop_int(ref tmpS, 22, out levelSong);
                            if (nextLevel == 0)
                            {
                                nextLevel = mainLevel + 1;
                            }
                            tmpS = s;
                            str_pop_int(ref tmpS, 25, out lvlFileNum);
                            loadLevelOk = true;
                            bonusLevelCurrent = (s.Length > 28) && (s[28] == '$');
                            normalBonusLevelCurrent = (s.Length > 27) && (s[27] == '$');
                            gameJustLoaded = false;
                            break;

                        case '@':
                            useLastBank = !useLastBank;
                            break;

                        case 'Q':
                            {
                                ESCPressed = false;
                                temp = (int)(secretHint + (mt_rand() % 3) * 3);

                                if (twoPlayerMode)
                                {
                                    for (uint i = 0; i < 2; ++i)
                                        levelWarningText[i] = miscText[40] + " " + player[i].cash;
                                    levelWarningText[2] = "";
                                    levelWarningLines = 3;
                                }
                                else
                                {
                                    levelWarningText[0] = miscText[37] + " " + JE_totalScore(player[0]);
                                    levelWarningText[1] = "";
                                    levelWarningLines = 2;
                                }

                                for (x = 0; x < temp - 1; x++)
                                {
                                    do
                                        s = read_encrypted_pascal_string(ep_f);
                                    while (s.Length == 0 || s[0] != '#');
                                }

                                do
                                {
                                    levelWarningText[levelWarningLines] = read_encrypted_pascal_string(ep_f);
                                    levelWarningLines++;
                                }
                                while (s.Length == 0 || s[0] != '#');
                                levelWarningLines--;

                                JE_wipeKey();
                                frameCountMax = 4;
                                if (!constantPlay)
                                    yield return Run(e_JE_displayText());

                                yield return Run(e_fade_black(15));

                                yield return Run(e_JE_nextEpisode());

                                if (jumpBackToEpisode1 && !twoPlayerMode)
                                {
                                    JE_loadPic(VGAScreen, 1, false); // huh?
                                    JE_clr256(VGAScreen);

                                    if (superTyrian)
                                    {
                                        // if completed Zinglon's Revenge, show SuperTyrian and Destruct codes
                                        // if completed SuperTyrian, show Nort-Ship Z code
                                        superArcadeMode = (initialDifficulty == 8) ? 8 : 1;
                                    }

                                    if (superArcadeMode < SA_ENGAGE)
                                    {
                                        if (SANextShip[superArcadeMode] == SA_ENGAGE)
                                        {
                                            buffer = miscTextB[4] + " " + pName[0];
                                            JE_dString(VGAScreen, JE_fontCenter(buffer, FONT_SHAPES), 100, buffer, FONT_SHAPES);

                                            buffer = "Or play... " + specialName[7];
                                            JE_dString(VGAScreen, 80, 180, buffer, SMALL_FONT_SHAPES);
                                        }
                                        else
                                        {
                                            JE_dString(VGAScreen, JE_fontCenter(superShips[0], FONT_SHAPES), 30, superShips[0], FONT_SHAPES);
                                            JE_dString(VGAScreen, JE_fontCenter(superShips[SANextShip[superArcadeMode]], SMALL_FONT_SHAPES), 100, superShips[SANextShip[superArcadeMode]], SMALL_FONT_SHAPES);
                                        }

                                        if (SANextShip[superArcadeMode] < SA_NORTSHIPZ)
                                            blit_sprite2x2(VGAScreen, 148, 70, shapes9, ships[SAShip[SANextShip[superArcadeMode] - 1]].shipgraphic);
                                        else if (SANextShip[superArcadeMode] == SA_NORTSHIPZ)
                                            trentWin = true;

                                        buffer = "Type " + specialName[SANextShip[superArcadeMode] - 1] + " at Title";
                                        JE_dString(VGAScreen, JE_fontCenter(buffer, SMALL_FONT_SHAPES), 160, buffer, SMALL_FONT_SHAPES);
                                        JE_showVGA();

                                        yield return Run(e_fade_palette(colors, 50, 0, 255));

                                        if (!constantPlay)
                                            yield return coroutine_wait_input(true, true, true);
                                    }

                                    jumpSection = true;

                                    if (isNetworkGame)
                                        JE_readTextSync();

                                    if (superTyrian)
                                    {
                                        yield return Run(e_fade_black(10));

                                        // back to titlescreen
                                        mainLevel = 0;
                                        yield break;
                                    }
                                }
                                break;
                            }

                        case 'P':
                            if (!constantPlay)
                            {
                                int tempX;
                                str_pop_int(ref s, 3, out tempX);
                                if (tempX > 900)
                                {
                                    System.Array.Copy(palettes[pcxpal[tempX - 1 - 900]], colors, colors.Length);
                                    JE_clr256(VGAScreen);
                                    JE_showVGA();
                                    yield return Run(e_fade_palette(colors, 1, 0, 255));
                                }
                                else
                                {
                                    if (tempX == 0)
                                        JE_loadPCX("tshp2.pcx");
                                    else
                                        JE_loadPic(VGAScreen, (byte)tempX, false);

                                    JE_showVGA();
                                    yield return Run(e_fade_palette(colors, 10, 0, 255));
                                }
                            }
                            break;

                        case 'U':
                            if (!constantPlay)
                            {
                                System.Array.Copy(VGAScreen.pixels, VGAScreen2.pixels, VGAScreen2.pixels.Length);

                                int tempX;
                                str_pop_int(ref s, 3, out tempX);

                                JE_loadPic(VGAScreen, (byte)tempX, false);
                                System.Array.Copy(VGAScreen.pixels, pic_buffer, pic_buffer.Length);

                                service_SDL_events(true);

                                for (int z = 0; z <= 199; z++)
                                {
                                    if (!newkey)
                                    {
                                        vga = VGAScreen.pixels;
                                        vga2 = VGAScreen2.pixels;

                                        pic = pic_buffer;
                                        int picIdx = (199 - z) * 320;
                                        int vgaIdx = 0, vga2Idx = 0;

                                        setjasondelay(1); /* attempting to emulate JE_waitRetrace();*/

                                        for (y = 0; y <= 199; y++)
                                        {
                                            if (y <= z)
                                            {
                                                System.Array.Copy(pic, picIdx, vga, vgaIdx, 320);
                                                picIdx += 320;
                                            }
                                            else
                                            {
                                                System.Array.Copy(vga2, vga2Idx, vga, vgaIdx, VGAScreen.w);
                                                vga2Idx += VGAScreen.w;
                                            }
                                            vgaIdx += VGAScreen.w;
                                        }

                                        JE_showVGA();

                                        if (isNetworkGame)
                                        {
                                            /* TODO: NETWORK */
                                        }

                                        yield return coroutine_service_wait_delay();
                                    }
                                }

                                System.Array.Copy(pic_buffer, VGAScreen.pixels, pic_buffer.Length);
                            }
                            break;

                        case 'V':
                            if (!constantPlay)
                            {
                                /* TODO: NETWORK */
                                System.Array.Copy(VGAScreen.pixels, VGAScreen2.pixels, VGAScreen2.pixels.Length);

                                str_pop_int(ref s, 3, out temp);
                                JE_loadPic(VGAScreen, (byte)temp, false);
                                System.Array.Copy(VGAScreen.pixels, pic_buffer, pic_buffer.Length);

                                service_SDL_events(true);
                                for (int z = 0; z <= 199; z++)
                                {
                                    if (!newkey)
                                    {
                                        vga = VGAScreen.pixels;
                                        vga2 = VGAScreen2.pixels;
                                        pic = pic_buffer;
                                        int vgaIdx = 0, vga2Idx = 0, picIdx = 0;

                                        setjasondelay(1); /* attempting to emulate JE_waitRetrace();*/

                                        for (y = 0; y < 199; y++)
                                        {
                                            if (y <= 199 - z)
                                            {
                                                System.Array.Copy(vga2, vga2Idx, vga, vgaIdx, VGAScreen.w);
                                                vga2Idx += VGAScreen.w;
                                            }
                                            else
                                            {
                                                System.Array.Copy(pic, picIdx, vga, vgaIdx, 320);
                                                picIdx += 320;
                                            }
                                            vgaIdx += VGAScreen.w;
                                        }

                                        JE_showVGA();

                                        if (isNetworkGame)
                                        {
                                            /* TODO: NETWORK */
                                        }

                                        yield return coroutine_service_wait_delay();
                                    }
                                }

                                System.Array.Copy(pic_buffer, VGAScreen.pixels, pic_buffer.Length);
                            }
                            break;

                        case 'R':
                            if (!constantPlay)
                            {

                                System.Array.Copy(VGAScreen.pixels, VGAScreen2.pixels, VGAScreen2.pixels.Length);

                                int tempX;
                                str_pop_int(ref s, 3, out tempX);
                                JE_loadPic(VGAScreen, (byte)tempX, false);
                                System.Array.Copy(VGAScreen.pixels, pic_buffer, pic_buffer.Length);

                                service_SDL_events(true);

                                for (int z = 0; z <= 318; z++)
                                {
                                    if (!newkey)
                                    {
                                        vga = VGAScreen.pixels;
                                        vga2 = VGAScreen2.pixels;
                                        pic = pic_buffer;
                                        int vgaIdx = 0, vga2Idx = 0, picIdx = 0;

                                        setjasondelay(1); /* attempting to emulate JE_waitRetrace();*/

                                        for (y = 0; y < 200; y++)
                                        {
                                            System.Array.Copy(vga2, vga2Idx + z, vga, vgaIdx, 319 - z);
                                            vgaIdx += 320 - z;
                                            vga2Idx += VGAScreen2.w;
                                            System.Array.Copy(pic, picIdx, vga, vgaIdx, z + 1);
                                            vgaIdx += z;
                                            picIdx += 320;
                                        }

                                        JE_showVGA();

                                        if (isNetworkGame)
                                        {
                                            /* TODO: NETWORK */
                                        }

                                        yield return coroutine_service_wait_delay();
                                    }
                                }

                                System.Array.Copy(pic_buffer, VGAScreen.pixels, pic_buffer.Length);
                            }
                            break;

                        case 'C':
                            if (!isNetworkGame)
                            {
                                yield return Run(e_fade_black(10));
                            }
                            JE_clr256(VGAScreen);
                            JE_showVGA();
                            System.Array.Copy(palettes[7], colors, colors.Length);
                            set_palette(colors, 0, 255);
                            break;

                        case 'B':
                            if (!isNetworkGame)
                            {
                                yield return Run(e_fade_black(10));
                            }
                            break;
                        case 'F':
                            if (!isNetworkGame)
                            {
                                yield return Run(e_fade_white(100));
                                yield return Run(e_fade_black(30));
                            }
                            JE_clr256(VGAScreen);
                            JE_showVGA();
                            break;

                        case 'W':
                            if (!constantPlay)
                            {
                                if (!ESCPressed)
                                {
                                    JE_wipeKey();
                                    warningCol = 14 * 16 + 5;
                                    warningColChange = 1;
                                    warningSoundDelay = 0;
                                    levelWarningDisplay = (s[2] == 'y');
                                    levelWarningLines = 0;
                                    str_pop_int(ref s, 4, out temp);
                                    frameCountMax = (JE_word)temp;
                                    setjasondelay2(6);
                                    warningRed = (frameCountMax / 10) != 0;
                                    frameCountMax = (JE_word)(frameCountMax % 10);

                                    do
                                    {
                                        s = read_encrypted_pascal_string(ep_f);

                                        if (s?.Length > 0 && s[0] != '#')
                                        {
                                            levelWarningText[levelWarningLines] = s;
                                            levelWarningLines++;
                                        }
                                    }
                                    while (!(s?.Length > 0 && s[0] == '#'));

                                    yield return Run(e_JE_displayText());
                                    newkey = false;
                                }
                            }
                            break;

                        case 'H':
                            if (initialDifficulty < 3)
                            {
                                str_pop_int(ref s, 4, out mainLevel);
                                jumpSection = true;
                            }
                            break;

                        case 'h':
                            if (initialDifficulty > 2)
                            {
                                s = read_encrypted_pascal_string(ep_f);
                            }
                            break;

                        case 'S':
                            if (isNetworkGame)
                            {
                                JE_readTextSync();
                            }
                            break;

                        case 'n':
                            ESCPressed = false;
                            break;

                        case 'M':
                            str_pop_int(ref s, 3, out temp);
                            play_song(temp - 1);
                            break;

#if TYRIAN2000
                                            case 'T':
                                                /* TODO: Timed Battle ]T[ 43 44 45 46 47 */
                                                printf("]T[ 43 44 45 46 47 handle timed battle!");
                                                break;

                                            case 'q':
                                                /* TODO: Timed Battle end */
                                                printf("handle timed battle end flag!");
                                                break;
#endif
                    }
                } while (!(loadLevelOk || jumpSection));
                ep_f.Close();
            } while (!loadLevelOk);
        }

        if (play_demo)
            load_next_demo();
        else
            yield return Run(e_fade_black(50));


        //Load level from file

        int yy;

        JE_byte[] shape = new JE_byte[24 * 28]; /* [1..(24*28) div 2] */

        JE_byte[] mapBuf = new JE_byte[15 * 600]; /* [1..15 * 600] */
        JE_word bufLoc;

        BinaryReader level_f = open(levelFile);
        level_f.BaseStream.Seek(lvlPos[(lvlFileNum - 1) * 2], SeekOrigin.Begin);

        level_f.ReadByte(); // char_mapFile
        JE_char char_shapeFile = (char)level_f.ReadByte();
        mapX = level_f.ReadUInt16();
        mapX2 = level_f.ReadUInt16();
        mapX3 = level_f.ReadUInt16();

        levelEnemyMax = level_f.ReadUInt16();
        for (x = 0; x < levelEnemyMax; x++)
        {
            levelEnemy[x] = level_f.ReadUInt16();
        }

        maxEvent = level_f.ReadUInt16();
        for (x = 0; x < maxEvent; x++)
        {
            eventRec[x].eventtime = level_f.ReadUInt16();
            eventRec[x].eventtype = level_f.ReadByte();
            eventRec[x].eventdat = level_f.ReadInt16();
            eventRec[x].eventdat2 = level_f.ReadInt16();
            eventRec[x].eventdat3 = level_f.ReadSByte();
            eventRec[x].eventdat5 = level_f.ReadSByte();
            eventRec[x].eventdat6 = level_f.ReadSByte();
            eventRec[x].eventdat4 = level_f.ReadByte();
        }
        eventRec[x].eventtime = 65500;  /*Not needed but just in case*/

        /*debuginfo('Level loaded.');*/

        /*debuginfo('Loading Map');*/

        /* MAP SHAPE LOOKUP TABLE - Each map is directly after level */
        JE_word[][] mapSh = new JE_word[3][]; /* [1..3, 0..127] */
        for (temp = 0; temp < 3; temp++)
        {
            mapSh[temp] = level_f.ReadUInt16s(128);
            for (temp2 = 0; temp2 < mapSh[temp].Length; ++temp2)
            {
                JE_word word = mapSh[temp][temp2];
                mapSh[temp][temp2] = (JE_word)((word << 8) + (word >> 8));
            }
        }

        /* Read Shapes.DAT */
        string tempStr = "shapes" + char.ToLower(char_shapeFile) + ".dat";
        BinaryReader shpFile = open(tempStr);

        for (int z = 0; z < 600; z++)
        {
            JE_boolean shapeBlank = shpFile.ReadBoolean();

            if (shapeBlank)
                shape = new JE_byte[24 * 28];
            else
            {
                shape = shpFile.ReadBytes(24 * 28);
            }

            /* Match 1 */
            for (x = 0; x <= 71; ++x)
            {
                if (mapSh[0][x] == z + 1)
                {
                    megaData1.shapes[x].sh = shape;
                }
            }

            /* Match 2 */
            for (x = 0; x <= 71; ++x)
            {
                if (mapSh[1][x] == z + 1)
                {
                    if (x != 71 && !shapeBlank)
                    {
                        megaData2.shapes[x].sh = shape;

                        bool fill = true;
                        for (yy = 0; yy < (24 * 28) >> 1; yy++)
                        {
                            if (shape[yy] == 0)
                            {
                                fill = false;
                                break;
                            }
                        }

                        megaData2.shapes[x].fill = fill;
                    }
                }
            }

            /*Match 3*/
            for (x = 0; x <= 71; ++x)
            {
                if (mapSh[2][x] == z + 1)
                {
                    if (x < 70 && !shapeBlank)
                    {
                        megaData3.shapes[x].sh = shape;
                        bool fill = true;
                        for (yy = 0; yy < (24 * 28) >> 1; yy++)
                        {
                            if (shape[yy] == 0)
                            {
                                fill = false;
                                break;
                            }
                        }

                        megaData3.shapes[x].fill = fill;
                    }
                }
            }
        }

        shpFile.Close();

        mapBuf = level_f.ReadBytes(300 * 14);
        bufLoc = 0;              /* MAP NUMBER 1 */
        for (y = 0; y < 300; y++)
        {
            for (x = 0; x < 14; x++)
            {
                megaData1.mainmap[y][x] = mapBuf[bufLoc];
                bufLoc++;
            }
        }

        mapBuf = level_f.ReadBytes(600 * 14);
        bufLoc = 0;              /* MAP NUMBER 2 */
        for (y = 0; y < 600; y++)
        {
            for (x = 0; x < 14; x++)
            {
                megaData2.mainmap[y][x] = mapBuf[bufLoc];
                bufLoc++;
            }
        }

        mapBuf = level_f.ReadBytes(600 * 15);
        bufLoc = 0;              /* MAP NUMBER 3 */
        for (y = 0; y < 600; y++)
        {
            for (x = 0; x < 15; x++)
            {
                megaData3.mainmap[y][x] = mapBuf[bufLoc];
                bufLoc++;
            }
        }

        level_f.Close();

        /* Note: The map data is automatically calculated with the correct mapsh
        value and then the pointer is calculated using the formula (MAPSH-1)*168.
        Then, we'll automatically add S2Ofs to get the exact offset location into
        the shape table! This makes it VERY FAST! */
    }

    public static IEnumerator e_JE_titleScreen(JE_boolean animate, bool[] refQuit)
    { UnityEngine.Debug.Log("e_JE_titleScreen");
        refQuit[0] = false;

        Application.targetFrameRate = 60;

#if TYRIAN2000
        const int menunum = 6;
#else
        const int menunum = 7;
#endif

        int[] arcade_code_i = new int[SA_ENGAGE];

        JE_byte menu = 0;
        JE_boolean redraw = true,
                   fadeIn = false;

        play_demo = false;
        stopped_demo = false;

        redraw = true;
        fadeIn = false;

        gameLoaded = false;
        jumpSection = false;

#if WITH_NETWORK
        if (isNetworkGame)
        {
            JE_loadPic(VGAScreen, 2, false);
            memcpy(VGAScreen2.pixels, VGAScreen.pixels, VGAScreen2.pitch * VGAScreen2.h);
            JE_dString(VGAScreen, JE_fontCenter("Waiting for other player.", SMALL_FONT_SHAPES), 140, "Waiting for other player.", SMALL_FONT_SHAPES);
            JE_showVGA();
            fade_palette(colors, 10, 0, 255);

            network_connect();

            twoPlayerMode = true;
            if (thisPlayerNum == 1)
            {
                fade_black(10);

                if (select_episode() && select_difficulty())
                {
                    initialDifficulty = difficultyLevel;

                    difficultyLevel++;  /*Make it one step harder for 2-player mode!*/

                    network_prepare(PACKET_DETAILS);
                    SDLNet_Write16(episodeNum, &packet_out_temp.data[4]);
                    SDLNet_Write16(difficultyLevel, &packet_out_temp.data[6]);
                    network_send(8);  // PACKET_DETAILS
                }
                else
                {
                    network_prepare(PACKET_QUIT);
                    network_send(4);  // PACKET QUIT

                    network_tyrian_halt(0, true);
                }
            }
            else
            {
                memcpy(VGAScreen.pixels, VGAScreen2.pixels, VGAScreen.pitch * VGAScreen.h);
                JE_dString(VGAScreen, JE_fontCenter(networkText[4 - 1], SMALL_FONT_SHAPES), 140, networkText[4 - 1], SMALL_FONT_SHAPES);
                JE_showVGA();

                // until opponent sends details packet
                while (true)
                {
                    service_SDL_events(false);
                    JE_showVGA();

                    if (packet_in[0] && SDLNet_Read16(&packet_in[0].data[0]) == PACKET_DETAILS)
                        break;

                    network_update();
                    network_check();

                    SDL_Delay(16);
                }

                JE_initEpisode(SDLNet_Read16(&packet_in[0].data[4]));
                difficultyLevel = SDLNet_Read16(&packet_in[0].data[6]);
                initialDifficulty = difficultyLevel - 1;
                fade_black(10);

                network_update();
            }

            for (uint i = 0; i < COUNTOF(player); ++i)
                player[i].cash = 0;

            player[0].items.ship = 11;  // Silver Ship

            while (!network_is_sync())
            {
                service_SDL_events(false);
                JE_showVGA();

                network_check();
                SDL_Delay(16);
            }
        }
        else
#endif
        {
            do
            {
                defaultBrightness = -3;

                /* Animate instead of quickly fading in */
                if (redraw)
                {
                    play_song(SONG_TITLE);

                    menu = 0;
                    redraw = false;
                    if (animate)
                    {
                        if (fadeIn)
                        {
                            yield return Run(e_fade_black(10));
                            fadeIn = false;
                        }

                        JE_loadPic(VGAScreen, 4, false);

                        draw_font_hv_shadow(VGAScreen, 2, 192, opentyrian_version, small_font, left_aligned, 15, 0, false, 1);

                        System.Array.Copy(VGAScreen.pixels, VGAScreen2.pixels, VGAScreen2.pixels.Length);

                        temp = moveTyrianLogoUp ? 62 : 4;

                        blit_sprite(VGAScreenSeg, 11, temp, PLANET_SHAPES, 146); // tyrian logo

                        JE_showVGA();

                        yield return Run(e_fade_palette(colors, 10, 0, 255 - 16));

                        if (moveTyrianLogoUp)
                        {
                            for (temp = 61; temp >= 4; temp -= 2)
                            {
                                setjasondelay(2);

                                System.Array.Copy(VGAScreen2.pixels, VGAScreen.pixels, VGAScreen.pixels.Length);

                                blit_sprite(VGAScreenSeg, 11, temp, PLANET_SHAPES, 146); // tyrian logo

                                JE_showVGA();

                                yield return coroutine_service_wait_delay();
                            }
                            moveTyrianLogoUp = false;
                        }

                        menuText[4] = opentyrian_str;  // OpenTyrian override

                        /* Draw Menu Text on Screen */
                        for (int i = 0; i < menunum; ++i)
                        {
                            int x = VGAScreen.w / 2, y = 104 + i * 13;

                            draw_font_hv(VGAScreen, x - 1, y - 1, menuText[i], normal_font, centered, 15, -10);
                            draw_font_hv(VGAScreen, x + 1, y + 1, menuText[i], normal_font, centered, 15, -10);
                            draw_font_hv(VGAScreen, x + 1, y - 1, menuText[i], normal_font, centered, 15, -10);
                            draw_font_hv(VGAScreen, x - 1, y + 1, menuText[i], normal_font, centered, 15, -10);
                            draw_font_hv(VGAScreen, x, y, menuText[i], normal_font, centered, 15, -3);
                        }

                        JE_showVGA();

                        yield return Run(e_fade_palette(colors, 20, 255 - 16 + 1, 255)); // fade in menu items

                        System.Array.Copy(VGAScreen.pixels, VGAScreen2.pixels, VGAScreen2.pixels.Length);
                    }
                }

                System.Array.Copy(VGAScreen2.pixels, VGAScreen.pixels, VGAScreen.pixels.Length);

                // highlight selected menu item
                draw_font_hv(VGAScreen, VGAScreen.w / 2, 104 + menu * 13, menuText[menu], normal_font, centered, 15, -1);

                JE_showVGA();

                if (trentWin)
                {
                    quit = true;
                    goto trentWinsGame;
                }

                JE_word[] waitForDemo = { 2000 };
                yield return Run(e_JE_textMenuWait(waitForDemo, false));

                if (waitForDemo[0] == 1)
                    play_demo = true;

                if (newkey)
                {
                    switch (lastkey_sym)
                    {
                        case KeyCode.UpArrow:
                            if (menu == 0)
                                menu = menunum - 1;
                            else
                                menu--;
                            JE_playSampleNum(S_CURSOR);
                            break;
                        case KeyCode.DownArrow:
                            if (menu == menunum - 1)
                                menu = 0;
                            else
                                menu++;
                            JE_playSampleNum(S_CURSOR);
                            break;
                        default:
                            break;
                    }
                }

                for (int i = 0; i < SA_ENGAGE; i++)
                {
                    if (char.ToUpper(lastkey_char) == specialName[i][arcade_code_i[i]])
                        arcade_code_i[i]++;
                    else
                        arcade_code_i[i] = 0;

                    if (arcade_code_i[i] > 0 && arcade_code_i[i] == specialName[i].Length)
                    {
                        if (i + 1 == SA_DESTRUCT)
                        {
                            loadDestruct = true;
                        }
                        else if (i + 1 == SA_ENGAGE)
                        {
                            /* SuperTyrian */

                            JE_playSampleNum(V_DATA_CUBE);
                            yield return Run(e_JE_whoa());

                            initialDifficulty = keysactive[(int)KeyCode.ScrollLock] ? 6 : 8;

                            JE_clr256(VGAScreen);
                            JE_outText(VGAScreen, 10, 10, "Cheat codes have been disabled.", 15, 4);
                            if (initialDifficulty == 8)
                                JE_outText(VGAScreen, 10, 20, "Difficulty level has been set to Lord of Game.", 15, 4);
                            else
                                JE_outText(VGAScreen, 10, 20, "Difficulty level has been set to Suicide.", 15, 4);
                            JE_outText(VGAScreen, 10, 30, "It is imperative that you discover the special codes.", 15, 4);
                            if (initialDifficulty == 8)
                                JE_outText(VGAScreen, 10, 40, "(Next time, for an easier challenge hold down SCROLL LOCK.)", 15, 4);
                            JE_outText(VGAScreen, 10, 60, "Prepare to play...", 15, 4);

                            string buf = miscTextB[4] + " " + pName[0];
                            JE_dString(VGAScreen, JE_fontCenter(buf, FONT_SHAPES), 110, buf, FONT_SHAPES);

                            play_song(16);
                            JE_playSampleNum(V_DANGER);
                            JE_showVGA();

                            yield return coroutine_wait_noinput(true, true, true);
                            yield return coroutine_wait_input(true, true, true);

                            JE_initEpisode(1);
                            constantDie = false;
                            superTyrian = true;
                            onePlayerAction = true;
                            gameLoaded = true;
                            difficultyLevel = initialDifficulty;

                            player[0].cash = 0;

                            player[0].items.ship = 13;                     // The Stalker 21.126
                            player[0].items.weapon[FRONT_WEAPON].id = 39;  // Atomic RailGun
                        }
                        else
                        {
                            player[0].items.ship = SAShip[i];

                            yield return Run(e_fade_black(10));
                            bool[] refResult = { false };
                            yield return Run(e_select_episode(refResult));
                            if (refResult[0])
                                yield return Run(e_select_difficulty(refResult));
                            if (refResult[0])
                            {
                                /* Start special mode! */
                                yield return Run(e_fade_black(10));
                                JE_loadPic(VGAScreen, 1, false);
                                JE_clr256(VGAScreen);
                                JE_dString(VGAScreen, JE_fontCenter(superShips[0], FONT_SHAPES), 30, superShips[0], FONT_SHAPES);
                                JE_dString(VGAScreen, JE_fontCenter(superShips[i + 1], SMALL_FONT_SHAPES), 100, superShips[i + 1], SMALL_FONT_SHAPES);
                                tempW = ships[player[0].items.ship].shipgraphic;
                                if (tempW != 1)
                                    blit_sprite2x2(VGAScreen, 148, 70, shapes9, tempW);

                                JE_showVGA();
                                yield return Run(e_fade_palette(colors, 50, 0, 255));

                                yield return coroutine_wait_input(true, true, true);

                                twoPlayerMode = false;
                                onePlayerAction = true;
                                superArcadeMode = i + 1;
                                gameLoaded = true;
                                initialDifficulty = ++difficultyLevel;

                                player[0].cash = 0;

                                player[0].items.weapon[FRONT_WEAPON].id = SAWeapon[i][0];
                                player[0].items.special = SASpecialWeapon[i];
                                if (superArcadeMode == SA_NORTSHIPZ)
                                {
                                    for (int j = 0; j < player[0].items.sidekick.Length; ++j)
                                        player[0].items.sidekick[j] = 24;  // Companion Ship Quicksilver
                                }
                            }
                            else
                            {
                                redraw = true;
                                fadeIn = true;
                            }
                        }
                        newkey = false;
                    }
                }
                lastkey_char = '\0';

                if (newkey)
                {
                    switch (lastkey_sym)
                    {
                        case KeyCode.Escape:
                            quit = true;
                            break;
                        case KeyCode.Return:
                            JE_playSampleNum(S_SELECT);
                            switch (menu)
                            {
                                case 0: /* New game */
                                    yield return Run(e_fade_black(10));

                                    bool[] refResult = { true };
                                    yield return Run(e_select_gameplay(refResult));

                                    if (refResult[0])
                                    {
                                        yield return Run(e_select_episode(refResult));
                                        if (refResult[0])
                                            yield return Run(e_select_difficulty(refResult));
                                        if (refResult[0])
                                            gameLoaded = true;

                                        initialDifficulty = difficultyLevel;

                                        if (onePlayerAction)
                                        {
                                            player[0].cash = 0;

                                            player[0].items.ship = 8;  // Stalker
                                        }
                                        else if (twoPlayerMode)
                                        {
                                            for (uint i = 0; i < player.Length; ++i)
                                                player[i].cash = 0;

                                            player[0].items.ship = 11;  // Silver Ship

                                            difficultyLevel++;

                                            inputDevice[0] = 1;
                                            inputDevice[1] = 2;
                                        }
                                        else if (richMode)
                                        {
                                            player[0].cash = 1000000;
                                        }
                                        else if (gameLoaded)
                                        {
                                            // allows player to smuggle arcade/super-arcade ships into full game

                                            int[] initial_cash = { 10000, 15000, 20000, 30000, 35000 };

                                            player[0].cash = initial_cash[episodeNum - 1];
                                        }
                                    }
                                    fadeIn = true;
                                    break;
                                case 1: /* Load game */
                                    yield return Run(e_JE_loadScreen());
                                    fadeIn = true;
                                    break;
                                case 2: /* High scores */
                                    yield return Run(e_JE_highScoreScreen());
                                    fadeIn = true;
                                    break;
                                case 3: /* Instructions */
                                    yield return Run(e_JE_helpSystem(1));
                                    fadeIn = true;
                                    break;
                                case 4: /* Ordering info, now OpenTyrian menu */
                                    yield return Run(e_opentyrian_menu());
                                    fadeIn = true;
                                    break;
#if TYRIAN2000
                                case 5: /* Quit */
                                    quit = true;
                                    break;
#else
                                case 5: /* Demo */
                                    play_demo = true;
                                    break;
                                case 6: /* Quit */
                                    quit = true;
                                    break;
#endif
                            }
                            redraw = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            while (!(quit || gameLoaded || jumpSection || play_demo || loadDestruct));

        trentWinsGame:
            yield return Run(e_fade_black(15));
        }

        refQuit[0] = quit;
    }

    public static IEnumerator e_intro_logos()
    { UnityEngine.Debug.Log("e_intro_logos");
        JE_clr256(VGAScreen);

        yield return Run(e_fade_black(50));

        JE_loadPic(VGAScreen, 10, false);
        JE_showVGA();

        yield return Run(e_fade_palette(colors, 50, 0, 255));

        setjasondelay(200);

        yield return coroutine_wait_delayorinput(true, true, true);

        yield return Run(e_fade_black(10));

        JE_loadPic(VGAScreen, 12, false);
        JE_showVGA();

        yield return Run(e_fade_palette(colors, 10, 0, 255));

        setjasondelay(200);
        yield return coroutine_wait_delayorinput(true, true, true);

        yield return Run(e_fade_black(10));
    }

    private static void JE_readTextSync()
    {
        return;  // this function seems to be unnecessary
    }

    private static IEnumerator e_JE_displayText()
    { UnityEngine.Debug.Log("e_JE_displayText");
        /* Display Warning Text */
        tempY = 55;
        if (warningRed)
        {
            tempY = 2;
        }
        for (temp = 0; temp < levelWarningLines; temp++)
        {
            if (!ESCPressed)
            {
                yield return Run(e_JE_outCharGlow(10, tempY, levelWarningText[temp]));

                if (haltGame)
                {
                    JE_tyrianHalt(5);
                }

                tempY += 10;
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
        textGlowFont = TINY_FONT;
        tempW = 184;
        if (warningRed)
        {
            tempW = 7 * 16 + 6;
        }

        yield return Run(e_JE_outCharGlow(JE_fontCenter(miscText[4], TINY_FONT), tempW, miscText[4]));

        do
        {
            if (levelWarningDisplay)
            {
                JE_updateWarning(VGAScreen);
            }

            setjasondelay(1);

            //NETWORK_KEEP_ALIVE();

            yield return coroutine_wait_delay();

        } while (!(JE_anyButton() || (frameCountMax == 0 && temp == 1) || ESCPressed));
        levelWarningDisplay = false;
    }

    private static int JE_newEnemy(int enemyOffset, ushort eDatI, short uniqueShapeTableI)
    {
        for (int i = enemyOffset; i < enemyOffset + 25; ++i)
        {
            if (enemyAvail[i] == 1)
            {
                enemyAvail[i] = JE_makeEnemy(i, eDatI, uniqueShapeTableI);
                return i + 1;
            }
        }

        return 0;
    }

    private static byte JE_makeEnemy(int enemyIdx, ushort eDatI, short uniqueShapeTableI )
    {
        JE_SingleEnemyType enemy = VarzC.enemy[enemyIdx];
	    byte avail;

        JE_byte shapeTableI;

	    if (superArcadeMode != SA_NONE && eDatI == 534)
		    eDatI = 533;

	    enemyShapeTables[5 - 1] = 21;   /*Coins&Gems*/
	    enemyShapeTables[6 - 1] = 26;   /*Two-Player Stuff*/

	    if (uniqueShapeTableI > 0)
	    {
		    shapeTableI = (byte)uniqueShapeTableI;
	    }
	    else
	    {
		    shapeTableI = enemyDat[eDatI].shapebank;
	    }
	
	    byte[] sprite2s = null;
	    for (uint i = 0; i< 6; ++i)
		    if (shapeTableI == enemyShapeTables[i])
			    sprite2s = eShapes[i];
	
	    if (sprite2s != null)
		    enemy.sprite2s = sprite2s;
	    //else
		   // // maintain buggy Tyrian behavior (use shape table value from previous enemy that occupied this index in the enemy array)
		   // fprintf(stderr, "warning: ignoring sprite from unloaded shape table %d\n", shapeTableI);

        enemy.enemydatofs = enemyDat[eDatI];

	    enemy.mapoffset = 0;

	    for (uint i = 0; i< 3; ++i)
	    {
		    enemy.eshotmultipos[i] = 0;
	    }

	    enemy.enemyground = (enemyDat[eDatI].explosiontype & 1) == 0;
        enemy.explonum = (byte)(enemyDat[eDatI].explosiontype >> 1);

	    enemy.launchfreq = enemyDat[eDatI].elaunchfreq;
	    enemy.launchwait = enemyDat[eDatI].elaunchfreq;
	    enemy.launchtype = (ushort)(enemyDat[eDatI].elaunchtype % 1000);
	    enemy.launchspecial = (byte)(enemyDat[eDatI].elaunchtype / 1000);

	    enemy.xaccel = (byte)enemyDat[eDatI].xaccel;
	    enemy.yaccel = (byte)enemyDat[eDatI].yaccel;

	    enemy.xminbounce = -10000;
	    enemy.xmaxbounce = 10000;
	    enemy.yminbounce = -10000;
	    enemy.ymaxbounce = 10000;
	    /*Far enough away to be impossible to reach*/

	    for (uint i = 0; i< 3; ++i)
	    {
		    enemy.tur[i] = enemyDat[eDatI].tur[i];
	    }

	    enemy.ani = enemyDat[eDatI].ani;
	    enemy.animin = 1;

	    switch (enemyDat[eDatI].animate)
	    {
	    case 0:
		    enemy.enemycycle = 1;
		    enemy.aniactive = 0;
		    enemy.animax = 0;
		    enemy.aniwhenfire = 0;
		    break;
	    case 1:
		    enemy.enemycycle = 0;
		    enemy.aniactive = 1;
		    enemy.animax = 0;
		    enemy.aniwhenfire = 0;
		    break;
	    case 2:
		    enemy.enemycycle = 1;
		    enemy.aniactive = 2;
		    enemy.animax = enemy.ani;
		    enemy.aniwhenfire = 2;
		    break;
	    }

        if (enemyDat[eDatI].startxc != 0)
            enemy.ex = (short)(enemyDat[eDatI].startx + (mt_rand() % (enemyDat[eDatI].startxc * 2)) - enemyDat[eDatI].startxc + 1);
        else
            enemy.ex = (short)(enemyDat[eDatI].startx + 1);

        if (enemyDat[eDatI].startyc != 0)
            enemy.ey = (short)(enemyDat[eDatI].starty + (mt_rand() % (enemyDat[eDatI].startyc * 2)) - enemyDat[eDatI].startyc + 1);
        else
            enemy.ey = (short)(enemyDat[eDatI].starty + 1);

	    enemy.exc = enemyDat[eDatI].xmove;
	    enemy.eyc = enemyDat[eDatI].ymove;
	    enemy.excc = enemyDat[eDatI].xcaccel;
	    enemy.eycc = enemyDat[eDatI].ycaccel;
	    enemy.exccw = Abs(enemy.excc);
        enemy.exccwmax = (byte)enemy.exccw;
	    enemy.eyccw = Abs(enemy.eycc);
        enemy.eyccwmax = (byte)enemy.eyccw;
	    enemy.exccadd = (short)((enemy.excc > 0) ? 1 : -1);
	    enemy.eyccadd = (short)((enemy.eycc > 0) ? 1 : -1);
	    enemy.special = false;
	    enemy.iced = 0;

	    if (enemyDat[eDatI].xrev == 0)
		    enemy.exrev = 100;
	    else if (enemyDat[eDatI].xrev == -99)
		    enemy.exrev = 0;
	    else
		    enemy.exrev = enemyDat[eDatI].xrev;

	    if (enemyDat[eDatI].yrev == 0)
		    enemy.eyrev = 100;
	    else if (enemyDat[eDatI].yrev == -99)
		    enemy.eyrev = 0;
	    else
		    enemy.eyrev = enemyDat[eDatI].yrev;

	    enemy.exca = (sbyte)((enemy.xaccel > 0) ? 1 : -1);
	    enemy.eyca = (sbyte)((enemy.yaccel > 0) ? 1 : -1);

	    enemy.enemytype = eDatI;

	    for (uint i = 0; i< 3; ++i)
	    {
		    if (enemy.tur[i] == 252)
			    enemy.eshotwait[i] = 1;
		    else if (enemy.tur[i] > 0)
			    enemy.eshotwait[i] = 20;
		    else
			    enemy.eshotwait[i] = 255;
	    }
	    for (uint i = 0; i< 20; ++i)
		    enemy.egr[i] = enemyDat[eDatI].egraphic[i];
	    enemy.size = enemyDat[eDatI].esize;
	    enemy.linknum = 0;
	    enemy.edamaged = enemyDat[eDatI].dani< 0;
	    enemy.enemydie = enemyDat[eDatI].eenemydie;

	    enemy.freq[1 - 1] = enemyDat[eDatI].freq[1 - 1];
	    enemy.freq[2 - 1] = enemyDat[eDatI].freq[2 - 1];
	    enemy.freq[3 - 1] = enemyDat[eDatI].freq[3 - 1];

	    enemy.edani   = enemyDat[eDatI].dani;
	    enemy.edgr    = enemyDat[eDatI].dgr;
	    enemy.edlevel = enemyDat[eDatI].dlevel;

	    enemy.fixedmovey = 0;

	    enemy.filter = 0x00;

	    int tempValue = 0;
	    if (enemyDat[eDatI].value > 1 && enemyDat[eDatI].value< 10000)
	    {
		    switch (difficultyLevel)
		    {
		    case -1:
		    case 0:
                    tempValue = (int)(enemyDat[eDatI].value * 0.75f);
			    break;
		    case 1:
		    case 2:
			    tempValue = enemyDat[eDatI].value;
			    break;
		    case 3:
                    tempValue = (int)(enemyDat[eDatI].value * 1.125f);
			    break;
		    case 4:
                    tempValue = (int)(enemyDat[eDatI].value * 1.5f);
			    break;
		    case 5:
			    tempValue = enemyDat[eDatI].value* 2;
			    break;
		    case 6:
                    tempValue = (int)(enemyDat[eDatI].value * 2.5f);
			    break;
		    case 7:
		    case 8:
			    tempValue = enemyDat[eDatI].value* 4;
			    break;
		    case 9:
		    case 10:
			    tempValue = enemyDat[eDatI].value* 8;
			    break;
		    }
		    if (tempValue > 10000)
			    tempValue = 10000;
		    enemy.evalue = (short)tempValue;
	    }
	    else
	    {
		    enemy.evalue = enemyDat[eDatI].value;
	    }

	    int tempArmor = 1;
	    if (enemyDat[eDatI].armor > 0)
	    {
		    if (enemyDat[eDatI].armor != 255)
		    {
			    switch (difficultyLevel)
			    {
			    case -1:
			    case 0:
                        tempArmor = (int)(enemyDat[eDatI].armor * 0.5f + 1);
				    break;
			    case 1:
				    tempArmor = (int)(enemyDat[eDatI].armor* 0.75f + 1);
				    break;
			    case 2:
				    tempArmor = enemyDat[eDatI].armor;
				    break;
			    case 3:
				    tempArmor = (int)(enemyDat[eDatI].armor* 1.2f);
				    break;
			    case 4:
				    tempArmor = (int)(enemyDat[eDatI].armor* 1.5f);
				    break;
			    case 5:
				    tempArmor = (int)(enemyDat[eDatI].armor* 1.8f);
				    break;
			    case 6:
				    tempArmor = enemyDat[eDatI].armor* 2;
				    break;
			    case 7:
				    tempArmor = enemyDat[eDatI].armor* 3;
				    break;
			    case 8:
				    tempArmor = enemyDat[eDatI].armor* 4;
				    break;
			    case 9:
			    case 10:
				    tempArmor = enemyDat[eDatI].armor* 8;
				    break;
			    }

			    if (tempArmor > 254)
			    {
				    tempArmor = 254;
			    }
		    }
		    else
		    {
			    tempArmor = 255;
		    }

		    enemy.armorleft = (byte)tempArmor;

		    avail = 0;
		    enemy.scoreitem = false;
	    }
	    else
	    {
		    avail = 2;
		    enemy.armorleft = 255;
		    if (enemy.evalue != 0)
			    enemy.scoreitem = true;
	    }

	    if (!enemy.scoreitem)
	    {
		    totalEnemy++;  /*Destruction ratio*/
	    }

	    /* indicates what to set ENEMYAVAIL to */
	    return avail;
    }

    private static void JE_createNewEventEnemy(JE_byte enemyTypeOfs, JE_word enemyOffset, short uniqueShapeTableI)
    {
        int i;

        b = 0;

        for (i = enemyOffset; i < enemyOffset + 25; i++)
        {
            if (enemyAvail[i] == 1)
            {
                b = i + 1;
                break;
            }
        }

        if (b == 0)
        {
            return;
        }

        tempW = (JE_word)(eventRec[eventLoc - 1].eventdat + enemyTypeOfs);

        enemyAvail[b - 1] = JE_makeEnemy(b - 1, tempW, uniqueShapeTableI);

        if (eventRec[eventLoc - 1].eventdat2 != -99)
        {
            switch (enemyOffset)
            {
                case 0:
                    enemy[b - 1].ex = (JE_integer)(eventRec[eventLoc - 1].eventdat2 - (mapX - 1) * 24);
                    enemy[b - 1].ey -= (JE_integer)backMove2;
                    break;
                case 25:
                case 75:
                    enemy[b - 1].ex = (JE_integer)(eventRec[eventLoc - 1].eventdat2 - (mapX - 1) * 24 - 12);
                    enemy[b - 1].ey -= (JE_integer)(backMove);
                    break;
                case 50:
                    if (background3x1)
                    {
                        enemy[b - 1].ex = (JE_integer)(eventRec[eventLoc - 1].eventdat2 - (mapX - 1) * 24 - 12);
                    }
                    else
                    {
                        enemy[b - 1].ex = (JE_integer)(eventRec[eventLoc - 1].eventdat2 - mapX3 * 24 - 24 * 2 + 6);
                    }
                    enemy[b - 1].ey -= (JE_integer)(backMove3);

                    if (background3x1b)
                    {
                        enemy[b - 1].ex -= 6;
                    }
                    break;
            }
            enemy[b - 1].ey = -28;
            if (background3x1b && enemyOffset == 50)
            {
                enemy[b - 1].ey += 4;
            }
        }

        if (smallEnemyAdjust && enemy[b - 1].size == 0)
        {
            enemy[b - 1].ex -= 10;
            enemy[b - 1].ey -= 7;
        }

        enemy[b - 1].ey += eventRec[eventLoc - 1].eventdat5;
        enemy[b - 1].eyc += eventRec[eventLoc - 1].eventdat3;
        enemy[b - 1].linknum = eventRec[eventLoc - 1].eventdat4;
        enemy[b - 1].fixedmovey = eventRec[eventLoc - 1].eventdat6;
    }

    private static void JE_eventJump(int jump)
    {
        JE_word tempW;

        if (jump == 65535)
        {
            curLoc = returnLoc;
        }
        else
        {
            returnLoc = (ushort)(curLoc + 1);
            curLoc = (ushort)jump;
        }
        tempW = 0;
        do
        {
            tempW++;
        }
        while (!(eventRec[tempW - 1].eventtime >= curLoc));
        eventLoc = (ushort)(tempW - 1);
    }

    private static bool JE_searchFor/*enemy*/(int PLType, out JE_byte out_index)
    {
        int found_id = -1;

        for (int i = 0; i < 100; i++)
        {
            if (enemyAvail[i] == 0 && enemy[i].linknum == PLType)
            {
                found_id = i;
                if (galagaMode)
                {
                    enemy[i].evalue += enemy[i].evalue;
                }
            }
        }

        if (found_id != -1)
        {
            out_index = (byte)found_id;
            return true;
        }
        else
        {
            out_index = 0;
            return false;
        }
    }
    private static void JE_eventSystem()
    {
        switch (eventRec[eventLoc - 1].eventtype)
        {
            case 1:
                starfield_speed = eventRec[eventLoc - 1].eventdat;
                break;

            case 2:
                map1YDelay = 1;
                map1YDelayMax = 1;
                map2YDelay = 1;
                map2YDelayMax = 1;

                backMove = (JE_word)eventRec[eventLoc - 1].eventdat;
                backMove2 = (JE_word)eventRec[eventLoc - 1].eventdat2;

                if (backMove2 > 0)
                    explodeMove = backMove2;
                else
                    explodeMove = backMove;

                backMove3 = (JE_word)eventRec[eventLoc - 1].eventdat3;

                if (backMove > 0)
                    stopBackgroundNum = 0;
                break;

            case 3:
                backMove = 1;
                map1YDelay = 3;
                map1YDelayMax = 3;
                backMove2 = 1;
                map2YDelay = 2;
                map2YDelayMax = 2;
                backMove3 = 1;
                break;

            case 4:
                stopBackgrounds = true;
                switch (eventRec[eventLoc - 1].eventdat)
                {
                    case 0:
                    case 1:
                        stopBackgroundNum = 1;
                        break;
                    case 2:
                        stopBackgroundNum = 2;
                        break;
                    case 3:
                        stopBackgroundNum = 3;
                        break;
                }
                break;

            case 5:  // load enemy shape banks
                {
                    byte[] newEnemyShapeTables =
                    {
                        eventRec[eventLoc-1].eventdat > 0  ? (byte)eventRec[eventLoc-1].eventdat  : (byte)0,
                        eventRec[eventLoc-1].eventdat2 > 0 ? (byte)eventRec[eventLoc-1].eventdat2 : (byte)0,
                        eventRec[eventLoc-1].eventdat3 > 0 ? (byte)eventRec[eventLoc-1].eventdat3 : (byte)0,
                        eventRec[eventLoc-1].eventdat4 > 0 ? (byte)eventRec[eventLoc-1].eventdat4 : (byte)0,
                    };

                    for (int i = 0; i < newEnemyShapeTables.Length; ++i)
                    {
                        if (enemyShapeTables[i] != newEnemyShapeTables[i])
                        {
                            if (newEnemyShapeTables[i] > 0)
                            {
                                JE_loadCompShapes(out eShapes[i], shapeFile[newEnemyShapeTables[i] - 1]);
                            }
                            else
                                eShapes[i] = null;

                            enemyShapeTables[i] = newEnemyShapeTables[i];
                        }
                    }
                }
                break;

            case 6: /* Ground Enemy */
                JE_createNewEventEnemy(0, 25, 0);
                break;

            case 7: /* Top Enemy */
                JE_createNewEventEnemy(0, 50, 0);
                break;

            case 8:
                starActive = false;
                break;

            case 9:
                starActive = true;
                break;

            case 10: /* Ground Enemy 2 */
                JE_createNewEventEnemy(0, 75, 0);
                break;

            case 11:
                if (allPlayersGone || eventRec[eventLoc - 1].eventdat == 1)
                    reallyEndLevel = true;
                else
                    if (!endLevel)
                {
                    readyToEndLevel = false;
                    endLevel = true;
                    levelEnd = 40;
                }
                break;

            case 12: /* Custom 4x4 Ground Enemy */
                {
                    JE_word temp = 0;
                    switch (eventRec[eventLoc - 1].eventdat6)
                    {
                        case 0:
                        case 1:
                            temp = 25;
                            break;
                        case 2:
                            temp = 0;
                            break;
                        case 3:
                            temp = 50;
                            break;
                        case 4:
                            temp = 75;
                            break;
                    }
                    eventRec[eventLoc - 1].eventdat6 = 0;   /* We use EVENTDAT6 for the background */
                    JE_createNewEventEnemy(0, temp, 0);
                    JE_createNewEventEnemy(1, temp, 0);
                    enemy[b - 1].ex += 24;
                    JE_createNewEventEnemy(2, temp, 0);
                    enemy[b - 1].ey -= 28;
                    JE_createNewEventEnemy(3, temp, 0);
                    enemy[b - 1].ex += 24;
                    enemy[b - 1].ey -= 28;
                    break;
                }
            case 13:
                enemiesActive = false;
                break;

            case 14:
                enemiesActive = true;
                break;

            case 15: /* Sky Enemy */
                JE_createNewEventEnemy(0, 0, 0);
                break;

            case 16:
                if (eventRec[eventLoc - 1].eventdat > 9)
                {
                    //fprintf(stderr, "warning: event 16: bad event data\n");
                }
                else
                {
                    JE_drawTextWindow(outputs[eventRec[eventLoc - 1].eventdat - 1]);
                    soundQueue[3] = windowTextSamples[eventRec[eventLoc - 1].eventdat - 1];
                }
                break;

            case 17: /* Ground Bottom */
                JE_createNewEventEnemy(0, 25, 0);
                if (b > 0)
                {
                    enemy[b - 1].ey = (short)(190 + eventRec[eventLoc - 1].eventdat5);
                }
                break;

            case 18: /* Sky Enemy on Bottom */
                JE_createNewEventEnemy(0, 0, 0);
                if (b > 0)
                {
                    enemy[b - 1].ey = (short)(190 + eventRec[eventLoc - 1].eventdat5);
                }
                break;

            case 19: /* Enemy Global Move */
                {
                    int initial_i = 0, max_i = 0;
                    bool all_enemies = false;

                    if (eventRec[eventLoc - 1].eventdat3 > 79 && eventRec[eventLoc - 1].eventdat3 < 90)
                    {
                        initial_i = 0;
                        max_i = 100;
                        all_enemies = false;
                        eventRec[eventLoc - 1].eventdat4 = newPL[eventRec[eventLoc - 1].eventdat3 - 80];
                    }
                    else
                    {
                        switch (eventRec[eventLoc - 1].eventdat3)
                        {
                            case 0:
                                initial_i = 0;
                                max_i = 100;
                                all_enemies = false;
                                break;
                            case 2:
                                initial_i = 0;
                                max_i = 25;
                                all_enemies = true;
                                break;
                            case 1:
                                initial_i = 25;
                                max_i = 50;
                                all_enemies = true;
                                break;
                            case 3:
                                initial_i = 50;
                                max_i = 75;
                                all_enemies = true;
                                break;
                            case 99:
                                initial_i = 0;
                                max_i = 100;
                                all_enemies = true;
                                break;
                        }
                    }

                    for (int i = initial_i; i < max_i; i++)
                    {
                        if (all_enemies || enemy[i].linknum == eventRec[eventLoc - 1].eventdat4)
                        {
                            if (eventRec[eventLoc - 1].eventdat != -99)
                                enemy[i].exc = (sbyte)(eventRec[eventLoc - 1].eventdat);

                            if (eventRec[eventLoc - 1].eventdat2 != -99)
                                enemy[i].eyc = (sbyte)(eventRec[eventLoc - 1].eventdat2);

                            if (eventRec[eventLoc - 1].eventdat6 != 0)
                                enemy[i].fixedmovey = eventRec[eventLoc - 1].eventdat6;

                            if (eventRec[eventLoc - 1].eventdat6 == -99)
                                enemy[i].fixedmovey = 0;

                            if (eventRec[eventLoc - 1].eventdat5 > 0)
                                enemy[i].enemycycle = (byte)(eventRec[eventLoc - 1].eventdat5);
                        }
                    }
                    break;
                }

            case 20: /* Enemy Global Accel */
                if (eventRec[eventLoc - 1].eventdat3 > 79 && eventRec[eventLoc - 1].eventdat3 < 90)
                    eventRec[eventLoc - 1].eventdat4 = newPL[eventRec[eventLoc - 1].eventdat3 - 80];

                for (temp = 0; temp < 100; temp++)
                {
                    if (enemyAvail[temp] != 1
                        && (enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4 || eventRec[eventLoc - 1].eventdat4 == 0))
                    {
                        if (eventRec[eventLoc - 1].eventdat != -99)
                        {
                            enemy[temp].excc = (sbyte)eventRec[eventLoc - 1].eventdat;
                            enemy[temp].exccw = (sbyte)Abs(eventRec[eventLoc - 1].eventdat);
                            enemy[temp].exccwmax = (byte)Abs(eventRec[eventLoc - 1].eventdat);
                            if (eventRec[eventLoc - 1].eventdat > 0)
                                enemy[temp].exccadd = 1;
                            else
                                enemy[temp].exccadd = -1;
                        }

                        if (eventRec[eventLoc - 1].eventdat2 != -99)
                        {
                            enemy[temp].eycc = (sbyte)eventRec[eventLoc - 1].eventdat2;
                            enemy[temp].eyccw = (sbyte)Abs(eventRec[eventLoc - 1].eventdat2);
                            enemy[temp].eyccwmax = (byte)Abs(eventRec[eventLoc - 1].eventdat2);
                            if (eventRec[eventLoc - 1].eventdat2 > 0)
                                enemy[temp].eyccadd = 1;
                            else
                                enemy[temp].eyccadd = -1;
                        }

                        if (eventRec[eventLoc - 1].eventdat5 > 0)
                        {
                            enemy[temp].enemycycle = (byte)eventRec[eventLoc - 1].eventdat5;
                        }
                        if (eventRec[eventLoc - 1].eventdat6 > 0)
                        {
                            enemy[temp].ani = (byte)eventRec[eventLoc - 1].eventdat6;
                            enemy[temp].animin = (byte)eventRec[eventLoc - 1].eventdat5;
                            enemy[temp].animax = 0;
                            enemy[temp].aniactive = 1;
                        }
                    }
                }
                break;

            case 21:
                background3over = 1;
                break;

            case 22:
                background3over = 0;
                break;

            case 23: /* Sky Enemy on Bottom */
                JE_createNewEventEnemy(0, 50, 0);
                if (b > 0)
                    enemy[b - 1].ey = (short)(180 + eventRec[eventLoc - 1].eventdat5);
                break;

            case 24: /* Enemy Global Animate */
                for (temp = 0; temp < 100; temp++)
                {
                    if (enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                    {
                        enemy[temp].aniactive = 1;
                        enemy[temp].aniwhenfire = 0;
                        if (eventRec[eventLoc - 1].eventdat2 > 0)
                        {
                            enemy[temp].enemycycle = (byte)eventRec[eventLoc - 1].eventdat2;
                            enemy[temp].animin = enemy[temp].enemycycle;
                        }
                        else
                        {
                            enemy[temp].enemycycle = 0;
                        }

                        if (eventRec[eventLoc - 1].eventdat > 0)
                            enemy[temp].ani = (byte)eventRec[eventLoc - 1].eventdat;

                        if (eventRec[eventLoc - 1].eventdat3 == 1)
                        {
                            enemy[temp].animax = enemy[temp].ani;
                        }
                        else if (eventRec[eventLoc - 1].eventdat3 == 2)
                        {
                            enemy[temp].aniactive = 2;
                            enemy[temp].animax = enemy[temp].ani;
                            enemy[temp].aniwhenfire = 2;
                        }
                    }
                }
                break;

            case 25: /* Enemy Global Damage change */
                for (temp = 0; temp < 100; temp++)
                {
                    if (eventRec[eventLoc - 1].eventdat4 == 0 || enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                    {
                        if (galagaMode)
                            enemy[temp].armorleft = (byte)Round(eventRec[eventLoc - 1].eventdat * (difficultyLevel / 2.0));
                        else
                            enemy[temp].armorleft = (byte)eventRec[eventLoc - 1].eventdat;
                    }
                }
                break;

            case 26:
                smallEnemyAdjust = eventRec[eventLoc - 1].eventdat != 0;
                break;

            case 27: /* Enemy Global AccelRev */
                if (eventRec[eventLoc - 1].eventdat3 > 79 && eventRec[eventLoc - 1].eventdat3 < 90)
                    eventRec[eventLoc - 1].eventdat4 = newPL[eventRec[eventLoc - 1].eventdat3 - 80];

                for (temp = 0; temp < 100; temp++)
                {
                    if (eventRec[eventLoc - 1].eventdat4 == 0 || enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                    {
                        if (eventRec[eventLoc - 1].eventdat != -99)
                            enemy[temp].exrev = (sbyte)eventRec[eventLoc - 1].eventdat;
                        if (eventRec[eventLoc - 1].eventdat2 != -99)
                            enemy[temp].eyrev = (sbyte)eventRec[eventLoc - 1].eventdat2;
                        if (eventRec[eventLoc - 1].eventdat3 != 0 && eventRec[eventLoc - 1].eventdat3 < 17)
                            enemy[temp].filter = (byte)eventRec[eventLoc - 1].eventdat3;
                    }
                }
                break;

            case 28:
                topEnemyOver = false;
                break;

            case 29:
                topEnemyOver = true;
                break;

            case 30:
                map1YDelay = 1;
                map1YDelayMax = 1;
                map2YDelay = 1;
                map2YDelayMax = 1;

                backMove = (ushort)eventRec[eventLoc - 1].eventdat;
                backMove2 = (ushort)eventRec[eventLoc - 1].eventdat2;
                explodeMove = backMove2;
                backMove3 = (ushort)eventRec[eventLoc - 1].eventdat3;
                break;

            case 31: /* Enemy Fire Override */
                for (temp = 0; temp < 100; temp++)
                {
                    if (eventRec[eventLoc - 1].eventdat4 == 99 || enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                    {
                        enemy[temp].freq[1 - 1] = (byte)eventRec[eventLoc - 1].eventdat;
                        enemy[temp].freq[2 - 1] = (byte)eventRec[eventLoc - 1].eventdat2;
                        enemy[temp].freq[3 - 1] = (byte)eventRec[eventLoc - 1].eventdat3;
                        for (temp2 = 0; temp2 < 3; temp2++)
                        {
                            enemy[temp].eshotwait[temp2] = 1;
                        }
                        if (enemy[temp].launchtype > 0)
                        {
                            enemy[temp].launchfreq = (byte)eventRec[eventLoc - 1].eventdat5;
                            enemy[temp].launchwait = 1;
                        }
                    }
                }
                break;

            case 32:  // create enemy
                JE_createNewEventEnemy(0, 50, 0);
                if (b > 0)
                    enemy[b - 1].ey = 190;
                break;

            case 33: /* Enemy From other Enemies */
                if (!((eventRec[eventLoc - 1].eventdat == 512 || eventRec[eventLoc - 1].eventdat == 513) && (twoPlayerMode || onePlayerAction || superTyrian)))
                {
                    if (superArcadeMode != SA_NONE)
                    {
                        if (eventRec[eventLoc - 1].eventdat == 534)
                            eventRec[eventLoc - 1].eventdat = 827;
                    }
                    else if (!superTyrian)
                    {
                        int lives = player[0].lives;

                        if (eventRec[eventLoc - 1].eventdat == 533 && (lives == 11 || (mt_rand() % 15) < lives))
                        {
                            // enemy will drop random special weapon
                            eventRec[eventLoc - 1].eventdat = (short)(829 + (mt_rand() % 6));
                        }
                    }
                    if (eventRec[eventLoc - 1].eventdat == 534 && superTyrian)
                        eventRec[eventLoc - 1].eventdat = (short)(828 + superTyrianSpecials[mt_rand() % 4]);

                    for (temp = 0; temp < 100; temp++)
                    {
                        if (enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                            enemy[temp].enemydie = (ushort)(eventRec[eventLoc - 1].eventdat);
                    }
                }
                break;

            case 34: /* Start Music Fade */
                if (firstGameOver)
                {
                    musicFade = true;
                    tempVolume = tyrMusicVolume;
                }
                break;

            case 35: /* Play new song */
                if (firstGameOver)
                {
                    play_song(eventRec[eventLoc - 1].eventdat - 1);
                    set_volume(tyrMusicVolume, fxVolume);
                }
                musicFade = false;
                break;

            case 36:
                readyToEndLevel = true;
                break;

            case 37:
                levelEnemyFrequency = (ushort)eventRec[eventLoc - 1].eventdat;
                break;

            case 38:
                curLoc = (ushort)eventRec[eventLoc - 1].eventdat;
                int new_event_loc = 1;
                for (tempW = 0; tempW < maxEvent; tempW++)
                {
                    if (eventRec[tempW].eventtime <= curLoc)
                    {
                        new_event_loc = tempW + 1 - 1;
                    }
                }
                eventLoc = (ushort)new_event_loc;
                break;

            case 39: /* Enemy Global Linknum Change */
                for (temp = 0; temp < 100; temp++)
                {
                    if (enemy[temp].linknum == eventRec[eventLoc - 1].eventdat)
                        enemy[temp].linknum = (byte)eventRec[eventLoc - 1].eventdat2;
                }
                break;

            case 40: /* Enemy Continual Damage */
                enemyContinualDamage = true;
                break;

            case 41:
                if (eventRec[eventLoc - 1].eventdat == 0)
                {
                    FillByteArrayWithOnes(enemyAvail);
                }
                else
                {
                    for (x = 0; x <= 24; x++)
                        enemyAvail[x] = 1;
                }
                break;

            case 42:
                background3over = 2;
                break;

            case 43:
                background2over = (byte)eventRec[eventLoc - 1].eventdat;
                break;

            case 44:
                filterActive = (eventRec[eventLoc - 1].eventdat > 0);
                filterFade = (eventRec[eventLoc - 1].eventdat == 2);
                levelFilter = eventRec[eventLoc - 1].eventdat2;
                levelBrightness = eventRec[eventLoc - 1].eventdat3;
                levelFilterNew = eventRec[eventLoc - 1].eventdat4;
                levelBrightnessChg = eventRec[eventLoc - 1].eventdat5;
                filterFadeStart = (eventRec[eventLoc - 1].eventdat6 == 0);
                break;

            case 45: /* arcade-only enemy from other enemies */
                if (!superTyrian)
                {
                    int lives = player[0].lives;

                    if (eventRec[eventLoc - 1].eventdat == 533 && (lives == 11 || (mt_rand() % 15) < lives))
                    {
                        eventRec[eventLoc - 1].eventdat = (short)(829 + (mt_rand() % 6));
                    }
                    if (twoPlayerMode || onePlayerAction)
                    {
                        for (temp = 0; temp < 100; temp++)
                        {
                            if (enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                                enemy[temp].enemydie = (ushort)eventRec[eventLoc - 1].eventdat;
                        }
                    }
                }
                break;

            case 46:  // change difficulty
                if (eventRec[eventLoc - 1].eventdat3 != 0)
                    damageRate = (byte)eventRec[eventLoc - 1].eventdat3;

                if (eventRec[eventLoc - 1].eventdat2 == 0 || twoPlayerMode || onePlayerAction)
                {
                    difficultyLevel += eventRec[eventLoc - 1].eventdat;
                    if (difficultyLevel < 1)
                        difficultyLevel = 1;
                    if (difficultyLevel > 10)
                        difficultyLevel = 10;
                }
                break;

            case 47: /* Enemy Global AccelRev */
                for (temp = 0; temp < 100; temp++)
                {
                    if (eventRec[eventLoc - 1].eventdat4 == 0 || enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                        enemy[temp].armorleft = (byte)eventRec[eventLoc - 1].eventdat;
                }
                break;

            case 48: /* Background 2 Cannot be Transparent */
                background2notTransparent = true;
                break;

            case 49:
            case 50:
            case 51:
            case 52:
                tempDat2 = eventRec[eventLoc - 1].eventdat;
                eventRec[eventLoc - 1].eventdat = 0;
                tempDat = eventRec[eventLoc - 1].eventdat3;
                eventRec[eventLoc - 1].eventdat3 = 0;
                tempDat3 = eventRec[eventLoc - 1].eventdat6;
                eventRec[eventLoc - 1].eventdat6 = 0;
                enemyDat[0].armor = (byte)tempDat3;
                enemyDat[0].egraphic[1 - 1] = (ushort)tempDat2;
                switch (eventRec[eventLoc - 1].eventtype - 48)
                {
                    case 1:
                        temp = 25;
                        break;
                    case 2:
                        temp = 0;
                        break;
                    case 3:
                        temp = 50;
                        break;
                    case 4:
                        temp = 75;
                        break;
                }
                JE_createNewEventEnemy(0, (ushort)temp, tempDat);
                eventRec[eventLoc - 1].eventdat = tempDat2;
                eventRec[eventLoc - 1].eventdat3 = (sbyte)tempDat;
                eventRec[eventLoc - 1].eventdat6 = (sbyte)tempDat3;
                break;

            case 53:
                forceEvents = (eventRec[eventLoc - 1].eventdat != 99);
                break;

            case 54:
                JE_eventJump(eventRec[eventLoc - 1].eventdat);
                break;

            case 55: /* Enemy Global AccelRev */
                if (eventRec[eventLoc - 1].eventdat3 > 79 && eventRec[eventLoc - 1].eventdat3 < 90)
                    eventRec[eventLoc - 1].eventdat4 = newPL[eventRec[eventLoc - 1].eventdat3 - 80];

                for (temp = 0; temp < 100; temp++)
                {
                    if (eventRec[eventLoc - 1].eventdat4 == 0 || enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                    {
                        if (eventRec[eventLoc - 1].eventdat != -99)
                            enemy[temp].xaccel = (byte)eventRec[eventLoc - 1].eventdat;
                        if (eventRec[eventLoc - 1].eventdat2 != -99)
                            enemy[temp].yaccel = (byte)eventRec[eventLoc - 1].eventdat2;
                    }
                }
                break;

            case 56: /* Ground2 Bottom */
                JE_createNewEventEnemy(0, 75, 0);
                if (b > 0)
                    enemy[b - 1].ey = 190;
                break;

            case 57:
                superEnemy254Jump = (ushort)eventRec[eventLoc - 1].eventdat;
                break;

            case 60: /*Assign Special Enemy*/
                for (temp = 0; temp < 100; temp++)
                {
                    if (enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                    {
                        enemy[temp].special = true;
                        enemy[temp].flagnum = (byte)eventRec[eventLoc - 1].eventdat;
                        enemy[temp].setto = (eventRec[eventLoc - 1].eventdat2 == 1);
                    }
                }
                break;

            case 61:  // if specific flag set to specific value, skip events
                if (globalFlags[eventRec[eventLoc - 1].eventdat - 1] == (eventRec[eventLoc - 1].eventdat2 != 0))
                    eventLoc += (ushort)eventRec[eventLoc - 1].eventdat3;
                break;

            case 62: /*Play sound effect*/
                soundQueue[3] = (byte)eventRec[eventLoc - 1].eventdat;
                break;

            case 63:  // skip events if not in 2-player mode
                if (!twoPlayerMode && !onePlayerAction)
                    eventLoc += (ushort)eventRec[eventLoc - 1].eventdat;
                break;

            case 64:
                if (!(eventRec[eventLoc - 1].eventdat == 6 && twoPlayerMode && difficultyLevel > 2))
                {
                    smoothies[eventRec[eventLoc - 1].eventdat - 1] = eventRec[eventLoc - 1].eventdat2 != 0;
                    temp = eventRec[eventLoc - 1].eventdat;
                    if (temp == 5)
                        temp = 3;
                    smoothie_data[temp - 1] = (byte)eventRec[eventLoc - 1].eventdat3;
                }
                break;

            case 65:
                background3x1 = (eventRec[eventLoc - 1].eventdat == 0);
                break;

            case 66: /*If not on this difficulty level or higher then...*/
                if (initialDifficulty <= eventRec[eventLoc - 1].eventdat)
                    eventLoc += (ushort)eventRec[eventLoc - 1].eventdat2;
                break;

            case 67:
                levelTimer = (eventRec[eventLoc - 1].eventdat == 1);
                levelTimerCountdown = (ushort)(eventRec[eventLoc - 1].eventdat3 * 100);
                levelTimerJumpTo = (ushort)eventRec[eventLoc - 1].eventdat2;
                break;

            case 68:
                randomExplosions = (eventRec[eventLoc - 1].eventdat == 1);
                break;

            case 69:
                for (uint i = 0; i < player.Length; ++i)
                    player[i].invulnerable_ticks = eventRec[eventLoc - 1].eventdat;
                break;

            case 70:
                byte ignore;
                if (eventRec[eventLoc - 1].eventdat2 == 0)
                {  /*1-10*/
                    bool found = false;

                    for (temp = 1; temp <= 19; temp++)
                        found = found || JE_searchFor(temp, out ignore);

                    if (!found)
                        JE_eventJump(eventRec[eventLoc - 1].eventdat);
                }
                else if (!JE_searchFor(eventRec[eventLoc - 1].eventdat2, out ignore)
                         && (eventRec[eventLoc - 1].eventdat3 == 0 || !JE_searchFor(eventRec[eventLoc - 1].eventdat3, out ignore))
                         && (eventRec[eventLoc - 1].eventdat4 == 0 || !JE_searchFor(eventRec[eventLoc - 1].eventdat4, out ignore)))
                {
                    JE_eventJump(eventRec[eventLoc - 1].eventdat);
                }
                break;

            case 71:
                if (checkCase71())
                {
                    JE_eventJump(eventRec[eventLoc - 1].eventdat);
                }
                break;

            case 72:
                background3x1b = (eventRec[eventLoc - 1].eventdat == 1);
                break;

            case 73:
                skyEnemyOverAll = (eventRec[eventLoc - 1].eventdat == 1);
                break;

            case 74: /* Enemy Global BounceParams */
                for (temp = 0; temp < 100; temp++)
                {
                    if (eventRec[eventLoc - 1].eventdat4 == 0 || enemy[temp].linknum == eventRec[eventLoc - 1].eventdat4)
                    {
                        if (eventRec[eventLoc - 1].eventdat5 != -99)
                            enemy[temp].xminbounce = eventRec[eventLoc - 1].eventdat5;

                        if (eventRec[eventLoc - 1].eventdat6 != -99)
                            enemy[temp].yminbounce = eventRec[eventLoc - 1].eventdat6;

                        if (eventRec[eventLoc - 1].eventdat != -99)
                            enemy[temp].xmaxbounce = eventRec[eventLoc - 1].eventdat;

                        if (eventRec[eventLoc - 1].eventdat2 != -99)
                            enemy[temp].ymaxbounce = eventRec[eventLoc - 1].eventdat2;
                    }
                }
                break;

            case 75:
                ;
                bool temp_no_clue = false; // TODO: figure out what this is doing

                for (temp = 0; temp < 100; temp++)
                {
                    if (enemyAvail[temp] == 0
                        && enemy[temp].eyc == 0
                        && enemy[temp].linknum >= eventRec[eventLoc - 1].eventdat
                        && enemy[temp].linknum <= eventRec[eventLoc - 1].eventdat2)
                    {
                        temp_no_clue = true;
                    }
                }

                if (temp_no_clue)
                {
                    JE_byte enemy_i;
                    do
                    {
                        temp = (int)((mt_rand() % (eventRec[eventLoc - 1].eventdat2 + 1 - eventRec[eventLoc - 1].eventdat)) + eventRec[eventLoc - 1].eventdat);
                    }
                    while (!(JE_searchFor(temp, out enemy_i) && enemy[enemy_i].eyc == 0));

                    newPL[eventRec[eventLoc - 1].eventdat3 - 80] = (byte)temp;
                }
                else
                {
                    newPL[eventRec[eventLoc - 1].eventdat3 - 80] = 255;
                    if (eventRec[eventLoc - 1].eventdat4 > 0)
                    { /*Skip*/
                        curLoc = (ushort)(eventRec[eventLoc - 1 + eventRec[eventLoc - 1].eventdat4].eventtime - 1);
                        eventLoc += (ushort)(eventRec[eventLoc - 1].eventdat4 - 1);
                    }
                }

                break;

            case 76:
                returnActive = true;
                break;

            case 77:
                applyCase77();
                break;

            case 78:
                if (galagaShotFreq < 10)
                    galagaShotFreq++;
                break;

            case 79:
                boss_bar[0].link_num = eventRec[eventLoc - 1].eventdat;
                boss_bar[1].link_num = eventRec[eventLoc - 1].eventdat2;
                break;

            case 80:  // skip events if in 2-player mode
                if (twoPlayerMode)
                    eventLoc += (ushort)eventRec[eventLoc - 1].eventdat;
                break;

            case 81: /*WRAP2*/
                applyCase81();
                break;

            case 82: /*Give SPECIAL WEAPON*/
                player[0].items.special = (ushort)eventRec[eventLoc - 1].eventdat;
                shotMultiPos[SHOT_SPECIAL] = 0;
                shotRepeat[SHOT_SPECIAL] = 0;
                shotMultiPos[SHOT_SPECIAL2] = 0;
                shotRepeat[SHOT_SPECIAL2] = 0;
                break;

            default:
                //fprintf(stderr, "warning: ignoring unknown event %d\n", eventRec[eventLoc - 1].eventtype);
                break;
        }

        eventLoc++;
    }

    private static bool checkCase71()
    {
        Debug.LogError("Figure this out");
        //return ((mapYPos - &megaData1.mainmap) / sizeof(JE_byte*) * 2) <= (uint)eventRec[eventLoc - 1].eventdat2;
        return false;
    }

    private static void applyCase77()
    {
        mapYPos = eventRec[eventLoc - 1].eventdat / 2;
        if (eventRec[eventLoc - 1].eventdat2 > 0)
        {
            mapY2Pos = eventRec[eventLoc - 1].eventdat2 / 2;
        }
        else
        {
            mapY2Pos = eventRec[eventLoc - 1].eventdat / 2;
        }
    }

    private static void applyCase81()
    {
        BKwrap2   = eventRec[eventLoc - 1].eventdat  / 2;
        BKwrap2to = eventRec[eventLoc - 1].eventdat2 / 2;
    }

    public static IEnumerator e_JE_whoa()
    { UnityEngine.Debug.Log("e_JE_whoa");
        int i, j, offset, timer;
        byte color;
        int screenSize, topBorder, bottomBorder;
        byte[] TempScreen1, TempScreen2, TempScreenSwap;

        /* 'whoa' gets us that nifty screen fade used when you type in
         * 'engage'.  We need two temporary screen buffers (char arrays can
         * work too, but these screens already exist) for our effect.
         * This could probably be a lot more efficient (there's probably a
         * way to get vgascreen as one of the temp buffers), but it's only called
         * once so don't worry about it. */

        TempScreen1 = game_screen.pixels;
        TempScreen2 = VGAScreen2.pixels;

        screenSize = VGAScreenSeg.pixels.Length;
        topBorder = VGAScreenSeg.w * 4; /* Seems an arbitrary number of lines */
        bottomBorder = VGAScreenSeg.w * 7;


        /* Clear the top and bottom borders.  We don't want to process
         * them and we don't want to draw them. */
        for (i = 0; i < topBorder; ++i)
        {
            VGAScreenSeg.pixels[i] = 0;
        }
        for (i = 0; i < bottomBorder; ++i)
        {
            VGAScreenSeg.pixels[screenSize - bottomBorder + i] = 0;
        }

        /* Copy our test subject to one of the temporary buffers.  Blank the other */
        JE_clr256Pixels(TempScreen1);
        System.Array.Copy(VGAScreenSeg.pixels, TempScreen2, VGAScreenSeg.pixels.Length);

        service_SDL_events(true);
        timer = 300; /* About 300 rounds is enough to make the screen mostly black */

        do
        {
            setjasondelay(1);

            /* This gets us our 'whoa' effect with pixel bleeding magic.
             * I'm willing to bet the guy who originally wrote the asm was goofing
             * around on acid and thought this looked good enough to use. */
            for (i = screenSize - bottomBorder, j = topBorder / 2; i > 0; i--, j++)
            {
                offset = j + i / 8192 - 4;
                color = (byte)((TempScreen2[offset] * 12 +
                         TempScreen1[offset - VGAScreenSeg.w] +
                         TempScreen1[offset - 1] +
                         TempScreen1[offset + 1] +
                         TempScreen1[offset + VGAScreenSeg.w]) / 16);

                TempScreen1[j] = color;
            }

            /* Now copy that mess to the buffer. */
            System.Array.Copy(TempScreen1, topBorder, VGAScreenSeg.pixels, topBorder, screenSize - bottomBorder);

            JE_showVGA();

            timer--;
            yield return coroutine_wait_delay();

            /* Flip the buffer. */
            TempScreenSwap = TempScreen1;
            TempScreen1 = TempScreen2;
            TempScreen2 = TempScreenSwap;

        } while (!(timer == 0 || JE_anyButton()));

        levelWarningLines = 4;
    }

    private static void JE_barX(int x1, int y1, int x2, int y2, JE_byte col)
    {
        fill_rectangle_xy(VGAScreen, x1, y1, x2, y1, (byte)(col + 1));
        fill_rectangle_xy(VGAScreen, x1, y1 + 1, x2, y2 - 1, col);
        fill_rectangle_xy(VGAScreen, x1, y2, x2, y2, (byte)(col - 1));
    }

    public static void draw_boss_bar()
    {
        for (int b = 0; b < boss_bar.Length; b++)
        {
            if (boss_bar[b].link_num == 0)
                continue;

            int armor = 256;  // higher than armor max

            for (int e = 0; e < enemy.Length; e++)  // find most damaged
            {
                if (enemyAvail[e] != 1 && enemy[e].linknum == boss_bar[b].link_num)
                    if (enemy[e].armorleft < armor)
                        armor = enemy[e].armorleft;
            }

            if (armor > 255 || armor == 0)  // boss dead?
                boss_bar[b].link_num = 0;
            else
                boss_bar[b].armor = (armor == 255) ? 254 : armor;  // 255 would make the bar too long
        }

        int bars = (boss_bar[0].link_num != 0 ? 1 : 0)
                          + (boss_bar[1].link_num != 0 ? 1 : 0);

        // if only one bar left, make it the first one
        if (bars == 1 && boss_bar[0].link_num == 0)
        {
            boss_bar[0] = boss_bar[1];
            boss_bar[1].link_num = 0;
        }

        for (int b = 0; b < bars; b++)
        {
            int x = (bars == 2)
                           ? ((b == 0) ? 125 : 185)
                           : ((levelTimer) ? 250 : 155);  // level timer and boss bar would overlap

            JE_barX(x - 25, 7, x + 25, 12, 115);
            JE_barX(x - (boss_bar[b].armor / 10), 7, x + (boss_bar[b].armor + 5) / 10, 12, (byte)(118 + boss_bar[b].color));

            if (boss_bar[b].color > 0)
                boss_bar[b].color--;
        }
    }
}