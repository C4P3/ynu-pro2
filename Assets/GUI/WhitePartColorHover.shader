Shader "Custom/WhitePartColorHoverClickDarkenSaturation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HoverColor ("Hover Color", Color) = (1, 0, 0, 1)
        _NormalColor ("Normal Color", Color) = (0, 0, 0, 0)
        _Tolerance ("Tolerance", Range(0, 0.5)) = 0.05
        _IsHover ("Is Hover", Float) = 0
        _ClickDarkness ("Click Darkness", Range(0.5, 1.0)) = 1.0
        _ClickSaturation ("Click Saturation", Range(1.0, 2.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _HoverColor;
            float4 _NormalColor;
            float _Tolerance;
            float _IsHover;
            float _ClickDarkness;
            float _ClickSaturation;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 白色判定
                if (col.r > 1.0 - _Tolerance && col.g > 1.0 - _Tolerance && col.b > 1.0 - _Tolerance)
                {
                    col.rgb = lerp(_NormalColor.rgb, _HoverColor.rgb, _IsHover);
                }

                // 暗くする
                col.rgb *= _ClickDarkness;

                // 彩度上げ処理
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114)); // 輝度取得（NTSC係数）
                col.rgb = lerp(float3(gray, gray, gray), col.rgb, _ClickSaturation);

                return col;
            }
            ENDCG
        }
    }
}
