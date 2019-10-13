using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static NortsongC;
using static LoudnessC;
using static VarzC;
using static PlayerC;
using static EpisodesC;
using static FileIO;
using static LibC;

using UnityEngine;
using System.IO;

public static class ConfigC
{
    public const int SAVE_FILES_NUM = (11 * 2);

    /* These are necessary because the size of the structure has changed from the original, but we
       need to know the original sizes in order to find things in TYRIAN.SAV */
    public const int SAVE_FILES_SIZE = 2398;
    public const int SIZEOF_SAVEGAMETEMP = SAVE_FILES_SIZE + 4 + 100;
    public const int SAVE_FILE_SIZE = (SIZEOF_SAVEGAMETEMP - 4);

    public static JE_boolean[] smoothies = /* [1..9] */
    { false, false, false, false, false, false, false, false, false };

    public static JE_byte starShowVGASpecialCode;

    /* CubeData */
    public static int lastCubeMax, cubeMax;
    public static int[] cubeList = new int[4]; /* [1..4] */

    /* High-Score Stuff */
    public static JE_boolean gameHasRepeated;  // can only get highscore on first play-through

    /* Difficulty */
    public static int difficultyLevel, oldDifficultyLevel,
                initialDifficulty;  // can only get highscore on initial episode

    /* Player Stuff */
    public static int power, lastPower, powerAdd;
    public static JE_byte shieldWait, shieldT;

    public const int SHOT_FRONT = 0,
    SHOT_REAR = 1,
    SHOT_LEFT_SIDEKICK = 2,
    SHOT_RIGHT_SIDEKICK = 3,
    SHOT_MISC = 4,
    SHOT_P2_CHARGE = 5,
    SHOT_P1_SUPERBOMB = 6,
    SHOT_P2_SUPERBOMB = 7,
    SHOT_SPECIAL = 8,
    SHOT_NORTSPARKS = 9,
    SHOT_SPECIAL2 = 10;

    public static JE_byte[] shotRepeat = new JE_byte[11], shotMultiPos = new JE_byte[11];
    public static JE_boolean portConfigChange, portConfigDone;

    /* Level Data */
    public static string lastLevelName = "", levelName = ""; /* string [10] */
    public static int mainLevel, nextLevel, saveLevel;   /*Current Level #*/

    /* Keyboard Junk */
    public static KeyCode[] keySettings = new KeyCode[8];

    /* Configuration */
    public static int levelFilter, levelFilterNew, levelBrightness, levelBrightnessChg;
    public static JE_boolean filtrationAvail, filterActive, filterFade, filterFadeStart;

    public static JE_boolean gameJustLoaded;

    public static JE_boolean galagaMode;

    public static JE_boolean extraGame;

    public static JE_boolean twoPlayerMode, twoPlayerLinked, onePlayerAction, superTyrian;
    public static JE_boolean trentWin = false;
    public static int superArcadeMode;

    public static JE_byte superArcadePowerUp;

    public static JE_real linkGunDirec;
    public static JE_byte[] inputDevice = { 1, 2 }; // 0:any  1:keyboard  2:mouse  3+:joystick

    public static JE_byte secretHint;
    public static JE_byte background3over;
    public static JE_byte background2over;
    public static JE_byte gammaCorrection;
    public static JE_boolean superPause = false;
    public static JE_boolean explosionTransparent,
               youAreCheating,
               displayScore,
               background2, smoothScroll, wild, superWild, starActive,
               topEnemyOver,
               skyEnemyOverAll,
               background2notTransparent;

    public static JE_byte soundEffects; // dummy value for config
    public static JE_byte versionNum;   /* SW 1.0 and SW/Reg 1.1 = 0 or 1
                       * EA 1.2 = 2 */

    public static JE_byte fastPlay;
    public static JE_boolean pentiumMode;

    /* Savegame files */
    public static JE_byte gameSpeed;
    public static JE_byte processorType;  /* 1=386 2=486 3=Pentium Hyper */

    public class JE_SaveFileType
    {

