using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static NortsongC;
using static FileIO;
using static LdsPlayC;
using static OplC;

using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class LoudnessC
{
    public const int SFX_CHANNELS = 8;

    public const int BYTES_PER_SAMPLE = 2;

    public static float music_volume, sample_volume;

    public static int song_playing;

    public static bool audio_disabled, music_disabled, samples_disabled;

    private static bool music_stopped = true;

    /* SYN: These shouldn't be used outside this file. Hands off! */
    private static BinaryReader music_file;
    private static uint[] song_offset;
    private static ushort song_count = 0;

    private static ushort[][] channel_buffer = new ushort[SFX_CHANNELS][];
    private static ushort[][] channel_pos = new ushort[SFX_CHANNELS][];
    private static uint[] channel_pos_offset = new uint[SFX_CHANNELS];

    public static uint[] channel_len = new uint[SFX_CHANNELS];
    private static byte[] channel_vol = new JE_byte[SFX_CHANNELS];

    private const int freq = 44100;

    public static AudioSource MusicPlayer;
    public static AudioSource[] SampleChannels;
    private static AudioClip musicClip;

    public static bool init_audio()
    {
        if (audio_disabled)
            return false;

        musicClip = AudioClip.Create("music", 1024, 1, freq, true, audio_cb_unity);
        MusicPlayer.clip = musicClip;

        return true;
    }

    private static int ct;
    private static void audio_cb_unity(float[] feedme)
    {
        if (!music_disabled && !music_stopped)
        {
            int howmuch = feedme.Length;
            /* SYN: Simulate the fm synth chip */
            int feedmeIdx = 0;
            int remaining = howmuch;
            while (remaining > 0)
            {
                while (ct < 0)
                {
                    ct += freq;
                    lds_update(); /* SYN: Do I need to use the return value for anything here? */
                }
                /* SYN: Okay, about the calculations below. I still don't 100% get what's going on, but...
                - freq is samples/time as output by SDL.
                - REFRESH is how often the play proc would have been called in Tyrian. Standard speed is
                70Hz, which is the default value of 70.0f
                - ct represents the margin between play time (representing # of samples) and tick speed of
                the songs (70Hz by default). It keeps track of which one is ahead, because they don't
                synch perfectly. */

                /* set i to smaller of data requested by SDL and a value calculated from the refresh rate */
                int i = (int)((ct / REFRESH) + 4) & ~3;
                i = (i > remaining) ? remaining : i; /* i should now equal the number of samples we get */
                opl_update(feedme, feedmeIdx, i);
                feedmeIdx += i;
                remaining -= i;
                ct -= (int)(REFRESH * i);
            }

            /* Reduce the music volume. */
            //int qu = howmuch / BYTES_PER_SAMPLE;
            //for (int smp = 0; smp < qu; smp++)
            //{
            //    feedme[smp] = (short)(feedme[smp] * music_volume);
            //}
        }

        //If we were blending all audio into a single clip:
        //if (!samples_disabled)
        //{
        //    /* SYN: Mix sound channels and shove into audio buffer */
        //    for (int ch = 0; ch < SFX_CHANNELS; ch++)
        //    {
        //        float volume = sample_volume * (channel_vol[ch] / (float)SFX_CHANNELS);

        //        /* SYN: Don't copy more data than is in the channel! */
        //        uint qu = ((uint)howmuch > channel_len[ch] ? channel_len[ch] : (uint)howmuch) / BYTES_PER_SAMPLE;
        //        for (uint smp = 0; smp < qu; smp++)
        //        {
        //            int clip = (int)(feedme[smp] * 32768) + (int)(channel_pos[ch][smp + channel_pos_offset[ch]] * volume);
        //            feedme[smp] = ((clip > 0x7fff) ? 0x7fff : (clip <= -0x8000) ? -0x8000 : (short)clip) / 32768.0f;
        //        }

        //        channel_pos_offset[ch] += qu;
        //        channel_len[ch] -= qu * BYTES_PER_SAMPLE;

        //        /* SYN: If we've emptied a channel buffer, let's free the memory and clear the channel. */
        //        if (channel_len[ch] == 0)
        //        {
        //            channel_buffer[ch] = channel_pos[ch] = null;
        //            channel_pos_offset[ch] = 0;
        //        }
        //    }
        //}

        //TODO do conversion
        //SDL_ConvertAudio(&audio_cvt);
    }

    public static void deinit_audio()
    {
        if (audio_disabled)
            return;

        for (int i = 0; i < SFX_CHANNELS; i++)
        {
            channel_buffer[i] = channel_pos[i] = null;
            channel_pos_offset[i] = 0;
            channel_len[i] = 0;
        }

        lds_free();
        music_file.Close();
    }


    public static void load_music()
    {
        if (music_file == null)
        {
            music_file = open("music.mus");

            song_count = music_file.ReadUInt16();

            song_offset = new uint[song_count + 1];

            for (int i = 0; i < song_count; ++i)
                song_offset[i] = music_file.ReadUInt32();

            song_offset[song_count] = (uint)music_file.BaseStream.Length;
        }
    }

    private static void load_song(int song_num)
    {
        if (audio_disabled)
            return;

        if (song_num < song_count)
        {
            uint song_size = song_offset[song_num + 1] - song_offset[song_num];
            lds_load(music_file, song_offset[song_num], song_size);
        }
    }

    public static void play_song(int song_num)
    {
        if (song_num != song_playing)
        {
            load_song(song_num);
            song_playing = song_num;
            ResetAudioSource();
        }

        music_stopped = false;
    }

    public static void restart_song()
    {
        int temp = song_playing;
        song_playing = -1;
        play_song(temp);
    }

    public static void stop_song()
    {
        music_stopped = true;
        ResetAudioSource();
    }

    private static void ResetAudioSource()
    {
        MusicPlayer.Stop();
        MusicPlayer.Play();
    }

    public static void fade_song()
    {
        /* STUB: we have no implementation of this to port */
    }

    public static void set_volume(int music, int sample)
    {
        MusicPlayer.volume = music * (1.5f / 255.0f);
        music_volume = music * (1.5f / 255.0f);
        sample_volume = sample * (1.0f / 255.0f);
    }

    private static Dictionary<byte[], AudioClip> createdSounds;
    public static void JE_multiSamplePlay(byte[] buffer, JE_word size, JE_byte chan, JE_byte vol)
    {
        if (audio_disabled || samples_disabled)
            return;

        if (createdSounds == null)
            createdSounds = new Dictionary<JE_byte[], AudioClip>();
        if (!createdSounds.ContainsKey(buffer))
        {
            AudioClip clip = AudioClip.Create("snd" + createdSounds.Count, size, 1, 11025, false);
            float[] samples = buffer.Select(e => ((sbyte)e) / 128.0f).ToArray();
            clip.SetData(samples, 0);
            createdSounds[buffer] = clip;
        }
        AudioSource channel = SampleChannels[chan];
        channel.clip = createdSounds[buffer];
        channel.volume = sample_volume * ((vol + 1) / 8.0f);
        channel.Play();
    }
}