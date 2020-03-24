using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;
using System.IO;
using UnityEngine;

using static PlayerC;
using static VideoC;
using static ConfigC;
using static SpriteC;
using static EpisodesC;
using static EditShipC;
using static VGA256dC;
using static SndMastC;
using static ShotsC;
using static MouseC;
using static NortVarsC;
using static NortsongC;
using static LibC;
using static MainIntC;
using static BackgrndC;

using static System.Math;
using static SurfaceC;

using System.Linq;

public static class VarzC
{
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
    public const int SA = 7;

    public static JE_byte SA_NONE = 0,
        SA_NORTSHIPZ = 7,

        // only used for code entry
        SA_DESTRUCT = 8,
        SA_ENGAGE = 9,

        // only used in pItems[P_SUPERARCADE]
        SA_SUPERTYRIAN = 254,
        SA_ARCADE = 255
    ;

    public const int ENEMY_SHOT_MAX = 60;

    public const int CURRENT_KEY_SPEED = 1;  /*Keyboard/Joystick movement rate*/

    public const int MAX_EXPLOSIONS = 200;
    public const int MAX_REPEATING_EXPLOSIONS = 20;
    public const int MAX_SUPERPIXELS = 101;

    public class JE_SingleEnemyType
    {
        public JE_byte fillbyte;
        public JE_integer ex, ey;     /* POSITION */
        public JE_shortint exc, eyc;   /* CURRENT SPEED */
        public JE_shortint exca, eyca; /* RANDOM ACCELERATION */
        public JE_shortint excc, eycc; /* FIXED ACCELERATION WAITTIME */
        public JE_shortint exccw, eyccw;
        public JE_byte armorleft;
        public JE_byte[] eshotwait = new JE_byte[3], eshotmultipos = new JE_byte[3]; /* [1..3] */
        public JE_byte enemycycle;
        public JE_byte ani;
        public JE_word[] egr = new JE_word[20]; /* [1..20] */
        public JE_byte size;
        public JE_byte linknum;
        public JE_byte aniactive;
        public JE_byte animax;
        public JE_byte aniwhenfire;
        public byte[] sprite2s;
        public JE_shortint exrev, eyrev;
        public JE_integer exccadd, eyccadd;
        public JE_byte exccwmax, eyccwmax;
        public JE_EnemyDatType enemydatofs;
        public JE_boolean edamaged;
        public JE_word enemytype;
        public JE_byte animin;
        public JE_word edgr;
        public JE_shortint edlevel;
        public JE_shortint edani;
        public JE_byte fill1;
        public JE_byte filter;
        public JE_integer evalue;
        public JE_integer fixedmovey;
        public JE_byte[] freq = new JE_byte[3]; /* [1..3] */
        public JE_byte launchwait;
        public JE_word launchtype;
        public JE_byte launchfreq;
        public JE_byte xaccel;
        public JE_byte yaccel;
        public JE_byte[] tur = new JE_byte[3]; /* [1..3] */
        public JE_word enemydie; /* Enemy created when this one dies */
        public JE_boolean enemyground;
        public JE_byte explonum;
        public JE_word mapoffset;
        public JE_boolean scoreitem;

        public JE_boolean special;
        public JE_byte flagnum;
        public JE_boolean setto;

        public JE_byte iced; /*Duration*/

        public JE_byte launchspecial;

        public JE_integer xminbounce;
        public JE_integer xmaxbounce;
        public JE_integer yminbounce;
        public JE_integer ymaxbounce;
        public JE_byte[] fill = new JE_byte[3]; /* [1..3] */
    };

    //typedef struct JE_SingleEnemyType JE_MultiEnemyType[100]; /* [1..100] */

    //typedef JE_char JE_CharString[256]; /* [1..256] */

    //typedef JE_byte JE_Map1Buffer[24 * 28 * 13 * 4]; /* [1..24*28*13*4] */

    public struct JE_EventRecType
    {
        public JE_word eventtime;
        public JE_byte eventtype;
        public JE_integer eventdat, eventdat2;
        public JE_shortint eventdat3, eventdat5, eventdat6;
        public JE_byte eventdat4;
    }

    public static JE_byte[] enemyAvail = new JE_byte[100]; /* [1..100] */  /* values: 0: used, 1: free, 2: secret pick-up */

    public struct JE_MegaDataType
    {
        public JE_MegaDataType(int t)
        {
            switch (t)
            {
                case 1:
                    shapes = new Shape[72];
                    mainmap = DoubleEmptyArray<byte>(300, 14, 0);
                    break;
                case 2:
                    shapes = new Shape[71];
                    mainmap = DoubleEmptyArray<byte>(600, 14, 0);
                    break;
                case 3:
                    shapes = new Shape[70];
                    mainmap = DoubleEmptyArray<byte>(600, 15, 0);
                    break;
                default:
                    shapes = new Shape[0];
                    mainmap = new byte[0][];
                    break;
            }
            tempdat = 0;
        }

        public byte[][] mainmap;  //Indexes into the shape array

        public struct Shape
        {
            public int fill;
            public JE_byte[] sh;
        }
        public Shape[] shapes;
        public JE_byte tempdat;
    }

    public struct EnemyShotType {
        public JE_integer sx, sy;
        public JE_integer sxm, sym;
        public JE_shortint sxc, syc;
        public JE_byte tx, ty;
        public JE_word sgr;
        public JE_byte sdmg;
        public JE_byte duration;
        public JE_word animate;
        public JE_word animax;
    }

    public struct explosion_type {
        public uint ttl;
        public int x, y;
        public int delta_x, delta_y;
        public bool fixed_position;
        public bool follow_player;
        public int sprite;
    }

    public struct rep_explosion_type {
        public uint delay;
        public uint ttl;
        public uint x, y;
        public bool big;
    }

    public struct superpixel_type {
        public ushort x, y, z;
        public short delta_x, delta_y;
        public byte color;
    }

