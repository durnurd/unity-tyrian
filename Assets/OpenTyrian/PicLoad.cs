using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static SurfaceC;
using static FileIO;
using static PcxMastC;
using static PaletteC;

using System.IO;
using UnityEngine;

public static class PicLoadC
{
    private static bool first = true;
    
    public static void JE_loadPic(Surface screen, int PCXnumber, JE_boolean storepal)
    {
        PCXnumber--;


        BinaryReader f = open("tyrian.pic");

        if (first)
        {
            first = false;

            ushort temp = f.ReadUInt16();

            for (int i = 0; i < PCX_NUM; i++)
            {
                pcxpos[i] = f.ReadInt32();
            }

            pcxpos[PCX_NUM] = (int)f.BaseStream.Length;
        }

        int size = pcxpos[PCXnumber + 1] - pcxpos[PCXnumber];

        f.BaseStream.Seek(pcxpos[PCXnumber], SeekOrigin.Begin);
        byte[] p = f.ReadBytes(size);
        f.Close();

        int pIdx = 0;
        int sIdx = 0;

        byte[] s = screen.pixels;

        for (int i = 0; i < 320 * 200;)
        {
            if ((p[pIdx] & 0xc0) == 0xc0)
            {
                i += (p[pIdx] & 0x3f);
                int len = p[pIdx] & 0x3f;
                byte col = p[pIdx + 1];
                for (int j = 0; j < len; ++j)
                {
                    s[sIdx + j] = col;
                }
                sIdx += (p[pIdx] & 0x3f); pIdx += 2;
            }
            else
            {
                i++;
                s[sIdx] = p[pIdx];
                sIdx++; pIdx++;
            }
            if (i != 0 && (i % 320 == 0))
            {
                sIdx += screen.w - 320;
            }
        }

        System.Array.Copy(palettes[pcxpal[PCXnumber]], colors, colors.Length);

        if (storepal)
            set_palette(colors, 0, 255);
    }

}