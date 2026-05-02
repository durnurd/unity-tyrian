using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class IMFPlayer : MonoBehaviour
{
    public TextAsset IMFFile;
    public int frequency = 44100;
    public float REFRESH = 280;

    public AudioSource MusicPlayer;
    private AudioClip musicClip;
    private BinaryReader f;

    // Start is called before the first frame update
    void Start()
    {
        f = new BinaryReader(new MemoryStream(IMFFile.bytes));

        musicClip = AudioClip.Create("music", 1024, 1, frequency, true, audio_cb_unity);
        OplC.opl_init((uint)frequency);

        MusicPlayer.clip = musicClip;
        MusicPlayer.Play();
    }

    private double inputDelay;
    private bool kill;
    private void audio_cb_unity(float[] feedme)
    {
        int howmuch = feedme.Length;
        
        int feedmeIdx = 0;
        int remaining = howmuch;

        while (remaining > 0 && !kill)
        {
            double seconds = (double)(feedme.Length - feedmeIdx) / frequency;
            double inputCycles = REFRESH * seconds;

            double inputAdvance = System.Math.Min(inputCycles, inputDelay);
            int outputAdvance = (int)System.Math.Ceiling(inputAdvance * frequency / REFRESH);
            if (outputAdvance > 0)
            {
                OplC.opl_update(feedme, feedmeIdx, outputAdvance);
                inputDelay -= inputAdvance;
                feedmeIdx += outputAdvance;
                remaining -= outputAdvance;
            }
            if (inputDelay <= 0)
            {
                UpdateAudio();
            }
        }
    }

    private void UpdateAudio()
    {
        if (f.BaseStream.Position > f.BaseStream.Length - 4)
        {
            f.Close();
            f = null;
            kill = true;
        }
        byte register = f.ReadByte();
        byte data = f.ReadByte();
        inputDelay = f.ReadUInt16();
        OplC.opl_write(register, data);
    }
}
