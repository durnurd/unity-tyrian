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


using System;
using System.Collections;
using System.Collections.Generic;
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

using static PaletteC;
using static SurfaceC;
using static FileIO;

public static class SpriteC
{
    public const int FONT_SHAPES       = 0;
    public const int SMALL_FONT_SHAPES = 1;
    public const int TINY_FONT         = 2;
    public const int PLANET_SHAPES     = 3;
    public const int FACE_SHAPES       = 4;
    public const int OPTION_SHAPES     = 5; /*Also contains help shapes*/
    public const int WEAPON_SHAPES     = 6;
    public const int EXTRA_SHAPES      = 7; /*Used for Ending pics*/

    const int SPRITE_TABLES_MAX = 8;
#if TYRIAN2000
    const int SPRITES_PER_TABLE_MAX = 152;
#else
const int SPRITES_PER_TABLE_MAX = 151;
#endif
    
    public class Sprite_data
    {
        public UInt16 width;
        public UInt16 height;
        public byte[] data;
    }

    public static readonly Sprite_data[][] sprite_table = new Sprite_data[SPRITE_TABLES_MAX][];

    public static byte[][] eShapes = new byte[6][];
    public static byte[] shapesC1, shapes6, shapes9, shapesW2;

    public static void load_sprites_file(uint table, string filename ) {
        using (BinaryReader f = open(filename))
        {
            load_sprites(table, f);
        }
    }

    public static Sprite_data sprite(int table, int index)
    {
        return sprite_table[table][index];
    }

    static void load_sprites(uint table, BinaryReader f)
    {
        UInt16 temp = f.ReadUInt16();

        sprite_table[table] = new Sprite_data[temp];
        for (uint i = 0; i < sprite_table[table].Length; ++i)
        {
            if (!f.ReadBoolean()) // sprite is empty
                continue;

            UInt16 width = f.ReadUInt16();
            UInt16 height = f.ReadUInt16();
            UInt16 size = f.ReadUInt16();
            byte[] data = f.ReadBytes(size);

            sprite_table[table][i] = new Sprite_data() { width = width, height = height, data = data };
        }
    }

    private static void draw(Surface surface, int x, int y, Sprite_data cur_sprite, int mode, int h = 0, int v = 0)
    {
        if (cur_sprite == null)
            return;

        byte[] pixels = surface.pixels;

        byte[] data = cur_sprite.data;

        int width = cur_sprite.width;
        int x_offset = 0;

        int pixelIdx = y * surface.w + x,
        pixels_ll = 0,  // lower limit
        pixels_ul = pixels.Length;  // upper limit

        for (int dataIdx = 0; dataIdx < data.Length; ++dataIdx)
        {
            switch (data[dataIdx])
            {
                case 255:  // transparent pixels
                    dataIdx++;  // next byte tells how many
                    pixelIdx += data[dataIdx];
                    x_offset += data[dataIdx];
                    break;

                case 254:  // next pixel row
                    pixelIdx += width - x_offset;
                    x_offset = width;
                    break;

                case 253:  // 1 transparent pixel
                    pixelIdx++;
                    x_offset++;
                    break;

                default:  // set a pixel
                    if (pixelIdx >= pixels_ul)
                        return;
                    if (pixelIdx >= pixels_ll)
                    {
                        int color;
                        switch (mode)
                        {
                            case 0: //blit_sprite
                                color = data[dataIdx];
                                break;
                            case 1: //blit_sprite_blend
                                color = (data[dataIdx] & 0xf0) | (((pixels[pixelIdx] & 0x0f) + (data[dataIdx] & 0x0f)) / 2);
                                break;
                            case 2: //blit_sprite_hv_unsafe
                                color = h | ((data[dataIdx] & 0x0f) + v);
                                break;
                            case 3: //blit_sprite_hv
                                byte temp_value = (byte)((data[dataIdx] & 0x0f) + v);
                                if (temp_value > 0xf)
                                    temp_value = (byte)((temp_value >= 0x1f) ? 0x0 : 0xf);
                                color = h | temp_value;
                                break;
                            case 4: //blit_sprite_hv_blend
                                temp_value = (byte)((data[dataIdx] & 0x0f) + v);
                                if (temp_value > 0xf)
                                    temp_value = (byte)((temp_value >= 0x1f) ? 0x0 : 0xf);

                                color = h| (((pixels[pixelIdx] & 0x0f) + temp_value) / 2);
                                break;
                            case 5: //blit_sprite_dark
                                color = h != 0 ? 0x00 : ((pixels[pixelIdx] & 0xf0) | ((pixels[pixelIdx] & 0x0f) / 2));
                                break;
                            default:
                                color = 0;
                                break;
                        }
                        pixels[pixelIdx] = (byte)color;
                    }
                    pixelIdx++;
                    x_offset++;
                    break;
            }
            if (x_offset >= width)
            {
                pixelIdx += surface.w - x_offset;
                x_offset = 0;
            }
        }
    }

