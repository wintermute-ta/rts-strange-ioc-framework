Shader "Custom/Highlight" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Transparent"  "Queue" = "Transparent" }

		LOD 200
		//Cull off
		ZTest Off
		//ZWrite Off // don't write to depth buffer 
				   // in order not to occlude other objects

		Blend SrcAlpha OneMinusSrcAlpha // use alpha blending

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		//#pragma target 3.0

		//sampler2D _MainTex;

		struct Input {
			//fixed2 uv_MainTex;
			fixed4 color : COLOR;
		};


		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.color;
			o.Alpha = IN.color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
