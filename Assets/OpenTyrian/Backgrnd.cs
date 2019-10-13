using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static LibC;
using static VideoC;
using static SurfaceC;
using static ConfigC;
using static VarzC;

using static System.Math;

public static class BackgrndC
{
    /*Special Background 2 and Background 3*/

    /*Back Pos 3*/
    public static JE_word backPos, backPos2, backPos3;
    public static JE_word backMove, backMove2, backMove3;

    /*Main Maps*/
    public static JE_word mapX, mapY, mapX2, mapX3, mapY2, mapY3;

    public static int mapYPos, mapY2Pos, mapY3Pos;
    public static JE_word mapXPos, oldMapXOfs, mapXOfs, mapX2Ofs, mapX2Pos, mapX3Pos, oldMapX3Ofs, mapX3Ofs, tempMapXOfs;
    public static int /*intptr_t*/ mapXbpPos, mapX2bpPos, mapX3bpPos;
    public static JE_byte map1YDelay, map1YDelayMax, map2YDelay, map2YDelayMax;
    
    public static JE_boolean anySmoothies;
    public static readonly JE_byte[] smoothie_data = new JE_byte[9]; /* [1..9] */

    public static void JE_darkenBackground(JE_word neat)  /* wild detail level */
    {
        byte[] s = VGAScreen.pixels; /* screen pointer, 8-bit specific */
        int x, y;

        int sIdx = 24;

        for (y = 184; y > 0; y--)
        {
            for (x = 264; x > 0; x--)
            {
                s[sIdx] = (byte)(((((s[sIdx] & 0x0f) << 4) - (s[sIdx] & 0x0f) + ((((x - neat - y) >> 2) + s[sIdx - 2] + (y == 184 ? 0 : s[sIdx - (VGAScreen.w - 1)])) & 0x0f)) >> 4) | (s[sIdx] & 0xf0));
                sIdx++;
            }
            sIdx += VGAScreen.w - 264;
        }
    }

    private static void blit_background_row(Surface surface, int mX, int mY, ref JE_MegaDataType map, int rowIdx, int leftCol)
    {
        leftCol--;
        byte[] pixels = surface.pixels;
        int pixelsIdx = (mY * surface.w) + mX,
          pixels_ll = 0,  // lower limit
          pixels_ul = (surface.h * surface.w);  // upper limit

        byte[] row = map.mainmap[rowIdx];
        byte[] prevRow = map.mainmap[rowIdx - 1];
        int maxTile = leftCol + 12;

        for (int y = 0; y < 28; y++)
        {
            // not drawing on screen yet; skip y
            if ((pixelsIdx + (12 * 24)) < pixels_ll)
            {
                pixelsIdx += surface.w;
                continue;
            }

            for (int tileIdx = leftCol; tileIdx < maxTile; tileIdx++)
            {
                byte shapeIdx;
                if (tileIdx < 0)
                {
                    shapeIdx = prevRow[tileIdx + prevRow.Length];
                }
                else
                {
                    shapeIdx = row[tileIdx];
                }

                // no tile; skip tile
                if (shapeIdx >= map.shapes.Length || map.shapes[shapeIdx].sh == null)
                {
                    pixelsIdx += 24;
                    continue;
                }

                var shape = map.shapes[shapeIdx];
                byte[] data = shape.sh;

                int dataIdx = y * 24;

                for (int x = 24; x > 0; x--) {
                    if (pixelsIdx >= pixels_ul)
                        return;

                    byte byt = data[dataIdx];
                    if (pixelsIdx >= pixels_ll && byt != 0)
                    {
                        pixels[pixelsIdx] = byt;
                    }

                    pixelsIdx++;
                    dataIdx++;
                }
            }

            pixelsIdx += surface.w - 12 * 24;   //Skip the HUD
        }
    }