    public static void blit_sprite(Surface surface, int x, int y, uint table, int index)
    {
        draw(surface, x, y, sprite_table[table][index], 0);
    }
    public static void blit_sprite_blend(Surface surface, int x, int y, uint table, int index)
    {
        draw(surface, x, y, sprite_table[table][index], 1);
    }

    //// does not clip on left or right edges of surface
    //// unsafe because it doesn't check that value won't overflow into hue
    //// we can replace it when we know that we don't rely on that 'feature'
    public static void blit_sprite_hv_unsafe(Surface surface, int x, int y, uint table, int index, byte hue, sbyte value)
    {
        hue <<= 4;
        draw(surface, x, y, sprite_table[table][index], 2, hue, value);
    }


    //// does not clip on left or right edges of surface
    public static void blit_sprite_hv(Surface surface, int x, int y, uint table, int index, byte hue, sbyte value)
    {
        hue <<= 4;
        draw(surface, x, y, sprite_table[table][index], 3, hue, value);
    }

    //// does not clip on left or right edges of surface
    public static void blit_sprite_hv_blend(Surface surface, int x, int y, uint table, int index, byte hue, sbyte value)
    {
        hue <<= 4;
        draw(surface, x, y, sprite_table[table][index], 4, hue, value);
    }
 
    //// does not clip on left or right edges of surface
    public static void blit_sprite_dark(Surface surface, int x, int y, int table, int index, bool dark)
    {
        draw(surface, x, y, sprite_table[table][index], 5, dark ? (byte)1 : (byte)0);
    }
    
    public static void JE_loadCompShapes(out byte[] sprite2s, char s)
    {
        sprite2s = readAllBytes("newsh" + s + ".shp");
    }

    //// does not clip on left or right edges of surface
    private static void draw(Surface surface, int x, int y, byte[] data, int index, int mode, int filter = 0)
    {
        byte[] pixels = surface.pixels;

        int pixelIdx = y * surface.w + x,
        pixels_ll = 0,  // lower limit
        pixels_ul = (int)pixels.Length;  // upper limit

        --index;
        byte low = data[index * 2];
        byte high = data[index * 2 + 1];
        int dataIdx = (high << 8) | low;

        for (; data[dataIdx] != 0x0f; ++dataIdx)
        {
            pixelIdx += data[dataIdx] & 0x0f;                   // second nibble: transparent pixel count
            int count = (data[dataIdx] & 0xf0) >> 4; // first nibble: opaque pixel count

            if (count == 0) // move to next pixel row
            {
                pixelIdx += surface.w - 12;
            }
            else
            {
                while (count-- > 0)
                {
                    ++dataIdx;

                    if (pixelIdx >= pixels_ul)
                        return;
                    if (pixelIdx >= pixels_ll)
                    {
                        byte color;
                        switch(mode)
                        {
                            case 0:
                                color = data[dataIdx];
                                break;
                            case 1:
                                color = (byte)((((data[dataIdx] & 0x0f) + (pixels[pixelIdx] & 0x0f)) / 2) | (data[dataIdx] & 0xf0));
                                break;
                            case 2:
                                color = (byte)(((pixels[pixelIdx] & 0x0f) / 2) + (pixels[pixelIdx] & 0xf0));
                                break;
                            case 3:
                                color = (byte)(filter | (data[dataIdx] & 0x0f));
                                break;
                            default:
                                color = 0;
                                break;
                        }
                        pixels[pixelIdx] = color;
                    }

                    ++pixelIdx;
                }
            }
        }
    }

