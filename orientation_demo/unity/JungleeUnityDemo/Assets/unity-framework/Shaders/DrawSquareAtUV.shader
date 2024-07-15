Shader "XG/Draw Square at UV" {

/*
    Draws a square at given UV with given size & color.
*/
 Properties {
     _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _U ("U", Range(0,1)) = 0
        _V ("V", Range(0,1)) = 0
        _Color ("Color", Color) = (1,0,0,1)
        _Width ("Width", Range(0,1)) = 0.5
 }
 SubShader {
     Tags { "RenderType"="Opaque" }
     LOD 200

     CGPROGRAM
     // Physically based Standard lighting model, and enable shadows on all light types
     #pragma surface surf Standard fullforwardshadows

     // Use shader model 3.0 target, to get nicer looking lighting
     #pragma target 3.0

     sampler2D _MainTex;
        float _U;
        float _V;
        fixed4 _Color;
        float _Width;

     struct Input {
            //UV of the main texture
         float2 uv_MainTex;
     };

     // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
     // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
     // #pragma instancing_options assumeuniformscaling
     UNITY_INSTANCING_BUFFER_START(Props)
         // put more per-instance properties here
     UNITY_INSTANCING_BUFFER_END(Props)

     void surf (Input IN, inout SurfaceOutputStandard o) {
     half halfWidth = _Width / 2;
            if(((IN.uv_MainTex.x >= (_U-halfWidth)) && (IN.uv_MainTex.x <= _U + halfWidth)) && ((IN.uv_MainTex.y >= (_V-halfWidth)) && (IN.uv_MainTex.y <= _V + halfWidth)))
            {
                o.Albedo = _Color;
            }
            else
            {
                o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
            }
     }
     ENDCG
 }
 FallBack "Diffuse"
}