    public static JE_integer tempDat, tempDat2, tempDat3;
    public static readonly JE_byte[] SANextShip /* [0..SA + 1] */ = { 3, 9, 6, 2, 5, 1, 4, 3, 7 }; // 0 . 3 . 2 . 6 . 4 . 5 . 1 . 9 . 7
    public static readonly JE_word[] SASpecialWeapon /* [1..SA] */  = { 7, 8, 9, 10, 11, 12, 13 };
    public static readonly JE_word[] SASpecialWeaponB /* [1..SA] */ = { 37, 6, 15, 40, 16, 14, 41 };
    public static readonly JE_byte[] SAShip /* [1..SA] */ = { 3, 1, 5, 10, 2, 11, 12 };
    public static readonly JE_word[][] SAWeapon /* [1..SA, 1..5] */ =
{               /*  R  Bl  Bk  G   P */
	new JE_word[]{  9, 31, 32, 33, 34 },  /* Stealth Ship */
	new JE_word[]{ 19,  8, 22, 41, 34 },  /* StormWind    */
	new JE_word[]{ 27,  5, 20, 42, 31 },  /* Techno       */
	new JE_word[]{ 15,  3, 28, 22, 12 },  /* Enemy        */
	new JE_word[]{ 23, 35, 25, 14,  6 },  /* Weird        */
	new JE_word[]{  2,  5, 21,  4,  7 },  /* Unknown      */
	new JE_word[]{ 40, 38, 37, 41, 36 }   /* NortShip Z   */
};

    public static JE_byte[] specialArcadeWeapon /* [1..Portnum] */ =
    {
        17,17,18,0,0,0,10,0,0,0,0,0,44,0,10,0,19,0,0,-0,0,0,0,0,0,0,
        -0,0,0,0,45,0,0,0,0,0,0,0,0,0,0,0
    };

    public static readonly int[][][] optionSelect /* [0..15, 1..3, 1..2] */ =
{	/*  MAIN    OPT    FRONT */
	new []{ new []{ 0, 0},new []{ 0, 0},new []{ 0, 0} },  /**/
	new []{ new []{ 1, 1},new []{16,16},new []{30,30} },  /*Single Shot*/
	new []{ new []{ 2, 2},new []{29,29},new []{29,20} },  /*Dual Shot*/
	new []{ new []{ 3, 3},new []{21,21},new []{12, 0} },  /*Charge Cannon*/
	new []{ new []{ 4, 4},new []{18,18},new []{16,23} },  /*Vulcan*/
	new []{ new []{ 0, 0},new []{ 0, 0},new []{ 0, 0} },  /**/
	new []{ new []{ 6, 6},new []{29,16},new []{ 0,22} },  /*Super Missile*/
	new []{ new []{ 7, 7},new []{19,19},new []{19,28} },  /*Atom Bomb*/
	new []{ new []{ 0, 0},new []{ 0, 0},new []{ 0, 0} },  /**/
	new []{ new []{ 0, 0},new []{ 0, 0},new []{ 0, 0} },  /**/
	new []{ new []{10,10},new []{21,21},new []{21,27} },  /*Mini Missile*/
	new []{ new []{ 0, 0},new []{ 0, 0},new []{ 0, 0} },  /**/
	new []{ new []{ 0, 0},new []{ 0, 0},new []{ 0, 0} },  /**/
	new []{ new []{13,13},new []{17,17},new []{13,26} },  /*MicroBomb*/
	new []{ new []{ 0, 0},new []{ 0, 0},new []{ 0, 0} },  /**/
	new []{ new []{15,15},new []{15,16},new[] { 15,16} }   /*Post-It*/
};

    public static readonly JE_word[] PGR /* [1..21] */ =
    {
        4,
        1,2,3,
        41-21,57-21,73-21,89-21,105-21,
        121-21,137-21,153-21,
        151,151,151,151,73-21,73-21,1,2,4
	    /*151,151,151*/
    };

    public static readonly bool[] PAni /* [1..21] */ = { true, false, false, false, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, true };


    public static JE_word[] linkGunWeapons /* [1..38] */ =
    {
        0,0,0,0,0,0,0,0,444,445,446,447,0,448,449,0,0,0,0,0,450,451,0,506,0,564,
        445,446,447,448,449,445,446,447,448,449,450,451
    };
    public static JE_word[] chargeGunWeapons /* [1..38] */ =
    {
        0,0,0,0,0,0,0,0,476,458,464,482,0,488,470,0,0,0,0,0,494,500,0,528,0,558,
        458,458,458,458,458,458,458,458,458,458,458,458
    };
    public static JE_byte[] randomEnemyLaunchSounds /* [1..3] */ = { 13, 6, 26 };

    /* YKS: Twiddle cheat sheet:
     * 1: UP
     * 2: DOWN
     * 3: LEFT
     * 4: RIGHT
     * 5: UP+FIRE
     * 6: DOWN+FIRE
     * 7: LEFT+FIRE
     * 8: RIGHT+FIRE
     * 9: Release all keys (directions and fire)
     */
    public static readonly JE_byte[][] keyboardCombos /* [1..26, 1..8] */ =
    {
	    new byte[]{ 2, 1,   2,   5, 137,           0, 0, 0}, /*Invulnerability*/
	    new byte[]{ 4, 3,   2,   5, 138,           0, 0, 0}, /*Atom Bomb*/
	    new byte[]{ 3, 4,   6, 139,             0, 0, 0, 0}, /*Seeker Bombs*/
	    new byte[]{ 2, 5, 142,               0, 0, 0, 0, 0}, /*Ice Blast*/
	    new byte[]{ 6, 2,   6, 143,             0, 0, 0, 0}, /*Auto Repair*/
	    new byte[]{ 6, 7,   5,   8,   6,   7,  5, 112     }, /*Spin Wave*/
	    new byte[]{ 7, 8, 101,               0, 0, 0, 0, 0}, /*Repulsor*/
	    new byte[]{ 1, 7,   6, 146,             0, 0, 0, 0}, /*Protron Field*/
	    new byte[]{ 8, 6,   7,   1, 120,           0, 0, 0}, /*Minefield*/
	    new byte[]{ 3, 6,   8,   5, 121,           0, 0, 0}, /*Post-It Blast*/
	    new byte[]{ 1, 2,   7,   8, 119,           0, 0, 0}, /*Drone Ship - TBC*/
	    new byte[]{ 3, 4,   3,   6, 123,           0, 0, 0}, /*Repair Player 2*/
	    new byte[]{ 6, 7,   5,   8, 124,           0, 0, 0}, /*Super Bomb - TBC*/
	    new byte[]{ 1, 6, 125,               0, 0, 0, 0, 0}, /*Hot Dog*/
	    new byte[]{ 9, 5, 126,               0, 0, 0, 0, 0}, /*Lightning UP      */
	    new byte[]{ 1, 7, 127,               0, 0, 0, 0, 0}, /*Lightning UP+LEFT */
	    new byte[]{ 1, 8, 128,               0, 0, 0, 0, 0}, /*Lightning UP+RIGHT*/
	    new byte[]{ 9, 7, 129,               0, 0, 0, 0, 0}, /*Lightning    LEFT */
	    new byte[]{ 9, 8, 130,               0, 0, 0, 0, 0}, /*Lightning    RIGHT*/
	    new byte[]{ 4, 2,   3,   5, 131,           0, 0, 0}, /*Warfly            */
	    new byte[]{ 3, 1,   2,   8, 132,           0, 0, 0}, /*FrontBlaster      */
	    new byte[]{ 2, 4,   5, 133,             0, 0, 0, 0}, /*Gerund            */
	    new byte[]{ 3, 4,   2,   8, 134,           0, 0, 0}, /*FireBomb          */
	    new byte[]{ 1, 4,   6, 135,             0, 0, 0, 0}, /*Indigo            */
	    new byte[]{ 1, 3,   6, 137,             0, 0, 0, 0}, /*Invulnerability [easier] */
	    new byte[]{ 1, 4,   3,   4,   7, 136,         0, 0}  /*D-Media Protron Drone    */
    };