    public static void blit_sprite2(Surface surface, int x, int y, byte[] data, int index)
    {
        draw(surface, x, y, data, index, 0);
    }

    //// does not clip on left or right edges of surface
    public static void blit_sprite2_blend(Surface surface, int x, int y, byte[] data, int index)
    {
        draw(surface, x, y, data, index, 1);
    }

    //// does not clip on left or right edges of surface
    public static void blit_sprite2_darken(Surface surface, int x, int y, byte[] data, int index)
    {
        draw(surface, x, y, data, index, 2);
    }

    //// does not clip on left or right edges of surface
    public static void blit_sprite2_filter(Surface surface, int x, int y, byte[] data, int index, byte filter)
    {
        draw(surface, x, y, data, index, 3, filter);
    }

    //// does not clip on left or right edges of surface
    public static void blit_sprite2x2(Surface surface, int x, int y, byte[] data, int index)
    {
        blit_sprite2(surface, x, y, data, index);
        blit_sprite2(surface, x + 12, y, data, index + 1);
        blit_sprite2(surface, x, y + 14, data, index + 19);
        blit_sprite2(surface, x + 12, y + 14, data, index + 20);
    }

    //// does not clip on left or right edges of surface
    public static void blit_sprite2x2_blend(Surface surface, int x, int y, byte[] data, int index)
    {
        blit_sprite2_blend(surface, x, y, data, index);
        blit_sprite2_blend(surface, x + 12, y, data, index + 1);
        blit_sprite2_blend(surface, x, y + 14, data, index + 19);
        blit_sprite2_blend(surface, x + 12, y + 14, data, index + 20);
    }

    //// does not clip on left or right edges of surface
    public static void blit_sprite2x2_darken(Surface surface, int x, int y, byte[] data, int index)
    {
        blit_sprite2_darken(surface, x, y, data, index);
        blit_sprite2_darken(surface, x + 12, y, data, index + 1);
        blit_sprite2_darken(surface, x, y + 14, data, index + 19);
        blit_sprite2_darken(surface, x + 12, y + 14, data, index + 20);
    }


    public static void JE_loadMainShapeTables(string shpfile)
    {
#if TYRIAN2000
    const int SHP_NUM = 13;
#else
        const int SHP_NUM = 12;
#endif
        BinaryReader f = open(shpfile);

        JE_word shpNumb;
        JE_longint[] shpPos = new JE_longint[SHP_NUM + 1]; // +1 for storing file length

        shpNumb = f.ReadUInt16();

        uint i;
        for (i = 0; i < shpNumb; ++i)
            shpPos[i] = f.ReadInt32();

        for (i = shpNumb; i < shpPos.Length; ++i)
            shpPos[i] = (int)f.BaseStream.Length;

        // fonts, interface, option sprites

        for (i = 0; i < 7; i++)
        {
            f.BaseStream.Seek(shpPos[i], SeekOrigin.Begin);
            load_sprites(i, f);
        }

        // player shot sprites
        shapesC1 = f.ReadBytes(shpPos[i + 1] - shpPos[i]);
        i++;

        // player ship sprites
        shapes9 = f.ReadBytes(shpPos[i + 1] - shpPos[i]);
        i++;

        // power-up sprites
        eShapes[5] = f.ReadBytes(shpPos[i + 1] - shpPos[i]);
        i++;

        // coins, datacubes, etc sprites
        eShapes[4] = f.ReadBytes(shpPos[i + 1] - shpPos[i]);
        i++;

        // more player shot sprites
        shapesW2 = f.ReadBytes(shpPos[i + 1] - shpPos[i]);

        f.Close();
    }
}
