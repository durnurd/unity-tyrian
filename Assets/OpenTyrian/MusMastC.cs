using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

public static class MusMastC
{
    public const int DEFAULT_SONG_BUY = 2;
    public const int SONG_LEVELEND = 9;
    public const int SONG_GAMEOVER = 10;
    public const int SONG_MAPVIEW = 19;
    public const int SONG_ENDGAME1 = 7;
    public const int SONG_ZANAC = 31;
    public const int SONG_TITLE = 29;
    public const int MUSIC_NUM = 41;

    public static int songBuy;
    public static readonly string[] musicFile =
    {
        	/*  1 */  "ASTEROI2.DAT",
	/*  2 */  "ASTEROID.DAT",
	/*  3 */  "BUY.DAT",
	/*  4 */  "CAMANIS.DAT",
	/*  5 */  "CAMANISE.DAT",
	/*  6 */  "DELIANI.DAT",
	/*  7 */  "DELIANI2.DAT",
	/*  8 */  "ENDING1.DAT",
	/*  9 */  "ENDING2.DAT",
	/* 10 */  "ENDLEVEL.DAT",
	/* 11 */  "GAMEOVER.DAT",
	/* 12 */  "GRYPHON.DAT",
	/* 13 */  "GRYPHONE.DAT",
	/* 14 */  "GYGES.DAT",
	/* 15 */  "GYGESE.DAT",
	/* 16 */  "HALLOWS.DAT",
	/* 17 */  "ZICA.DAT",
	/* 18 */  "TYRSONG2.DAT",
	/* 19 */  "LOUDNESS.DAT",
	/* 20 */  "NAVC.DAT",
	/* 21 */  "SAVARA.DAT",
	/* 22 */  "SAVARAE.DAT",
	/* 23 */  "SPACE1.DAT",
	/* 24 */  "SPACE2.DAT",
	/* 25 */  "STARENDB.DAT",
	/* 26 */  "START5.DAT",
	/* 27 */  "TALK.DAT",
	/* 28 */  "TORM.DAT",
	/* 29 */  "TRANSON.DAT",
	/* 30 */  "TYRSONG.DAT",
	/* 31 */  "ZANAC3.DAT",
	/* 32 */  "ZANACS.DAT",
	/* 33 */  "SAVARA2.DAT",
	/* 34 */  "HISCORE.DAT",
	/* 35 */  "TYR4-1.DAT",    /* OMF */
	/* 36 */  "TYR4-3.DAT",    /* SARAH */
	/* 37 */  "TYR4-2.DAT",    /* MAGFIELD */
	/* 38 */  "TYR4-0.DAT",    /* ROCKME */
	/* 39 */  "TYR4-4.DAT",    /* quiet music */
	/* 40 */  "TYR4-5.DAT",    /* piano */
	/* 41 */  "TYR-BEER.DAT"   /* BEER */
    };
    public static readonly string[] musicTitle =
    {
        "Asteroid Dance Part 2",
        "Asteroid Dance Part 1",
        "Buy/Sell Music",
        "CAMANIS",
        "CAMANISE",
        "Deli Shop Quartet",
        "Deli Shop Quartet No. 2",
        "Ending Number 1",
        "Ending Number 2",
        "End of Level",
        "Game Over Solo",
        "Gryphons of the West",
        "Somebody pick up the Gryphone",
        "Gyges, Will You Please Help Me?",
        "I speak Gygese",
        "Halloween Ramble",
        "Tunneling Trolls",
        "Tyrian, The Level",
        "The MusicMan",
        "The Navigator",
        "Come Back to Me, Savara",
        "Come Back again to Savara",
        "Space Journey 1",
        "Space Journey 2",
        "The final edge",
        "START5",
        "Parlance",
        "Torm - The Gathering",
        "TRANSON",
        "Tyrian: The Song",
        "ZANAC3",
        "ZANACS",
        "Return me to Savara",
        "High Score Table",
        "One Mustn't Fall",
        "Sarah's Song",
        "A Field for Mag",
        "Rock Garden",
        "Quest for Peace",
        "Composition in Q",
        "BEER"
    };
    public static JE_boolean musicFade;
}