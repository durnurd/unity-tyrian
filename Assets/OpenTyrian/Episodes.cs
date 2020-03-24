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

using System.IO;
using UnityEngine;

using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static LvlMastC;
using static VarzC;
using static LvlLibC;
using static FileIO;
using static ConfigC;

public static class EpisodesC
{

    /* Episodes and general data */

    public const int FIRST_LEVEL = 1;
    public const int EPISODE_MAX = 5;
#if TYRIAN2000
    public const int EPISODE_AVAILABLE = 5;
#else
    public const int EPISODE_AVAILABLE = 4;
#endif

    public class JE_WeaponType
    {
        public JE_word drain;
        public JE_byte shotrepeat;
        public JE_byte multi;
        public JE_word weapani;
        public JE_byte max;
        public JE_byte tx, ty, aim;
        public JE_byte[] attack, del; /* [1..8] */
        public JE_shortint[] sx, sy; /* [1..8] */
        public JE_shortint[] bx, by; /* [1..8] */
        public JE_word[] sg = new JE_word[8]; /* [1..8] */
        public JE_shortint acceleration, accelerationx;
        public JE_byte circlesize;
        public JE_byte sound;
        public JE_byte trail;
        public JE_byte shipblastfilter;
    }

    public class JE_WeaponPortType
    {
        public string name; /* string [30] */
        public JE_byte opnum;
        public JE_word[][] op = new JE_word[2][]; /* [1..2, 1..11] */
        public JE_word cost;
        public JE_word itemgraphic;
        public JE_word poweruse;
    }

    public class JE_PowerType
    {
        public string name; /* string [30] */
        public JE_word itemgraphic;
        public JE_byte power;
        public JE_shortint speed;
        public JE_word cost;
    }

    public class JE_SpecialType
    {
        public string name; /* string [30] */
        public JE_word itemgraphic;
        public JE_byte pwr;
        public JE_byte stype;
        public JE_word wpn;
    }

    public class JE_OptionType
    {
        public string name; /* string [30] */
        public JE_byte pwr;
        public JE_word itemgraphic;
        public JE_word cost;
        public JE_byte tr, option;
        public JE_shortint opspd;
        public JE_byte ani;
        public JE_word[] gr; /* [1..20] */
        public JE_byte wport;
        public JE_word wpnum;
        public JE_byte ammo;
        public JE_boolean stop;
        public JE_byte icongr;
    }

    public class JE_ShieldType
    {
        public string name; /* string [30] */
        public JE_byte tpwr;
        public JE_byte mpwr;
        public JE_word itemgraphic;
        public JE_word cost;
    }


    public class JE_ShipType
    {
        public string name; /* string [30] */
        public JE_word shipgraphic;
        public JE_word itemgraphic;
        public JE_byte ani;
        public JE_shortint spd;
        public JE_byte dmg;
        public JE_word cost;
        public JE_byte bigshipgraphic;
    }

    /* EnemyData */
    public class JE_EnemyDatType
    {
        public JE_byte ani;
        public JE_byte[] tur; /* [1..3] */
        public JE_byte[] freq; /* [1..3] */
        public JE_shortint xmove;
        public JE_shortint ymove;
        public JE_shortint xaccel;
        public JE_shortint yaccel;
        public JE_shortint xcaccel;
        public JE_shortint ycaccel;
        public JE_integer startx;
        public JE_integer starty;
        public JE_shortint startxc;
        public JE_shortint startyc;
        public JE_byte armor;
        public JE_byte esize;
        public JE_word[] egraphic;  /* [1..20] */
        public JE_byte explosiontype;
        public JE_byte animate;       /* 0:Not Yet   1:Always   2:When Firing Only */
        public JE_byte shapebank;     /* See LEVELMAK.DOC */
        public JE_shortint xrev, yrev;
        public JE_word dgr;
        public JE_shortint dlevel;
        public JE_shortint dani;
        public JE_byte elaunchfreq;
        public JE_word elaunchtype;
        public JE_integer value;
        public JE_word eenemydie;
    }

