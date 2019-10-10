using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;
using UnityEngine;

using static LoudnessC;
using static SndMastC;
using static KeyboardC;
using static JoystickC;
using static FileIO;

using System.IO;

public static class NortsongC {
    public static uint target, target2;

    public static JE_boolean notYetLoadedSound = true;

    public static JE_word frameCount, frameCount2, frameCountMax;

    public static JE_byte[][] digiFx = new JE_byte[SAMPLE_COUNT][]; /* [1..soundnum + 9] */
    public static JE_word[] fxSize = new JE_word[SAMPLE_COUNT]; /* [1..soundnum + 9] */

    public static int tyrMusicVolume, fxVolume;
    public static int fxPlayVol;
    public static int tempVolume;

    public static JE_word speed; /* JE: holds timer speed for 70Hz */

    private static float jasondelay = 1000.0f / (1193180.0f / 0x4300);

    public static void setdelay(JE_byte delay)
    {
        target = (uint)((delay * 16) + SDL_GetTicks());
    }

    public static void setjasondelay(int delay)
    {
        target = (uint)(SDL_GetTicks() + delay * jasondelay);
    }

    public static void setjasondelay2(int delay)
    {
        target2 = (uint)(SDL_GetTicks() + delay * jasondelay);
    }

    public static uint delaycount()
    {
        return (SDL_GetTicks() < target ? target - SDL_GetTicks() : 0);
    }

    public static uint delaycount2()
    {
        return (SDL_GetTicks() < target2 ? target2 - SDL_GetTicks() : 0);
    }

    public static bool b_wait_delay()
    {
        return Time.time * 1000 < target;
    }

    public static WaitForSeconds coroutine_wait_delay()
    {
        return new WaitForSeconds(target / 1000.0f - Time.time);
    }

    public static uint SDL_GetTicks()
    {
        return (uint)(Time.time * 1000);
    }

    public static WaitWhile coroutine_service_wait_delay()
    {
        return new WaitWhile(() =>
        {
            service_SDL_events(false);
            return SDL_GetTicks() < target;
        });
    }

    public static WaitWhile coroutine_wait_delayorinput(JE_boolean keyboard, JE_boolean mouse, JE_boolean joystick)
    {
        service_SDL_events(true);
        return new WaitWhile(() => {
            if (!b_wait_delay())
                return false;
            push_joysticks_as_keyboard();
            service_SDL_events(false);
#if WITH_NETWORK
            if (isNetworkGame)
                network_check();
#endif
            return !((keyboard && keydown) || (mouse && mousedown) || (joystick && joydown));
        });
    }

    public static void JE_loadSndFile(string effects_sndfile, string voices_sndfile)
    {

        JE_byte y, z;
        JE_word x;
        JE_longint templ;
        JE_longint[][] sndPos = new[] { new JE_longint[SAMPLE_COUNT + 1], new JE_longint[SAMPLE_COUNT + 1] };
        JE_word sndNum;

        BinaryReader fi;

        /* SYN: Loading offsets into TYRIAN.SND */
        fi = open(effects_sndfile);
        sndNum = fi.ReadUInt16();

        for (x = 0; x < sndNum; x++)
        {
            sndPos[0][x] = fi.ReadInt32();
        }

        sndPos[0][sndNum] = (int)fi.BaseStream.Length; /* Store file size */

        for (z = 0; z < sndNum; z++)
        {
            fi.BaseStream.Seek(sndPos[0][z], SeekOrigin.Begin);
            fxSize[z] = (JE_word)(sndPos[0][z + 1] - sndPos[0][z]); /* Store sample sizes */
            digiFx[z] = fi.ReadBytes(fxSize[z]); /* JE: Load sample to buffer */
        }

        fi.Close();

        /* SYN: Loading offsets into VOICES.SND */
        fi = open(voices_sndfile);

        sndNum = fi.ReadUInt16();

        for (x = 0; x < sndNum; x++)
        {
            sndPos[1][x] = fi.ReadInt32();
        }

        sndPos[1][sndNum] = (int)fi.BaseStream.Length; /* Store file size */

        z = SAMPLE_COUNT - 9;

        for (y = 0; y < sndNum; y++)
        {
            fi.BaseStream.Seek(sndPos[1][y], SeekOrigin.Begin);

            templ = (sndPos[1][y + 1] - sndPos[1][y]) - 100; /* SYN: I'm not entirely sure what's going on here. */
            if (templ < 1) templ = 1;
            fxSize[z + y] = (JE_word)templ; /* Store sample sizes */
            digiFx[z + y] = fi.ReadBytes(fxSize[z + y]); /* JE: Load sample to buffer */
        }

        fi.Close();

        notYetLoadedSound = false;

    }

    public static void JE_playSampleNum(int samplenum)
    {
        JE_multiSamplePlay(digiFx[samplenum - 1], fxSize[samplenum - 1], 0, (JE_byte)fxPlayVol);
    }
    public static void JE_calcFXVol() // TODO: not sure *exactly* what this does
    {
        fxPlayVol = (fxVolume - 1) >> 5;
    }

    public static void JE_setTimerInt()
    {
        jasondelay = 1000.0f / (1193180.0f / speed);
    }

    public static void JE_resetTimerInt()
    {
        jasondelay = 1000.0f / (1193180.0f / 0x4300);
    }
    public static void JE_changeVolume(ref int music, int music_delta, ref int sample, int sample_delta)
    {
        int music_temp = music + music_delta,
            sample_temp = sample + sample_delta;

        if (music_delta != 0)
        {
            if (music_temp > 255)
            {
                music_temp = 255;
                JE_playSampleNum(S_CLINK);
            }
            else if (music_temp < 0)
            {
                music_temp = 0;
                JE_playSampleNum(S_CLINK);
            }
        }

        if (sample_delta != 0)
        {
            if (sample_temp > 255)
            {
                sample_temp = 255;
                JE_playSampleNum(S_CLINK);
            }
            else if (sample_temp < 0)
            {
                sample_temp = 0;
                JE_playSampleNum(S_CLINK);
            }
        }

        music = music_temp;
        sample = sample_temp;

        JE_calcFXVol();

        set_volume(music, sample);
    }
}