    public static readonly JE_byte[] shipCombosB /* [1..21] */ =
    {15,16,17,18,19,20,21,22,23,24, 7, 8, 5,25,14, 4, 6, 3, 9, 2,26};
    /*!! SUPER Tyrian !!*/
    public static readonly JE_byte[] superTyrianSpecials /* [1..4] */ = { 1, 2, 4, 5 };

    public static readonly JE_byte[][] shipCombos /* [0..12, 1..3] */ =
    {
	    new byte[]{ 5, 4, 7},  /*2nd Player ship*/
	    new byte[]{ 1, 2, 0},  /*USP Talon*/
	    new byte[]{14, 4, 0},  /*Super Carrot*/
	    new byte[]{ 4, 5, 0},  /*Gencore Phoenix*/
	    new byte[]{ 6, 5, 0},  /*Gencore Maelstrom*/
	    new byte[]{ 7, 8, 0},  /*MicroCorp Stalker*/
	    new byte[]{ 7, 9, 0},  /*MicroCorp Stalker-B*/
	    new byte[]{10, 3, 5},  /*Prototype Stalker-C*/
	    new byte[]{ 5, 8, 9},  /*Stalker*/
	    new byte[]{ 1, 3, 0},  /*USP Fang*/
	    new byte[]{ 7,16,17},  /*U-Ship*/
	    new byte[]{ 2,11,12},  /*1st Player ship*/
	    new byte[]{ 3, 8,10},  /*Nort ship*/
	    new byte[]{ 0, 0, 0}   // Dummy entry added for Stalker 21.126
    };

    /*Street-Fighter Commands*/
    public static JE_byte[][] SFCurrentCode = DoubleEmptyArray<JE_byte>(2,21,0); /* [1..2, 1..21] */
    public static JE_byte[] SFExecuted = new JE_byte[2]; /* [1..2] */

    public static int lvlFileNum;
    public static JE_word maxEvent, eventLoc;
    public static JE_word tempBackMove, explodeMove;
    public static JE_byte levelEnd;
    public static JE_word levelEndFxWait;
    public static JE_shortint levelEndWarp;
    public static JE_boolean endLevel, reallyEndLevel, waitToEndLevel, playerEndLevel, normalBonusLevelCurrent, bonusLevelCurrent, smallEnemyAdjust, readyToEndLevel, quitRequested;
    public static JE_byte[] newPL = new JE_byte[10];
    public static JE_word returnLoc;
    public static JE_boolean returnActive;
    public static JE_word galagaShotFreq;
    public static JE_longint galagaLife;
    public static JE_boolean debug;
    public static uint debugTime, lastDebugTime;
    public static JE_longint debugHistCount;
    public static JE_real debugHist;
    public static JE_word curLoc;
    public static JE_boolean firstGameOver, gameLoaded, enemyStillExploding;
    public static JE_word totalEnemy;
    public static JE_word enemyKilled;
    public static JE_MegaDataType megaData1 = new JE_MegaDataType(1);
    public static JE_MegaDataType megaData2 = new JE_MegaDataType(2);
    public static JE_MegaDataType megaData3 = new JE_MegaDataType(3);
    public static JE_byte flash;
    public static JE_shortint flashChange;
    public static JE_byte displayTime;

    public static bool play_demo, record_demo, stopped_demo;
    public static byte demo_num;
    public static BinaryReader demo_file;

    public static byte demo_keys, next_demo_keys;
    public static ushort demo_keys_wait;

