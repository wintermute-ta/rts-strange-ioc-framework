Shader "Custom/Terrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Terrain Texture Array", 2DArray) = "white" {}
		_GridTex("Grid Texture", 2D) = "white" {}
		//_Glossiness("Smoothness", Range(0,1)) = 0.5
		//_Metallic ("Metallic", Range(0,1)) = 0.0
		_CellPerturbStrength("Cell Perturb Strength", float) = 0.0
		_Point("a point in world space", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows vertex:vert
		#pragma target 3.5

		UNITY_DECLARE_TEX2DARRAY(_MainTex);

		sampler2D _GridTex;
		//half _Glossiness;
		//half _Metallic;
		fixed4 _Color;
		float4 _Point;
		float _CellPerturbStrength;

		struct Input {
			float4 color : COLOR;
			float3 worldPos;
			float3 terrain;
		};

		void vert (inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
			data.terrain = v.texcoord2.xyz;
		}

		float4 GetTerrainColor (Input IN, int index) {
			float3 uvw = float3(IN.worldPos.xz * 0.02, IN.terrain[index]);
			float4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uvw);
			return c * IN.color[index];
		}

		float3 GetHexCoord(float3 pos)
		{
			float outerRadius = 10.0;
			float outerToInner = 0.866025404;
			float innerRadius = outerRadius * outerToInner;
			
			float3 position = float3(pos.x - _CellPerturbStrength, pos.y, pos.z - _CellPerturbStrength);

			float x = position.x / (innerRadius * 2.0);
			float y = -x;

			float offset = position.z / (outerRadius * 3.0);
			x -= offset;
			y -= offset;

			float iX = round(x);
			float iY = round(y);
			float iZ = round(-x - y);

			if (iX + iY + iZ != 0)
			{
				float dX = abs(x - iX);
				float dY = abs(y - iY);
				float dZ = abs(-x - y - iZ);

				if (dX > dY && dX > dZ)
				{
					iX = -iY - iZ;
				}
				else if (dZ > dY)
				{
					iZ = -iX - iY;
				}
			}

			return float3(iX, iY, iZ);
		}

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c =
				GetTerrainColor(IN, 0) +
				GetTerrainColor(IN, 1) +
				GetTerrainColor(IN, 2);

			float2 gridUV = float2(IN.worldPos.x - _CellPerturbStrength, IN.worldPos.z - _CellPerturbStrength);
			gridUV.x *= 1 / (4 * 8.66025404);
			gridUV.y *= 1 / (2 * 15.0);
			fixed4 grid = tex2D(_GridTex, gridUV);
			
			o.Albedo = (c.rgb * (1.0 - grid.a) + grid.rgb * grid.a) * _Color;
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			float3 hexCoord = GetHexCoord(IN.worldPos);
			if ((hexCoord.x == _Point.x) && (hexCoord.z == _Point.z) && (_Point.w == 1.0))
			{
				o.Albedo = float3(0.0, 0.0, 0.0);
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}