        public JE_word encode;
        public JE_word level;
        public JE_byte[] items = new JE_byte[12];
        public JE_longint score;
        public JE_longint score2;
        public string levelName = ""; /* string [9]; */ /* SYN: Added one more byte to match lastLevelName below */
        public string name = ""; /* [1..14] */ /* SYN: Added extra byte for null */
        public JE_byte cubes;
        public JE_byte[] power = new JE_byte[2]; /* [1..2] */
        public JE_byte episode;
        public JE_byte[] lastItems = new JE_byte[12];
        public JE_byte difficulty;
        public JE_byte secretHint;
        public JE_byte input1;
        public JE_byte input2;
        public JE_boolean gameHasRepeated; /*See if you went from one episode to another*/
        public JE_byte initialDifficulty;

        /* High Scores - Each episode has both sets of 1&2 player selections - with 3 in each */
        public JE_longint highScore1,
                          highScore2;
        public string highScoreName; /* string [29] */
        public JE_byte highScoreDiff;

        public JE_SaveFileType Clone()
        {
            JE_SaveFileType ret = (JE_SaveFileType)MemberwiseClone();
            ret.items = new JE_byte[items.Length];
            ret.power = new JE_byte[power.Length];
            ret.lastItems = new JE_byte[lastItems.Length];
            System.Array.Copy(items, ret.items, items.Length);
            System.Array.Copy(power, ret.power, power.Length);
            System.Array.Copy(lastItems, ret.lastItems, lastItems.Length);
            return ret;
        }
    }

    public static JE_SaveFileType[] saveFiles = EmptyArray<JE_SaveFileType>(SAVE_FILES_NUM); /*array[1..saveLevelnum] of savefiletype;*/
    public static byte[] saveTemp = new byte[SAVE_FILES_SIZE + 4 + 100];

    public static JE_word editorLevel = 800;

    static readonly JE_byte[] cryptKey = /* [1..10] */
    {
        15, 50, 89, 240, 147, 34, 86, 9, 32, 208
    };

    public static readonly KeyCode[] defaultKeySettings =
    {
        KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Space, KeyCode.Return, KeyCode.LeftControl, KeyCode.LeftAlt
    };

    static readonly string[] defaultHighScoreNames = /* [1..34] of string [22] */
{/*1P*/
/*TYR*/   "The Prime Chair", /*13*/
          "Transon Lohk",
          "Javi Onukala",
          "Mantori",
          "Nortaneous",
          "Dougan",
          "Reid",
          "General Zinglon",
          "Late Gyges Phildren",
          "Vykromod",
          "Beppo",
          "Borogar",
          "ShipMaster Carlos",

/*OTHER*/ "Jill", /*5*/
          "Darcy",
          "Jake Stone",
          "Malvineous Havershim",
          "Marta Louise Velasquez",

/*JAZZ*/  "Jazz Jackrabbit", /*3*/
          "Eva Earlong",
          "Devan Shell",

/*OMF*/   "Crystal Devroe", /*11*/
          "Steffan Tommas",
          "Milano Angston",
          "Christian",
          "Shirro",
          "Jean-Paul",
          "Ibrahim Hothe",
          "Angel",
          "Cossette Akira",
          "Raven",
          "Hans Kreissack",

/*DARE*/  "Tyler", /*2*/
          "Rennis the Rat Guard"
};

    static readonly string[] defaultTeamNames = /* [1..22] of string [24] */
    {
    "Jackrabbits",
    "Team Tyrian",
    "The Elam Brothers",
    "Dare to Dream Team",
    "Pinball Freaks",
    "Extreme Pinball Freaks",
    "Team Vykromod",
    "Epic All-Stars",
    "Hans Keissack's WARriors",
    "Team Overkill",
    "Pied Pipers",
    "Gencore Growlers",
    "Microsol Masters",
    "Beta Warriors",
    "Team Loco",
    "The Shellians",
    "Jungle Jills",
    "Murderous Malvineous",
    "The Traffic Department",
    "Clan Mikal",
    "Clan Patrok",
    "Carlos' Crawlers"
};


