using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SceneManagerInspectorProxy))]
public class SceneManagerInspector : Editor
{
    public bool[] b;

    public override void OnInspectorGUI()
    {
        ////DrawDefaultInspector();
        //var sceneManager = (SceneManager)target;

        if (b == null || b.Length != SceneManager.Get.IngredientGroupsColorRanges.Count)
        {
            b = new bool[SceneManager.Get.IngredientGroupsColorRanges.Count];
        }

        EditorGUILayout.Space();
        
        for (int i = 0; i < SceneManager.Get.IngredientGroupsColorRanges.Count; i++) 
        {
            b[i] = EditorGUILayout.Foldout(b[i], "Group " + i);
            if (b[i])
            {
                var hclColor = new Vector3();
                var hclRange = new Vector3();

                SceneManager.Get.IngredientGroupsLerpFactors[i] = EditorGUILayout.Slider("Lerp factor", SceneManager.Get.IngredientGroupsLerpFactors[i], 0, 1);
                
                EditorGUILayout.Separator();

                hclColor.x = EditorGUILayout.Slider("Hue Centroid", SceneManager.Get.IngredientGroupsColorValues[i].x, 0, 360);
                hclColor.y = EditorGUILayout.Slider("Chroma Centroid", SceneManager.Get.IngredientGroupsColorValues[i].y, 0, 140);
                hclColor.z = EditorGUILayout.Slider("Luminance", SceneManager.Get.IngredientGroupsColorValues[i].z, 0, 100);

                EditorGUILayout.Separator();

                hclRange.x = EditorGUILayout.Slider("Hue Offset", SceneManager.Get.IngredientGroupsColorRanges[i].x, 0, 360);
                hclRange.y = EditorGUILayout.Slider("Chroma Offset", SceneManager.Get.IngredientGroupsColorRanges[i].y, 0, 140);
                hclRange.z = EditorGUILayout.Slider("Luminance Offset", SceneManager.Get.IngredientGroupsColorRanges[i].z, 0, 100);
                
                SceneManager.Get.IngredientGroupsColorValues[i] = hclColor;
                SceneManager.Get.IngredientGroupsColorRanges[i] = hclRange;
            }
            
            //EditorGUILayout.Separator();
        }

        if (GUI.changed)
        {
            UploadData();
            EditorUtility.SetDirty(target);
        }
    }

    public void UploadData()
    {
        GPUBuffers.Instance.IngredientGroupsColorValues.SetData(SceneManager.Get.IngredientGroupsColorValues.ToArray());
        GPUBuffers.Instance.IngredientGroupsColorRanges.SetData(SceneManager.Get.IngredientGroupsColorRanges.ToArray());
        GPUBuffers.Instance.IngredientGroupsLerpFactors.SetData(SceneManager.Get.IngredientGroupsLerpFactors.ToArray());
        //SceneManager.Get.Getager.UploadAllData();
    }
}