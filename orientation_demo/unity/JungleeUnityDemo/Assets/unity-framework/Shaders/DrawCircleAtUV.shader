Shader "XG/Draw Circle at UV" {

/*
    Draws a circle at given UV with given radius & color.
*/
 Properties {
     _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _UV ("UV", Vector) = (0,0,0,0)
        _Color ("Color", Color) = (1,0,0,1)
        _Radius ("Radius", Range(0,1)) = 0.5
        _Width ("Width", Range(0,1)) = 0.5
 }
 SubShader {
     Tags { 
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent" 
            }
     LOD 200

     CGPROGRAM
     // Physically based Standard lighting model, and enable shadows on all light types
     #pragma surface surf Standard alpha:fade

     // Use shader model 3.0 target, to get nicer looking lighting
     #pragma target 3.0

     sampler2D _MainTex;
        float4 _UV;
        fixed4 _Color;
        float _Radius;
        float _Width;

     struct Input {
            //UV of the main texture
         float2 uv_MainTex;
     };

     void surf (Input IN, inout SurfaceOutputStandard o) {

            float d = distance(_UV, IN.uv_MainTex);

            if((d > _Radius) && (d <(_Radius + _Width)))
            {
                o.Albedo = _Color;
                o.Alpha = 1;
            }
            else
            {
                float4 c = tex2D(_MainTex, IN.uv_MainTex);
                o.Albedo = c.rgb;
                o.Alpha = c.a;
            }
     }
     ENDCG
 }
 FallBack "Diffuse"
}
