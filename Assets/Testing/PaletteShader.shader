Shader "Unlit/PaletteShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_PalTex ("Palette", 2D) = "white" {}
		_PalIdx ("Palette Index", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			sampler2D _PalTex;
            float4 _MainTex_ST;
			fixed _PalIdx;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 index = tex2D(_MainTex, i.uv);
				fixed indexA = index.a;

				fixed2 pos = fixed2(indexA, _PalIdx);
				fixed4 col = tex2D(_PalTex, pos);

                return col;
            }
            ENDCG
        }
    }
}
