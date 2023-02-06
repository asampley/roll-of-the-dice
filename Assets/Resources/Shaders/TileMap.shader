Shader "Unlit/TileMap"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _ZSpread("Z Spread", Float) = 1.0
    }

    SubShader
    {
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }

        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define mod(x,y) (x-y*floor(x/y))

            struct appdata
            {
                float4 vertex : POSITION0;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _AlphaCutoff;
            float _ZSpread;

            // basis transform to isometric ((1,0) iso at (0.5, 0.25), (0,1) iso at (-0.5, 0.25)
            // {
            //      y01, -x01,
            //     -y10,  x10,
            // } / x10*y01 - x01*y10
            static const float2x2 _ToIso = {
                 1.0, 2.0,
                -1.0, 2.0,
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = v.vertex;

                // basis transform to isometric ((1,0) iso at (0.5, 0.25), (0,1) iso at (-0.5, 0.25)
                // {
                //      y01, -x01,
                //     -y10,  x10,
                // } / x10*y01 - x01*y10
                float2x2 toIso = {
                     1.0, 2.0,
                    -1.0, 2.0,
                };

                float2 iso = mul(toIso, mul(UNITY_MATRIX_M, float4(0, 0, 0, 1)).xy);
                o.pos.z = _ZSpread * (round(iso.x) + round(iso.y));

                o.pos = UnityObjectToClipPos(o.pos);

                o.uv = v.uv.xy;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // clip if the alpha is very low
                clip(col.a - _AlphaCutoff);

                return col;
            }
            ENDHLSL
        }
    }
}