    public static JE_WeaponPortType[] weaponPort = EmptyArray<JE_WeaponPortType>(PORT_NUM + 1);
    public static JE_WeaponType[] weapons = EmptyArray<JE_WeaponType>(WEAP_NUM + 1); /* [0..weapnum] */
    public static JE_PowerType[] powerSys = EmptyArray<JE_PowerType>(POWER_NUM + 1); /* [0..powernum] */
    public static JE_ShipType[] ships = EmptyArray<JE_ShipType>(SHIP_NUM + 1); /* [0..shipnum] */
    public static JE_OptionType[] options = EmptyArray<JE_OptionType>(OPTION_NUM + 1); /* [0..optionnum] */
    public static JE_ShieldType[] shields = EmptyArray<JE_ShieldType>(SHIELD_NUM + 1); /* [0..shieldnum] */
    public static JE_SpecialType[] special = EmptyArray<JE_SpecialType>(SPECIAL_NUM + 1); /* [0..specialnum] */
    public static JE_EnemyDatType[] enemyDat = EmptyArray<JE_EnemyDatType>(ENEMY_NUM + 1); /* [0..enemynum] */
    public static int initial_episode_num, episodeNum;
    public static JE_boolean[] episodeAvail = new JE_boolean[EPISODE_MAX];

    public static string episode_file;
    public static string cube_file;

    public static JE_longint episode1DataLoc;
    public static JE_boolean bonusLevel;
    public static JE_boolean jumpBackToEpisode1;

