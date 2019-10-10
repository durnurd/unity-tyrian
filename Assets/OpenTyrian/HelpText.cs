using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static VarzC;
using static FontHandC;
using static SurfaceC;
using static FileIO;
using static EpisodesC;
using static MenusC;

using static System.Math;

using System.IO;

public static class HelpTextC
{

    public const int MENU_MAX = 14;

    public const int DESTRUCT_MODES = 5;

#if TYRIAN2000
public const int HELPTEXT_MISCTEXT_COUNT = 72;
public const int HELPTEXT_MISCTEXTB_COUNT = 8;
public const int HELPTEXT_MISCTEXTB_SIZE = 12;
public const int HELPTEXT_MENUTEXT_SIZE = 29;
public const int HELPTEXT_MAINMENUHELP_COUNT = 37;
public const int HELPTEXT_NETWORKTEXT_COUNT = 5;
public const int HELPTEXT_NETWORKTEXT_SIZE = 33;
public const int HELPTEXT_SUPERSHIPS_COUNT = 13;
public const int HELPTEXT_SPECIALNAME_COUNT = 11;
public const int HELPTEXT_SHIPINFO_COUNT = 20;
public const int HELPTEXT_MENUINT3_COUNT = 9;
public const int HELPTEXT_MENUINT12_COUNT = 7;
#else
    public const int HELPTEXT_MISCTEXT_COUNT = 68;
    public const int HELPTEXT_MISCTEXTB_COUNT = 5;
    public const int HELPTEXT_MISCTEXTB_SIZE = 11;
    public const int HELPTEXT_MENUTEXT_SIZE = 21;
    public const int HELPTEXT_MAINMENUHELP_COUNT = 34;
    public const int HELPTEXT_NETWORKTEXT_COUNT = 4;
    public const int HELPTEXT_NETWORKTEXT_SIZE = 22;
    public const int HELPTEXT_SUPERSHIPS_COUNT = 11;
    public const int HELPTEXT_SPECIALNAME_COUNT = 9;
    public const int HELPTEXT_SHIPINFO_COUNT = 13;
#endif