    static readonly byte[] initialItemAvail =
    {
    1,1,1,0,0,1,1,0,1,1,1,1,1,0,1,0,1,1,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0, /* Front/Rear Weapons 1-38  */
	0,0,0,0,0,0,0,0,0,0,1,                                                           /* Fill                     */
	1,0,0,0,0,1,0,0,0,1,1,0,1,0,0,0,0,0,                                             /* Sidekicks          51-68 */
	0,0,0,0,0,0,0,0,0,0,0,                                                           /* Fill                     */
	1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,                                                   /* Special Weapons    81-93 */
	0,0,0,0,0                                                                        /* Fill                     */
};

    private static void playeritems_to_pitems(JE_byte[] pItems, PlayerItems items, int initial_episode_num)
    {
        pItems[0]  = (byte)items.weapon[FRONT_WEAPON].id;
        pItems[1]  = (byte)items.weapon[REAR_WEAPON].id;
        pItems[2]  = (byte)items.super_arcade_mode;
        pItems[3]  = (byte)items.sidekick[LEFT_SIDEKICK];
        pItems[4]  = (byte)items.sidekick[RIGHT_SIDEKICK];
        pItems[5]  = (byte)items.generator;
        pItems[6]  = (byte)items.sidekick_level;
        pItems[7]  = (byte)items.sidekick_series;
        pItems[8]  = (byte)initial_episode_num;
        pItems[9]  = (byte)items.shield;
        pItems[10] = (byte)items.special;
        pItems[11] = (byte)items.ship;
    }

    private static void pitems_to_playeritems(PlayerItems items, JE_byte[] pItems, out int initial_episode_num)
    {
        items.weapon[FRONT_WEAPON].id = pItems[0];
        items.weapon[REAR_WEAPON].id = pItems[1];
        items.super_arcade_mode = pItems[2];
        items.sidekick[LEFT_SIDEKICK] = pItems[3];
        items.sidekick[RIGHT_SIDEKICK] = pItems[4];
        items.generator = pItems[5];
        items.sidekick_level = pItems[6];
        items.sidekick_series = pItems[7];
        initial_episode_num = pItems[8];
        items.shield = pItems[9];
        items.special = pItems[10];
        items.ship = pItems[11];
    }
    public static void JE_saveGame(JE_byte slot, string name)
    {

        saveFiles[slot - 1].initialDifficulty = (byte)initialDifficulty;
        saveFiles[slot - 1].gameHasRepeated = gameHasRepeated;
        saveFiles[slot - 1].level = (JE_word)saveLevel;

        if (superTyrian)
            player[0].items.super_arcade_mode = SA_SUPERTYRIAN;
        else if (superArcadeMode == SA_NONE && onePlayerAction)
            player[0].items.super_arcade_mode = SA_ARCADE;
        else
            player[0].items.super_arcade_mode = (JE_word)superArcadeMode;

        playeritems_to_pitems(saveFiles[slot - 1].items, player[0].items, initial_episode_num);

        if (twoPlayerMode)
            playeritems_to_pitems(saveFiles[slot - 1].lastItems, player[1].items, 0);
        else
            playeritems_to_pitems(saveFiles[slot - 1].lastItems, player[0].last_items, 0);

        saveFiles[slot - 1].score = player[0].cash;
        saveFiles[slot - 1].score2 = player[1].cash;

        saveFiles[slot - 1].levelName = lastLevelName;
        saveFiles[slot - 1].cubes = (byte)lastCubeMax;

        if (lastLevelName == "Completed")
        {
            temp = episodeNum - 1;
            if (temp < 1)
            {
                temp = EPISODE_AVAILABLE; /* JE: {Episodemax is 4 for completion purposes} */
            }
            saveFiles[slot - 1].episode = (byte)temp;
        }
        else
        {
            saveFiles[slot - 1].episode = (byte)episodeNum;
        }

        saveFiles[slot - 1].difficulty = (byte)difficultyLevel;
        saveFiles[slot - 1].secretHint = secretHint;
        saveFiles[slot - 1].input1 = inputDevice[0];
        saveFiles[slot - 1].input2 = inputDevice[1];

        saveFiles[slot - 1].name = name;

        for (uint port = 0; port < 2; ++port)
        {
            // if two-player, use first player's front and second player's rear weapon
            saveFiles[slot - 1].power[port] = (byte)(player[twoPlayerMode ? port : 0].items.weapon[port].power);
        }

        JE_saveConfiguration();
    }

