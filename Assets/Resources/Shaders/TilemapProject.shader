Shader "Unlit/TileMapProject"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
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
            CGPROGRAM
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
                float4 original : POSITION1;
            };

            sampler2D _MainTex;
            float4 _Color;
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

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;
                o.original = v.vertex;

                return o;
            }

            fixed4 frag(v2f i, out float depth : SV_Depth) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // clip if the alpha is very low
                clip(col.a - _AlphaCutoff);

                // get iso cordinates (unknown offset reasons atm)
                float2 iso = mul(_ToIso, i.original.xy) + float2(-0.5, -0.5);

                // tile increasing numbers up and decreasing numbers down
                float z = _ZSpread * (round(iso.x) + round(iso.y));

                depth = UnityObjectToClipPos(float4(i.original.x, i.original.y, z, 1)).z;

                return col;
            }
            ENDCG
        }
    }
}
