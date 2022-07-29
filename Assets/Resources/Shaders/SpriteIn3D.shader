Shader "Unlit/SpriteIn3D"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
		_AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }
 
    SubShader
    {
        Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Geometry" "DisableBatching" = "True" }
 
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
				float4 color: COLOR0;
            };
			
			struct v2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float4 color: COLOR0;
			};

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _AlphaCutoff;
 
            v2f vert(appdata v)
            {
                v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);

                o.uv = v.uv.xy;
				o.color = v.color;
 
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

				// clip if the alpha is very low
				clip(col.a - _AlphaCutoff);
 
                return col;
            }
            ENDCG
        }
    }
}