    public static void JE_loadGame(JE_byte slot)
    {
        superTyrian = false;
        onePlayerAction = false;
        twoPlayerMode = false;
        extraGame = false;
        galagaMode = false;

        initialDifficulty = saveFiles[slot - 1].initialDifficulty;
        gameHasRepeated = saveFiles[slot - 1].gameHasRepeated;
        twoPlayerMode = (slot - 1) > 10;
        difficultyLevel = saveFiles[slot - 1].difficulty;

        pitems_to_playeritems(player[0].items, saveFiles[slot - 1].items, out initial_episode_num);

        superArcadeMode = player[0].items.super_arcade_mode;

        if (superArcadeMode == SA_SUPERTYRIAN)
            superTyrian = true;
        if (superArcadeMode != SA_NONE)
            onePlayerAction = true;
        if (superArcadeMode > SA_NORTSHIPZ)
            superArcadeMode = SA_NONE;

        int ignore;
        if (twoPlayerMode)
        {
            onePlayerAction = false;

            pitems_to_playeritems(player[1].items, saveFiles[slot - 1].lastItems, out ignore);
        }
        else
        {
            pitems_to_playeritems(player[0].last_items, saveFiles[slot - 1].lastItems, out ignore);
        }

        /* Compatibility with old version */
        if (player[1].items.sidekick_level < 101)
        {
            player[1].items.sidekick_level = 101;
            player[1].items.sidekick_series = player[1].items.sidekick[LEFT_SIDEKICK];
        }

        player[0].cash = saveFiles[slot - 1].score;
        player[1].cash = saveFiles[slot - 1].score2;

        mainLevel = saveFiles[slot - 1].level;
        cubeMax = saveFiles[slot - 1].cubes;
        lastCubeMax = cubeMax;

        secretHint = saveFiles[slot - 1].secretHint;
        inputDevice[0] = saveFiles[slot - 1].input1;
        inputDevice[1] = saveFiles[slot - 1].input2;

        for (uint port = 0; port < 2; ++port)
        {
            // if two-player, use first player's front and second player's rear weapon
            player[twoPlayerMode ? port : 0].items.weapon[port].power = saveFiles[slot - 1].power[port];
        }

        int episode = saveFiles[slot - 1].episode;

        levelName = saveFiles[slot - 1].levelName;

        if (levelName == "Completed")
        {
            if (episode == EPISODE_AVAILABLE)
            {
                episode = 1;
            }
            else if (episode < EPISODE_AVAILABLE)
            {
                episode++;
            }
            /* Increment episode.  Episode EPISODE_AVAILABLE goes to 1. */
        }

        JE_initEpisode(episode);
        saveLevel = mainLevel;
        lastLevelName = levelName;
    }

    public static void JE_initProcessorType()
    {
        /* SYN: Originally this proc looked at your hardware specs and chose appropriate options. We don't care, so I'll just set
           decent defaults here. */

        wild = false;
        superWild = false;
        smoothScroll = true;
        explosionTransparent = true;
        filtrationAvail = false;
        background2 = true;
        displayScore = true;

        switch (processorType)
        {
            case 1: /* 386 */
                background2 = false;
                displayScore = false;
                explosionTransparent = false;
                break;
            case 2: /* 486 - Default */
                break;
            case 3: /* High Detail */
                smoothScroll = false;
                break;
            case 4: /* Pentium */
                wild = true;
                filtrationAvail = true;
                break;
            case 5: /* Nonstandard VGA */
                smoothScroll = false;
                break;
            case 6: /* SuperWild */
                wild = true;
                superWild = true;
                filtrationAvail = true;
                break;
        }

        switch (gameSpeed)
        {
            case 1:  /* Slug Mode */
                fastPlay = 3;
                break;
            case 2:  /* Slower */
                fastPlay = 4;
                break;
            case 3: /* Slow */
                fastPlay = 5;
                break;
            case 4: /* Normal */
                fastPlay = 0;
                break;
            case 5: /* Pentium Hyper */
                fastPlay = 1;
                break;
        }

    }