    public static JE_byte[] soundQueue = new JE_byte[8];
    public static JE_boolean enemyContinualDamage;
    public static JE_boolean enemiesActive;
    public static JE_boolean forceEvents;
    public static JE_boolean stopBackgrounds;
    public static JE_byte stopBackgroundNum;
    public static JE_byte damageRate;
    public static JE_boolean background3x1;
    public static JE_boolean background3x1b;
    public static JE_boolean levelTimer;
    public static JE_word levelTimerCountdown;
    public static JE_word levelTimerJumpTo;
    public static JE_boolean randomExplosions;
    public static JE_boolean editShip1, editShip2;
    public static JE_boolean[] globalFlags = new JE_boolean[10];
    public static int levelSong;
    public static JE_boolean loadDestruct;
    public static int mapOrigin, mapPNum;
    public static int[] mapPlanet = new int[5], mapSection = new int[5];
    public static JE_boolean moveTyrianLogoUp = true;
    public static JE_boolean skipStarShowVGA;
    public static JE_SingleEnemyType[] enemy = EmptyArray<JE_SingleEnemyType>(100);
    public static JE_word enemyOffset;
    public static JE_word enemyOnScreen;
    public static JE_byte[] enemyShapeTables = new JE_byte[6];
    public static JE_word superEnemy254Jump;
    public static explosion_type[] explosions = new explosion_type[MAX_EXPLOSIONS];
    public static int explosionFollowAmountX, explosionFollowAmountY;
    public static JE_boolean fireButtonHeld;
    public static JE_boolean[] enemyShotAvail = new JE_boolean[ENEMY_SHOT_MAX];
    public static EnemyShotType[] enemyShot = new EnemyShotType[ENEMY_SHOT_MAX];
    public static JE_byte zinglonDuration;
    public static int astralDuration;
    public static int flareDuration;
    public static JE_boolean flareStart;
    public static JE_shortint flareColChg;
    public static JE_byte specialWait;
    public static JE_byte nextSpecialWait;
    public static JE_boolean spraySpecial;
    public static JE_byte doIced;
    public static JE_boolean infiniteShot;
    public static JE_boolean allPlayersGone;
    public const int shadowYDist = 10;
    public static JE_real optionSatelliteRotate;
    public static int optionAttachmentMove;
    public static JE_boolean optionAttachmentLinked, optionAttachmentReturn;
    public static int chargeWait, chargeLevel, chargeMax, chargeGr, chargeGrWait;
    public static JE_word neat;
    public static rep_explosion_type[] rep_explosions = new rep_explosion_type[MAX_REPEATING_EXPLOSIONS];
    public static superpixel_type[] superpixels = new superpixel_type[MAX_SUPERPIXELS];
    public static uint last_superpixel;
    public static int temp, temp2, temp3;
    public static JE_word tempX, tempY;
    public static JE_word tempW;
    public static JE_boolean doNotSaveBackup;
    public static JE_word x, y;
    public static int b;
    public static int BKwrap1to, BKwrap2to, BKwrap3to, BKwrap1, BKwrap2, BKwrap3;
    public static int specialWeaponFilter, specialWeaponFreq;
    public static JE_word specialWeaponWpn;
    public static JE_boolean linkToPlayer;
    public static JE_word shipGr, shipGr2;
    public static byte[] shipGrPtr, shipGr2ptr;

    public static readonly int[][] hud_sidekick_y =
    {
        new[]{  64,  82 }, // one player HUD
	    new[]{ 108, 126 }, // two player HUD
    };

    public static void JE_getShipInfo()
    {
        JE_boolean extraShip, extraShip2;

        shipGrPtr = shapes9;
        shipGr2ptr = shapes9;

        powerAdd = powerSys[player[0].items.generator].power;

        extraShip = player[0].items.ship > 90;
        if (extraShip)
        {
            JE_byte base1 = (JE_byte)((player[0].items.ship - 91) * 15);
            shipGr = JE_SGr((JE_word)(player[0].items.ship - 90), ref shipGrPtr);
            player[0].armor = extraShips[base1 + 7];
        }
        else
        {
            shipGr = ships[player[0].items.ship].shipgraphic;
            player[0].armor = ships[player[0].items.ship].dmg;
        }

        extraShip2 = player[1].items.ship > 90;
        if (extraShip2)
        {
            JE_byte base2 = (JE_byte)((player[1].items.ship - 91) * 15);
            shipGr2 = JE_SGr((JE_word)(player[1].items.ship - 90), ref shipGr2ptr);
            player[1].armor = extraShips[base2 + 7]; /* bug? */
        }
        else
        {
            shipGr2 = 0;
            player[1].armor = 10;
        }

        for (uint i = 0; i < player.Length; ++i)
        {
            player[i].initial_armor = player[i].armor;


            uint temp = ((i == 0 && extraShip) ||
                         (i == 1 && extraShip2)) ? 2 : (uint)ships[player[i].items.ship].ani;

            if (temp == 0)
            {
                player[i].shot_hit_area_x = 12;
                player[i].shot_hit_area_y = 10;
            }
            else
            {
                player[i].shot_hit_area_x = 11;
                player[i].shot_hit_area_y = 14;
            }
        }
    }

    private static readonly JE_word[] GR /* [1..15] */ = { 233, 157, 195, 271, 81, 0, 119, 5, 43, 81, 119, 157, 195, 233, 271 };
    public static JE_word JE_SGr(JE_word ship, ref byte[] ptr)
    {
        JE_word tempW = extraShips[(ship - 1) * 15];
        if (tempW > 7)
            ptr = extraShapes;

        return GR[tempW - 1];
    }

    public static void JE_drawOptions()
    {
        Surface temp_surface = VGAScreen;
        VGAScreen = VGAScreenSeg;

        Player this_player = player[twoPlayerMode ? 1 : 0];

        for (uint i = 0; i < this_player.sidekick.Length; ++i)
        {
            JE_OptionType this_option = options[this_player.items.sidekick[i]];

            this_player.sidekick[i].ammo =
            this_player.sidekick[i].ammo_max = this_option.ammo;

            this_player.sidekick[i].ammo_refill_ticks =
            this_player.sidekick[i].ammo_refill_ticks_max = (105 - this_player.sidekick[i].ammo) * 4;

            this_player.sidekick[i].style = this_option.tr;

            this_player.sidekick[i].animation_enabled = (this_option.option == 1);
            this_player.sidekick[i].animation_frame = 0;

            this_player.sidekick[i].charge = 0;
            this_player.sidekick[i].charge_ticks = 20;


            // draw initial sidekick HUD
            int y = hud_sidekick_y[twoPlayerMode ? 1 : 0][i];

            fill_rectangle_xy(VGAScreenSeg, 284, y, 284 + 28, y + 15, 0);
            if (this_option.icongr > 0)
                blit_sprite(VGAScreenSeg, 284, y, OPTION_SHAPES, this_option.icongr - 1);  // sidekick HUD icon
            draw_segmented_gauge(VGAScreenSeg, 284, y + 13, 112, 2, 2, Max(1, this_player.sidekick[i].ammo_max / 10), this_player.sidekick[i].ammo);
        }

        VGAScreen = temp_surface;

        JE_drawOptionLevel();
    }

    public static void JE_drawOptionLevel()
    {
        if (twoPlayerMode)
        {
            for (temp = 1; temp <= 3; temp++)
            {
                fill_rectangle_xy(VGAScreenSeg, 268, 127 + (temp - 1) * 6, 269, 127 + 3 + (temp - 1) * 6, (byte)(193 + (((player[1].items.sidekick_level - 100) == temp) ? 11 : 0)));
            }
        }
    }

