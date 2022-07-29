Shader "Unlit/SpriteIn3D"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
 
    SubShader
    {
        Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
 
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
 
            v2f vert(appdata v)
            {
                v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);

                o.uv = v.uv.xy;
 
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
 
                return col;
            }
            ENDCG
        }
    }
}