    public static void JE_setNewGameSpeed()
    {
        pentiumMode = false;
        Application.targetFrameRate = 30;

        switch (fastPlay)
        {
            case 0: //Normal
                speed = 0x4300;
                smoothScroll = true;
                frameCountMax = 2;
                break;
            case 1: //Turbo
                Application.targetFrameRate = 100;
                speed = 0x3000;
                smoothScroll = true;
                frameCountMax = 2;
                break;
            case 2: //HYPER SPEED
                Application.targetFrameRate = 999;
                speed = 0x2000;
                smoothScroll = false;
                frameCountMax = 2;
                break;
            case 3: //Slug Mode
                speed = 0x5300;
                smoothScroll = true;
                frameCountMax = 4;
                break;
            case 4: //Slower
                speed = 0x4300;
                smoothScroll = true;
                frameCountMax = 3;
                break;
            case 5: //Slow
                speed = 0x4300;
                smoothScroll = true;
                frameCountMax = 2;
                pentiumMode = true;
                break;
        }

        frameCount = frameCountMax;
        JE_resetTimerInt();
        JE_setTimerInt();
    }

    private static void JE_encryptSaveTemp()
    {
        byte[] s3 = new byte[saveTemp.Length];
        JE_word x;
        JE_byte y;

        System.Array.Copy(saveTemp, s3, s3.Length);

        y = 0;
        for (x = 0; x < SAVE_FILE_SIZE; x++)
        {
            y += s3[x];
        }
        saveTemp[SAVE_FILE_SIZE] = y;

        y = 0;
        for (x = 0; x < SAVE_FILE_SIZE; x++)
        {
            y -= s3[x];
        }
        saveTemp[SAVE_FILE_SIZE + 1] = y;

        y = 1;
        for (x = 0; x < SAVE_FILE_SIZE; x++)
        {
            y = (byte)((y * s3[x]) + 1);
        }
        saveTemp[SAVE_FILE_SIZE + 2] = y;

        y = 0;
        for (x = 0; x < SAVE_FILE_SIZE; x++)
        {
            y = (byte)(y ^ s3[x]);
        }
        saveTemp[SAVE_FILE_SIZE + 3] = y;

        for (x = 0; x < SAVE_FILE_SIZE; x++)
        {
            saveTemp[x] = (byte)(saveTemp[x] ^ cryptKey[(x + 1) % 10]);
            if (x > 0)
            {
                saveTemp[x] = (byte)(saveTemp[x] ^ saveTemp[x - 1]);
            }
        }
    }