    public static void JE_specialComplete(JE_byte playerNum, JE_byte specialType)
    {
        nextSpecialWait = 0;
        switch (special[specialType].stype)
        {
            /*Weapon*/
            case 1:
                if (playerNum == 1)
                    b = player_shot_create(0, SHOT_SPECIAL2, player[0].x, player[0].y, mouseX, mouseY, special[specialType].wpn, playerNum);
                else
                    b = player_shot_create(0, SHOT_SPECIAL2, player[1].x, player[1].y, mouseX, mouseY, special[specialType].wpn, playerNum);

                shotRepeat[SHOT_SPECIAL] = shotRepeat[SHOT_SPECIAL2];
                break;
            /*Repulsor*/
            case 2:
                for (temp = 0; temp < ENEMY_SHOT_MAX; temp++)
                {
                    if (!enemyShotAvail[temp])
                    {
                        if (player[0].x > enemyShot[temp].sx)
                            enemyShot[temp].sxm--;
                        else if (player[0].x < enemyShot[temp].sx)
                            enemyShot[temp].sxm++;

                        if (player[0].y > enemyShot[temp].sy)
                            enemyShot[temp].sym--;
                        else if (player[0].y < enemyShot[temp].sy)
                            enemyShot[temp].sym++;
                    }
                }
                break;
            /*Zinglon Blast*/
            case 3:
                zinglonDuration = 50;
                shotRepeat[SHOT_SPECIAL] = 100;
                soundQueue[7] = S_SOUL_OF_ZINGLON;
                break;
            /*Attractor*/
            case 4:
                for (temp = 0; temp < 100; temp++)
                {
                    if (enemyAvail[temp] != 1 && enemy[temp].scoreitem
                        && enemy[temp].evalue != 0)
                    {
                        if (player[0].x > enemy[temp].ex)
                            enemy[temp].exc++;
                        else if (player[0].x < enemy[temp].ex)
                            enemy[temp].exc--;

                        if (player[0].y > enemy[temp].ey)
                            enemy[temp].eyc++;
                        else if (player[0].y < enemy[temp].ey)
                            enemy[temp].eyc--;
                    }
                }
                break;
            /*Flare*/
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 16:
                if (flareDuration == 0)
                    flareStart = true;

                specialWeaponWpn = special[specialType].wpn;
                linkToPlayer = false;
                spraySpecial = false;
                switch (special[specialType].stype)
                {
                    case 5:
                        specialWeaponFilter = 7;
                        specialWeaponFreq = 2;
                        flareDuration = 50;
                        break;
                    case 6:
                        specialWeaponFilter = 1;
                        specialWeaponFreq = 7;
                        flareDuration = 200 + 25 * player[0].items.weapon[FRONT_WEAPON].power;
                        break;
                    case 7:
                        specialWeaponFilter = 3;
                        specialWeaponFreq = 3;
                        flareDuration = 50 + 10 * player[0].items.weapon[FRONT_WEAPON].power;
                        zinglonDuration = 50;
                        shotRepeat[SHOT_SPECIAL] = 100;
                        soundQueue[7] = S_SOUL_OF_ZINGLON;
                        break;
                    case 8:
                        specialWeaponFilter = -99;
                        specialWeaponFreq = 7;
                        flareDuration = 10 + player[0].items.weapon[FRONT_WEAPON].power;
                        break;
                    case 9:
                        specialWeaponFilter = -99;
                        specialWeaponFreq = 8;
                        flareDuration = 8 + 2 * player[0].items.weapon[FRONT_WEAPON].power;
                        linkToPlayer = true;
                        nextSpecialWait = special[specialType].pwr;
                        break;
                    case 10:
                        specialWeaponFilter = -99;
                        specialWeaponFreq = 8;
                        flareDuration = 14 + 4 * player[0].items.weapon[FRONT_WEAPON].power;
                        linkToPlayer = true;
                        break;
                    case 11:
                        specialWeaponFilter = -99;
                        specialWeaponFreq = special[specialType].pwr;
                        flareDuration = 10 + 10 * player[0].items.weapon[FRONT_WEAPON].power;
                        astralDuration = 20 + 10 * player[0].items.weapon[FRONT_WEAPON].power;
                        break;
                    case 16:
                        specialWeaponFilter = -99;
                        specialWeaponFreq = 8;
                        flareDuration = temp2 * 16 + 8;
                        linkToPlayer = true;
                        spraySpecial = true;
                        break;
                }
                break;
            case 12:
                player[playerNum - 1].invulnerable_ticks = temp2 * 10;

                if (superArcadeMode > 0 && superArcadeMode <= SA)
                {
                    shotRepeat[SHOT_SPECIAL] = 250;
                    b = player_shot_create(0, SHOT_SPECIAL2, player[0].x, player[0].y, mouseX, mouseY, 707, 1);
                    player[0].invulnerable_ticks = 100;
                }
                break;
            case 13:
                player[0].armor += temp2 / 4 + 1;

                soundQueue[3] = S_POWERUP;
                break;
            case 14:
                player[1].armor += temp2 / 4 + 1;

                soundQueue[3] = S_POWERUP;
                break;

            case 17:  // spawn left or right sidekick
                soundQueue[3] = S_POWERUP;

                if (player[0].items.sidekick[LEFT_SIDEKICK] == special[specialType].wpn)
                {
                    player[0].items.sidekick[RIGHT_SIDEKICK] = special[specialType].wpn;
                    shotMultiPos[RIGHT_SIDEKICK] = 0;
                }
                else
                {
                    player[0].items.sidekick[LEFT_SIDEKICK] = special[specialType].wpn;
                    shotMultiPos[LEFT_SIDEKICK] = 0;
                }

                JE_drawOptions();
                break;

            case 18:  // spawn right sidekick
                player[0].items.sidekick[RIGHT_SIDEKICK] = special[specialType].wpn;

                JE_drawOptions();

                soundQueue[4] = S_POWERUP;

                shotMultiPos[RIGHT_SIDEKICK] = 0;
                break;
        }
    }

