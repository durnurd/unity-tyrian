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

using static OpenTyrC;

using static KeyboardC;
using static LibC;
using static VideoC;
using static System.Math;
using System.Collections;

public static class StarLibC
{

    private const int starlib_MAX_STARS = 1000;
    private const int MAX_TYPES = 14;

    struct JE_StarType
    {
        public JE_integer spX, spY, spZ;
        public JE_integer lastX, lastY;
    };

    static int tempX, tempY;
    static JE_boolean run;
    static JE_StarType[] star = new JE_StarType[starlib_MAX_STARS];

    static JE_byte setup;
    static JE_word stepCounter;

    static JE_word nsp2;
    static JE_shortint nspVar2Inc;

    /* JE: new sprite pointer */
    static JE_real nsp;
    static JE_real nspVarInc;
    static JE_real nspVarVarInc;

    static JE_word changeTime;
    static JE_boolean doChange;

    static JE_boolean grayB;

    static JE_integer starlib_speed;
    static JE_shortint speedChange;

    static JE_byte pColor;

    public static IEnumerator e_JE_starlib_main()
    {
        int off;
        JE_word i;
        JE_integer tempZ;
        JE_byte tempCol;

        int starIdx;
        byte[] surf;

        JE_wackyCol();

        grayB = false;

        starlib_speed += speedChange;


        for (starIdx = 0, i = starlib_MAX_STARS; i > 0; starIdx++, i--)
        {
            /* Make a pointer to the screen... */
            surf = VGAScreenSeg.pixels;

            /* Calculate the offset to where we wish to draw */
            off = (star[starIdx].lastX) + (star[starIdx].lastY) * 320;


            /* We don't want trails in our star field.  Erase the old graphic */
            if (off >= 640 && off < (320 * 200) - 640)
            {
                surf[off] = 0; /* Shade Level 0 */

                surf[off - 1] = 0; /* Shade Level 1, 2 */
                surf[off + 1] = 0;
                surf[off - 2] = 0;
                surf[off + 2] = 0;

                surf[off - 320] = 0;
                surf[off + 320] = 0;
                surf[off - 640] = 0;
                surf[off + 640] = 0;
            }

            /* Move star */
            tempZ = star[starIdx].spZ;
            tempX = (star[starIdx].spX / tempZ) + 160;
            tempY = (star[starIdx].spY / tempZ) + 100;
            tempZ -= starlib_speed;


            /* If star is out of range, make a new one */
            if (tempZ <= 0 ||
                tempY == 0 || tempY > 198 ||
                tempX > 318 || tempX < 1)
            {
                star[starIdx].spZ = 500;

                JE_newStar();

                star[starIdx].spX = (short)tempX;
                star[starIdx].spY = (short)tempY;
            }
            else /* Otherwise, update & draw it */
            {
                star[starIdx].lastX = (short)tempX;
                star[starIdx].lastY = (short)tempY;
                star[starIdx].spZ = tempZ;

                off = tempX + tempY * 320;

                if (grayB)
                {
                    tempCol = (byte)(tempZ >> 1);
                }
                else
                {
                    tempCol = (byte)(pColor + ((tempZ >> 4) & 31));
                }

                /* Draw the pixel! */
                if (off >= 640 && off < (320 * 200) - 640)
                {
                    surf[off] = tempCol;

                    tempCol += 72;
                    surf[off - 1] = tempCol;
                    surf[off + 1] = tempCol;
                    surf[off - 320] = tempCol;
                    surf[off + 320] = tempCol;

                    tempCol += 72;
                    surf[off - 2] = tempCol;
                    surf[off + 2] = tempCol;
                    surf[off - 640] = tempCol;
                    surf[off + 640] = tempCol;
                }
            }
        }

        if (newkey)
        {
            bool shift = keysactive[(int)UnityEngine.KeyCode.LeftShift] || keysactive[(int)UnityEngine.KeyCode.RightShift];
                switch (lastkey_sym)
            {
                case UnityEngine.KeyCode.Equals:
                    if (!shift)
                        break;
                    goto case UnityEngine.KeyCode.Plus;
                case UnityEngine.KeyCode.Plus:
                case UnityEngine.KeyCode.KeypadPlus:
                    starlib_speed++;
                    speedChange = 0;
                    break;
                case UnityEngine.KeyCode.Minus:
                case UnityEngine.KeyCode.KeypadMinus:
                    starlib_speed--;
                    speedChange = 0;
                    break;
                case UnityEngine.KeyCode.Alpha1:
                case UnityEngine.KeyCode.Keypad1:
                    JE_changeSetup(shift ? 11: 1);
                    break;
                case UnityEngine.KeyCode.Alpha2:
                case UnityEngine.KeyCode.Keypad2:
                    JE_changeSetup(shift ? 12 : 2);
                    break;
                case UnityEngine.KeyCode.Alpha3:
                case UnityEngine.KeyCode.Keypad3:
                    JE_changeSetup(shift ? 13 : 3);
                    break;
                case UnityEngine.KeyCode.Alpha4:
                case UnityEngine.KeyCode.Keypad4:
                    JE_changeSetup(shift ? 14 : 4);
                    break;
                case UnityEngine.KeyCode.Alpha5:
                case UnityEngine.KeyCode.Keypad5:
                    JE_changeSetup(5);
                    break;
                case UnityEngine.KeyCode.Alpha6:
                case UnityEngine.KeyCode.Keypad6:
                    JE_changeSetup(6);
                    break;
                case UnityEngine.KeyCode.Alpha7:
                case UnityEngine.KeyCode.Keypad7:
                    JE_changeSetup(7);
                    break;
                case UnityEngine.KeyCode.Alpha8:
                case UnityEngine.KeyCode.Keypad8:
                    JE_changeSetup(8);
                    break;
                case UnityEngine.KeyCode.Alpha9:
                case UnityEngine.KeyCode.Keypad9:
                    JE_changeSetup(9);
                    break;
                case UnityEngine.KeyCode.Alpha0:
                case UnityEngine.KeyCode.Keypad0:
                    JE_changeSetup(10);
                    break;
                case UnityEngine.KeyCode.C:
                    JE_resetValues();
                    break;
                case UnityEngine.KeyCode.S:
                    nspVarVarInc = mt_rand_1() * 0.01f - 0.005f;
                    break;
                case UnityEngine.KeyCode.X:
                case UnityEngine.KeyCode.Backspace:
                    run = false;
                    break;
                case UnityEngine.KeyCode.LeftBracket:
                    pColor -= (byte)(shift ? 72 : 1);
                    break;
                case UnityEngine.KeyCode.RightBracket:
                    pColor += (byte)(shift ? 72 : 1);
                    break;
                case UnityEngine.KeyCode.BackQuote:
                    doChange = !doChange;
                    break;
                case UnityEngine.KeyCode.P:
                    yield return coroutine_wait_noinput(true, false, false);
                    yield return coroutine_wait_input(true, false, false);
                    break;
                default:
                    break;
            }
        }

        if (doChange)
        {
            stepCounter++;
            if (stepCounter > changeTime)
            {
                JE_changeSetup(0);
            }
        }

        if ((mt_rand() % 1000) == 1)
        {
            nspVarVarInc = mt_rand_1() * 0.01f - 0.005f;
        }

        nspVarInc += nspVarVarInc;
    }