    private static void JE_decryptSaveTemp()
    {
        JE_boolean correct = true;
        byte[] s2 = new JE_byte[saveTemp.Length];
        int x;
        JE_byte y;

        /* Decrypt save game file */
        for (x = (SAVE_FILE_SIZE - 1); x >= 0; x--)
        {
            s2[x] = (byte)((JE_byte)saveTemp[x] ^ (JE_byte)(cryptKey[(x + 1) % 10]));
            if (x > 0)
            {
                s2[x] ^= (JE_byte)saveTemp[x - 1];
            }

        }

        /* for (x = 0; x < SAVE_FILE_SIZE; x++) printf("%c", s2[x]); */

        /* Check save file for correctitude */
        y = 0;
        for (x = 0; x < SAVE_FILE_SIZE; x++)
        {
            y += s2[x];
        }
        if (saveTemp[SAVE_FILE_SIZE] != y)
        {
            correct = false;
            Debug.LogWarning(string.Format("Failed additive checksum: %d vs %d", saveTemp[SAVE_FILE_SIZE], y));
        }

        y = 0;
        for (x = 0; x < SAVE_FILE_SIZE; x++)
        {
            y -= s2[x];
        }
        if (saveTemp[SAVE_FILE_SIZE + 1] != y)
        {
            correct = false;
            Debug.LogWarning(string.Format("Failed subtractive checksum: %d vs %d", saveTemp[SAVE_FILE_SIZE + 1], y));
        }

        y = 1;
        for (x = 0; x < SAVE_FILE_SIZE; x++)
        {
            y = (byte)((y * s2[x]) + 1);
        }
        if (saveTemp[SAVE_FILE_SIZE + 2] != y)
        {
            correct = false;
            Debug.LogWarning(string.Format("Failed multiplicative checksum: %d vs %d", saveTemp[SAVE_FILE_SIZE + 2], y));
        }

        y = 0;
        for (x = 0; x < SAVE_FILE_SIZE; x++)
        {
            y = (byte)(y ^ s2[x]);
        }
        if (saveTemp[SAVE_FILE_SIZE + 3] != y)
        {
            correct = false;
            Debug.LogWarning(string.Format("Failed XOR'd checksum: %d vs %d", saveTemp[SAVE_FILE_SIZE + 3], y));
        }

        /* Barf and die if save file doesn't validate */
        if (!correct)
        {
            throw new System.Exception("Error reading save file!");
        }

        /* Keep decrypted version plz */
        System.Array.Copy(s2, saveTemp, s2.Length);
    }

