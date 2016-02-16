using System;
using UnityEngine;
using System.Collections;

public static class ColorCompositeUtils
{
    public static void ComputeCoverage(RenderTexture instanceIdBuffer)
    {
        GPUBuffers.Get.IngredientGroupsColorInfo.SetData(CPUBuffers.Get.IngredientGroupsColorInfo.ToArray());
        GPUBuffers.Get.ProteinIngredientsColorInfo.SetData(CPUBuffers.Get.ProteinIngredientsColorInfo.ToArray());

        ComputeShaderManager.Get.ComputeColorInfo.SetTexture(0, "_InstanceIdBuffer", instanceIdBuffer);

        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_ProteinInstancesInfo", GPUBuffers.Get.ProteinInstancesInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_ProteinIngredientsInfo", GPUBuffers.Get.ProteinIngredientsInfo);

        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_RWProteinInstanceVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_RWIngredientGroupsColorInfo", GPUBuffers.Get.IngredientGroupsColorInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_RWProteinIngredientsColorInfo", GPUBuffers.Get.ProteinIngredientsColorInfo);

        ComputeShaderManager.Get.ComputeColorInfo.Dispatch(0, Mathf.CeilToInt(instanceIdBuffer.width / 8.0f), Mathf.CeilToInt(instanceIdBuffer.height / 8.0f), 1);
    }

    public static void CountInstances()
    {
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_ProteinInstancesInfo", GPUBuffers.Get.ProteinInstancesInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_ProteinIngredientsInfo", GPUBuffers.Get.ProteinIngredientsInfo);

        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_RWProteinInstanceVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_RWIngredientGroupsColorInfo", GPUBuffers.Get.IngredientGroupsColorInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_RWProteinIngredientsColorInfo", GPUBuffers.Get.ProteinIngredientsColorInfo);

        ComputeShaderManager.Get.ComputeColorInfo.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
    }

    public static void ComputeColorComposition(Material colorCompositeMaterial, RenderTexture dst, RenderTexture instanceIdBuffer, RenderTexture atomIdBuffer, RenderTexture depthBuffer)
    {
		colorCompositeMaterial.SetFloat("_depth", ColorManager.Get.depthSlider);
		
        // LOD infos

        var rangeValues = Matrix4x4.zero;
        
        int distAcc = 0;
        for (int i = 0; i < ColorManager.Get.LevelRanges.Length; i++)
        {
            distAcc += (int)(ColorManager.Get.LevelRanges[i] * ColorManager.Get.DistanceMax);
            rangeValues[i] = distAcc;
        }
        rangeValues[ColorManager.Get.LevelRanges.Length] = ColorManager.Get.DistanceMax;

        colorCompositeMaterial.SetInt("_UseDistanceLevels", Convert.ToInt32(ColorManager.Get.UseDistanceLevels));
        

        colorCompositeMaterial.SetInt("_NumLevelMax", ColorManager.Get.NumLevelMax);
        colorCompositeMaterial.SetInt("_DistanceMax", ColorManager.Get.DistanceMax);
        colorCompositeMaterial.SetMatrix("_LevelRanges", rangeValues);

        // *****

        colorCompositeMaterial.SetFloat("_LevelLerpFactor", ColorManager.Get.LevelLerpFactor);
        colorCompositeMaterial.SetInt("_NumPixels", instanceIdBuffer.width * instanceIdBuffer.height);
        //possible also with vectors, setvector

        colorCompositeMaterial.SetTexture("_DepthBuffer", depthBuffer);
        colorCompositeMaterial.SetTexture("_AtomIdBuffer", atomIdBuffer);
        colorCompositeMaterial.SetTexture("_InstanceIdBuffer", instanceIdBuffer);

        // Properties

        colorCompositeMaterial.SetBuffer("_IngredientGroupsColorInfo", GPUBuffers.Get.IngredientGroupsColorInfo);
        colorCompositeMaterial.SetBuffer("_ProteinIngredientsColorInfo", GPUBuffers.Get.ProteinIngredientsColorInfo);

        colorCompositeMaterial.SetBuffer("_ProteinAtomInfos", GPUBuffers.Get.ProteinAtomInfo);
        colorCompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
        colorCompositeMaterial.SetBuffer("_ProteinIngredientProperties", GPUBuffers.Get.ProteinIngredientsInfo);

        // Predifined colors 

        colorCompositeMaterial.SetBuffer("_AtomColors", GPUBuffers.Get.AtomColors);
        colorCompositeMaterial.SetBuffer("_AminoAcidColors", GPUBuffers.Get.AminoAcidColors);
        colorCompositeMaterial.SetBuffer("_ProteinIngredientsColors", GPUBuffers.Get.ProteinIngredientsColors);
        colorCompositeMaterial.SetBuffer("_ProteinIngredientsChainColors", GPUBuffers.Get.ProteinIngredientsChainColors);
        colorCompositeMaterial.SetBuffer("_IngredientGroupsColor", GPUBuffers.Get.IngredientGroupsColor);

        // Values for color generation on the fly 

        colorCompositeMaterial.SetBuffer("_IngredientGroupsLerpFactors", GPUBuffers.Get.IngredientGroupsLerpFactors);
        colorCompositeMaterial.SetBuffer("_IngredientGroupsColorValues", GPUBuffers.Get.IngredientGroupsColorValues);
        colorCompositeMaterial.SetBuffer("_IngredientGroupsColorRanges", GPUBuffers.Get.IngredientGroupsColorRanges);
        colorCompositeMaterial.SetBuffer("_ProteinIngredientsRandomValues", GPUBuffers.Get.ProteinIngredientsRandomValues);

        Graphics.Blit(null, dst, colorCompositeMaterial, 0);
    }
}
