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

using static FileIO;

public static class LvlLibC
{
    public static JE_longint[] lvlPos = new JE_longint[43];/* [1..42 + 1] */
    public static string levelFile;
    public static JE_word lvlNum;

    public static void JE_analyzeLevel()
    {
        BinaryReader f = open(levelFile);

        lvlNum = f.ReadUInt16();

        for (int x = 0; x < lvlNum; x++)
            lvlPos[x] = f.ReadInt32();

        lvlPos[lvlNum] = (int)f.BaseStream.Length;

        f.Close();
    }
}