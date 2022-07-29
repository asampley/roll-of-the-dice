Shader "Unlit/TileMap"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
		_AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		_YSpread("Y Spread", Float) = 4.0
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
			float _YSpread;
 
            v2f vert(appdata v)
            {
                v2f o;

				o.pos = v.vertex;
				o.pos.z = _YSpread * mul(UNITY_MATRIX_M, float4(0, 0, 0, 1)).y;

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
            ENDCG
        }
    }
}
