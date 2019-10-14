using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static VarzC;

public static class SndMastC
{
    public const int
        S_NONE = 0,
        S_WEAPON_1 = 1,
        S_WEAPON_2 = 2,
        S_ENEMY_HIT = 3,
        S_EXPLOSION_4 = 4,
        S_WEAPON_5 = 5,
        S_WEAPON_6 = 6,
        S_WEAPON_7 = 7,
        S_SELECT = 8, // S_EXPLOSION_8
        S_EXPLOSION_9 = 9,
        S_WEAPON_10 = 10,
        S_EXPLOSION_11 = 11,
        S_EXPLOSION_12 = 12,
        S_WEAPON_13 = 13,
        S_WEAPON_14 = 14,
        S_WEAPON_15 = 15,
        S_SPRING = 16,
        S_WARNING = 17,
        S_ITEM = 18,
        S_HULL_HIT = 19,
        S_MACHINE_GUN = 20,
        S_SOUL_OF_ZINGLON = 21,
        S_EXPLOSION_22 = 22,
        S_CLINK = 23,
        S_CLICK = 24,
        S_WEAPON_25 = 25,
        S_WEAPON_26 = 26,
        S_SHIELD_HIT = 27,
        S_CURSOR = 28,
        S_POWERUP = 29,
        V_CLEARED_PLATFORM = 30, // 30
        V_BOSS = 31,
        V_ENEMIES = 32,
        V_GOOD_LUCK = 33,
        V_LEVEL_END = 34,
        V_DANGER = 35,
        V_SPIKES = 36,
        V_DATA_CUBE = 37,
        V_ACCELERATE = 38,
        SAMPLE_COUNT = 38;

    public static string[] soundTitle = {
    "SCALEDN2", /*1*/
	"F2",       /*2*/
	"TEMP10",
    "EXPLSM",
    "PASS3",    /*5*/
	"TEMP2",
    "BYPASS1",
    "EXP1RT",
    "EXPLLOW",
    "TEMP13",   /*10*/
	"EXPRETAP",
    "MT2BOOM",
    "TEMP3",
    "LAZB",     /*28K*/
	"LAZGUN2",  /*15*/
	"SPRING",
    "WARNING",
    "ITEM",
    "HIT2",     /*14K*/
	"MACHNGUN", /*20*/
	"HYPERD2",
    "EXPLHUG",
    "CLINK1",
    "CLICK",
    "SCALEDN1", /*25*/
	"TEMP11",
    "TEMP16",
    "SMALL1",
    "POWERUP",
    "VOICE1",
    "VOICE2",
    "VOICE3",
    "VOICE4",
    "VOICE5",
    "VOICE6",
    "VOICE7",
    "VOICE8",
    "VOICE9"
    };
    public static JE_byte[] windowTextSamples = EmptyArray<JE_byte>(9);
}