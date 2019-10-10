using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static SurfaceC;
using static PaletteC;

public static class VGA256dC
{
    public static void JE_pix(Surface surface, int x, int y, JE_byte c)
    {
        /* Bad things happen if we don't clip */
        if (x < surface.w && y < surface.h)
        {
            surface.pixels[y * surface.w + x] = c;
        }
    }

    public static void JE_pix3(Surface surface, int x, int y, JE_byte c)
    {
        /* Originally impemented as several direct accesses */
        JE_pix(surface, x, y, c);
        JE_pix(surface, x - 1, y, c);
        JE_pix(surface, x + 1, y, c);
        JE_pix(surface, x, y - 1, c);
        JE_pix(surface, x, y + 1, c);
    }

    public static void fill_rectangle_xy(Surface surface, int x1, int y1, int x2, int y2, JE_byte c)
    {
        for (int y = y1; y <= y2; y++)
        {
            for (int x = x1; x <= x2; x++)
            {
                surface.pixels[y * surface.w + x] = c;
            }
        }
    }

    public static void JE_barShade(Surface surface, int a, int b, int c, int d) /* x1, y1, x2, y2 */
    {
        if (a < surface.w && b < surface.h &&
            c < surface.w && d < surface.h)
        {
            var vga = surface.pixels;
            int i, j, width;

            width = c - a + 1;

            for (i = b * surface.w + a; i <= d * surface.w + a; i += surface.w)
            {
                for (j = 0; j < width; j++)
                {
                    vga[i + j] = (byte)(((vga[i + j] & 0x0F) >> 1) | (vga[i + j] & 0xF0));
                }
            }
        }
    }

    public static void JE_rectangle(Surface surface, int a, int b, int c, int d, JE_byte e) /* x1, y1, x2, y2, color */
    {
        if (a < surface.w && b < surface.h &&
            c < surface.w && d < surface.h)
        {
            var vga = surface.pixels;
            int i;

            /* Top line */
            for (i = b * surface.w + a; i <= b * surface.w + c; i++)
            {
                vga[i] = e;
            }

            /* Bottom line */
            for (i = d * surface.w + a; i <= d * surface.w + c; i++)
            {
                vga[i] = e;
            }

            /* Left line */
            for (i = (b + 1) * surface.w + a; i < (d * surface.w + a); i += surface.w)
            {
                vga[i] = e;
            }

            /* Right line */
            for (i = (b + 1) * surface.w + c; i < (d * surface.w + c); i += surface.w)
            {
                vga[i] = e;
            }
        }
    }

    public static void JE_barBright(Surface surface, int a, int b, int c, int d) /* x1, y1, x2, y2 */
    {
        if (a < surface.w && b < surface.h &&
            c < surface.w && d < surface.h)
        {
            byte[] vga = surface.pixels;
            int i, j, width;

            width = c - a + 1;

            for (i = b * surface.w + a; i <= d * surface.w + a; i += surface.w)
            {
                for (j = 0; j < width; j++)
                {
                    JE_byte al, ah;
                    al = ah = vga[i + j];

                    ah &= 0xF0;
                    al = (byte)((al & 0x0F) + 2);

                    if (al > 0x0F)
                    {
                        al = 0x0F;
                    }

                    vga[i + j] = (byte)(al + ah);
                }
            }
        }
    }

    static void fill_rectangle_hw(Surface surface, int x, int y, int h, int w, byte color)
    {
        fill_rectangle_xy(surface, x, y, x + w - 1, y + h - 1, color);
    }

    public static void draw_segmented_gauge(Surface surface, int x, int y, byte color, int segment_width, int segment_height, int segment_value, int value)
    {
        int segments = value / segment_value,
                   partial_segment = value % segment_value;

        for (int i = 0; i < segments; ++i)
        {
            fill_rectangle_hw(surface, x, y, segment_width, segment_height, (byte)(color + 12));
            x += segment_width + 1;
        }
        if (partial_segment > 0)
            fill_rectangle_hw(surface, x, y, segment_width, segment_height, (byte)(color + (12 * partial_segment / segment_value)));
    }
}