    public static void JE_loadItemDat()
    {
        BinaryReader f = null;

        if (episodeNum <= 3)
        {
            f = open("tyrian.hdt");
            episode1DataLoc = f.ReadInt32();
            f.BaseStream.Seek(episode1DataLoc, SeekOrigin.Begin);
        }
        else
        {
            // episode 4 stores item data in the level file
            f = open(levelFile);
            f.BaseStream.Seek(lvlPos[lvlNum - 1], SeekOrigin.Begin);
        }

        JE_word[] itemNum = f.ReadUInt16s(7);

        for (int i = 0; i < WEAP_NUM + 1; ++i)
        {
            weapons[i].drain = f.ReadUInt16();
            weapons[i].shotrepeat = f.ReadByte();
            weapons[i].multi = f.ReadByte();
            weapons[i].weapani = f.ReadUInt16();
            weapons[i].max = f.ReadByte();
            weapons[i].tx = f.ReadByte();
            weapons[i].ty = f.ReadByte();
            weapons[i].aim = f.ReadByte();
            weapons[i].attack = f.ReadBytes(8);
            weapons[i].del = f.ReadBytes(8);
            weapons[i].sx = f.ReadSBytes(8);
            weapons[i].sy = f.ReadSBytes(8);
            weapons[i].bx = f.ReadSBytes(8);
            weapons[i].by = f.ReadSBytes(8);
            weapons[i].sg = f.ReadUInt16s(8);
            weapons[i].acceleration = f.ReadSByte();
            weapons[i].accelerationx = f.ReadSByte();
            weapons[i].circlesize = f.ReadByte();
            weapons[i].sound = f.ReadByte();
            weapons[i].trail = f.ReadByte();
            weapons[i].shipblastfilter = f.ReadByte();
        }

#if TYRIAN2000
        if (episodeNum <= 3) f.BaseStream.Seek(0x252A4, SeekOrigin.Begin);
        if (episodeNum == 4) f.BaseStream.Seek(0xC1F5E, SeekOrigin.Begin);
        if (episodeNum == 5) f.BaseStream.Seek(0x5C5B8, SeekOrigin.Begin);
#endif

        for (int i = 0; i < PORT_NUM + 1; ++i)
        {
            f.BaseStream.Seek(1, SeekOrigin.Current); /* skip string length */
            weaponPort[i].name = new string(f.ReadChars(30));
            weaponPort[i].opnum = f.ReadByte();
            for (int j = 0; j < 2; ++j)
            {
                weaponPort[i].op[j] = f.ReadUInt16s(11);
            }
            weaponPort[i].cost = f.ReadUInt16();
            weaponPort[i].itemgraphic = f.ReadUInt16();
            weaponPort[i].poweruse = f.ReadUInt16();
        }

        int specials_count = SPECIAL_NUM;
#if TYRIAN2000
        if (episodeNum <= 3) f.BaseStream.Seek(0x2662E, SeekOrigin.Begin);
        if (episodeNum == 4) f.BaseStream.Seek(0xC32E8, SeekOrigin.Begin);
        if (episodeNum == 5) f.BaseStream.Seek(0x5D942, SeekOrigin.Begin);
        if (episodeNum >= 4) {
            specials_count = SPECIAL_NUM + 8;
            special = EmptyArray<JE_SpecialType>(SPECIAL_NUM + 8 + 1);
        }; /*this ugly hack will need a fix*/
#endif

        for (int i = 0; i < specials_count + 1; ++i)
        {
            f.BaseStream.Seek(1, SeekOrigin.Current); /* skip string length */
            special[i].name = new string(f.ReadChars(30));
            special[i].itemgraphic = f.ReadUInt16();
            special[i].pwr = f.ReadByte();
            special[i].stype = f.ReadByte();
            special[i].wpn = f.ReadUInt16();
        }

#if TYRIAN2000
        if (episodeNum <= 3) f.BaseStream.Seek(0x26E21, SeekOrigin.Begin);
        if (episodeNum == 4) f.BaseStream.Seek(0xC3ADB, SeekOrigin.Begin);
        if (episodeNum == 5) f.BaseStream.Seek(0x5E135, SeekOrigin.Begin);
#endif

        for (int i = 0; i < POWER_NUM + 1; ++i)
        {
            f.BaseStream.Seek(1, SeekOrigin.Current); /* skip string length */
            powerSys[i].name = new string(f.ReadChars(30));
            powerSys[i].itemgraphic = f.ReadUInt16();
            powerSys[i].power = f.ReadByte();
            powerSys[i].speed = f.ReadSByte();
            powerSys[i].cost = f.ReadUInt16();
        }

#if TYRIAN2000
        if (episodeNum <= 3) f.BaseStream.Seek(0x26F24, SeekOrigin.Begin);
        if (episodeNum == 4) f.BaseStream.Seek(0xC3BDE, SeekOrigin.Begin);
        if (episodeNum == 5) f.BaseStream.Seek(0x5E238, SeekOrigin.Begin);
#endif

        for (int i = 0; i < SHIP_NUM + 1; ++i)
        {
            f.BaseStream.Seek(1, SeekOrigin.Current); /* skip string length */
            ships[i].name = new string(f.ReadChars(30));
            ships[i].shipgraphic = f.ReadUInt16();
            ships[i].itemgraphic = f.ReadUInt16();
            ships[i].ani = f.ReadByte();
            ships[i].spd = f.ReadSByte();
            ships[i].dmg = f.ReadByte();
            ships[i].cost = f.ReadUInt16();
            ships[i].bigshipgraphic = f.ReadByte();
        }

#if TYRIAN2000
        if (episodeNum <= 3) f.BaseStream.Seek(0x2722F, SeekOrigin.Begin);
        if (episodeNum == 4) f.BaseStream.Seek(0xC3EE9, SeekOrigin.Begin);
        if (episodeNum == 5) f.BaseStream.Seek(0x5E543, SeekOrigin.Begin);
#endif

        for (int i = 0; i < OPTION_NUM + 1; ++i)
        {
            f.BaseStream.Seek(1, SeekOrigin.Current); /* skip string length */
            options[i].name = new string(f.ReadChars(30));
            options[i].pwr = f.ReadByte();
            options[i].itemgraphic = f.ReadUInt16();
            options[i].cost = f.ReadUInt16();
            options[i].tr = f.ReadByte();
            options[i].option = f.ReadByte();
            options[i].opspd = f.ReadSByte();
            options[i].ani = f.ReadByte();
            options[i].gr = f.ReadUInt16s(20);
            options[i].wport = f.ReadByte();
            options[i].wpnum = f.ReadUInt16();
            options[i].ammo = f.ReadByte();
            options[i].stop = f.ReadBoolean();
            options[i].icongr = f.ReadByte();
        }

#if TYRIAN2000
        if (episodeNum <= 3) f.BaseStream.Seek(0x27EF3, SeekOrigin.Begin);
        if (episodeNum == 4) f.BaseStream.Seek(0xC4BAD, SeekOrigin.Begin);
        if (episodeNum == 5) f.BaseStream.Seek(0x5F207, SeekOrigin.Begin);
#endif

        for (int i = 0; i < SHIELD_NUM + 1; ++i)
        {
            f.BaseStream.Seek(1, SeekOrigin.Current); /* skip string length */
            shields[i].name = new string(f.ReadChars(30));
            shields[i].tpwr = f.ReadByte();
            shields[i].mpwr = f.ReadByte();
            shields[i].itemgraphic = f.ReadUInt16();
            shields[i].cost = f.ReadUInt16();
        }

        for (int i = 0; i < ENEMY_NUM + 1; ++i)
        {
            enemyDat[i].ani = f.ReadByte();
            enemyDat[i].tur = f.ReadBytes(3);
            enemyDat[i].freq = f.ReadBytes(3);
            enemyDat[i].xmove = f.ReadSByte();
            enemyDat[i].ymove = f.ReadSByte();
            enemyDat[i].xaccel = f.ReadSByte();
            enemyDat[i].yaccel = f.ReadSByte();
            enemyDat[i].xcaccel = f.ReadSByte();
            enemyDat[i].ycaccel = f.ReadSByte();
            enemyDat[i].startx = f.ReadInt16();
            enemyDat[i].starty = f.ReadInt16();
            enemyDat[i].startxc = f.ReadSByte();
            enemyDat[i].startyc = f.ReadSByte();
            enemyDat[i].armor = f.ReadByte();
            enemyDat[i].esize = f.ReadByte();
            enemyDat[i].egraphic = f.ReadUInt16s(20);
            enemyDat[i].explosiontype = f.ReadByte();
            enemyDat[i].animate = f.ReadByte();
            enemyDat[i].shapebank = f.ReadByte();
            enemyDat[i].xrev = f.ReadSByte();
            enemyDat[i].yrev = f.ReadSByte();
            enemyDat[i].dgr = f.ReadUInt16();
            enemyDat[i].dlevel = f.ReadSByte();
            enemyDat[i].dani = f.ReadSByte();
            enemyDat[i].elaunchfreq = f.ReadByte();
            enemyDat[i].elaunchtype = f.ReadUInt16();
            enemyDat[i].value = f.ReadInt16();
            enemyDat[i].eenemydie = f.ReadUInt16();
        }

        f.Close();
    }

    public static void JE_initEpisode(int newEpisode)
    {
        if (newEpisode == episodeNum)
            return;

        episodeNum = newEpisode;

        levelFile = "tyrian" + episodeNum + ".lvl";
        cube_file = "cubetxt" + episodeNum + ".dat";
        episode_file = "levels" + episodeNum + ".dat";

        JE_analyzeLevel();
        JE_loadItemDat();
    }

    public static void JE_scanForEpisodes()
    {
        for (int i = 0; i < EPISODE_MAX; ++i)
        {
            string ep_file = "tyrian" + (i + 1) + ".lvl";
            episodeAvail[i] = fileExists(ep_file);
        }
    }

    public static int JE_findNextEpisode()
    {
        int newEpisode = episodeNum;

        jumpBackToEpisode1 = false;

        while (true)
        {
            newEpisode++;

            if (newEpisode > EPISODE_MAX)
            {
                newEpisode = 1;
                jumpBackToEpisode1 = true;
                gameHasRepeated = true;
            }

            if (episodeAvail[newEpisode - 1] || newEpisode == episodeNum)
            {
                break;
            }
        }

        return newEpisode;
    }
}