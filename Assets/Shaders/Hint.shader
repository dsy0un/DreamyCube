Shader "Unlit/Hint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.5, 1, 1, 1)
        _Speed ("Scroll Speed", Float) = 1.0
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Speed;
            float _Alpha;
            float _TimeY;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // UV »Â∏ß
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.x -= _Time.y * _Speed;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                tex.rgb *= _Color.rgb;
                tex.a *= _Color.a * _Alpha;
                return tex;
            }
            ENDCG
        }
    }
}
