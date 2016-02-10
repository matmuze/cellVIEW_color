using System;
using UnityEngine;
using System.Collections;

public static class RenderUtils
{
    public static void DrawProteins(Material renderProteinsMaterial, Camera camera, RenderBuffer colorBuffer, RenderBuffer depthBuffer)
    {
        // Protein params
        renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
        renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

        renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstanceInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

        renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.ProteinIngredientsColors);
        renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
        renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
        renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        Graphics.SetRenderTarget(colorBuffer, depthBuffer);
        renderProteinsMaterial.SetPass(0);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }

    public static void DrawClippedProteins(Material renderProteinsMaterial, Camera camera, RenderBuffer colorBuffer, RenderBuffer depthBuffer)
    {
        // Protein params
        renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
        renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

        renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstanceInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

        renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.ProteinIngredientsColors);
        renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
        renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
        renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        Graphics.SetRenderTarget(colorBuffer, depthBuffer);
        renderProteinsMaterial.SetPass(3);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }


    public static void DrawProteinsAtoms(Material renderProteinsMaterial, Camera camera, RenderBuffer instanceId, RenderBuffer atomId, RenderBuffer depthBuffer, int pass)
    {
        // Protein params
        renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
        renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

        renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstanceInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

        renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.ProteinIngredientsColors);
        renderProteinsMaterial.SetBuffer("_ProteinAtomInfo", GPUBuffers.Get.ProteinAtomInfo);
        renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
        renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
        renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        /****/
        renderProteinsMaterial.SetInt("_NumCutObjects", SceneManager.Get.NumCutObjects);
        renderProteinsMaterial.SetInt("_NumIngredientTypes", SceneManager.Get.NumAllIngredients);
        

        renderProteinsMaterial.SetBuffer("_CutInfos", GPUBuffers.Get.CutInfo);
        renderProteinsMaterial.SetBuffer("_CutScales", GPUBuffers.Get.CutScales);
        renderProteinsMaterial.SetBuffer("_CutPositions", GPUBuffers.Get.CutPositions);
        renderProteinsMaterial.SetBuffer("_CutRotations", GPUBuffers.Get.CutRotations);
        /****/

        Graphics.SetRenderTarget(new[] { instanceId, atomId }, depthBuffer);
        renderProteinsMaterial.SetPass(1);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }

    public static void DrawProteinsShadowMap(Material renderProteinsMaterial, Camera camera, RenderBuffer eyeDepthBuffer, RenderBuffer depthBuffer, int pass)
    {
        // Protein params
        renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
        renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

        renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstanceInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

        renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.ProteinIngredientsColors);
        renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
        renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
        renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        Graphics.SetRenderTarget(eyeDepthBuffer, depthBuffer);
        renderProteinsMaterial.SetPass(2);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }

    //public static void DrawProteinSphereBatches(Material renderProteinsMaterial, MainCamera camera, RenderBuffer instanceId, RenderBuffer atomId, RenderBuffer depthBuffer, int pass)
    //{
    //    // Protein params
    //    renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
    //    renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
    //    renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
    //    renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

    //    renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
    //    renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstanceInfo);
    //    renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
    //    renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

    //    renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.ProteinIngredientsColors);
    //    renderProteinsMaterial.SetBuffer("_ProteinAtomInfo", GPUBuffers.Get.ProteinAtomInfo);
    //    renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
    //    renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
    //    renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

    //    Graphics.SetRenderTarget(new []{ instanceId , atomId } , depthBuffer);
    //    renderProteinsMaterial.SetPass(pass);
    //    Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    //}

    public static void ComputeSphereBatches(Camera camera)
    {
        if (SceneManager.Get.NumProteinInstances <= 0) return;

        // Always clear append buffer before usage
        GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("_Scale", GlobalProperties.Get.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumLevels", SceneManager.Get.NumLodLevels);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumInstances", SceneManager.Get.NumProteinInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraForward", camera.transform.forward);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraPosition", camera.transform.position);
        ComputeShaderManager.Instance.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(camera));

        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinRadii", GPUBuffers.Get.ProteinRadii);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomCount", GPUBuffers.Get.ProteinAtomCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinAtomStart", GPUBuffers.Get.ProteinAtomStart);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterCount", GPUBuffers.Get.ProteinAtomClusterCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinClusterStart", GPUBuffers.Get.ProteinAtomClusterStart);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstanceInfo);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(0, "_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
        ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);
    }

    public static void ComputeSphereBatchesClipped(Camera camera)
    {
        if (SceneManager.Get.NumProteinInstances <= 0) return;

        // Always clear append buffer before usage
        GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

        ComputeShaderManager.Instance.SphereBatchCS.SetFloat("_Scale", GlobalProperties.Get.Scale);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumLevels", SceneManager.Get.NumLodLevels);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_NumInstances", SceneManager.Get.NumProteinInstances);
        ComputeShaderManager.Instance.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraForward", camera.transform.forward);
        ComputeShaderManager.Instance.SphereBatchCS.SetVector("_CameraPosition", camera.transform.position);
        ComputeShaderManager.Instance.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(camera));

        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinRadii", GPUBuffers.Get.ProteinRadii);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinAtomCount", GPUBuffers.Get.ProteinAtomCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinAtomStart", GPUBuffers.Get.ProteinAtomStart);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinClusterCount", GPUBuffers.Get.ProteinAtomClusterCount);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinClusterStart", GPUBuffers.Get.ProteinAtomClusterStart);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstanceInfo);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinInstanceVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
        ComputeShaderManager.Instance.SphereBatchCS.SetBuffer(4, "_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        ComputeShaderManager.Instance.SphereBatchCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
        ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);
    }
}
