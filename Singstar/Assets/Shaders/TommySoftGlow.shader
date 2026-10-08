
Shader "UI/TommySoftGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (0.3, 1, 0.55, 1)
        _Radius ("Glow Radius", Range(0, 60)) = 20
        _Strength ("Glow Strength", Range(0, 1)) = 0.6

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "False"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _GlowColor;
            float _Radius;
            float _Strength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float centerAlpha = tex2D(_MainTex, i.uv).a;

                float glow = 0;

                // Four increasingly distant rings of samples.
                for (int ring = 1; ring <= 4; ring++)
                {
                    float fraction = ring / 4.0;
                    float distance = fraction * _Radius;

                    // Samples near the silhouette are brighter.
                    float falloff = pow(1.0 - fraction, 1.5);

                    // Sample around the character in 16 directions.
                    for (int direction = 0; direction < 16; direction++)
                    {
                        float angle = direction * 6.2831853 / 16.0;

                        float2 offset = float2(
                            cos(angle),
                            sin(angle)
                        ) * distance * _MainTex_TexelSize.xy;

                        float sampleAlpha =
                            tex2D(_MainTex, i.uv + offset).a;

                        glow = max(
                            glow,
                            sampleAlpha * falloff
                        );
                    }
                }

                // Prevent the effect covering opaque parts of Tommy.
                glow *= (1.0 - centerAlpha);

                fixed4 color = _GlowColor;
                color.a *= glow * _Strength;

                return color;
            }
            ENDCG
        }
    }
}
