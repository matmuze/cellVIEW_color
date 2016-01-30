﻿using System;
using System.Reflection;
using UnityEngine;

[ExecuteInEditMode]
public class GPUBuffers : MonoBehaviour
{
    public static int NumIngredientMax = 100;
    public static void CheckNumIngredientMax(int value)
    {
        if (value >= NumIngredientMax) throw new Exception("GPU buffer overflow");
    }

    public static int NumLipidAtomMax = 10000000;
    public static void CheckNumLipidAtomMax(int value)
    {
        if (value >= NumLipidAtomMax) throw new Exception("GPU buffer overflow");
    }

    public static int NumLipidInstancesMax = 1000000;
    public static void CheckNumLipidInstancesMax(int value)
    {
        if (value >= NumLipidInstancesMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumProteinTypeMax = 100;
    public static void CheckNumProteinTypeMax(int value)
    {
        if (value >= NumProteinTypeMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumProteinAtomMax = 3000000;
    public static void CheckNumProteinAtomMax(int value)
    {
        if (value >= NumProteinAtomMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumProteinAtomClusterMax = 1000000;
    public static void CheckNumProteinAtomClusterMax(int value)
    {
        if (value >= NumProteinAtomClusterMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumProteinInstancesMax = 100000;
    public static void CheckNumProteinInstancesMax(int value)
    {
        if (value >= NumProteinInstancesMax) throw new Exception("GPU buffer overflow");
    }

    public static int NumCurveIngredientMax = 100;
    public static void CheckNumCurveIngredientMax(int value)
    {
        if (value >= NumCurveIngredientMax) throw new Exception("GPU buffer overflow");
    }


    public static int NumCurveControlPointsMax = 1000000;
    public static void CheckNumCurveControlPointsMax(int value)
    {
        if (value >= NumCurveControlPointsMax) throw new Exception("GPU buffer overflow");
    }

    public static int NumCurveIngredientAtomsMax = 10000;
    public static void CheckNumCurveIngredientAtomsMax(int value)
    {
        if (value >= NumCurveIngredientAtomsMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumLodMax = 10;
    public static int NumCutsMax = 100;
    public static int NumSceneHierarchyNodes = 200;
    public static int NumProteinSphereBatchesMax = 2500000;

    /********************************/

    // Colors
    public ComputeBuffer IngredientProperties;
    public ComputeBuffer IngredientGroupsColor;
    public ComputeBuffer ProteinColors;
    public ComputeBuffer AtomColors;
    public ComputeBuffer AminoAcidColors;
    public ComputeBuffer ChainColors;

    //******//

    public ComputeBuffer IngredientGroupsColorRanges;
    public ComputeBuffer IngredientGroupsColorValues;
    public ComputeBuffer IngredientGroupsLerpFactors;
    public ComputeBuffer ProteinIngredientsRandomValues;

    //******//

    public ComputeBuffer ArgBuffer;
    public ComputeBuffer LodInfo;
    public ComputeBuffer SphereBatches;
    
    public ComputeBuffer IngredientStates;
    public ComputeBuffer IngredientMaskParams;
    public ComputeBuffer IngredientEdgeOpacity;


    // Protein buffers
    public ComputeBuffer ProteinRadii;

    public ComputeBuffer ProteinAtoms;
    public ComputeBuffer ProteinAtomInfo;
    public ComputeBuffer ProteinAtomCount;
    public ComputeBuffer ProteinAtomStart;

    public ComputeBuffer ProteinAtomClusters;
    public ComputeBuffer ProteinAtomClusterCount;
    public ComputeBuffer ProteinAtomClusterStart;

    public ComputeBuffer ProteinInstanceInfo;
    public ComputeBuffer ProteinInstancePositions;
    public ComputeBuffer ProteinInstanceRotations;
    public ComputeBuffer ProteinInstanceCullFlags;
    public ComputeBuffer ProteinInstanceOcclusionFlags;
    public ComputeBuffer ProteinInstanceVisibilityFlags;

    // lipid buffers
    public ComputeBuffer LipidAtomPositions;
    public ComputeBuffer LipidInstanceInfo;
    public ComputeBuffer LipidInstancePositions;
    public ComputeBuffer LipidInstanceCullFlags;
    public ComputeBuffer LipidInstanceOcclusionFlags;
    public ComputeBuffer LipidInstanceVisibilityFlags;

    // Curve ingredients buffers
    public ComputeBuffer CurveIngredientsInfo;
    public ComputeBuffer CurveIngredientsColors;
    public ComputeBuffer CurveIngredientsToggleFlags;

    public ComputeBuffer CurveIngredientsAtoms;
    public ComputeBuffer CurveIngredientsAtomCount;
    public ComputeBuffer CurveIngredientsAtomStart;

    public ComputeBuffer CurveControlPointsInfo;
    public ComputeBuffer CurveControlPointsNormals;
    public ComputeBuffer CurveControlPointsPositions;

    // Cut Objects
    public ComputeBuffer CutItems;
    public ComputeBuffer CutInfo;
    public ComputeBuffer CutScales;
    public ComputeBuffer CutPositions;
    public ComputeBuffer CutRotations;
    //public ComputeBuffer ProteinCutFilters;
    //public ComputeBuffer HistogramProteinTypes;
    //public ComputeBuffer HistogramStatistics;
    public ComputeBuffer HistogramsLookup;
    public ComputeBuffer Histograms;

    //*****//

    // Declare the buffer manager as a singleton
    private static GPUBuffers _instance = null;
    public static GPUBuffers Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GPUBuffers>();
                if (_instance == null)
                {
                    var go = GameObject.Find("_ComputeBufferManager");
                    if (go != null)
                        DestroyImmediate(go);

                    go = new GameObject("_ComputeBufferManager") {hideFlags = HideFlags.HideInInspector};
                    _instance = go.AddComponent<GPUBuffers>();
                }
            }
            return _instance;
        }
    }
    
    void OnEnable()
    {
        InitBuffers();
    }

    void OnDisable()
    {
        ReleaseBuffers();
    }
    
    public void InitBuffers ()
    {
        if (AtomColors == null) AtomColors = new ComputeBuffer(16, 16);
        if (AminoAcidColors == null) AminoAcidColors = new ComputeBuffer(32, 16);
        if (ProteinColors == null) ProteinColors = new ComputeBuffer(NumProteinTypeMax, 16);
        if (IngredientGroupsColor == null) IngredientGroupsColor = new ComputeBuffer(16, 16);
        if (IngredientProperties == null) IngredientProperties = new ComputeBuffer(NumIngredientMax, 16);

        //*****//

        if (IngredientGroupsLerpFactors == null) IngredientGroupsLerpFactors = new ComputeBuffer(16, 4);
        if (IngredientGroupsColorRanges == null) IngredientGroupsColorRanges = new ComputeBuffer(16, 16);
        if (IngredientGroupsColorValues == null) IngredientGroupsColorValues = new ComputeBuffer(16, 16);
        if (ProteinIngredientsRandomValues == null) ProteinIngredientsRandomValues = new ComputeBuffer(NumIngredientMax, 16);

        //*****//

        if (ArgBuffer == null) ArgBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);
        if (LodInfo == null) LodInfo = new ComputeBuffer(8, 16);
        if (SphereBatches == null) SphereBatches = new ComputeBuffer(NumProteinSphereBatchesMax, 16, ComputeBufferType.Append);

        if (IngredientStates == null) IngredientStates = new ComputeBuffer(NumIngredientMax, 4);
        if (IngredientMaskParams == null) IngredientMaskParams = new ComputeBuffer(NumIngredientMax, 4);
        if (IngredientEdgeOpacity == null) IngredientEdgeOpacity = new ComputeBuffer(NumIngredientMax, 4);

        //*****//

        if (ProteinRadii == null) ProteinRadii = new ComputeBuffer(NumProteinTypeMax, 4);

        if (ProteinAtoms == null) ProteinAtoms = new ComputeBuffer(NumProteinAtomMax, 16);
        if (ProteinAtomInfo == null) ProteinAtomInfo = new ComputeBuffer(NumProteinAtomMax, 16);
        if (ProteinAtomClusters == null) ProteinAtomClusters = new ComputeBuffer(NumProteinAtomClusterMax, 16);

        if (ProteinAtomCount == null) ProteinAtomCount = new ComputeBuffer(NumProteinTypeMax, 4);
        if (ProteinAtomStart == null) ProteinAtomStart = new ComputeBuffer(NumProteinTypeMax, 4);
        if (ProteinAtomClusterCount == null) ProteinAtomClusterCount = new ComputeBuffer(NumProteinTypeMax * NumLodMax, 4);
        if (ProteinAtomClusterStart == null) ProteinAtomClusterStart = new ComputeBuffer(NumProteinTypeMax * NumLodMax, 4);

        if (ProteinInstanceInfo == null) ProteinInstanceInfo = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstancePositions == null) ProteinInstancePositions = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstanceRotations == null) ProteinInstanceRotations = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstanceCullFlags == null) ProteinInstanceCullFlags = new ComputeBuffer(NumProteinInstancesMax, 4);
        if (ProteinInstanceOcclusionFlags == null) ProteinInstanceOcclusionFlags = new ComputeBuffer(NumProteinInstancesMax, 4);
        if (ProteinInstanceVisibilityFlags == null) ProteinInstanceVisibilityFlags = new ComputeBuffer(NumProteinInstancesMax, 4);

        //*****//

        if (LipidAtomPositions == null) LipidAtomPositions = new ComputeBuffer(NumLipidAtomMax, 16);
        if (LipidInstanceInfo == null) LipidInstanceInfo = new ComputeBuffer(NumLipidInstancesMax, 16);
        if (LipidInstancePositions == null) LipidInstancePositions = new ComputeBuffer(NumLipidInstancesMax, 16);
        if (LipidInstanceCullFlags == null) LipidInstanceCullFlags = new ComputeBuffer(NumLipidInstancesMax, 4);
        if (LipidInstanceOcclusionFlags == null) LipidInstanceOcclusionFlags = new ComputeBuffer(NumLipidInstancesMax, 4);
        if (LipidInstanceVisibilityFlags == null) LipidInstanceVisibilityFlags = new ComputeBuffer(NumLipidInstancesMax, 4);

        //*****//

        if (CurveIngredientsInfo == null) CurveIngredientsInfo = new ComputeBuffer(NumCurveIngredientMax, 16);
        if (CurveIngredientsColors == null) CurveIngredientsColors = new ComputeBuffer(NumCurveIngredientMax, 16);
        if (CurveIngredientsToggleFlags == null) CurveIngredientsToggleFlags = new ComputeBuffer(NumCurveIngredientMax, 4);

        if (CurveIngredientsAtomCount == null) CurveIngredientsAtomCount = new ComputeBuffer(NumCurveIngredientMax, 4);
        if (CurveIngredientsAtomStart == null) CurveIngredientsAtomStart = new ComputeBuffer(NumCurveIngredientMax, 4);
        if (CurveIngredientsAtoms == null) CurveIngredientsAtoms = new ComputeBuffer(NumCurveIngredientAtomsMax, 16);
        
        if (CurveControlPointsInfo == null) CurveControlPointsInfo = new ComputeBuffer(NumCurveControlPointsMax, 16);
        if (CurveControlPointsNormals == null) CurveControlPointsNormals = new ComputeBuffer(NumCurveControlPointsMax, 16);
        if (CurveControlPointsPositions == null) CurveControlPointsPositions = new ComputeBuffer(NumCurveControlPointsMax, 16);

        //*****//
        
        if (CutInfo == null) CutInfo = new ComputeBuffer(NumCutsMax * NumProteinTypeMax, 48);
        if (CutScales == null) CutScales = new ComputeBuffer(NumCutsMax, 16);
        if (CutPositions == null) CutPositions = new ComputeBuffer(NumCutsMax, 16);
        if (CutRotations == null) CutRotations = new ComputeBuffer(NumCutsMax, 16);
        //if (ProteinCutFilters == null) ProteinCutFilters = new ComputeBuffer(NumCutsMax * NumProteinMax, 4);
        //if (HistogramProteinTypes == null) HistogramProteinTypes = new ComputeBuffer(NumCutsMax * NumProteinMax, 4);
        //if (HistogramStatistics == null) HistogramStatistics = new ComputeBuffer(4, 4);
        if (HistogramsLookup == null) HistogramsLookup = new ComputeBuffer(NumProteinTypeMax, 4);
        if (Histograms == null) Histograms = new ComputeBuffer(NumSceneHierarchyNodes, 32);

    }
	
	// Flush buffers on exit
	void ReleaseBuffers ()
    {
        // Use reflection to release all the buffers in the scene
        foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.FieldType.FullName.Contains("UnityEngine.ComputeBuffer"))
            {
                var v = field.GetValue(this) as ComputeBuffer;
                if (v != null)
                {
                    v.Release();
                    v = null;
                }
            }
        }

	    //   if (ProteinColors != null) { ProteinColors.Release(); ProteinColors = null; }
	    //   if (ProteinAtomColors != null) { ProteinAtomColors.Release(); ProteinAtomColors = null; }
	    //   if (IngredientGroupsColor != null) { IngredientGroupsColor.Release(); IngredientGroupsColor = null; }


	    //   if (ArgBuffer != null) { ArgBuffer.Release(); ArgBuffer = null; }

	    //   // Cutaways
	    //   if (CutInfo != null) { CutInfo.Release(); CutInfo = null; }
	    //   if (CutScales != null) { CutScales.Release(); CutScales = null; }
	    //   if (CutPositions != null) { CutPositions.Release(); CutPositions = null; }
	    //   if (CutRotations != null) { CutRotations.Release(); CutRotations = null; }
	    //   //if (ProteinCutFilters != null) { ProteinCutFilters.Release(); ProteinCutFilters = null; }
	    //   //if (HistogramProteinTypes != null) { HistogramProteinTypes.Release(); HistogramProteinTypes = null; }
	    //   //if (HistogramStatistics != null) { HistogramStatistics.Release(); HistogramStatistics = null; }
	    //   if (HistogramsLookup != null) { HistogramsLookup.Release(); HistogramsLookup = null; }
	    //   if (Histograms != null) { Histograms.Release(); Histograms = null; }

	    //   //*****//

	    //   if (LodInfo != null) { LodInfo.Release(); LodInfo = null; }
	    //   if (SphereBatches != null) { SphereBatches.Release(); SphereBatches = null; }

	    //   if (IngredientStates != null) { IngredientStates.Release(); IngredientStates = null; }
	    //   if (IngredientMaskParams != null) { IngredientMaskParams.Release(); IngredientMaskParams = null; }
	    //   if (IngredientProperties != null) { IngredientProperties.Release(); IngredientProperties = null; }
	    //   if (IngredientEdgeOpacity != null) { IngredientEdgeOpacity.Release(); IngredientEdgeOpacity = null; }


	    //   //*****//


	    //   //*****//

	    //   if (ProteinRadii != null) { ProteinRadii.Release(); ProteinRadii = null; }


	    //   if (ProteinAtoms != null) { ProteinAtoms.Release(); ProteinAtoms = null; }
	    //   if (ProteinAtomInfo != null) { ProteinAtomInfo.Release(); ProteinAtomInfo = null; }
	    //if (ProteinAtomCount != null) { ProteinAtomCount.Release(); ProteinAtomCount = null; }
	    //if (ProteinAtomStart != null) { ProteinAtomStart.Release(); ProteinAtomStart = null;   }

	    //   if (ProteinAtomClusters != null) { ProteinAtomClusters.Release(); ProteinAtomClusters = null; }
	    //if (ProteinAtomClusterCount != null) { ProteinAtomClusterCount.Release(); ProteinAtomClusterCount = null; }
	    //if (ProteinAtomClusterStart != null) { ProteinAtomClusterStart.Release(); ProteinAtomClusterStart = null; }

	    //   if (ProteinInstanceInfo != null) { ProteinInstanceInfo.Release(); ProteinInstanceInfo = null; }
	    //   if (ProteinInstancePositions != null) { ProteinInstancePositions.Release(); ProteinInstancePositions = null; }
	    //   if (ProteinInstanceRotations != null) { ProteinInstanceRotations.Release(); ProteinInstanceRotations = null; }
	    //   if (ProteinInstanceCullFlags != null) { ProteinInstanceCullFlags.Release(); ProteinInstanceCullFlags = null; }
	    //   if (ProteinInstanceOcclusionFlags != null) { ProteinInstanceOcclusionFlags.Release(); ProteinInstanceOcclusionFlags = null; }
	    //   if (ProteinInstanceVisibilityFlags != null) { ProteinInstanceVisibilityFlags.Release(); ProteinInstanceVisibilityFlags = null; }

	    //   //*****//

	    //   if (CurveIngredientsInfo != null) { CurveIngredientsInfo.Release(); CurveIngredientsInfo = null; }
	    //   if (CurveIngredientsColors != null) { CurveIngredientsColors.Release(); CurveIngredientsColors = null; }
	    //   if (CurveIngredientsToggleFlags != null) { CurveIngredientsToggleFlags.Release(); CurveIngredientsToggleFlags = null; }

	    //   if (CurveIngredientsAtoms != null) { CurveIngredientsAtoms.Release(); CurveIngredientsAtoms = null; }
	    //   if (CurveIngredientsAtomCount != null) { CurveIngredientsAtomCount.Release(); CurveIngredientsAtomCount = null; }
	    //   if (CurveIngredientsAtomStart != null) { CurveIngredientsAtomStart.Release(); CurveIngredientsAtomStart = null; }

	    //   if (CurveControlPointsInfo != null) { CurveControlPointsInfo.Release(); CurveControlPointsInfo = null; }
	    //   if (CurveControlPointsNormals != null) { CurveControlPointsNormals.Release(); CurveControlPointsNormals = null; }
	    //   if (CurveControlPointsPositions != null) { CurveControlPointsPositions.Release(); CurveControlPointsPositions = null; }

	    //   //*****//

	    //   if (LipidAtomPositions != null) { LipidAtomPositions.Release(); LipidAtomPositions = null; }
	    //   if (LipidInstanceInfo != null) { LipidInstanceInfo.Release(); LipidInstanceInfo = null; }
	    //   if (LipidInstancePositions != null) { LipidInstancePositions.Release(); LipidInstancePositions = null; }
	    //   if (LipidInstanceCullFlags != null) { LipidInstanceCullFlags.Release(); LipidInstanceCullFlags = null; }
	    //   if (LipidInstanceOcclusionFlags != null) { LipidInstanceOcclusionFlags.Release(); LipidInstanceOcclusionFlags = null; }
	    //   if (LipidInstanceVisibilityFlags != null) { LipidInstanceVisibilityFlags.Release(); LipidInstanceVisibilityFlags = null; }
    }
}
