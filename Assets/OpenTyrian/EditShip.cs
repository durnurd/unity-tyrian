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

public static class EditShipC {
    private const int ShipTypes = 154;
    private const int SAS = ShipTypes - 4;

    public static JE_boolean extraAvail;
    public static JE_byte[] extraShips = new JE_byte[ShipTypes];
    public static byte[] extraShapes;

    private static readonly JE_byte[] extraCryptKey = { 58, 23, 16, 192, 254, 82, 113, 147, 62, 99 };

    public static void JE_decryptShips()
    {
        JE_boolean correct = true;
        JE_byte[] s2 = new JE_byte[ShipTypes];
        JE_byte y;

        for (int x = SAS - 1; x >= 0; x--)
        {
            s2[x] = (JE_byte)(extraShips[x] ^ extraCryptKey[(x + 1) % 10]);
            if (x > 0)
                s2[x] ^= extraShips[x - 1];
        }  /*  <= Key Decryption Test (Reversed key) */

        y = 0;
        for (uint x = 0; x < SAS; x++)
            y += s2[x];
        if (extraShips[SAS + 0] != y)
            correct = false;

        y = 0;
        for (uint x = 0; x < SAS; x++)
            y -= s2[x];
        if (extraShips[SAS + 1] != y)
            correct = false;

        y = 1;
        for (uint x = 0; x < SAS; x++)
            y = (byte)(y * s2[x] + 1);
        if (extraShips[SAS + 2] != y)
            correct = false;

        y = 0;
        for (uint x = 0; x < SAS; x++)
            y ^= s2[x];
        if (extraShips[SAS + 3] != y)
            correct = false;

        if (!correct)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
#else
            Application.Quit();
#endif
        }

        extraShips = s2;
    }

    public static void JE_loadExtraShapes()
    {
        BinaryReader f = open("newsh$.shp");

        if (f != null)
        {
            extraAvail = true;
            int extraShapeSize = (int)f.BaseStream.Length - ShipTypes;
            extraShapes = f.ReadBytes(extraShapeSize);
            extraShips = f.ReadBytes(ShipTypes);
            JE_decryptShips();
            f.Close();
        }
    }


}