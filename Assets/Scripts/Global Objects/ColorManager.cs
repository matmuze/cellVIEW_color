using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Loaders;
using UnityEngine;
using Random = UnityEngine.Random;


[ExecuteInEditMode]
public class ColorManager : MonoBehaviour
{
    // Declare the scene manager as a singleton
    private static ColorManager _instance = null;

    public static ColorManager Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<ColorManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_ColorManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_ColorManager");
                _instance = go.AddComponent<ColorManager>();
                //_instance.hideFlags = HideFlags.HideInInspector;
            }

            return _instance;
        }
    }

    [HideInInspector]
    public float[] hueShifts = { 0f, 0.6f, 0.2f, 0.8f, 0.4f };

    //*******//

    public void InitColors()
    {
        CPUBuffers.Get.ProteinIngredientsProperties.Clear();

        // Predefined colors

        CPUBuffers.Get.IngredientGroupsColor.Clear();
        CPUBuffers.Get.ProteinIngredientsColors.Clear();
        CPUBuffers.Get.ProteinIngredientsChainColors.Clear();

        // Properties to generate colors on the fly

        CPUBuffers.Get.IngredientGroupsLerpFactors.Clear();
        CPUBuffers.Get.IngredientGroupsColorRanges.Clear();
        CPUBuffers.Get.IngredientGroupsColorValues.Clear();
        CPUBuffers.Get.ProteinIngredientsRandomValues.Clear();

        foreach (var group in SceneManager.Get.IngredientGroups)
        {
            var currentHue = hueShifts[group.unique_id] * 360.0f;

            // Predified group color
            CPUBuffers.Get.IngredientGroupsColor.Add(MyUtility.ColorFromHSV(currentHue, 1, 1));
            
            //...
            CPUBuffers.Get.IngredientGroupsLerpFactors.Add(0);
            CPUBuffers.Get.IngredientGroupsColorValues.Add(new Vector4(currentHue, 75, 75));
            CPUBuffers.Get.IngredientGroupsColorRanges.Add(new Vector4(80, 0, 0));

            //*******//

            var offsetInc = 1.0f / group.Ingredients.Count;

            for (var i = 0; i < group.Ingredients.Count; i++)
            {
                if (!SceneManager.Get.ProteinIngredientNames.Contains(group.Ingredients[i].path))
                {
                    throw new Exception("Unknown ingredient: " + group.Ingredients[i].path);
                }

                var currentChroma = Random.Range(0.5f, 1);

                CPUBuffers.Get.ProteinIngredientsProperties.Add(new Vector4(group.unique_id, group.Ingredients[i].nbChains, CPUBuffers.Get.ProteinIngredientsChainColors.Count, 0));

                // Predefined ingredient color
                CPUBuffers.Get.ProteinIngredientsColors.Add(MyUtility.ColorFromHSV(currentHue, currentChroma, 1));

                for (var j = 0; j < group.Ingredients[i].nbChains; j++)
                {
                    var currentLuminance = Random.Range(0.5f, 1);
                    CPUBuffers.Get.ProteinIngredientsChainColors.Add(MyUtility.ColorFromHSV(currentHue, currentChroma, currentLuminance));
                }

                // ...
                CPUBuffers.Get.ProteinIngredientsRandomValues.Add(new Vector4(i * offsetInc, Random.Range(0.0f, 1.0f), 0));
            }
        }
    }

    public void InitColors2()
    {
        
    }

    public void ReloadColors()
    {
        Debug.Log("Reloading colors !!");

        CPUBuffers.Get.IngredientGroupsColor.Clear();
        CPUBuffers.Get.ProteinIngredientsColors.Clear();
        CPUBuffers.Get.ProteinIngredientsChainColors.Clear();

        foreach (var group in SceneManager.Get.IngredientGroups)
        {
            var currentHue = hueShifts[group.unique_id] * 360.0f;
            CPUBuffers.Get.IngredientGroupsColor.Add(MyUtility.ColorFromHSV(currentHue, 1, 1));
            //CPUBuffers.Get.IngredientGroupsColor.Add(Color.blue);

            foreach (var ingredient in group.Ingredients)
            {
                var currentChroma = Random.Range(0.5f, 1);
                CPUBuffers.Get.ProteinIngredientsColors.Add(MyUtility.ColorFromHSV(currentHue, currentChroma, 1));
                //CPUBuffers.Get.ProteinIngredientsColors.Add(Color.red);

                for (var j = 0; j < ingredient.nbChains; j++)
                {
                    var currentLuminance = Random.Range(0.25f, 1);
                    CPUBuffers.Get.ProteinIngredientsChainColors.Add(MyUtility.ColorFromHSV(currentHue, currentChroma, currentLuminance));
                    //CPUBuffers.Get.ProteinIngredientsChainColors.Add(Color.green);
                }
            }
        }

        GPUBuffers.Get.IngredientGroupsColor.SetData(CPUBuffers.Get.IngredientGroupsColor.ToArray());
        GPUBuffers.Get.ProteinIngredientsColors.SetData(CPUBuffers.Get.ProteinIngredientsColors.ToArray());
        GPUBuffers.Get.ProteinIngredientsChainColors.SetData(CPUBuffers.Get.ProteinIngredientsChainColors.ToArray());
    }
}