    public static void JE_doSpecialShot(JE_byte playerNum, ref int armor, ref int shield)
    {
        if (player[0].items.special > 0)
        {
            if (shotRepeat[SHOT_SPECIAL] == 0 && specialWait == 0 && flareDuration < 2 && zinglonDuration < 2)
                blit_sprite2(VGAScreen, 47, 4, shapes9, 94);
            else
                blit_sprite2(VGAScreen, 47, 4, shapes9, 93);
        }

        if (shotRepeat[SHOT_SPECIAL] > 0)
        {
            --shotRepeat[SHOT_SPECIAL];
        }
        if (specialWait > 0)
        {
            specialWait--;
        }
        temp = SFExecuted[playerNum - 1];
        if (temp > 0 && shotRepeat[SHOT_SPECIAL] == 0 && flareDuration == 0)
        {
            temp2 = special[temp].pwr;

            bool can_afford = true;

            if (temp2 > 0)
            {
                if (temp2 < 98)  // costs some shield
                {
                    if (shield >= temp2)
                        shield -= temp2;
                    else
                        can_afford = false;
                }
                else if (temp2 == 98)  // costs all shield
                {
                    if (shield < 4)
                        can_afford = false;
                    temp2 = shield;
                    shield = 0;
                }
                else if (temp2 == 99)  // costs half shield
                {
                    temp2 = shield / 2;
                    shield = temp2;
                }
                else  // costs some armor
                {
                    temp2 -= 100;
                    if (armor > temp2)
                        armor -= temp2;
                    else
                        can_afford = false;
                }
            }

            shotMultiPos[SHOT_SPECIAL] = 0;
            shotMultiPos[SHOT_SPECIAL2] = 0;

            if (can_afford)
                JE_specialComplete(playerNum, (byte)temp);

            SFExecuted[playerNum - 1] = 0;

            JE_wipeShieldArmorBars();
            VGAScreen = VGAScreenSeg; /* side-effect of game_screen */
            JE_drawShield();
            JE_drawArmor();
            VGAScreen = game_screen; /* side-effect of game_screen */
        }

        if (playerNum == 1 && player[0].items.special > 0)
        {  /*Main Begin*/

            if (superArcadeMode > 0 && (button[2 - 1] || button[3 - 1]))
            {
                fireButtonHeld = false;
            }
            if (!button[1 - 1] && !(superArcadeMode != SA_NONE && (button[2 - 1] || button[3 - 1])))
            {
                fireButtonHeld = false;
            }
            else if (shotRepeat[SHOT_SPECIAL] == 0 && !fireButtonHeld && !(flareDuration > 0) && specialWait == 0)
            {
                fireButtonHeld = true;
                JE_specialComplete(playerNum, (byte)player[0].items.special);
            }

        }  /*Main End*/

        if (astralDuration > 0)
            astralDuration--;

        shotAvail[MAX_PWEAPON - 1] = 0;
        if (flareDuration > 1)
        {
            if (specialWeaponFilter != -99)
            {
                if (levelFilter == -99 && levelBrightness == -99)
                {
                    filterActive = false;
                }
                if (!filterActive)
                {
                    levelFilter = specialWeaponFilter;
                    if (levelFilter == 7)
                    {
                        levelBrightness = 0;
                    }
                    filterActive = true;
                }

                if (mt_rand() % 2 == 0)
                    flareColChg = -1;
                else
                    flareColChg = 1;

                if (levelFilter == 7)
                {
                    if (levelBrightness < -6)
                    {
                        flareColChg = 1;
                    }
                    if (levelBrightness > 6)
                    {
                        flareColChg = -1;
                    }
                    levelBrightness += flareColChg;
                }
            }

            if ((int)(mt_rand() % 6) < specialWeaponFreq)
            {
                b = MAX_PWEAPON;

                if (linkToPlayer)
                {
                    if (shotRepeat[SHOT_SPECIAL] == 0)
                    {
                        b = player_shot_create(0, SHOT_SPECIAL, player[0].x, player[0].y, mouseX, mouseY, specialWeaponWpn, playerNum);
                    }
                }
                else
                {
                    b = player_shot_create(0, SHOT_SPECIAL, (int)(mt_rand() % 280), (int)(mt_rand() % 180), mouseX, mouseY, specialWeaponWpn, playerNum);
                }

                if (spraySpecial && b != MAX_PWEAPON)
                {
                    playerShotData[b].shotXM = (JE_integer)((mt_rand() % 5) - 2);
                    playerShotData[b].shotYM = (JE_integer)((mt_rand() % 5) - 2);
                    if (playerShotData[b].shotYM == 0)
                    {
                        playerShotData[b].shotYM++;
                    }
                }
            }

            flareDuration--;
            if (flareDuration == 1)
            {
                specialWait = nextSpecialWait;
            }
        }
        else if (flareStart)
        {
            flareStart = false;
            shotRepeat[SHOT_SPECIAL] = linkToPlayer ? (byte)15 : (byte)200;
            flareDuration = 0;
            if (levelFilter == specialWeaponFilter)
            {
                levelFilter = -99;
                levelBrightness = -99;
                filterActive = false;
            }
        }

        if (zinglonDuration > 1)
        {
            temp = 25 - Abs(zinglonDuration - 25);

            JE_barBright(VGAScreen, player[0].x + 7 - temp, 0, player[0].x + 7 + temp, 184);
            JE_barBright(VGAScreen, player[0].x + 7 - temp - 2, 0, player[0].x + 7 + temp + 2, 184);

            zinglonDuration--;
            if (zinglonDuration % 5 == 0)
            {
                shotAvail[MAX_PWEAPON - 1] = 1;
            }
        }
    }

