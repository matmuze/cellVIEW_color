using System;
using UnityEngine;
using System.Collections;

public static class ColorCompositeUtils
{
    public static void ComputeColorComposition(Material colorCompositeMaterial, RenderTexture dst, RenderTexture instanceIdBuffer, RenderTexture atomIdBuffer)
    {
        // Generated textures

        colorCompositeMaterial.SetTexture("_AtomIdBuffer", atomIdBuffer);
        colorCompositeMaterial.SetTexture("_InstanceIdBuffer", instanceIdBuffer);

        // Properties

        colorCompositeMaterial.SetBuffer("_ProteinAtomInfos", GPUBuffers.Get.ProteinAtomInfo);
        colorCompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstanceInfo);
        colorCompositeMaterial.SetBuffer("_ProteinIngredientProperties", GPUBuffers.Get.ProteinIngredientProperties);

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
