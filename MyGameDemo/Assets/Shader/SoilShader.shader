Shader "Custom/SoilShader"  
{  
    Properties  
    {  
        _MainTex ("Texture", 2D) = "white" {}  
        _BumpMap ("Normal Map", 2D) = "bump" {}  
        _SoilColor ("Soil Color", Color) = (0.6, 0.4, 0.2, 1)  
        _Roughness ("Roughness", Range(0, 1)) = 0.5  
        _Metallic ("Metallic", Range(0, 1)) = 0.0  
    }  
    SubShader  
    {  
        Tags { "RenderType"="Opaque" }  
        LOD 200  
  
        CGPROGRAM  
        // Physically based Standard lighting model, and enable shadows on all light types  
        #pragma surface surf Standard fullforwardshadows  
  
        // Use shader model 3.0 target, to get nicer looking lighting  
        #pragma target 3.0  
  
        sampler2D _MainTex;  
        sampler2D _BumpMap;  
        fixed4 _SoilColor;  
        half _Roughness;  
        half _Metallic;  
  
        struct Input  
        {  
            float2 uv_MainTex;  
            float2 uv_BumpMap;  
        };  
  
        void surf (Input IN, inout SurfaceOutputStandard o)  
        {  
            // Albedo comes from a texture tinted by color  
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _SoilColor;  
            o.Albedo = c.rgb;  
            o.Alpha = c.a;  
  
            // Normal information from a bump map  
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));  
  
            // Metallic and smoothness come from slider variables  
            o.Metallic = _Metallic;  
            o.Smoothness = _Roughness;  
        }  
        ENDCG  
    }  
    FallBack "Diffuse"  
}