    public static void JE_loadConfiguration()
    {
        BinaryReader fi;
        int z;
        JE_byte[] p;
        int y;

        //fi = dir_fopen_warn(get_user_directory(), "tyrian.cfg", "rb");
        //if (fi && ftell_eof(fi) == 20 + sizeof(keySettings))
        //{
        //    /* SYN: I've hardcoded the sizes here because the .CFG file format is fixed
        //       anyways, so it's not like they'll change. */
        //    background2 = 0;
        //    efread(&background2, 1, 1, fi);
        //    efread(&gameSpeed, 1, 1, fi);

        //    efread(&inputDevice_, 1, 1, fi);
        //    efread(&jConfigure, 1, 1, fi);

        //    efread(&versionNum, 1, 1, fi);

        //    efread(&processorType, 1, 1, fi);
        //    efread(&midiPort, 1, 1, fi);
        //    efread(&soundEffects, 1, 1, fi);
        //    efread(&gammaCorrection, 1, 1, fi);
        //    efread(&difficultyLevel, 1, 1, fi);

        //    efread(joyButtonAssign, 1, 4, fi);

        //    efread(&tyrMusicVolume, 2, 1, fi);
        //    efread(&fxVolume, 2, 1, fi);

        //    efread(inputDevice, 1, 2, fi);

        //    efread(keySettings, sizeof(*keySettings), COUNTOF(keySettings), fi);

        //    fclose(fi);
        //}
        //else
        {
            //printf("\nInvalid or missing TYRIAN.CFG! Continuing using defaults.\n\n");

            soundEffects = 1;

            System.Array.Copy(defaultKeySettings, keySettings, keySettings.Length);
            background2 = true;
            tyrMusicVolume = fxVolume = 128;
            gammaCorrection = 0;
            processorType = 4;
            gameSpeed = 4;
        }

        if (tyrMusicVolume > 255)
            tyrMusicVolume = 255;
        if (fxVolume > 255)
            fxVolume = 255;

        JE_calcFXVol();

        set_volume(tyrMusicVolume, fxVolume);


        fi = openData("tyrian.sav");
        if (fi != null)
        {
            saveTemp = fi.ReadBytes(saveTemp.Length);
            fi.Close();
            JE_decryptSaveTemp();
            fi = new BinaryReader(new MemoryStream(saveTemp));

            /* SYN: The original mostly blasted the save file into raw memory. However, our lives are not so
               easy, because the C struct is necessarily a different size. So instead we have to loop
               through each record and load fields manually. *emo tear* :'( */

            p = saveTemp;
            for (z = 0; z < SAVE_FILES_NUM; z++)
            {
                saveFiles[z].encode = fi.ReadUInt16();
                //saveFiles[z].encode = SDL_SwapLE16(saveFiles[z].encode);

                saveFiles[z].level = fi.ReadUInt16();
                //saveFiles[z].level = SDL_SwapLE16(saveFiles[z].level);

                saveFiles[z].items = fi.ReadBytes(saveFiles[z].items.Length);

                saveFiles[z].score = fi.ReadInt32();
                //saveFiles[z].score = SDL_SwapLE32(saveFiles[z].score);

                saveFiles[z].score2 = fi.ReadInt32();
                //saveFiles[z].score2 = SDL_SwapLE32(saveFiles[z].score2);

                /* SYN: Pascal strings are prefixed by a byte holding the length! */
                int len = fi.ReadByte();
                byte[] levelName = fi.ReadBytes(9);
                saveFiles[z].levelName = System.Text.Encoding.ASCII.GetString(levelName, 0, len);

                /* This was a BYTE array, not a STRING, in the original. Go fig. */
                byte[] name = fi.ReadBytes(14);
                saveFiles[z].name = System.Text.Encoding.ASCII.GetString(name);


                saveFiles[z].cubes = fi.ReadByte();
                saveFiles[z].power = fi.ReadBytes(2);
                saveFiles[z].episode = fi.ReadByte();
                saveFiles[z].lastItems = fi.ReadBytes(saveFiles[z].lastItems.Length);
                saveFiles[z].difficulty = fi.ReadByte();
                saveFiles[z].secretHint = fi.ReadByte();
                saveFiles[z].input1 = fi.ReadByte();
                saveFiles[z].input2 = fi.ReadByte();
                saveFiles[z].gameHasRepeated = fi.ReadBoolean();

                saveFiles[z].initialDifficulty = fi.ReadByte();

                saveFiles[z].highScore1 = fi.ReadInt32();
                //saveFiles[z].highScore1 = SDL_SwapLE32(saveFiles[z].highScore1);

                saveFiles[z].highScore2 = fi.ReadInt32();
                //saveFiles[z].highScore2 = SDL_SwapLE32(saveFiles[z].highScore2);

                len = fi.ReadByte();
                byte[] highScoreName = fi.ReadBytes(29);
                saveFiles[z].highScoreName = System.Text.Encoding.ASCII.GetString(highScoreName, 0, len);

                saveFiles[z].highScoreDiff = fi.ReadByte();
            }

            fi.BaseStream.Seek(-6, SeekOrigin.End);
            int low = fi.ReadByte();
            int high = fi.ReadByte();

            /* SYN: This is truncating to bytes. I have no idea what this is doing or why. */
            /* TODO: Figure out what this is about and make sure it isn't broked. */
            editorLevel = (JE_word)((high << 8) | low);

            fi.Close();
        } else {
            /* We didn't have a save file! Let's make up random stuff! */
            editorLevel = 800;

            for (z = 0; z < 100; z++)
            {
                saveTemp[SAVE_FILES_SIZE + z] = initialItemAvail[z];
            }

            for (z = 0; z < SAVE_FILES_NUM; z++)
            {
                saveFiles[z].level = 0;

                saveFiles[z].name = "             ";

                saveFiles[z].highScore1 = (int)((mt_rand() % 20) + 1) * 1000;

                if (z % 6 > 2)
                {
                    saveFiles[z].highScore2 = (int)((mt_rand() % 20) + 1) * 1000;
                    saveFiles[z].highScoreName = defaultTeamNames[mt_rand() % 22];
                }
                else
                {
                    saveFiles[z].highScoreName = defaultHighScoreNames[mt_rand() % 34];
                }
            }
        }

        JE_initProcessorType();
    }

