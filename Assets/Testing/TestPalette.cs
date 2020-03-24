using UnityEngine;

using static VideoC;
using static OpenTyrC;
using static LoudnessC;
using static ParamsC;
using static XmasC;
using UnityEngine.Serialization;

public class TestPalette : MonoBehaviour
{
    public Material TargetMaterial;
    
    public bool RichMode;
    public bool ConstantPlay;
    public bool ConstantDie;
    public bool Xmas;
    public bool TouchscreenOverride;
    
    void Start()
    {
        richMode = RichMode;
        constantPlay = ConstantPlay;
        constantDie = ConstantDie;
        xmasOverride = Xmas;
        if (TouchscreenOverride)
        {
            MouseC.touchscreen = true;
        }

        MusicPlayer = new GameObject("Music Player").AddComponent<AudioSource>();
        MusicPlayer.loop = true;

        AudioSource[] channels = new AudioSource[8];
        for (int i = 0; i < channels.Length; ++i)
        {
            channels[i] = new GameObject("Channel " + (i + 1)).AddComponent<AudioSource>();
        }
        LoudnessC.SampleChannels = channels;

        Texture2D tex = new Texture2D(VGAScreen.w, VGAScreen.h, TextureFormat.Alpha8, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        ScreenTexture = tex;
        JE_showVGA();
        TargetMaterial.mainTexture = tex;

        Texture2D pal = new Texture2D(256, 1, TextureFormat.RGB24, false);
        pal.filterMode = FilterMode.Point;
        pal.wrapMode = TextureWrapMode.Clamp;
        PaletteTexture = pal;
        TargetMaterial.SetTexture("_PalTex", pal);

        StartCoroutine(e_main(0, new string[0]));
    }

    private void OnDestroy()
    {
        deinit_audio();
    }

    public void Escape()
    {
        KeyboardC.OverrideEscapePress = true;
    }
}
