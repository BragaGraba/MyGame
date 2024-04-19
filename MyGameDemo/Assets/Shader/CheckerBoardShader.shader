Shader "Custom/CheckerBoardShader"
{
    Properties  
    {  
        _LightColor ("Light Color", Color) = (1, 1, 1, 1)  
        _DarkColor ("Dark Color", Color) = (0.2, 0.2, 0.2, 1)  
        _Scale ("Scale", Range(0.01, 10)) = 1  
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
  
            float4 _LightColor;  
            float4 _DarkColor;  
            float _Scale;  
  
            v2f vert (appdata v)  
            {  
                v2f o;  
                o.vertex = UnityObjectToClipPos(v.vertex);  
                o.uv = v.uv;  
                return o;  
            }  
  
            fixed4 frag (v2f i) : SV_Target  
            {  
                // 计算棋盘的格子索引（基于UV坐标和缩放因子）  
                float checkerIndex = floor(i.uv.x * _Scale) + floor(i.uv.y * _Scale);  
                // 使用格子索引的奇偶性来决定颜色  
                fixed4 col = (fmod(checkerIndex, 2.0) < 0.5) ? _LightColor : _DarkColor;  
                return col;  
            }  
            ENDCG  
        }  
    }  
    FallBack "Diffuse"
}
