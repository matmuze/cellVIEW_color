Shader "Custom/ColorComposite"
{
	Properties
	{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }		

		Pass
		{
			ZTest Always

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag	

			#pragma target 5.0	
			#pragma only_renderers d3d11	

			#include "UnityCG.cginc"
			#include "Helper.cginc"	
		
			uniform Texture2D<int> _IdTexture;
			uniform Texture2D<int> _AtomIdTexture;
			
			StructuredBuffer<float4> _ProteinAtomInfos;
			StructuredBuffer<float4> _ProteinInstanceInfo;
			StructuredBuffer<float4> _IngredientProperties;

			StructuredBuffer<float4> _AtomColors;
			StructuredBuffer<float4> _ProteinColors;
			StructuredBuffer<float4> _AminoAcidColors;
			StructuredBuffer<float4> _IngredientGroupsColor;

			//*****//

			StructuredBuffer<float> _IngredientGroupsLerpFactors;
			StructuredBuffer<float4> _IngredientGroupsColorValues;
			StructuredBuffer<float4> _IngredientGroupsColorRanges;
			StructuredBuffer<float4> _ProteinIngredientsRandomValues;
			
			//*****//

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

			//float HCLgamma = 3;
			//float HCLy0 = 100;
			//float HCLmaxL = 0.530454533953517; // == exp(HCLgamma / HCLy0) - 0.5
			

			/*float3 HCLtoRGB_(float3 HCL)
			{
				float3 RGB = 0;
				if (HCL.z != 0)
				{
					float H = HCL.x;
					float C = HCL.y;
					float L = HCL.z * HCLmaxL;
					float Q = exp((1 - C / (2 * L)) * (HCLgamma / HCLy0));
					float U = (2 * L - C) / (2 * Q - 1);
					float V = C / Q;
					float T = tan((H + min(frac(2 * H) / 4, frac(-2 * H) / 8)) * PI * 2);
					H *= 6;

					if (H <= 1)
					{
						RGB.r = 1;
						RGB.g = T / (1 + T);
					}
					else if (H <= 2)
					{
						RGB.r = (1 + T) / T;
						RGB.g = 1;
					}
					else if (H <= 3)
					{
						RGB.g = 1;
						RGB.b = 1 + T;
					}
					else if (H <= 4)
					{
						RGB.g = 1 / (1 + T);
						RGB.b = 1;
					}
					else if (H <= 5)
					{
						RGB.r = -1 / T;
						RGB.b = 1;
					}
					else
					{
						RGB.r = 1;
						RGB.b = -T;
					}

					return RGB * V + U;
				}
				
				return float3(1,1,0);
			}*/

			// D65 standard referent
			/*const float d3_lab_X = 0.950470;
			float d3_lab_Y = 1.0;
			float d3_lab_Z = 1.088830;

			float PI = 3.1415926536;*/
			

			float d3_lab_xyz(float x)
			{
				return x > 0.206893034 ? x * x * x : (x - 4.0 / 29.0) / 7.787037;
			}

			float d3_xyz_rgb(float r)
			{
				return round(255 * (r <= 0.00304 ? 12.92 * r : 1.055 * pow(r, 1.0 / 2.4) - 0.055));
			}

			float3 d3_lab_rgb(float l,float a,float b)
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


			void frag(v2f_img i, out float4 color : COLOR0)
			{
				int2 uv = i.uv * _ScreenParams.xy;

				int id = _IdTexture[uv];
				int atomId = _AtomIdTexture[uv];
				
				if (id >= 0)
				{		
					float4 atomInfo = _ProteinAtomInfos[atomId];
					float4 proteinInfo = _ProteinInstanceInfo[id];
					float4 ingredientGroupInfo = _IngredientProperties[proteinInfo.x];
					
					float4 atomColor = _AtomColors[atomInfo.y];
					float4 aminoAcidColor = _AminoAcidColors[atomInfo.z];
					float4 chainColor = float4(GetUniqueColor(atomInfo.w, 16),1);
					float4 proteinColor = _ProteinColors[proteinInfo.x];
					float4 ingredientGroupColor = _IngredientGroupsColor[ingredientGroupInfo.x];
					
					//*******//

					float ingredientGroupsLerpFactors = _IngredientGroupsLerpFactors[ingredientGroupInfo.x];

					float3 proteinRandomValues = _ProteinIngredientsRandomValues[proteinInfo.x].xyz;
					float3 ingredientGroupsColorValues = _IngredientGroupsColorValues[ingredientGroupInfo.x].xyz;
					float3 ingredientGroupsColorRanges = _IngredientGroupsColorRanges[ingredientGroupInfo.x].xyz;

					float h = ingredientGroupsColorValues.x + (ingredientGroupsColorRanges.x) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
					float c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
					//float c = lerp(ingredientGroupsColorValues.y, ingredientGroupsColorRanges.y, ingredientGroupsLerpFactors);
					float l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
					//float l = ingredientGroupsColorValues.z;

					/*if (false)
					{
						c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (proteinRandomValues.y - 0.5f) * ingredientGroupsLerpFactors;
						l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (proteinRandomValues.z - 0.5f) * ingredientGroupsLerpFactors;
					}*/
					
					color = float4(d3_hcl_lab(h, c, l), 1);
					//color = float4(HCLtoRGB_(float3(1,0,1)), 1);
					//color = float4(HSVtoRGB_(ingredientGroupsColorValues.x, 1, 1), 1);
				}
				else
				{
					discard;
				}
			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}