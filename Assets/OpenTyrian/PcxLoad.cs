using static VideoC;
using static FileIO;
using static PaletteC;

using UnityEngine;
using System.IO;

public static class PcxLoadC
{
    public static void JE_loadPCX(string file) // this is only meant to load tshp2.pcx
    {

        byte[] s = VGAScreen.pixels; /* 8-bit specific */

        BinaryReader f = open(file);

        f.BaseStream.Seek(-769, SeekOrigin.End);

        if (f.ReadByte() == 12)
        {
            for (int i = 0; i < 256; i++)
            {
                colors[i].r = f.ReadByte();
                colors[i].g = f.ReadByte();
                colors[i].b = f.ReadByte();
                colors[i].a = (byte)i;
            }
        }

        f.BaseStream.Seek(128, SeekOrigin.Begin);

        int sIdx = 0;

        for (int i = 0; i < 320 * 200;)
        {
            byte p = f.ReadByte();
            if ((p & 0xc0) == 0xc0)
            {
                i += (p & 0x3f);
                byte value = f.ReadByte();
                int end = p & 0x3f;
                for (int j = 0; j < end; ++j)
                {
                    s[sIdx + j] = value;
                }
                sIdx += (p & 0x3f);
            }
            else
            {
                i++;

                s[sIdx] = p;
                sIdx++;
            }
            if (i != 0 && (i % 320 == 0))
            {
                sIdx += VGAScreen.w - 320;
            }
        }

        f.Close();
    }
}