    private static void JE_wackyCol()
    {
        /* YKS: Does nothing */
    }

    private static JE_boolean initialized = false;
    public static void JE_starlib_init()
    {

        if (!initialized)
        {
            initialized = true;

            JE_resetValues();
            JE_changeSetup(2);
            doChange = true;

            /* RANDOMIZE; */
            for (int x = 0; x < starlib_MAX_STARS; x++)
            {
                star[x].spX = (short)((mt_rand() % 64000) - 32000);
                star[x].spY = (short)((mt_rand() % 40000) - 20000);
                star[x].spZ = (short)(x + 1);
            }
        }
    }

    private static void JE_resetValues()
    {
        nsp2 = 1;
        nspVar2Inc = 1;
        nspVarInc = 0.1f;
        nspVarVarInc = 0.0001f;
        nsp = 0;
        pColor = 32;
        starlib_speed = 2;
        speedChange = 0;
    }

    private static void JE_changeSetup(int setupType)
    {
        stepCounter = 0;
        changeTime = (ushort)(mt_rand() % 1000);

        if (setupType > 0)
        {
            setup = (byte)setupType;
        }
        else
        {
            setup = (byte)(mt_rand() % (MAX_TYPES + 1));
        }

        if (setup == 1)
        {
            nspVarInc = 0.1f;
        }
        if (nspVarInc > 2.2f)
        {
            nspVarInc = 0.1f;
        }
    }