        static (int sprite, int ttl)[] explosion_data = /* [1..53] */ new[]{
                    ( 144,  7 ),
                    ( 120, 12 ),
                    ( 190, 12 ),
                    ( 209, 12 ),
                    ( 152, 12 ),
                    ( 171, 12 ),
                    ( 133,  7 ),   /*White Smoke*/
            		(   1, 12 ),
                    (  20, 12 ),
                    (  39, 12 ),
                    (  58, 12 ),
                    ( 110,  3 ),
                    (  76,  7 ),
                    (  91,  3 ),
            /*15*/	( 227,  3 ),
                    ( 230,  3 ),
                    ( 233,  3 ),
                    ( 252,  3 ),
                    ( 246,  3 ),
            /*20*/	( 249,  3 ),
                    ( 265,  3 ),
                    ( 268,  3 ),
                    ( 271,  3 ),
                    ( 236,  3 ),
            /*25*/	( 239,  3 ),
                    ( 242,  3 ),
                    ( 261,  3 ),
                    ( 274,  3 ),
                    ( 277,  3 ),
            /*30*/	( 280,  3 ),
                    ( 299,  3 ),
                    ( 284,  3 ),
                    ( 287,  3 ),
                    ( 290,  3 ),
            /*35*/	( 293,  3 ),
                    ( 165,  8 ),   /*Coin Values*/
            		( 184,  8 ),
                    ( 203,  8 ),
                    ( 222,  8 ),
                    ( 168,  8 ),
                    ( 187,  8 ),
                    ( 206,  8 ),
                    ( 225, 10 ),
                    ( 169, 10 ),
                    ( 188, 10 ),
                    ( 207, 20 ),
                    ( 226, 14 ),
                    ( 170, 14 ),
                    ( 189, 14 ),
                    ( 208, 14 ),
                    ( 246, 14 ),
                    ( 227, 14 ),
                    ( 265, 14 )
        };
    public static void JE_setupExplosion(int x, int y, int delta_y, int type, bool fixed_position, bool follow_player)
    {

        if (y > -16 && y < 190)
        {
            for (int i = 0; i < MAX_EXPLOSIONS; i++)
            {
                if (explosions[i].ttl == 0)
                {
                    explosions[i].x = x;
                    explosions[i].y = y;
                    if (type == 6)
                    {
                        explosions[i].y += 12;
                        explosions[i].x += 2;
                    }
                    else if (type == 98)
                    {
                        type = 6;
                    }
                    explosions[i].sprite = explosion_data[type].sprite;
                    explosions[i].ttl = (byte)explosion_data[type].ttl;
                    explosions[i].follow_player = follow_player;
                    explosions[i].fixed_position = fixed_position;
                    explosions[i].delta_x = 0;
                    explosions[i].delta_y = delta_y;
                    break;
                }
            }
        }
    }

    public static void JE_setupExplosionLarge(JE_boolean enemyGround, JE_byte exploNum, JE_integer x, JE_integer y)
    {
        if (y >= 0)
        {
            if (enemyGround)
            {
                JE_setupExplosion(x - 6, y - 14, 0, 2, false, false);
                JE_setupExplosion(x + 6, y - 14, 0, 4, false, false);
                JE_setupExplosion(x - 6, y, 0, 3, false, false);
                JE_setupExplosion(x + 6, y, 0, 5, false, false);
            }
            else
            {
                JE_setupExplosion(x - 6, y - 14, 0, 7, false, false);
                JE_setupExplosion(x + 6, y - 14, 0, 9, false, false);
                JE_setupExplosion(x - 6, y, 0, 8, false, false);
                JE_setupExplosion(x + 6, y, 0, 10, false, false);
            }

            bool big;

            if (exploNum > 10)
            {
                exploNum -= 10;
                big = true;
            }
            else
            {
                big = false;
            }

            if (exploNum != 0)
            {
                for (int i = 0; i < MAX_REPEATING_EXPLOSIONS; i++)
                {
                    if (rep_explosions[i].ttl == 0)
                    {
                        rep_explosions[i].ttl = exploNum;
                        rep_explosions[i].delay = 2;
                        rep_explosions[i].x = (uint)x;
                        rep_explosions[i].y = (uint)y;
                        rep_explosions[i].big = big;
                        break;
                    }
                }
            }
        }
    }

    public static void JE_wipeShieldArmorBars()
    {
        if (!twoPlayerMode || galagaMode)
        {
            fill_rectangle_xy(VGAScreenSeg, 270, 137, 278, 194 - player[0].shield * 2, 0);
        }
        else
        {
            fill_rectangle_xy(VGAScreenSeg, 270, 60 - 44, 278, 60, 0);
            fill_rectangle_xy(VGAScreenSeg, 270, 194 - 44, 278, 194, 0);
        }
        if (!twoPlayerMode || galagaMode)
        {
            fill_rectangle_xy(VGAScreenSeg, 307, 137, 315, 194 - player[0].armor * 2, 0);
        }
        else
        {
            fill_rectangle_xy(VGAScreenSeg, 307, 60 - 44, 315, 60, 0);
            fill_rectangle_xy(VGAScreenSeg, 307, 194 - 44, 315, 194, 0);
        }
    }

    public static JE_byte JE_playerDamage(int temp, Player this_player)
    {
        int playerDamage = 0;
        soundQueue[7] = S_SHIELD_HIT;

        /* Player Damage Routines */
        if (this_player.shield < temp)
        {
            playerDamage = temp;
            temp -= this_player.shield;
            this_player.shield = 0;

            if (temp > 0)
            {
                /*Through Shields - Now Armor */

                if (this_player.armor < temp)
                {
                    temp -= this_player.armor;
                    this_player.armor = 0;

                    if (this_player.is_alive && !youAreCheating)
                    {
                        levelTimer = false;
                        this_player.is_alive = false;
                        this_player.exploding_ticks = 60;
                        levelEnd = 40;
                        tempVolume = tyrMusicVolume;
                        soundQueue[1] = S_EXPLOSION_22;
                    }
                }
                else
                {
                    this_player.armor -= temp;
                    soundQueue[7] = S_HULL_HIT;
                }
            }
        }
        else
        {
            this_player.shield -= temp;

            JE_setupExplosion(this_player.x - 17, this_player.y - 12, 0, 14, false, !twoPlayerMode);
            JE_setupExplosion(this_player.x - 5, this_player.y - 12, 0, 15, false, !twoPlayerMode);
            JE_setupExplosion(this_player.x + 7, this_player.y - 12, 0, 16, false, !twoPlayerMode);
            JE_setupExplosion(this_player.x + 19, this_player.y - 12, 0, 17, false, !twoPlayerMode);

            JE_setupExplosion(this_player.x - 17, this_player.y + 2, 0, 18, false, !twoPlayerMode);
            JE_setupExplosion(this_player.x + 19, this_player.y + 2, 0, 19, false, !twoPlayerMode);

            JE_setupExplosion(this_player.x - 17, this_player.y + 16, 0, 20, false, !twoPlayerMode);
            JE_setupExplosion(this_player.x - 5, this_player.y + 16, 0, 21, false, !twoPlayerMode);
            JE_setupExplosion(this_player.x + 7, this_player.y + 16, 0, 22, false, !twoPlayerMode);
        }

        JE_wipeShieldArmorBars();
        VGAScreen = VGAScreenSeg; /* side-effect of game_screen */
        JE_drawShield();
        JE_drawArmor();
        VGAScreen = game_screen; /* side-effect of game_screen */

        return (byte)playerDamage;
    }

