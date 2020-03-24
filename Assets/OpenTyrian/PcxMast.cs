using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

public static class PcxMastC
{
    public const int PCX_NUM = 13;

    public static readonly string[] pcxfile = {
        "INTSHPB.PCX",
        "SETUP2.PCX",
        "TYRPLAY.PCX",
        "TYRLOG2.PCX",
        "P1.PCX",
        "TYRPLAY2.PCX",
        "BUC4.PCX",
        "GMOVR4a.PCX",
        "GMOVR4b.PCX",
        "EPICSKY.PCX",
        "DESTRUCT.PCX",
        "ECLIPSE.PCX",
        "FIREPICA.PCX"
    };
    public static readonly JE_byte[] pcxpal = { 0, 7, 5, 8, 10, 5, 18, 19, 19, 20, 21, 22, 5 };    /* [1..PCXnum] */
    public static readonly JE_byte[] facepal = { 1, 2, 3, 4, 6, 9, 11, 12, 16, 13, 14, 15, 1, 5, 1, 1, 21 };       /* [1..16] */
    public static readonly JE_longint[] pcxpos = new JE_longint[PCX_NUM + 1];   /* [1..PCXnum + 1] */
}