    public static readonly JE_byte[][] menuHelp = /* [1..maxmenu, 1..11] */
{
    new JE_byte[]{  1, 34,  2,  3,  4,  5,                  0, 0, 0, 0, 0 },
    new JE_byte[]{  6,  7,  8,  9, 10, 11, 11, 12,                0, 0, 0 },
    new JE_byte[]{ 13, 14, 15, 15, 16, 17, 12,                 0, 0, 0, 0 },
    new JE_byte[]{                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    new JE_byte[]{                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    new JE_byte[]{                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    new JE_byte[]{                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    new JE_byte[]{                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    new JE_byte[]{                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    new JE_byte[]{  4, 30, 30,  3,  5,                   0, 0, 0, 0, 0, 0 },
    new JE_byte[]{                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    new JE_byte[]{ 16, 17, 15, 15, 12,                   0, 0, 0, 0, 0, 0 },
    new JE_byte[]{ 31, 31, 31, 31, 32, 12,                  0, 0, 0, 0, 0 },
    new JE_byte[]{  4, 34,  3,  5,                    0, 0, 0, 0, 0, 0, 0 }
};
    
    public static JE_byte verticalHeight = 7;
    public static JE_byte helpBoxColor = 12;
    public static JE_byte helpBoxBrightness = 1;
    public static JE_byte helpBoxShadeType = FULL_SHADE;

    public static string[] helpTxt = EmptyArray<string>(39, null);                                                   /* [1..39] of string [230] */
    public static string[] pName = EmptyArray<string>(21, null);                                                      /* [1..21] of string [15] */
    public static string[] miscText = EmptyArray<string>(HELPTEXT_MISCTEXT_COUNT, null);                              /* [1..68] of string [41] */
    public static string[] miscTextB = EmptyArray<string>(HELPTEXT_MISCTEXTB_COUNT, null);       /* [1..5] of string [10] */
    public static string[] keyName = EmptyArray<string>(8, null);                                                     /* [1..8] of string [17] */
    public static string[] menuText = EmptyArray<string>(7, null);                                /* [1..7] of string [20] */
    public static string[] outputs = EmptyArray<string>(9, null);                                                     /* [1..9] of string [30] */
    public static string[] topicName = EmptyArray<string>(6, null);                                                   /* [1..6] of string [20] */
    public static string[] mainMenuHelp = EmptyArray<string>(HELPTEXT_MAINMENUHELP_COUNT, null);                      /* [1..34] of string [65] */
    public static string[] inGameText = EmptyArray<string>(6, null);                                                  /* [1..6] of string [20] */
    public static string[] detailLevel = EmptyArray<string>(6, null);                                                 /* [1..6] of string [12] */
    public static string[] gameSpeedText = EmptyArray<string>(5, null);                                               /* [1..5] of string [12] */
    public static string[] inputDevices = EmptyArray<string>(3, null);                                                /* [1..3] of string [12] */
    public static string[] networkText = EmptyArray<string>(HELPTEXT_NETWORKTEXT_COUNT, null); /* [1..4] of string [20] */
    public static string[] difficultyNameB = EmptyArray<string>(11, null);                                            /* [0..9] of string [20] */
    public static string[] joyButtonNames = EmptyArray<string>(5, null);                                              /* [1..5] of string [20] */
    public static string[] superShips = EmptyArray<string>(HELPTEXT_SUPERSHIPS_COUNT, null);                          /* [0..10] of string [25] */
    public static string[] specialName = EmptyArray<string>(HELPTEXT_SPECIALNAME_COUNT, null);                        /* [1..9] of string [9] */
    public static string[] destructHelp = EmptyArray<string>(25, null);                                               /* [1..25] of string [21] */
    public static string[] weaponNames = EmptyArray<string>(17, null);                                                /* [1..17] of string [16] */
    public static string[] destructModeName = EmptyArray<string>(DESTRUCT_MODES, null);                               /* [1..destructmodes] of string [12] */
    public static string[][] shipInfo = DoubleEmptyArray<string>(HELPTEXT_SHIPINFO_COUNT, 2, null);                          /* [1..13, 1..2] of string */
    public static string[][] menuInt = DoubleEmptyArray<string>(MENU_MAX + 1, 11, null);                                        /* [0..14, 1..11] of string [17] */


    private static readonly byte[] crypt_key = { 204, 129, 63, 255, 71, 19, 25, 62, 1, 99 };
    public static string decrypt_pascal_string(byte[] s, int len)
    {
        for (int i = len - 1; i >= 0; --i)
        {
            s[i] ^= crypt_key[i % crypt_key.Length];
            if (i > 0)
                s[i] ^= s[i - 1];
        }
        return System.Text.Encoding.ASCII.GetString(s);
    }

    public static string read_encrypted_pascal_string(BinaryReader f)
    {
        int len = f.ReadByte();
        if (len != -1)
        {
            byte[] chars = f.ReadBytes(len);
            return decrypt_pascal_string(chars, len);
        }
        return null;
    }

    public static void skip_pascal_string(BinaryReader f)
    {
        int len = f.ReadByte();
        f.BaseStream.Seek(len, SeekOrigin.Current);
    }

    public static void JE_helpBox(Surface screen, int x, int y, string message, int boxwidth)
    {

        int startpos, endpos, pos;
        JE_boolean endstring;

        if (message.Length == 0)
        {
            return;
        }

        pos = 1;
        endpos = 0;
        endstring = false;

        do
        {
            startpos = endpos + 1;

            do
            {
                endpos = pos;
                do
                {
                    pos++;
                    if (pos == message.Length)
                    {
                        endstring = true;
                        if ((uint)(pos - startpos) < boxwidth)
                        {
                            endpos = pos;
                        }
                    }

                } while (!(message[pos - 1] == ' ' || endstring));

            } while (!((uint)(pos - startpos) > boxwidth || endstring));

            string substring = message.Substring(startpos - 1, endpos - startpos + 1);
            JE_textShade(screen, x, y, substring, helpBoxColor, helpBoxBrightness, helpBoxShadeType);

            y += verticalHeight;

        } while (!endstring);

        if (endpos != pos + 1)
        {
            JE_textShade(screen, x, y, message.Substring(endpos), helpBoxColor, helpBoxBrightness, helpBoxShadeType);
        }

        helpBoxColor = 12;
        helpBoxShadeType = FULL_SHADE;
    }

    public static void JE_HBox(Surface screen, int x, int y, int messagenum, int boxwidth)
    {
        JE_helpBox(screen, x, y, helpTxt[messagenum - 1], boxwidth);
    }

    public static void JE_loadHelpText()
    {
#if TYRIAN2000
        int[] menuInt_entries = { -1, 7, 9, 9, -1, -1, 11, -1, -1, -1, 7, 4, 6, 7, 5 };
#else
        int[] menuInt_entries = { -1, 7, 9, 8, -1, -1, 11, -1, -1, -1, 6, 4, 6, 7, 5 };
#endif

        BinaryReader f = open("tyrian.hdt");
        episode1DataLoc = f.ReadInt32();

        /*Online Help*/
        skip_pascal_string(f);
        for (int i = 0; i < helpTxt.Length; ++i)
            helpTxt[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Planet names*/
        skip_pascal_string(f);
        for (int i = 0; i < pName.Length; ++i)
            pName[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Miscellaneous text*/
        skip_pascal_string(f);
        for (int i = 0; i < miscText.Length; ++i)
            miscText[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Little Miscellaneous text*/
        skip_pascal_string(f);
        for (int i = 0; i < miscTextB.Length; ++i)
            miscTextB[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Key names*/
        skip_pascal_string(f);
        for (int i = 0; i < menuInt_entries[6]; ++i)
            menuInt[6][i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Main Menu*/
        skip_pascal_string(f);
        for (int i = 0; i < menuText.Length; ++i)
            menuText[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Event text*/
        skip_pascal_string(f);
        for (int i = 0; i < outputs.Length; ++i)
            outputs[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Help topics*/
        skip_pascal_string(f);
        for (int i = 0; i < topicName.Length; ++i)
            topicName[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Main Menu Help*/
        skip_pascal_string(f);
        for (int i = 0; i < mainMenuHelp.Length; ++i)
            mainMenuHelp[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Menu 1 - Main*/
        skip_pascal_string(f);
        for (int i = 0; i < menuInt_entries[1]; ++i)
            menuInt[1][i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Menu 2 - Items*/
        skip_pascal_string(f);
        for (int i = 0; i < menuInt_entries[2]; ++i)
            menuInt[2][i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Menu 3 - Options*/
        skip_pascal_string(f);
        for (int i = 0; i < menuInt_entries[3]; ++i)
            menuInt[3][i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*InGame Menu*/
        skip_pascal_string(f);
        for (int i = 0; i < inGameText.Length; ++i)
            inGameText[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Detail Level*/
        skip_pascal_string(f);
        for (int i = 0; i < detailLevel.Length; ++i)
            detailLevel[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Game speed text*/
        skip_pascal_string(f);
        for (int i = 0; i < gameSpeedText.Length; ++i)
            gameSpeedText[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        // episode names
        skip_pascal_string(f);
        for (int i = 0; i < episode_name.Length; ++i)
            episode_name[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        // difficulty names
        skip_pascal_string(f);
        for (int i = 0; i < difficulty_name.Length; ++i)
            difficulty_name[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        // gameplay mode names
        skip_pascal_string(f);
        for (int i = 0; i < gameplay_name.Length; ++i)
            gameplay_name[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Menu 10 - 2Player Main*/
        skip_pascal_string(f);
        for (int i = 0; i < menuInt_entries[10]; ++i)
            menuInt[10][i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Input Devices*/
        skip_pascal_string(f);
        for (int i = 0; i < inputDevices.Length; ++i)
            inputDevices[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Network text*/
        skip_pascal_string(f);
        for (int i = 0; i < networkText.Length; ++i)
            networkText[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Menu 11 - 2Player Network*/
        skip_pascal_string(f);
        for (int i = 0; i < menuInt_entries[11]; ++i)
            menuInt[11][i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*HighScore Difficulty Names*/
        skip_pascal_string(f);
        for (int i = 0; i < difficultyNameB.Length; ++i)
            difficultyNameB[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Menu 12 - Network Options*/
        skip_pascal_string(f);
        for (int i = 0; i < menuInt_entries[12]; ++i)
            menuInt[12][i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Menu 13 - Joystick*/
        skip_pascal_string(f);
        for (int i = 0; i < menuInt_entries[13]; ++i)
            menuInt[13][i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Joystick Button Assignments*/
        skip_pascal_string(f);
        for (int i = 0; i < joyButtonNames.Length; ++i)
            joyButtonNames[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*SuperShips - For Super Arcade Mode*/
        skip_pascal_string(f);
        for (int i = 0; i < superShips.Length; ++i)
            superShips[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*SuperShips - For Super Arcade Mode*/
        skip_pascal_string(f);
        for (int i = 0; i < specialName.Length; ++i)
            specialName[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Secret DESTRUCT game*/
        skip_pascal_string(f);
        for (int i = 0; i < destructHelp.Length; ++i)
            destructHelp[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Secret DESTRUCT weapons*/
        skip_pascal_string(f);
        for (int i = 0; i < weaponNames.Length; ++i)
            weaponNames[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*Secret DESTRUCT modes*/
        skip_pascal_string(f);
        for (int i = 0; i < destructModeName.Length; ++i)
            destructModeName[i] = read_encrypted_pascal_string(f);
        skip_pascal_string(f);

        /*NEW: Ship Info*/
        skip_pascal_string(f);
        for (int i = 0; i < shipInfo.Length; ++i)
        {
            shipInfo[i][0] = read_encrypted_pascal_string(f);
            shipInfo[i][1] = read_encrypted_pascal_string(f);
        }
        skip_pascal_string(f);

#if !TYRIAN2000
        /*Menu 12 - Network Options*/
        skip_pascal_string(f);
        for (int i = 0; i < menuInt_entries[14]; ++i)
            menuInt[14][i] = read_encrypted_pascal_string(f);
#endif

        f.Close();
    }
}