    private static void blit_background_row_blend(Surface surface, int mX, int mY, ref JE_MegaDataType map, int rowIdx, int leftCol)
    {
        leftCol--;
        byte[] pixels = surface.pixels;
        int pixelsIdx = (mY * surface.w) + mX,
          pixels_ll = 0,  // lower limit
          pixels_ul = (surface.h * surface.w);  // upper limit

        byte[] row = map.mainmap[rowIdx];
        byte[] prevRow = map.mainmap[rowIdx - 1];
        int maxTile = leftCol + 12;

        for (int y = 0; y < 28; y++)
        {
            // not drawing on screen yet; skip y
            if ((pixelsIdx + (12 * 24)) < pixels_ll)
            {
                pixelsIdx += surface.w;
                continue;
            }
            for (int tileIdx = leftCol; tileIdx < maxTile; tileIdx++)
            {
                byte shapeIdx;
                if (tileIdx < 0)
                {
                    shapeIdx = prevRow[tileIdx + prevRow.Length];
                }
                else
                {
                    shapeIdx = row[tileIdx];
                }

                // no tile; skip tile
                if (shapeIdx >= map.shapes.Length || map.shapes[shapeIdx].sh == null)
                {
                    pixelsIdx += 24;
                    continue;
                }

                var data = map.shapes[shapeIdx].sh;
                int dataIdx = y * 24;

                for (int x = 24; x != 0; x--)
                {
                    if (pixelsIdx >= pixels_ul)
                        return;

                    byte byt = data[dataIdx];
                    if (pixelsIdx >= pixels_ll && byt != 0)
                    {
                        pixels[pixelsIdx] = (byte)((byt & 0xf0) | (((pixels[pixelsIdx] & 0x0f) + (byt & 0x0f)) / 2));
                    }

                    pixelsIdx++;
                    dataIdx++;
                }
            }

            pixelsIdx += surface.w - 12 * 24;
        }
    }

    public static void draw_background_1(Surface surface)
    {
        JE_clr256(surface);

        int mapIdx = mapXbpPos - 12;

        for (int i = -1; i < 7; i++) {
            blit_background_row(surface, mapXPos, (i * 28) + backPos, ref megaData1, mapYPos + i + 1, mapIdx);
        }
    }

    public static void draw_background_2(Surface surface)
    {
        if (map2YDelayMax > 1 && backMove2 < 2)
            backMove2 = (byte)((map2YDelay == 1) ? 1 : 0);

        if (background2)
        {
            // water effect combines background 1 and 2 by syncronizing the x coordinate
            int x = smoothies[1] ? mapXPos : mapX2Pos;

            int mapIdx = (smoothies[1] ? mapXbpPos : mapX2bpPos) - 12;

            for (int i = -1; i < 7; i++)
            {
                blit_background_row(surface, x, (i * 28) + backPos2, ref megaData2, mapY2Pos + i + 1, mapIdx);
            }
        }

        /*Set Movement of background*/
        if (--map2YDelay == 0)
        {
            map2YDelay = map2YDelayMax;

            backPos2 += backMove2;

            if (backPos2 > 27)
            {
                backPos2 -= 28;
                mapY2--;
                mapY2Pos--;
            }
        }
    }

    public static void draw_background_2_blend(Surface surface)
    {
        if (map2YDelayMax > 1 && backMove2 < 2)
            backMove2 = (byte)((map2YDelay == 1) ? 1 : 0);

        int mapIdx = mapX2bpPos - 12;

        for (int i = -1; i < 7; i++)
        {
            blit_background_row_blend(surface, mapX2Pos, (i * 28) + backPos2, ref megaData2, mapY2Pos + i + 1, mapIdx);
        }

        /*Set Movement of background*/
        if (--map2YDelay == 0)
        {
            map2YDelay = map2YDelayMax;

            backPos2 += backMove2;

            if (backPos2 > 27)
            {
                backPos2 -= 28;
                mapY2--;
                mapY2Pos--;
            }
        }
    }
    public static void draw_background_3(Surface surface)
    {
        /* Movement of background */
        backPos3 += backMove3;

        if (backPos3 > 27)
        {
            backPos3 -= 28;
            mapY3--;
            mapY3Pos--;
        }


        int mapIdx = mapX3bpPos - 12;

        for (int i = -1; i < 7; i++)
        {
            blit_background_row(surface, mapX3Pos, (i * 28) + backPos3, ref megaData3, mapY3Pos + i + 1, mapIdx);
        }
    }

