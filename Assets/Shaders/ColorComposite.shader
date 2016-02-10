Shader "Custom/ColorComposite"
{
	Properties
	{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}


	CGINCLUDE

	#include "UnityCG.cginc"

	float3 Hue(float H)
	{
		float R = abs(H * 6 - 3) - 1;
		float G = 2 - abs(H * 6 - 2);
		float B = 2 - abs(H * 6 - 4);
		return saturate(float3(R, G, B));
	}

	float3 HSVtoRGB_(in float3 HSV)
	{
		return ((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z;
	}

	float3 GetUniqueColor(int id, int total)
	{
		float hue = (1.0f / total) * id;
		return HSVtoRGB_(float3(hue, 1, 1));
	}

	float d3_lab_xyz(float x)
	{
		return x > 0.206893034 ? x * x * x : (x - 4.0 / 29.0) / 7.787037;
	}

	float d3_xyz_rgb(float r)
	{
		return round(255 * (r <= 0.00304 ? 12.92 * r : 1.055 * pow(r, 1.0 / 2.4) - 0.055));
	}

	float3 d3_lab_rgb(float l, float a, float b)
	{
		float y = (l + 16.0) / 116.0;
		float x = y + a / 500.0;
		float z = y - b / 200.0;

		x = d3_lab_xyz(x) * 0.950470;
		y = d3_lab_xyz(y) * 1.0;
		z = d3_lab_xyz(z) * 1.088830;

		return float3(
			d3_xyz_rgb(3.2404542 * x - 1.5371385 * y - 0.4985314 * z),
			d3_xyz_rgb(-0.9692660 * x + 1.8760108 * y + 0.0415560 * z),
			d3_xyz_rgb(0.0556434 * x - 0.2040259 * y + 1.0572252 * z)
			);
	}

	float3 d3_hcl_lab(float h, float c, float l)
	{
		float d3_radians = 0.01745329252;

		/*if (isNaN(h)) h = 0;
		if (isNaN(c)) c = 0;*/
		//return d3_lab_rgb(l, cos(h *= d3_radians) * c, sin(h) * c);
		/*h = 50;
		c = 50;
		l = 50;*/
		return d3_lab_rgb(l, cos(h * d3_radians) * c, sin(h * d3_radians) * c) / 255;
	}

	struct AtomInfo
	{
		float atomIndex;
		float atomSymbolId;
		float residueSymbolId;
		float chainSymbolId;
	};

	struct ProteinInstanceInfo
	{
		float proteinIngredientType;
		float state;
		float z;
		float w;
	};

	struct ProteinIngredientInfo
	{
		float proteinIngredientType;
		float numChains;
		float chainColorStartIndex;
		float w;
	};

	StructuredBuffer<AtomInfo> _ProteinAtomInfos;
	StructuredBuffer<ProteinInstanceInfo> _ProteinInstanceInfo;
	StructuredBuffer<ProteinIngredientInfo> _ProteinIngredientProperties;

	//*****//

	StructuredBuffer<float4> _AtomColors;
	StructuredBuffer<float4> _AminoAcidColors;
	StructuredBuffer<float4> _ProteinIngredientsColors;
	StructuredBuffer<float4> _ProteinIngredientsChainColors;
	StructuredBuffer<float4> _IngredientGroupsColor;

	//*****//

	StructuredBuffer<float> _IngredientGroupsLerpFactors;
	StructuredBuffer<float4> _IngredientGroupsColorValues;
	StructuredBuffer<float4> _IngredientGroupsColorRanges;
	StructuredBuffer<float4> _ProteinIngredientsRandomValues;

	//*****//

	uniform Texture2D<int> _AtomIdBuffer;
	uniform Texture2D<int> _InstanceIdBuffer;

	//*****//

	void frag1(v2f_img i, out float4 color : COLOR0)
	{
		int2 uv = i.uv * _ScreenParams.xy;
				
		int atomId = _AtomIdBuffer[uv];
		int instanceId = _InstanceIdBuffer[uv];

		color = float4(0, 0, 0, 1);

		if (instanceId >= 0)
		{
			AtomInfo atomInfo = _ProteinAtomInfos[atomId];
			ProteinInstanceInfo proteinInstanceInfo = _ProteinInstanceInfo[instanceId];
			ProteinIngredientInfo proteinIngredientInfo = _ProteinIngredientProperties[proteinInstanceInfo.proteinIngredientType];

			// Predefined colors
			float4 atomColor = _AtomColors[atomInfo.atomSymbolId];
			float4 aminoAcidColor = _AminoAcidColors[atomInfo.residueSymbolId];
			float4 proteinIngredientsChainColors = _ProteinIngredientsChainColors[proteinIngredientInfo.chainColorStartIndex + atomInfo.chainSymbolId];
			float4 proteinIngredientsColors = _ProteinIngredientsColors[proteinInstanceInfo.proteinIngredientType];
			float4 ingredientGroupColor = _IngredientGroupsColor[proteinIngredientInfo.proteinIngredientType];
						
			color = ingredientGroupColor;
			return;

			////*******//

			float3 proteinRandomValues = _ProteinIngredientsRandomValues[proteinInstanceInfo.proteinIngredientType].xyz;
			float ingredientGroupsLerpFactors = _IngredientGroupsLerpFactors[proteinIngredientInfo.proteinIngredientType];
			float3 ingredientGroupsColorValues = _IngredientGroupsColorValues[proteinIngredientInfo.proteinIngredientType].xyz;
			float3 ingredientGroupsColorRanges = _IngredientGroupsColorRanges[proteinIngredientInfo.proteinIngredientType].xyz;

			float h = ingredientGroupsColorValues.x + (ingredientGroupsColorRanges.x) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
			float c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
			float l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
			
			color = float4(d3_hcl_lab(h, c, l), 1);
		}
		else
		{
			// abort the shader for this pixel
			discard;
		}
	}
	
	//void frag2(v2f_img i, out float4 color : COLOR0)
	//{
	//	int2 uv = i.uv * _ScreenParams.xy;

	//	int id = _IdTexture[uv];

	//	color = float4(1, 0, 0, 1);

	//	if (id >= 0)
	//	{
	//		float4 proteinInfo = _ProteinInstanceInfo[id];
	//		float4 ingredientGroupInfo = _IngredientProperties[proteinInfo.x];

	//		////*******//

	//		float ingredientGroupsLerpFactors = _IngredientGroupsLerpFactors[ingredientGroupInfo.x];

	//		float3 proteinRandomValues = _ProteinIngredientsRandomValues[proteinInfo.x].xyz;
	//		float3 ingredientGroupsColorValues = _IngredientGroupsColorValues[ingredientGroupInfo.x].xyz;
	//		float3 ingredientGroupsColorRanges = _IngredientGroupsColorRanges[ingredientGroupInfo.x].xyz;

	//		float h = ingredientGroupsColorValues.x + (ingredientGroupsColorRanges.x) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
	//		float c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
	//		float l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;

	//		color = float4(d3_hcl_lab(h, c, l), 1);
	//	}
	//	else
	//	{
	//		discard;
	//	}
	//}
	ENDCG
	
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }		

		Pass
		{
			ZTest Always

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert_img
			#pragma fragment frag1	

			#pragma target 5.0	
			#pragma only_renderers d3d11	
			
			ENDCG
		}

		/*Pass
		{
			ZTest Always

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert_img
			#pragma fragment frag2	

			#pragma target 5.0	
			#pragma only_renderers d3d11	

			ENDCG
				
		}*/
	}

	FallBack "Diffuse"
}