    public static JE_word JE_portConfigs()
    {
        int player_index = twoPlayerMode ? 1 : 0;
        return tempW = weaponPort[player[player_index].items.weapon[REAR_WEAPON].id].opnum;
    }

    public static void JE_drawShield()
    {
        if (twoPlayerMode && !galagaMode)
        {
            for (int i = 0; i < player.Length; ++i)
                JE_dBar3(VGAScreen, 270, 60 + 134 * i, (int)Round(player[i].shield * 0.8f), 144);
        }
        else
        {
            JE_dBar3(VGAScreen, 270, 194, player[0].shield, 144);
            if (player[0].shield != player[0].shield_max)
            {
                int y = 193 - (player[0].shield_max * 2);
                JE_rectangle(VGAScreen, 270, y, 278, y, 68); /* <MXD> SEGa000 */
            }
        }
    }

    public static void JE_drawArmor()
    {
        for (int i = 0; i < player.Length; ++i)
            if (player[i].armor > 28)
                player[i].armor = 28;

        if (twoPlayerMode && !galagaMode)
        {
            for (int i = 0; i < player.Length; ++i)
                JE_dBar3(VGAScreen, 307, 60 + 134 * i, (int)Round(player[i].armor * 0.8f), 224);
        }
        else
        {
            JE_dBar3(VGAScreen, 307, 194, player[0].armor, 224);
        }
    }


    /*SuperPixels*/
    public static void JE_doSP(int x, int y, int num, JE_byte explowidth, JE_byte color) /* superpixels */
    {
        for (temp = 0; temp < num; temp++)
        {
            JE_real tempr = mt_rand() * (2 * (float)PI);
            int tempy = (int)Round(Cos(tempr) * mt_rand() * explowidth);
            int tempx = (int)Round(Sin(tempr) * mt_rand() * explowidth);

            if (++last_superpixel >= MAX_SUPERPIXELS)
                last_superpixel = 0;
            superpixels[last_superpixel].x = (ushort)(tempx + x);
            superpixels[last_superpixel].y = (ushort)(tempy + y);
            superpixels[last_superpixel].delta_x = (short)tempx;
            superpixels[last_superpixel].delta_y = (short)(tempy + 1);
            superpixels[last_superpixel].color = color;
            superpixels[last_superpixel].z = 15;
        }
    }

    public static void JE_drawSP()
    {
        byte[] pixels = VGAScreen.pixels;
        for (int i = MAX_SUPERPIXELS; i-- > 0;)
        {
            if (superpixels[i].z != 0)
            {
                superpixels[i].x = (ushort)(superpixels[i].x + superpixels[i].delta_x);
                superpixels[i].y = (ushort)(superpixels[i].y + superpixels[i].delta_y);

                if (superpixels[i].x < VGAScreen.w && superpixels[i].y < VGAScreen.h)
                {
                    int s = superpixels[i].y * VGAScreen.w;
                    s += superpixels[i].x;

                    pixels[s] = (byte)((((pixels[s] & 0x0f) + superpixels[i].z) >> 1) + superpixels[i].color);
                    if (superpixels[i].x > 0)
                        pixels[s - 1] = (byte)((((pixels[s - 1] & 0x0f) + (superpixels[i].z >> 1)) >> 1) + superpixels[i].color);
                    if (superpixels[i].x < VGAScreen.w - 1u)
                        pixels[s + 1] = (byte)((((pixels[s + 1] & 0x0f) + (superpixels[i].z >> 1)) >> 1) + superpixels[i].color);
                    if (superpixels[i].y > 0)
                        pixels[s - VGAScreen.w] = (byte)((((pixels[s - VGAScreen.w] & 0x0f) + (superpixels[i].z >> 1)) >> 1) + superpixels[i].color);
                    if (superpixels[i].y < VGAScreen.h - 1u)
                        pixels[s + VGAScreen.w] = (byte)((((pixels[s + VGAScreen.w] & 0x0f) + (superpixels[i].z >> 1)) >> 1) + superpixels[i].color);
                }

                superpixels[i].z--;
            }
        }
    }

    public static T[] EmptyArray<T>(int count) where T : new()
    {
        T[] ret = new T[count];
        for (int i = 0; i < count; ++i)
        {
            ret[i] = new T();
        }
        return ret;
    }

    public static T[][] DoubleEmptyArray<T>(int count1, int count2, T value) 
    {
        T[][] ret = new T[count1][];
        for (int i = 0; i < count1; ++i)
        {
            ret[i] = new T[count2];
            for (int j = 0; j < count2; ++j)
            {
                ret[i][j] = value;
            }
        }
        return ret;
    }

    private static readonly byte[] _256BytesOf0 = new byte[256];
    private static readonly byte[] _256BytesOf1 = Enumerable.Repeat((byte)1, 256).ToArray();
    public static void FillByteArrayWithZeros(byte[] arr)
    {
        for (int i = 0; i < arr.Length; i += 256)
        {
            System.Array.Copy(_256BytesOf0, arr, Min(256, arr.Length - i));
        }
    }

    public static void FillByteArrayWithOnes(byte[] arr)
    {
        for (int i = 0; i < arr.Length; i += 256)
        {
            System.Array.Copy(_256BytesOf1, arr, Min(256, arr.Length - i));
        }
    }

    private static readonly bool[] _256Falses = new bool[256];
    private static readonly bool[] _256Trues = Enumerable.Repeat(true, 256).ToArray();

    public static void FillBoolArrayWithTrues(bool[] arr)
    {
        for (int i = 0; i < arr.Length; i += 256)
        {
            System.Array.Copy(_256Trues, arr, Min(256, arr.Length - i));
        }
    }

    public static void FillBoolArrayWithFalses(bool[] arr)
    {
        for (int i = 0; i < arr.Length; i += 256)
        {
            System.Array.Copy(_256Falses, arr, Min(256, arr.Length - i));
        }
    }
    
    public static T[] EmptyArray<T>(int count, T value)
    {
        T[] ret = new T[count];
        for (int i = 0; i < count; ++i)
        {
            ret[i] = value;
        }
        return ret;
    }

    public static void JE_tyrianHalt(int exitCode)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}