    private static void JE_newStar()
    {
        if (setup == 0)
        {
            tempX = (int)((mt_rand() % 64000) - 32000);
            tempY = (int)((mt_rand() % 40000) - 20000);
        }
        else
        {
            nsp = nsp + nspVarInc; /* YKS: < lol */
            switch (setup)
            {
                case 1:
                    tempX = (int)(Sin(nsp / 30) * 20000);
                    tempY = (int)((mt_rand() % 40000) - 20000);
                    break;
                case 2:
                    tempX = (int)(Cos(nsp) * 20000);
                    tempY = (int)(Sin(nsp) * 20000);
                    break;
                case 3:
                    tempX = (int)(Cos(nsp * 15) * 100) * ((int)(nsp / 6) % 200);
                    tempY = (int)(Sin(nsp * 15) * 100) * ((int)(nsp / 6) % 200);
                    break;
                case 4:
                    tempX = (int)(Sin(nsp / 60) * 20000);
                    tempY = (int)(Cos(nsp) * (int)(Sin(nsp / 200) * 300) * 100);
                    break;
                case 5:
                    tempX = (int)(Sin(nsp / 2) * 20000);
                    tempY = (int)(Cos(nsp) * (int)(Sin(nsp / 200) * 300) * 100);
                    break;
                case 6:
                    tempX = (int)(Sin(nsp) * 40000);
                    tempY = (int)(Cos(nsp) * 20000);
                    break;
                case 8:
                    tempX = (int)(Sin(nsp / 2) * 40000);
                    tempY = (int)(Cos(nsp) * 20000);
                    break;
                case 7:
                    tempX = (int)(mt_rand() % 65535);
                    if ((mt_rand() % 2) == 0)
                    {
                        tempY = (int)(Cos(nsp / 80) * 10000) + 15000;
                    }
                    else
                    {
                        tempY = 50000 - (int)(Cos(nsp / 80) * 13000);
                    }
                    break;
                case 9:
                    nsp2 = (ushort)(nsp2 + nspVar2Inc);
                    if ((nsp2 == 65535) || (nsp2 == 0))
                    {
                        nspVar2Inc = (sbyte)(-nspVar2Inc);
                    }
                    tempX = (int)(Cos(Sin(nsp2 / 10.0f) + (nsp / 500)) * 32000);
                    tempY = (int)(Sin(Cos(nsp2 / 10.0f) + (nsp / 500)) * 30000);
                    break;
                case 10:
                    nsp2 = (ushort)(nsp2 + nspVar2Inc);
                    if ((nsp2 == 65535) || (nsp2 == 0))
                    {
                        nspVar2Inc = (sbyte)(-nspVar2Inc);
                    }
                    tempX = (int)(Cos(Sin(nsp2 / 5.0f) + (nsp / 100)) * 32000);
                    tempY = (int)(Sin(Cos(nsp2 / 5.0f) + (nsp / 100)) * 30000);
                    break; ;
                case 11:
                    nsp2 = (ushort)(nsp2 + nspVar2Inc);
                    if ((nsp2 == 65535) || (nsp2 == 0))
                    {
                        nspVar2Inc = (sbyte)(-nspVar2Inc);
                    }
                    tempX = (int)(Cos(Sin(nsp2 / 1000.0f) + (nsp / 2)) * 32000);
                    tempY = (int)(Sin(Cos(nsp2 / 1000.0f) + (nsp / 2)) * 30000);
                    break;
                case 12:
                    if (nsp != 0)
                    {
                        nsp2 = (ushort)(nsp2 + nspVar2Inc);
                        if ((nsp2 == 65535) || (nsp2 == 0))
                        {
                            nspVar2Inc = (sbyte)(-nspVar2Inc);
                        }
                        tempX = (int)(Cos(Sin(nsp2 / 2.0f) / (Sqrt(Abs(nsp)) / 10.0f + 1) + (nsp2 / 100.0f)) * 32000);
                        tempY = (int)(Sin(Cos(nsp2 / 2.0f) / (Sqrt(Abs(nsp)) / 10.0f + 1) + (nsp2 / 100.0f)) * 30000);
                    }
                    break;
                case 13:
                    if (nsp != 0)
                    {
                        nsp2 = (ushort)(nsp2 + nspVar2Inc);
                        if ((nsp2 == 65535) || (nsp2 == 0))
                        {
                            nspVar2Inc = (sbyte)(-nspVar2Inc);
                        }
                        tempX = (int)(Cos(Sin(nsp2 / 10.0f) / 2 + (nsp / 20)) * 32000);
                        tempY = (int)(Sin(Sin(nsp2 / 11.0f) / 2 + (nsp / 20)) * 30000);
                    }
                    break;
                case 14:
                    nsp2 = (ushort)(nsp2 + nspVar2Inc);
                    tempX = (int)((Sin(nsp) + Cos(nsp2 / 1000.0f) * 3) * 12000);
                    tempY = (int)(Cos(nsp) * 10000) + nsp2;
                    break;
            }
        }
    }
}