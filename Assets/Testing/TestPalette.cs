using UnityEngine;

using static VideoC;
using static OpenTyrC;

public class TestPalette : MonoBehaviour
{
    public Material TargetMaterial;

    void Start()
    {
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