    public static void JE_saveConfiguration()
    {
        BinaryWriter f = new BinaryWriter(new MemoryStream(saveTemp));
        int z;

        for (z = 0; z < SAVE_FILES_NUM; z++)
        {
            JE_SaveFileType tempSaveFile = saveFiles[z].Clone();

            //tempSaveFile.encode = SDL_SwapLE16(tempSaveFile.encode);
            f.Write(tempSaveFile.encode);

            //tempSaveFile.level = SDL_SwapLE16(tempSaveFile.level);
            f.Write(tempSaveFile.level);

            f.Write(tempSaveFile.items);

            //tempSaveFile.score = SDL_SwapLE32(tempSaveFile.score);
            f.Write(tempSaveFile.score);

            //tempSaveFile.score2 = SDL_SwapLE32(tempSaveFile.score2);
            f.Write(tempSaveFile.score2);

            /* SYN: Pascal strings are prefixed by a byte holding the length! */
            byte[] levelName = new byte[9];
            byte len = (byte)System.Text.Encoding.ASCII.GetBytes(tempSaveFile.levelName, 0, tempSaveFile.levelName.Length, levelName, 0);
            f.Write(len);
            f.Write(levelName);

            /* This was a BYTE array, not a STRING, in the original. Go fig. */
            byte[] name = new byte[14];
            System.Text.Encoding.ASCII.GetBytes(tempSaveFile.name, 0, tempSaveFile.name.Length, name, 0);
            f.Write(name);

            f.Write(tempSaveFile.cubes);
            f.Write(tempSaveFile.power);
            f.Write(tempSaveFile.episode);
            f.Write(tempSaveFile.lastItems);
            f.Write(tempSaveFile.difficulty);
            f.Write(tempSaveFile.secretHint);
            f.Write(tempSaveFile.input1);
            f.Write(tempSaveFile.input2);

            /* booleans were 1 byte in pascal -- working around it */
            f.Write(tempSaveFile.gameHasRepeated);

            f.Write(tempSaveFile.initialDifficulty);

            //tempSaveFile.highScore1 = SDL_SwapLE32(tempSaveFile.highScore1);
            f.Write(tempSaveFile.highScore1);

            //tempSaveFile.highScore2 = SDL_SwapLE32(tempSaveFile.highScore2);
            f.Write(tempSaveFile.highScore2);

            byte[] highScoreName = new byte[29];
            len = (byte)System.Text.Encoding.ASCII.GetBytes(tempSaveFile.highScoreName, 0, tempSaveFile.highScoreName.Length, highScoreName, 0);
            f.Write(len);
            f.Write(highScoreName);

            f.Write(tempSaveFile.highScoreDiff);
        }
        f.Close();

        saveTemp[SIZEOF_SAVEGAMETEMP - 6] = (byte)(editorLevel >> 8);
        saveTemp[SIZEOF_SAVEGAMETEMP - 5] = (byte)editorLevel;

        JE_encryptSaveTemp();

        writeAllDataBytes("tyrian.sav", saveTemp);

        JE_decryptSaveTemp();

        //f = dir_fopen_warn(get_user_directory(), "tyrian.cfg", "wb");
//        if (f != NULL)
//        {
//            efwrite(&background2, 1, 1, f);
//            efwrite(&gameSpeed, 1, 1, f);

//            efwrite(&inputDevice_, 1, 1, f);
//            efwrite(&jConfigure, 1, 1, f);

//            efwrite(&versionNum, 1, 1, f);
//            efwrite(&processorType, 1, 1, f);
//            efwrite(&midiPort, 1, 1, f);
//            efwrite(&soundEffects, 1, 1, f);
//            efwrite(&gammaCorrection, 1, 1, f);
//            efwrite(&difficultyLevel, 1, 1, f);
//            efwrite(joyButtonAssign, 1, 4, f);

//            efwrite(&tyrMusicVolume, 2, 1, f);
//            efwrite(&fxVolume, 2, 1, f);

//            efwrite(inputDevice, 1, 2, f);

//            efwrite(keySettings, sizeof(*keySettings), COUNTOF(keySettings), f);

//# ifndef TARGET_WIN32
//            fsync(fileno(f));
//#endif
//            fclose(f);
//        }

//        save_opentyrian_config();
    }

}