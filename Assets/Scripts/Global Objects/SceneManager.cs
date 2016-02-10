using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Loaders;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

enum InstanceState
{
    Null = -1,           // Get will not be displayed
    Normal = 0,          // Get will be displayed with normal color
    Highlighted = 1      // Get will be displayed with highlighted color
}

[ExecuteInEditMode]
public class SceneManager : MonoBehaviour
{
    // Declare the scene manager as a singleton
    private static SceneManager _instance = null;

    public static SceneManager Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<SceneManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_SceneManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_SceneManager");
                _instance = go.AddComponent<SceneManager>();
                //_instance.hideFlags = HideFlags.HideInInspector;
            }

            _instance.OnUnityReload();
            return _instance;
        }
    }

    //--------------------------------------------------------------
    
    [HideInInspector]
    public string SceneName;
    [HideInInspector]
    public int NumLodLevels;
    [HideInInspector]
    public int ProteinInstanceCount;
    [HideInInspector]
    public int TotalNumProteinAtoms;

    // Scene data
    //[HideInInspector]
    //public List<Compartment> Compartments;
    [HideInInspector]
    public List<IngredientGroup> IngredientGroups;
    //[HideInInspector]
    //public List<Ingredient> ProteinIngredients;

    // Scene data bis
    [HideInInspector]
    public List<string> SceneHierarchy = new List<string>();
    [HideInInspector]
    public List<string> ProteinIngredientNames = new List<string>();
    [HideInInspector]
    public List<string> CurveIngredientNames = new List<string>();
    [HideInInspector]
    public List<string> LipidIngredientNames = new List<string>();

    private List<string> _ingredientNames = new List<string>();
    public List<string> AllIngredientNames
    {
        get
        {
            if (_ingredientNames.Count != (ProteinIngredientNames.Count + LipidIngredientNames.Count))
            {
                _ingredientNames.Clear();
                _ingredientNames.AddRange(ProteinIngredientNames);
                _ingredientNames.AddRange(LipidIngredientNames);
                _ingredientNames.AddRange(CurveIngredientNames);
            }

            return _ingredientNames;
        }
    }

    [HideInInspector]
    public List<int> AllIngredientStates = new List<int>();

    //--------------------------------------------------------------

    public int NumAllIngredients
    {
        get { return AllIngredientNames.Count; }
    }

    public int NumLipidIngredients
    {
        get { return LipidIngredientNames.Count; }
    }

    public int NumProteinIngredients
    {
        get { return ProteinIngredientNames.Count; }
    }

    public int NumLipidInstances
    {
        get { return CPUBuffers.Get.LipidInstancePositions.Count; }
    }

    public int NumProteinInstances
    {
        get { return CPUBuffers.Get.ProteinInstancePositions.Count; }
    }

    public int NumCutObjects
    {
        get { return CutObjectManager.Get.CutObjects.Count; }
    }

    public int NumDnaControlPoints
    {
        get { return CPUBuffers.Get.CurveControlPointsPositions.Count; }
    }

    public int NumDnaSegments
    {
        get { return Math.Max(CPUBuffers.Get.CurveControlPointsPositions.Count - 1, 0); }
    }

    //--------------------------------------------------------------

    public void Awake()
    {
        var s = Get;
    }

    public void OnDestroy()
    {
        ClearScene();
    }

    public static bool CheckInstance()
    {
        return _instance != null;
    }

    private void Update()
    {
        CutObjectManager.Get.UpdateCutObjects();
    }

    private void OnUnityReload()
    {
        Debug.Log("Reload Scene");
        CPUBuffers.Get.CopyDataToGPU();
    }

    // Scene data automatically reloaded, to clear the scene call this function
    public void ClearScene()
    {
        System.GC.Collect();

        Debug.Log("Clear Scene");

        SceneName = "";
        NumLodLevels = 0;
        ProteinInstanceCount = 0;
        TotalNumProteinAtoms = 0;

        // Clear all lists
        foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.FieldType.FullName.Contains("System.Collections.Generic.List"))
            {
                var v = field.GetValue(this) as IList;
                v.Clear();
            }
        }

        CPUBuffers.Get.Clear();
        CPUBuffers.Get.CopyDataToGPU();
    }

    //--------------------------------------------------------------

    #region Ingredients

    //*** Global Ingredients ****//

    public void AddIngredientToHierarchy(string ingredientUrlPath)
    {
        var urlPathSplit = MyUtility.SplitUrlPath(ingredientUrlPath);

        if (urlPathSplit.Count() == 1)
        {
            if (!SceneHierarchy.Contains(urlPathSplit.First()))
                SceneHierarchy.Add(urlPathSplit.First());
        }
        else
        {
            var parentUrlPath = MyUtility.GetParentUrlPath(ingredientUrlPath);

            if (!SceneHierarchy.Contains(parentUrlPath))
            {
                AddIngredientToHierarchy(parentUrlPath);
            }

            if (!SceneHierarchy.Contains(ingredientUrlPath))
            {
                SceneHierarchy.Add(ingredientUrlPath);
            }
            else
            {
                throw new Exception("Ingredient path already used");
            }
        }
    }

    //*** Protein Ingredients ****//

    public void AddProteinIngredient(string path, List<Atom> atoms, List<List<Vector4>> clusterLevels = null)
    {
        if (SceneHierarchy.Contains(path))
            throw new Exception("Invalid protein path: " + path);
        if (ProteinIngredientNames.Contains(path))
            throw new Exception("Invalid protein path: " + path);

        if (clusterLevels != null)
        {
            if (NumLodLevels != 0 && NumLodLevels != clusterLevels.Count)
                throw new Exception("Uneven cluster levels number: " + path);
        }

        AddIngredientToHierarchy(path);
        ProteinIngredientNames.Add(path);

        CPUBuffers.Get.ProteinToggleFlags.Add(1);
        CPUBuffers.Get.ProteinIngredientsRadii.Add(AtomHelper.ComputeRadius(atoms));

        CPUBuffers.Get.ProteinAtomCount.Add(atoms.Count);
        CPUBuffers.Get.ProteinAtomStart.Add(CPUBuffers.Get.ProteinAtoms.Count);

        for (int i = 0; i < atoms.Count; i++)
        {
            CPUBuffers.Get.ProteinAtoms.Add(new Vector4(atoms[i].position.x, atoms[i].position.y, atoms[i].position.z, atoms[i].radius));
            CPUBuffers.Get.ProteinAtomInfo.Add(new Vector4(i, atoms[i].symbolId, atoms[i].residueId, atoms[i].chainId));
        }

        if (clusterLevels != null)
        {
            NumLodLevels = clusterLevels.Count;
            foreach (var level in clusterLevels)
            {
                CPUBuffers.Get.ProteinAtomClusterCount.Add(level.Count);
                CPUBuffers.Get.ProteinAtomClusterStart.Add(CPUBuffers.Get.ProteinAtomClusters.Count);
                CPUBuffers.Get.ProteinAtomClusters.AddRange(level);
            }
        }
    }

    //public void AddProteinInstance(string path, Vector3 position, Quaternion rotation)
    //{
    //    if (!ProteinIngredientNames.Contains(path))
    //    {
    //        throw new Exception("Ingredient path do not exists");
    //    }

    //    var ingredientId = ProteinIngredientNames.IndexOf(path);

    //    CPUBuffers.Get.ProteinInstanceInfos.Add(new Vector4(ingredientId, (int) InstanceState.Normal, 0));
    //    CPUBuffers.Get.ProteinInstancePositions.Add(position);
    //    CPUBuffers.Get.ProteinInstanceRotations.Add(MyUtility.QuanternionToVector4(rotation));

    //    TotalNumProteinAtoms += CPUBuffers.Get.ProteinAtomCount[ingredientId];
    //}

    //*** Curve Ingredients ****//

    public void AddCurveIngredient(string path, string pdbName)
    {
        if (SceneHierarchy.Contains(path)) throw new Exception("Invalid curve ingredient path: " + path);
        if (CurveIngredientNames.Contains(path)) throw new Exception("Invalid curve ingredient path: " + path);

        AddIngredientToHierarchy(path);
        CurveIngredientNames.Add(path);

        var numSteps = 1;
        var twistAngle = 0.0f;
        var segmentLength = 34.0f;
        var color = new Color(1,1,1,1);

        if (path.Contains("DNA"))
        {
            numSteps = 12;
            twistAngle = 34.3f;
            segmentLength = 34.0f;
            color = Color.yellow;

            var atomSpheres = PdbLoader.LoadAtomSpheres(pdbName);

            CPUBuffers.Get.CurveIngredientsAtomStart.Add(CPUBuffers.Get.CurveIngredientsAtoms.Count);
            CPUBuffers.Get.CurveIngredientsAtomCount.Add(atomSpheres.Count);
            CPUBuffers.Get.CurveIngredientsAtoms.AddRange(atomSpheres);
        }
        else if (path.Contains("mRNA"))
        {
            numSteps = 12;
            twistAngle = 34.3f;
            segmentLength = 34.0f;
            color = Color.red;

            var atomSpheres = PdbLoader.LoadAtomSpheres(pdbName);

            CPUBuffers.Get.CurveIngredientsAtomStart.Add(CPUBuffers.Get.CurveIngredientsAtoms.Count);
            CPUBuffers.Get.CurveIngredientsAtomCount.Add(atomSpheres.Count);
            CPUBuffers.Get.CurveIngredientsAtoms.AddRange(atomSpheres);
        }
        else if (path.Contains("peptide"))
        {
            numSteps = 10;
            twistAngle = 0;
            segmentLength = 20.0f;
            color = Color.magenta;

            var atomSphere = new Vector4(0, 0, 0, 3);
            CPUBuffers.Get.CurveIngredientsAtomStart.Add(CPUBuffers.Get.CurveIngredientsAtoms.Count);
            CPUBuffers.Get.CurveIngredientsAtomCount.Add(1);
            CPUBuffers.Get.CurveIngredientsAtoms.Add(atomSphere);
        }
        else if (path.Contains("lypoglycane"))
        {
            numSteps = 10;
            twistAngle = 0;
            segmentLength = 20;
            color = Color.green;

            var atomSphere = new Vector4(0, 0, 0, 8);
            CPUBuffers.Get.CurveIngredientsAtomStart.Add(CPUBuffers.Get.CurveIngredientsAtoms.Count);
            CPUBuffers.Get.CurveIngredientsAtomCount.Add(1);
            CPUBuffers.Get.CurveIngredientsAtoms.Add(atomSphere);
        }
        else
        {
            throw new Exception("Curve ingredient unknown");
        }

        CPUBuffers.Get.CurveIngredientsColors.Add(color);
        CPUBuffers.Get.CurveIngredientToggleFlags.Add(1);
        CPUBuffers.Get.CurveIngredientsInfos.Add(new Vector4(numSteps, twistAngle, segmentLength, 0));
    }

    public void AddCurveInstance(string path, List<Vector4> curvePath)
    {
        if (!CurveIngredientNames.Contains(path))
        {
            throw new Exception("Curve ingredient type do not exists");
        }

        var curveIngredientId = CurveIngredientNames.IndexOf(path);
        var positions = MyUtility.ResampleControlPoints(curvePath, CPUBuffers.Get.CurveIngredientsInfos[curveIngredientId].z);
        var normals = MyUtility.GetSmoothNormals(positions);

        var curveId = CPUBuffers.Get.CurveControlPointsPositions.Count;
        var curveType = CurveIngredientNames.IndexOf(path);

        for (int i = 0; i < positions.Count; i++)
        {
            CPUBuffers.Get.CurveControlPointsInfos.Add(new Vector4(curveId, curveType, 0, 0));
        }

        CPUBuffers.Get.CurveControlPointsNormals.AddRange(normals);
        CPUBuffers.Get.CurveControlPointsPositions.AddRange(positions);

        //Debug.Log(positions.Count);
    }

    //*** Membrane Ingredients ****//

    public void AddMembrane(string filePath, Vector3 position, Quaternion rotation)
    {
        var pathInner = "root.membrane.inner_membrane";
        var pathOuter = "root.membrane.outer_membrane";

        AddIngredientToHierarchy(pathInner);
        AddIngredientToHierarchy(pathOuter);

        LipidIngredientNames.Clear();
        LipidIngredientNames.Add(pathInner);
        LipidIngredientNames.Add(pathOuter);

        CPUBuffers.Get.LipidAtomPositions.Clear();
        CPUBuffers.Get.LipidInstanceInfos.Clear();
        CPUBuffers.Get.LipidInstancePositions.Clear();

        var currentLipidAtoms = new List<Vector4>();
        var membraneData = MyUtility.ReadBytesAsFloats(filePath);

        var ingredientIdInner = AllIngredientNames.IndexOf(pathInner);
        var ingredientIdOuter = AllIngredientNames.IndexOf(pathOuter);

        var step = 5;
        var dataIndex = 0;
        var lipidAtomStart = 0;
        var previousLipidId = -1;

        while (true)
        {
            var flushCurrentBatch = false;
            var breakAfterFlushing = false;

            if (dataIndex >= membraneData.Count())
            {
                flushCurrentBatch = true;
                breakAfterFlushing = true;
            }
            else
            {
                var lipidId = (int)membraneData[dataIndex + 4];
                if (previousLipidId < 0) previousLipidId = lipidId;
                if (lipidId != previousLipidId)
                {
                    flushCurrentBatch = true;
                    previousLipidId = lipidId;
                }
            }

            if (flushCurrentBatch)
            {
                var bounds = AtomHelper.ComputeBounds(currentLipidAtoms);
                var center = new Vector4(bounds.center.x, bounds.center.y, bounds.center.z, 0);
                for (var j = 0; j < currentLipidAtoms.Count; j++) currentLipidAtoms[j] -= center;

                var innerMembrane = Vector3.Magnitude(bounds.center) < 727;

                Vector4 batchPosition = position + bounds.center;
                batchPosition.w = Vector3.Magnitude(bounds.extents);

                CPUBuffers.Get.LipidInstancePositions.Add(batchPosition);
                CPUBuffers.Get.LipidInstanceInfos.Add(new Vector4(innerMembrane ? ingredientIdInner : ingredientIdOuter, lipidAtomStart, currentLipidAtoms.Count));

                lipidAtomStart += currentLipidAtoms.Count;
                CPUBuffers.Get.LipidAtomPositions.AddRange(currentLipidAtoms);
                currentLipidAtoms.Clear();

                if (breakAfterFlushing) break;
            }

            var currentAtom = new Vector4(membraneData[dataIndex], membraneData[dataIndex + 1], membraneData[dataIndex + 2], AtomHelper.AtomRadii[(int)membraneData[dataIndex + 3]]);
            currentLipidAtoms.Add(currentAtom);
            dataIndex += step;
        }
    }

    #endregion

}