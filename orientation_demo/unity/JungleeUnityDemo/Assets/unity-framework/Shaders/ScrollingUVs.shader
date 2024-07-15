Shader "XG/ScrollingUVs" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ScrollSpeedX ("Speed X", Range(-10,10)) = 2
		_ScrollSpeedY ("Speed Y", Range(-10,10)) = 2
       _U("U", Range(0,1)) = 0.5
        _V("V", Range(0,1)) = 0.5
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

		struct Input {
			float2 uv_MainTex;
		};

		fixed _ScrollSpeedX;
		fixed _ScrollSpeedY;
		fixed4 _Color;
                   float _U;
            float _V;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {

            fixed2 scrolledUV = IN.uv_MainTex;

            fixed xScrollValue = _ScrollSpeedX * _Time;
            fixed yScrollValue = _ScrollSpeedY * _Time;

            scrolledUV += fixed2(xScrollValue, yScrollValue);
            half4 c = tex2D(_MainTex, scrolledUV);
			o.Albedo = c.rgb * _Color;
			o.Alpha = c.a;

             if(IN.uv_MainTex.y > _V)
                {
                    o.Albedo = (0,0,0);
                }
                if(IN.uv_MainTex.x > _U)
                {
                    o.Albedo = (0,0,0);
                }
		}
		ENDCG
	}
	FallBack "Diffuse"
}