    public static void JE_filterScreen(JE_shortint col, JE_shortint int_)
    {
        byte[] s; /* screen pointer, 8-bit specific */
        int sIdx;
        int x, y;
        ushort temp;

        if (filterFade)
        {
            levelBrightness += levelBrightnessChg;
            if ((filterFadeStart && levelBrightness < -14) || levelBrightness > 14)
            {
                levelBrightnessChg = -levelBrightnessChg;
                filterFadeStart = false;
                levelFilter = levelFilterNew;
            }
            if (!filterFadeStart && levelBrightness == 0)
            {
                filterFade = false;
                levelBrightness = -99;
            }
        }

        if (col != -99 && filtrationAvail)
        {
            s = VGAScreen.pixels;
            sIdx = 24;

            col <<= 4;

            for (y = 184; y != 0; y--)
            {
                for (x = 264; x != 0; x--)
                {
                    s[sIdx] = (byte)((byte)col | (s[sIdx] & 0x0f));
                    sIdx++;
                }
                sIdx += VGAScreen.w - 264;
            }
        }

        if (int_ != -99 && explosionTransparent)
        {
            s = VGAScreen.pixels;
            sIdx = 24;

            for (y = 184; y != 0; y--)
            {
                for (x = 264; x != 0; x--)
                {
                    temp = (ushort)((s[sIdx] & 0x0f) + int_);
                    s[sIdx] = (byte)((s[sIdx] & 0xf0) | (temp >= 0x1f ? 0 : (temp >= 0x0f ? 0x0f : temp)));
                    sIdx++;
                }
                sIdx += VGAScreen.w - 264;
            }
        }
    }

    public static void JE_checkSmoothies()
    {
        anySmoothies = (processorType > 2 && (smoothies[1 - 1] || smoothies[2 - 1])) || (processorType > 1 && (smoothies[3 - 1] || smoothies[4 - 1] || smoothies[5 - 1]));
    }

    public static void lava_filter(Surface dst, Surface src)
    {
        /* we don't need to check for over-reading the pixel surfaces since we only
         * read from the top 185+1 scanlines, and there should be 320 */

        byte[] dst_pixel = dst.pixels;
        int dstIdx = (185 * dst.w);

        byte[] src_pixel = src.pixels;
        int srcIdx = (185 * src.w);

        int w = 320 * 185 - 1;

        for (int y = 185 - 1; y >= 0; --y)
        {
            dstIdx -= (dst.w - 320);  // in case pitch is not 320
            srcIdx -= (src.w - 320);  // in case pitch is not 320

            for (int x = 320 - 1; x >= 0; x -= 8)
            {
                int waver = Abs(((w >> 9) & 0x0f) - 8) - 1;
                w -= 8;

                for (int xi = 8 - 1; xi >= 0; --xi)
                {
                    --dstIdx;
                    --srcIdx;

                    // value is average value of source pixel (2x), destination pixel above, and destination pixel below (all with waver)
                    // hue is red
                    byte value = 0;

                    if (srcIdx + waver >= 0)
                        value += (byte)(((src_pixel[srcIdx + waver]) & 0x0f) * 2);
                    value += (byte)(dst_pixel[dstIdx + waver + dst.w] & 0x0f);
                    if (dstIdx + waver - dst.w >= 0)
                        value += (byte)(dst_pixel[dstIdx + waver - dst.w] & 0x0f);

                    dst_pixel[dstIdx] = (byte)((value / 4) | 0x70);
                }
            }
        }
    }

    public static void water_filter(Surface dst, Surface src)
    {
        byte hue = (byte)(smoothie_data[1] << 4);

        /* we don't need to check for over-reading the pixel surfaces since we only
         * read from the top 185+1 scanlines, and there should be 320 */

        byte[] dst_pixel = dst.pixels;
        int dstIdx = (185 * dst.w);

        byte[] src_pixel = src.pixels;
        int srcIdx = (185 * src.w);

        int w = 320 * 185 - 1;

        for (int y = 185 - 1; y >= 0; --y)
        {
            dstIdx -= (dst.w - 320);  // in case pitch is not 320
            srcIdx -= (src.w - 320);  // in case pitch is not 320

            for (int x = 320 - 1; x >= 0; x -= 8)
            {
                int waver = Abs(((w >> 10) & 0x07) - 4) - 1;
                w -= 8;

                for (int xi = 8 - 1; xi >= 0; --xi)
                {
                    --dstIdx;
                    --srcIdx;

                    // pixel is copied from source if not blue
                    // otherwise, value is average of value of source pixel and destination pixel below (with waver)
                    if ((src_pixel[srcIdx] & 0x30) == 0)
                    {
                        dst_pixel[dstIdx] = src_pixel[srcIdx];
                    }
                    else
                    {
                        byte value = (byte)(src_pixel[srcIdx] & 0x0f);
                        value += (byte)((dst_pixel[dstIdx] + waver + dst.w) & 0x0f);
                        dst_pixel[dstIdx] = (byte)((value / 2) | hue);
                    }
                }
            }
        }
    }


