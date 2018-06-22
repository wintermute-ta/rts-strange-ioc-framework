Shader "Custom/Terrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Terrain Texture Atlas", 2D) = "white" {}
		_GridTex("Grid Texture", 2D) = "white" {}
		//_Glossiness("Smoothness", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0
		_CellPerturbStrength("Cell Perturb Strength", float) = 0.0
		_Params("External params", Vector) = (0, 0, 0, 0)
		_Point("A point in world space", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows vertex:vert

		sampler2D _MainTex;
		sampler2D _GridTex;
		//half _Glossiness;
		//half _Metallic;
		fixed4 _Color;
		fixed4 _Point;
		half _CellPerturbStrength;
		float4 _Params;

		struct Input {
			fixed4 color : COLOR;
			half3 worldPos;
            fixed4 mainTex_Blend_1_2;
			fixed2 mainTex_Blend_3;
		};

		void vert (inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
            data.mainTex_Blend_1_2 = v.texcoord;
            data.mainTex_Blend_3 = v.texcoord1.xy;
		}


		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.mainTex_Blend_1_2.xy) * IN.color[0] + tex2D(_MainTex, IN.mainTex_Blend_1_2.zw) * IN.color[1] + tex2D(_MainTex, IN.mainTex_Blend_3.xy) * IN.color[2];

			half2 gridUV = half2(IN.worldPos.x - _CellPerturbStrength, IN.worldPos.z - _CellPerturbStrength);
			gridUV.xy *= _Params.xy;
			fixed4 grid = tex2D(_GridTex, gridUV);
			o.Albedo = (c.rgb * (1.0 - grid.a) + grid.rgb * grid.a) * _Color.rgb;

			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			//fixed3 hexCoord = GetHexCoord(IN.worldPos);
			//if ((hexCoord.x == _Point.x) && (hexCoord.z == _Point.z) && (_Point.w == 1.0))
			//{
			//	o.Albedo = fixed3(0.0, 0.0, 0.0);
			//}
		}
		ENDCG
	}
	FallBack "Diffuse"
}