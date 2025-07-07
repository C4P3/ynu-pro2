Shader "Unlit/Multiple"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}      // ベース画像
        _MultiplyTex ("Multiply Texture", 2D) = "white" {}  // 乗算用画像
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

            sampler2D _MainTex;
            sampler2D _MultiplyTex;
            float4 _MainTex_ST;
            float4 _MultiplyTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uvMain : TEXCOORD0;
                float2 uvMultiply : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvMultiply = TRANSFORM_TEX(v.uv, _MultiplyTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, i.uvMain);
                fixed4 multiplyColor = tex2D(_MultiplyTex, i.uvMultiply);

                fixed4 finalColor = baseColor * multiplyColor;

                return finalColor;
            }
            ENDCG
        }
    }
}