    public static void iced_blur_filter(Surface dst, Surface src)
    {
        byte[] dst_pixel = dst.pixels;
        byte[] src_pixel = src.pixels;
        int dstIdx = 0, srcIdx = 0;

        for (int y = 0; y < 184; ++y)
        {
            for (int x = 0; x < 320; ++x)
            {
                // value is average value of source pixel and destination pixel
                // hue is icy blue

                byte value = (byte)((src_pixel[srcIdx] & 0x0f) + (dst_pixel[dstIdx] & 0x0f));
                dst_pixel[dstIdx] = (byte)((value / 2) | 0x80);

                ++dstIdx;
                ++srcIdx;
            }

            dstIdx += (dst.w - 320);  // in case pitch is not 320
            srcIdx += (src.w - 320);  // in case pitch is not 320
        }
    }

    public static void blur_filter(Surface dst, Surface src)
    {
        byte[] dst_pixel = dst.pixels;
        byte[] src_pixel = src.pixels;
        int dstIdx = 0, srcIdx = 0;

        for (int y = 0; y < 184; ++y)
        {
            for (int x = 0; x < 320; ++x)
            {
                // value is average value of source pixel and destination pixel
                // hue is source pixel hue

                byte value = (byte)((src_pixel[srcIdx] & 0x0f) + (dst_pixel[dstIdx] & 0x0f));
                dst_pixel[dstIdx] = (byte)((value / 2) | (src_pixel[srcIdx] & 0xf0));

                ++dstIdx;
                ++srcIdx;
            }

            dstIdx += (dst.w - 320);  // in case pitch is not 320
            srcIdx += (src.w - 320);  // in case pitch is not 320
        }
    }

    struct StarfieldStar {
        public byte color;
        public JE_word position; // relies on overflow wrap-around
        public int speed;
    }

    private static int MAX_STARS = 100;
    private static int STARFIELD_HUE = 0x90;
    private static StarfieldStar[] starfield_stars = new StarfieldStar[MAX_STARS];
    public static int starfield_speed;

    public static void initialize_starfield()
    {
        for (int i = MAX_STARS - 1; i >= 0; --i)
        {
            starfield_stars[i].position = (JE_word)(mt_rand() % 320 + mt_rand() % 200 * VGAScreen.w);
            starfield_stars[i].speed = (int)(mt_rand() % 3 + 2);
            starfield_stars[i].color = (byte)(mt_rand() % 16 + STARFIELD_HUE);
        }
    }

    public static void update_and_draw_starfield(Surface surface, int move_speed)
    {
        byte[] p = surface.pixels;

        for (int i = MAX_STARS - 1; i >= 0; --i)
        {
            StarfieldStar star = starfield_stars[i];

            star.position += (JE_word)((star.speed + move_speed) * surface.w);

            if (star.position < 177 * surface.w)
            {
                if (p[star.position] == 0)
                {
                    p[star.position] = star.color;
                }

                // If star is bright enough, draw surrounding pixels
                if (star.color - 4 >= STARFIELD_HUE)
                {
                    if (p[star.position + 1] == 0)
                        p[star.position + 1] = (byte)(star.color - 4);

                    if (star.position > 0 && p[star.position - 1] == 0)
                        p[star.position - 1] = (byte)(star.color - 4);

                    if (p[star.position + surface.w] == 0)
                        p[star.position + surface.w] = (byte)(star.color - 4);

                    if (star.position >= surface.w && p[star.position - surface.w] == 0)
                        p[star.position - surface.w] = (byte)(star.color - 4);
                }
            }
            starfield_stars[i] = star;
        }
    }
}