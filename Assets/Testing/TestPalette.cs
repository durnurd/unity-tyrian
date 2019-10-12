using UnityEngine;

using System.Linq;

using static VideoC;
using static OpenTyrC;

public class TestPalette : MonoBehaviour
{
    public Material TargetMaterial;

    public AudioClip[] Songs;
    public bool[] Loops;

    public AudioSource Player;

    void Start()
    {
        LoudnessC.Songs = Songs;
        LoudnessC.Loops = Loops;
        LoudnessC.SongPlayer = Player;

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
        TargetMaterial.mainTexture = tex;

        Texture2D pal = new Texture2D(256, 1, TextureFormat.RGB24, false);
        pal.filterMode = FilterMode.Point;
        pal.wrapMode = TextureWrapMode.Clamp;
        PaletteTexture = pal;
        TargetMaterial.SetTexture("_PalTex", pal);

        StartCoroutine(e_main(0, new string[0]));
    }
}
