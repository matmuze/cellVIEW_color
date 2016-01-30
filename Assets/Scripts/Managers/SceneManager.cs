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

struct CutInfoStruct
{
    public Vector4 info;
    public Vector4 info2;
    public Vector4 info3;
}

public struct HistStruct
{
    public int parent; //also write data to this id, unless it is < 0

    public int all;
    public int cutaway;
    public int occluding;
    public int visible;

    public int pad0;
    public int pad1;
    public int pad2;
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
                _instance.hideFlags = HideFlags.HideInInspector;
            }

            _instance.OnUnityReload();
            return _instance;
        }
    }

    public void Awake()
    {
        var s = SceneManager.Get;
    }

    public void OnEnable()
    {
        
    }

    public static bool CheckInstance()
    {
        return _instance != null;
    }

    //--------------------------------------------------------------

    public string SceneName;

    [HideInInspector] public List<Compartment> Compartments { get; set; }
    [HideInInspector] public List<Ingredient> ProteinIngredients { get; set; }

   

    // Scene data
    [HideInInspector] public List<string> SceneHierarchy = new List<string>();

    
    [HideInInspector]
    public List<float> IngredientEdgeOpacity = new List<float>();

    // Lipid data 
    [HideInInspector]
    public List<string> LipidIngredientNames = new List<string>();
    [HideInInspector]
    public List<Vector4> LipidAtomPositions = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> LipidInstanceInfos = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> LipidInstancePositions = new List<Vector4>();

    // Protein ingredients data
    [HideInInspector]
    public List<Vector4> ProteinInstanceInfos = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> ProteinInstancePositions = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> ProteinInstanceRotations = new List<Vector4>();

    [HideInInspector]
    public List<uint> ProteinInstanceRotations_c = new List<uint>();


    [HideInInspector]
    public List<int> ProteinAtomCount = new List<int>();
    [HideInInspector]
    public List<int> ProteinAtomStart = new List<int>();
    [HideInInspector]
    public List<int> ProteinToggleFlags = new List<int>();
    [HideInInspector]
    public List<string> ProteinIngredientNames = new List<string>();
    [HideInInspector]
    public List<Vector4> ProteinAtoms = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> ProteinAtomInfo = new List<Vector4>();
    [HideInInspector]
    public List<float> ProteinRadii = new List<float>();
    [HideInInspector]
    public List<Vector4> ProteinAtomClusters = new List<Vector4>();
    [HideInInspector]
    public List<int> ProteinAtomClusterCount = new List<int>();
    [HideInInspector]
    public List<int> ProteinAtomClusterStart = new List<int>();

    // Curve ingredients data
    [HideInInspector]
    public List<int> CurveIngredientsAtomStart = new List<int>();
    [HideInInspector]
    public List<int> CurveIngredientsAtomCount = new List<int>();
    [HideInInspector]
    public List<int> CurveIngredientToggleFlags = new List<int>();

    [HideInInspector]
    public List<string> CurveIngredientNames = new List<string>();
    [HideInInspector]
    public List<Vector4> CurveIngredientsAtoms = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> CurveIngredientsInfos = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> CurveIngredientsColors = new List<Vector4>();

    [HideInInspector]
    public List<Vector4> CurveControlPointsInfos = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> CurveControlPointsNormals = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> CurveControlPointsPositions = new List<Vector4>();

    // Histogram data

    [NonSerialized]
    public List<HistStruct> HistogramData = new List<HistStruct>();

    [NonSerialized]
    public List<int> IngredientToNodeLookup = new List<int>();

    [NonSerialized]
    public List<int> NodeToIngredientLookup = new List<int>();

    //*****

    // This serves as a cache to avoid calling GameObject.Find on every update because not efficient
    // The cache will be filled automatically via the CutObject script onEnable

    [NonSerialized]
    public int ResetCutSnapshot = -1;

    [NonSerialized] public int SelectedCutObject = 0;
    [NonSerialized] public List<CutObject> CutObjects = new List<CutObject>();

    public CutObject GetSelectedCutObject()
    {
        return CutObjects[SelectedCutObject];
    }

    public List<CutObject> GetSelectedCutObjects()
    {
        var selectedCutObjects = new List<CutObject>();
        selectedCutObjects.Add(CutObjects[SelectedCutObject]);
        return selectedCutObjects;
    }

    //*** Ingredients ****//

    private List<string> _ingredientNames = new List<string>();
    public List<string> AllIngredientNames
    {
        get
        {
            if(_ingredientNames.Count != (ProteinIngredientNames.Count + LipidIngredientNames.Count))
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

    public int NumAllIngredients
    {
        get { return AllIngredientNames.Count; }
    }

    //--------------------------------------------------------------

    [HideInInspector]
    public int NumLodLevels = 0;

    [HideInInspector]
    public int TotalNumProteinAtoms = 0;

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
        get { return LipidInstancePositions.Count; }
    }

    public int NumProteinInstances
    {
        get { return ProteinInstancePositions.Count; }
    }

    public int NumCutObjects
    {
        get { return CutObjects.Count; }
    }

    public int NumDnaControlPoints
    {
        get { return CurveControlPointsPositions.Count; }
    }

    public int NumDnaSegments
    {
        get { return Math.Max(CurveControlPointsPositions.Count - 1, 0); }
    }

    //public bool OffsetChromaLuminance { get; set; }

    //--------------------------------------------------------------

    private void Update()
    {
        UpdateCutObjects();
    }

    private void OnUnityReload()
    {
        Debug.Log("Reload Scene");
        
        UploadAllData();
    }

    //--------------------------------------------------------------

    [HideInInspector]
    public List<Vector4> ProteinColors = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> IngredientProperties = new List<Vector4>();
    [HideInInspector]
    public List<Color> IngredientGroupsColor = new List<Color>();

    //*******//

    [HideInInspector]
    public List<float> IngredientGroupsLerpFactors = new List<float>();
    [HideInInspector]
    public List<Vector4> IngredientGroupsColorRanges = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> IngredientGroupsColorValues = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> ProteinIngredientsRandomValues = new List<Vector4>();

    #region Colors

    public float[] hueShifts = {0f, 0.6f, 0.2f, 0.8f, 0.4f};

    public void InitColors(List<IngredientGroup> ingredientGroups)
    {
        //IngredientGroups.Clear();

        IngredientGroupsLerpFactors.Clear();
        IngredientGroupsColorRanges.Clear();
        IngredientGroupsColorValues.Clear();
        ProteinIngredientsRandomValues.Clear();

        //*******//

        IngredientGroupsColor.Clear();

        //var angle = 360.0f / ingredientGroups.Count;
        //var currentHue = 0.0f;

        var ingredientColor = new Vector4[ProteinColors.Count];
        var ingredientProperties = new Vector4[ProteinColors.Count];
        var ingredientsRandomValues = new Vector4[ProteinColors.Count];

        var groupCount = 0;

        foreach (var group in ingredientGroups)
        {
            IngredientGroupsLerpFactors.Add(0);
            IngredientGroupsColorValues.Add(new Vector4(hueShifts[groupCount] * 360, 75, 75));
            IngredientGroupsColorRanges.Add(new Vector4(80, 75, 75));
            
            //*******//

            IngredientGroupsColor.Add(MyUtility.ColorFromHSV(hueShifts[groupCount], 1, 1));

            //*******//

            var ingredientCount = 0;
            foreach (var ingredient in group.Ingredients)
            {
                if (ingredient.path == "root.HIV1_capsid_3j3q_PackInner_0_1_0.surface.HIV1_CA_mono_0_1_0")
                {
                    ingredient.path = "root.HIV1_capsid_3j3q_PackOuter_0_1_1.surface.HIV1_CA_mono_0_1_0";
                }

                if (ProteinIngredientNames.Contains(ingredient.path))
                {
                    var index = ProteinIngredientNames.IndexOf(ingredient.path);
                    var currentSaturation = Random.Range(0.5f, 1);
                    var currentValue = Random.Range(0.5f, 1);

                    ingredientColor[index] = MyUtility.ColorFromHSV(hueShifts[groupCount], currentSaturation, currentValue);
                    ingredientProperties[index] = new Vector4(group.unique_id, 0, 0);
                    ingredientCount++;
                }
            }

            var offsetInc = 1.0f / ingredientCount;

            ingredientCount = 0;
            foreach (var ingredient in group.Ingredients)
            {
                if (ingredient.path == "root.HIV1_capsid_3j3q_PackInner_0_1_0.surface.HIV1_CA_mono_0_1_0")
                {
                    ingredient.path = "root.HIV1_capsid_3j3q_PackOuter_0_1_1.surface.HIV1_CA_mono_0_1_0";
                }

                if (ProteinIngredientNames.Contains(ingredient.path))
                {
                    var index = ProteinIngredientNames.IndexOf(ingredient.path);
                    ingredientsRandomValues[index] = new Vector4(ingredientCount * offsetInc, Random.Range(0.0f,1.0f), 0);
                    ingredientCount++;
                }
            }

            //currentHue += angle;
            //currentHue = currentHue % 360;
            groupCount ++;
        }

        ProteinColors = ingredientColor.ToList();
        IngredientProperties = ingredientProperties.ToList();
        ProteinIngredientsRandomValues = ingredientsRandomValues.ToList();

        GPUBuffers.Instance.ProteinColors.SetData(ProteinColors.ToArray());
        GPUBuffers.Instance.IngredientProperties.SetData(IngredientProperties.ToArray());
        GPUBuffers.Instance.IngredientGroupsColor.SetData(IngredientGroupsColor.ToArray());

        //*****

        GPUBuffers.Instance.IngredientGroupsColorRanges.SetData(IngredientGroupsColorRanges.ToArray());
        GPUBuffers.Instance.IngredientGroupsColorValues.SetData(IngredientGroupsColorValues.ToArray());
        GPUBuffers.Instance.IngredientGroupsLerpFactors.SetData(IngredientGroupsLerpFactors.ToArray());
        GPUBuffers.Instance.ProteinIngredientsRandomValues.SetData(ProteinIngredientsRandomValues.ToArray());
    }

    #endregion


    #region Ingredients

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

    public void AddProteinIngredient(string path, List<Atom> atoms, Color color, List<float> clusterLevels = null,
        bool nolod = false)
    {
        if (SceneHierarchy.Contains(path)) throw new Exception("Invalid protein path: " + path); 
        if (ProteinIngredientNames.Contains(path)) throw new Exception("Invalid protein path: " + path);

        if (clusterLevels != null)
        {
            if (NumLodLevels != 0 && NumLodLevels != clusterLevels.Count)
                throw new Exception("Uneven cluster levels number: " + path);
        }
        if (color == null)
        {
            color = MyUtility.GetRandomColor();
        }
        
        AddIngredientToHierarchy(path);
        
        ProteinColors.Add(color);
        ProteinToggleFlags.Add(1);
        ProteinIngredientNames.Add(path);
        ProteinRadii.Add(AtomHelper.ComputeRadius(atoms));

        ProteinAtomCount.Add(atoms.Count);
        ProteinAtomStart.Add(ProteinAtoms.Count);

        for (int i = 0; i< atoms.Count; i++)
        {
            ProteinAtoms.Add(new Vector4(atoms[i].position.x, atoms[i].position.y, atoms[i].position.z, atoms[i].radius));
            ProteinAtomInfo.Add(new Vector4(i, atoms[i].symbolId, atoms[i].residueId, atoms[i].chainId));
            //ProteinAtomInfo.Add(new Vector4(i, atoms[i].symbolId, atoms[i].residueId, atoms[i].chainId));
        }

        var atomSpheres = AtomHelper.GetAtomSpheres(atoms);

        if (clusterLevels != null)
        {
            NumLodLevels = clusterLevels.Count;
            foreach (var level in clusterLevels)
            {
                var numClusters = Math.Max(atomSpheres.Count*level, 5);
                List<Vector4> clusterSpheres;
                if (!nolod)
                    clusterSpheres = KMeansClustering.GetClusters(atomSpheres, (int) numClusters);
                else
                    clusterSpheres = new List<Vector4>(atomSpheres);
                ProteinAtomClusterCount.Add(clusterSpheres.Count);
                ProteinAtomClusterStart.Add(ProteinAtomClusters.Count);
                ProteinAtomClusters.AddRange(clusterSpheres);
            }
        }
    }

    public void AddProteinInstance(string path, Vector3 position, Quaternion rotation, int unitId = 0)
    {
        if (!ProteinIngredientNames.Contains(path))
        {
            throw new Exception("Ingredient path do not exists");
        }

        var ingredientId = ProteinIngredientNames.IndexOf(path);
        
        ProteinInstanceInfos.Add(new Vector4(ingredientId, (int) InstanceState.Normal, 0));
        ProteinInstancePositions.Add(position);
        ProteinInstanceRotations.Add(MyUtility.QuanternionToVector4(rotation));

        TotalNumProteinAtoms += ProteinAtomCount[ingredientId];
    }

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

            CurveIngredientsAtomStart.Add(CurveIngredientsAtoms.Count);
            CurveIngredientsAtomCount.Add(atomSpheres.Count);
            CurveIngredientsAtoms.AddRange(atomSpheres);
        }
        else if (path.Contains("mRNA"))
        {
            numSteps = 12;
            twistAngle = 34.3f;
            segmentLength = 34.0f;
            color = Color.red;

            var atomSpheres = PdbLoader.LoadAtomSpheres(pdbName);

            CurveIngredientsAtomStart.Add(CurveIngredientsAtoms.Count);
            CurveIngredientsAtomCount.Add(atomSpheres.Count);
            CurveIngredientsAtoms.AddRange(atomSpheres);
        }
        else if (path.Contains("peptide"))
        {
            numSteps = 10;
            twistAngle = 0;
            segmentLength = 20.0f;
            color = Color.magenta;

            var atomSphere = new Vector4(0, 0, 0, 3);
            CurveIngredientsAtomStart.Add(CurveIngredientsAtoms.Count);
            CurveIngredientsAtomCount.Add(1);
            CurveIngredientsAtoms.Add(atomSphere);
        }
        else if (path.Contains("lypoglycane"))
        {
            numSteps = 10;
            twistAngle = 0;
            segmentLength = 20;
            color = Color.green;

            var atomSphere = new Vector4(0, 0, 0, 8);
            CurveIngredientsAtomStart.Add(CurveIngredientsAtoms.Count);
            CurveIngredientsAtomCount.Add(1);
            CurveIngredientsAtoms.Add(atomSphere);
        }
        else
        {
            throw new Exception("Curve ingredient unknown");
        }

        CurveIngredientsColors.Add(color);
        CurveIngredientToggleFlags.Add(1);
        CurveIngredientsInfos.Add(new Vector4(numSteps, twistAngle, segmentLength, 0));
    }

    public void AddCurveIntance(string path, List<Vector4> curvePath)
    {
        if (!CurveIngredientNames.Contains(path))
        {
            throw new Exception("Curve ingredient type do not exists");
        }

        var curveIngredientId = CurveIngredientNames.IndexOf(path);
        var positions = MyUtility.ResampleControlPoints(curvePath, CurveIngredientsInfos[curveIngredientId].z);
        var normals = MyUtility.GetSmoothNormals(positions);

        var curveId = CurveControlPointsPositions.Count;
        var curveType = CurveIngredientNames.IndexOf(path);

        for (int i = 0; i < positions.Count; i++)
        {
            CurveControlPointsInfos.Add(new Vector4(curveId, curveType, 0, 0));
        }

        CurveControlPointsNormals.AddRange(normals);
        CurveControlPointsPositions.AddRange(positions);

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

        LipidAtomPositions.Clear();
        LipidInstanceInfos.Clear();
        LipidInstancePositions.Clear();

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

                LipidInstancePositions.Add(batchPosition);
                LipidInstanceInfos.Add(new Vector4(innerMembrane ? ingredientIdInner : ingredientIdOuter, lipidAtomStart, currentLipidAtoms.Count));

                lipidAtomStart += currentLipidAtoms.Count;
                LipidAtomPositions.AddRange(currentLipidAtoms);
                currentLipidAtoms.Clear();

                if (breakAfterFlushing) break;
            }

            var currentAtom = new Vector4(membraneData[dataIndex], membraneData[dataIndex + 1], membraneData[dataIndex + 2], AtomHelper.AtomRadii[(int)membraneData[dataIndex + 3]]);
            currentLipidAtoms.Add(currentAtom);
            dataIndex += step;
        }

        int a = 0;
    }

    #endregion

    //--------------------------------------------------------------

    #region Cut Objects

    //public void AddCutObject(CutType type)
    //{
    //    var gameObject =
    //        Instantiate(Resources.Load("Prefabs/CutObjectPrefab"), Vector3.zero, Quaternion.identity) as GameObject;
    //    var cutObject = gameObject.GetComponent<CutObject>().CutType = type;
    //}

    // Todo: proceed only if changes are made 
    public void UpdateCutObjects()
    {
        var CutInfos = new List<CutInfoStruct>();
        var CutScales = new List<Vector4>();
        var CutPositions = new List<Vector4>();
        var CutRotations = new List<Vector4>();

        // For each cut object
        foreach (var cut in CutObjects)
        {
            if (cut == null) throw new Exception("Cut object not fofund");

            CutScales.Add(cut.transform.localScale);
            CutPositions.Add(cut.transform.position);
            CutRotations.Add(MyUtility.QuanternionToVector4(cut.transform.rotation));
            //CutInfos.Add(new Vector4((float)cut.CutType, cut.Value1, cut.Value2, cut.Inverse ? 1.0f : 0.0f));
        }

        foreach (var cut in CutObjects)
        {
            foreach (var cutParam in cut.IngredientCutParameters)
            {
                CutInfos.Add(new CutInfoStruct
                {
                    info = new Vector4((float) cut.CutType, cutParam.value1, cutParam.value2, cut.Inverse ? 1.0f : 0.0f),
                    info2 = new Vector4(cutParam.fuzziness, cutParam.fuzzinessDistance, cutParam.fuzzinessCurve, cutParam.Aperture),
                    info3 = new Vector4(0,0,0,0)
                });
            }
        }

        GPUBuffers.Instance.CutInfo.SetData(CutInfos.ToArray());
        GPUBuffers.Instance.CutScales.SetData(CutScales.ToArray());
        GPUBuffers.Instance.CutPositions.SetData(CutPositions.ToArray());
        GPUBuffers.Instance.CutRotations.SetData(CutRotations.ToArray());
        //GPUBuffer.Get.ProteinCutFilters.SetData(ProteinCutFilters.ToArray());
        //GPUBuffer.Get.HistogramProteinTypes.SetData(HistogramProteinTypes.ToArray());
        //GPUBuffer.Get.HistogramStatistics.SetData(new[] { 0, 1, 2, 3 });

        GPUBuffers.Instance.IngredientEdgeOpacity.SetData(IngredientEdgeOpacity.ToArray());
    }

    public void UpdateCutObjectParams()
    {
        CutObjects.Clear();
        foreach (var cutObject in FindObjectsOfType<CutObject>())
        {
            cutObject.InitCutParameters();
            CutObjects.Add(cutObject);
        }
    }

    #endregion

    //--------------------------------------------------------------

    #region Misc

    // Scene data gets serialized on each reload, to clear the scene call this function
    public void ClearScene()
    {
        System.GC.Collect();

        Debug.Log("Clear Scene");

        NumLodLevels = 0;
        TotalNumProteinAtoms = 0;

        // Clear all lists
        foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.FieldType.FullName.Contains("System.Collections.Generic.List"))
            {
                var v = field.GetValue(this) as IList;
                v.Clear();
                //int a = 0;
            }
        }

        //// Clear lipid data
        //LipidIngredientNames.Clear();
        //LipidAtomPositions.Clear();
        //LipidInstanceInfos.Clear();
        //LipidInstancePositions.Clear();

        //// Clear scene data
        //ProteinInstanceInfos.Clear();
        //ProteinInstancePositions.Clear();
        //ProteinInstanceRotations.Clear();

        //// Clear ingredient data
        //ProteinIngredientNames.Clear();
        //ProteinColors.Clear();
        //ProteinToggleFlags.Clear();
        //ProteinRadii.Clear();

        //// Clear atom data
        //ProteinAtoms.Clear();
        //ProteinAtomCount.Clear();
        //ProteinAtomStart.Clear();

        //// Clear cluster data
        //ProteinAtomClusters.Clear();
        //ProteinAtomClusterStart.Clear();
        //ProteinAtomClusterCount.Clear();

        //// Clear curve data
        //CurveIngredientsInfos.Clear();
        //CurveIngredientsNames.Clear();
        //CurveIngredientsColors.Clear();
        //CurveIngredientToggleFlags.Clear();
        //CurveIngredientsAtoms.Clear();
        //CurveIngredientsAtomCount.Clear();
        //CurveIngredientsAtomStart.Clear();
        
        //CurveControlPointsPositions.Clear();
        //CurveControlPointsNormals.Clear();
        //CurveControlPointsInfos.Clear();

        UploadAllData();
    }

    private void CheckBufferSizes()
    {
        GPUBuffers.CheckNumIngredientMax(AllIngredientNames.Count);
        GPUBuffers.CheckNumLipidAtomMax(LipidAtomPositions.Count);
        GPUBuffers.CheckNumLipidInstancesMax(LipidInstancePositions.Count);
        GPUBuffers.CheckNumProteinTypeMax(ProteinIngredientNames.Count);
        GPUBuffers.CheckNumProteinAtomMax(ProteinAtoms.Count);
        GPUBuffers.CheckNumProteinAtomClusterMax(ProteinAtomClusters.Count);
        GPUBuffers.CheckNumProteinInstancesMax(ProteinInstancePositions.Count);
        GPUBuffers.CheckNumCurveIngredientMax(CurveIngredientNames.Count);
        GPUBuffers.CheckNumCurveControlPointsMax(CurveControlPointsPositions.Count);
        GPUBuffers.CheckNumCurveIngredientAtomsMax(CurveIngredientsAtoms.Count);
           
        if (Get.ProteinAtomClusterCount.Count >= GPUBuffers.NumProteinTypeMax * GPUBuffers.NumLodMax) throw new Exception("GPU buffer overflow");
    }

    public void UploadAllData()
    {
        System.GC.Collect();

        InitHistogramLookups();
        UpdateCutObjectParams();

        CheckBufferSizes();
        GPUBuffers.Instance.InitBuffers();
        GPUBuffers.Instance.ArgBuffer.SetData(new[] { 0, 1, 0, 0 });
        
        GPUBuffers.Instance.AtomColors.SetData(AtomHelper.AtomColors);
        GPUBuffers.Instance.AminoAcidColors.SetData(AtomHelper.ResidueColors);
        GPUBuffers.Instance.ProteinColors.SetData(ProteinColors.ToArray());
        GPUBuffers.Instance.ProteinAtomInfo.SetData(ProteinAtomInfo.ToArray());
        GPUBuffers.Instance.IngredientProperties.SetData(IngredientProperties.ToArray());
        GPUBuffers.Instance.IngredientGroupsColor.SetData(IngredientGroupsColor.ToArray());

        //*****//

        GPUBuffers.Instance.IngredientGroupsLerpFactors.SetData(IngredientGroupsLerpFactors.ToArray());
        GPUBuffers.Instance.IngredientGroupsColorRanges.SetData(IngredientGroupsColorRanges.ToArray());
        GPUBuffers.Instance.IngredientGroupsColorValues.SetData(IngredientGroupsColorValues.ToArray());
        GPUBuffers.Instance.ProteinIngredientsRandomValues.SetData(ProteinIngredientsRandomValues.ToArray());


        // Upload histogram info
        GPUBuffers.Instance.Histograms.SetData(HistogramData.ToArray());
        GPUBuffers.Instance.HistogramsLookup.SetData(IngredientToNodeLookup.ToArray());

        // Upload Lod levels info
        GPUBuffers.Instance.LodInfo.SetData(PersistantSettings.Get.LodLevels);

        // Upload ingredient data
        GPUBuffers.Instance.ProteinRadii.SetData(ProteinRadii.ToArray());
        GPUBuffers.Instance.IngredientMaskParams.SetData(ProteinToggleFlags.ToArray());

        GPUBuffers.Instance.ProteinAtoms.SetData(ProteinAtoms.ToArray());
        GPUBuffers.Instance.ProteinAtomCount.SetData(ProteinAtomCount.ToArray());
        GPUBuffers.Instance.ProteinAtomStart.SetData(ProteinAtomStart.ToArray());

        GPUBuffers.Instance.ProteinAtomClusters.SetData(ProteinAtomClusters.ToArray());
        GPUBuffers.Instance.ProteinAtomClusterCount.SetData(ProteinAtomClusterCount.ToArray());
        GPUBuffers.Instance.ProteinAtomClusterStart.SetData(ProteinAtomClusterStart.ToArray());

        GPUBuffers.Instance.ProteinInstanceInfo.SetData(ProteinInstanceInfos.ToArray());
        GPUBuffers.Instance.ProteinInstancePositions.SetData(ProteinInstancePositions.ToArray());
        GPUBuffers.Instance.ProteinInstanceRotations.SetData(ProteinInstanceRotations.ToArray());

        // Upload curve ingredient data
        GPUBuffers.Instance.CurveIngredientsAtoms.SetData(CurveIngredientsAtoms.ToArray());
        GPUBuffers.Instance.CurveIngredientsAtomCount.SetData(CurveIngredientsAtomCount.ToArray());
        GPUBuffers.Instance.CurveIngredientsAtomStart.SetData(CurveIngredientsAtomStart.ToArray());
        
        GPUBuffers.Instance.CurveIngredientsInfo.SetData(CurveIngredientsInfos.ToArray());
        GPUBuffers.Instance.CurveIngredientsColors.SetData(CurveIngredientsColors.ToArray());
        GPUBuffers.Instance.CurveIngredientsToggleFlags.SetData(CurveIngredientToggleFlags.ToArray());

        GPUBuffers.Instance.CurveControlPointsInfo.SetData(CurveControlPointsInfos.ToArray());
        GPUBuffers.Instance.CurveControlPointsNormals.SetData(CurveControlPointsNormals.ToArray());
        GPUBuffers.Instance.CurveControlPointsPositions.SetData(CurveControlPointsPositions.ToArray());

        // Upload lipid data
        GPUBuffers.Instance.LipidAtomPositions.SetData(LipidAtomPositions.ToArray());
        GPUBuffers.Instance.LipidInstanceInfo.SetData(LipidInstanceInfos.ToArray());
        GPUBuffers.Instance.LipidInstancePositions.SetData(LipidInstancePositions.ToArray());
    }

    void InitHistogramLookups()
    {
        // Init histogram GPU buffer
        HistogramData.Clear();
        foreach (var path in SceneHierarchy)
        {
            var hist = new HistStruct
            {
                parent = -1,
                all = 0,
                cutaway = 0,
                occluding = 0,
                visible = 0
            };

            if (MyUtility.IsPathRoot(path))
            {
                hist.parent = -1;
            }
            else
            {
                var parentPath = MyUtility.GetParentUrlPath(path);
                if(!SceneHierarchy.Contains(parentPath)) throw new Exception("Hierarchy corrupted");
                hist.parent = SceneHierarchy.IndexOf(parentPath);
            }

            HistogramData.Add(hist);
        }

        //*******************************//

        IngredientToNodeLookup.Clear();

        foreach (var ingredientName in AllIngredientNames)
        {
            if (SceneHierarchy.Contains(ingredientName))
            {
                IngredientToNodeLookup.Add(SceneHierarchy.IndexOf(ingredientName));
            }
        }

        //*******************************//

        NodeToIngredientLookup.Clear();

        foreach (var path in SceneHierarchy)
        {
            if (AllIngredientNames.Contains(path))
            {
                NodeToIngredientLookup.Add(AllIngredientNames.IndexOf(path));
            }
            else
            {
                NodeToIngredientLookup.Add(-1);
            }
        }

        int a = 0;
    }
    
    #endregion

    
}
