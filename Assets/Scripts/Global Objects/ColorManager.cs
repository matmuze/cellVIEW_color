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

    [Range(0, 1)]
    public float depthSlider = 0;

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
                _instance.hideFlags = HideFlags.None;
            }

            return _instance;
        }
    }

    [HideInInspector]
    public float[] hueShifts = { 0f, 0.6f, 0.2f, 0.8f, 0.4f };

    //*******//

    public int level;



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
        setHueCircleColors();
        

        GPUBuffers.Get.IngredientGroupsColor.SetData(CPUBuffers.Get.IngredientGroupsColor.ToArray());
        GPUBuffers.Get.ProteinIngredientsColors.SetData(CPUBuffers.Get.ProteinIngredientsColors.ToArray());
        GPUBuffers.Get.ProteinIngredientsChainColors.SetData(CPUBuffers.Get.ProteinIngredientsChainColors.ToArray());

        GPUBuffers.Get.IngredientGroupsLerpFactors.SetData(CPUBuffers.Get.IngredientGroupsLerpFactors.ToArray());
        GPUBuffers.Get.IngredientGroupsColorValues.SetData(CPUBuffers.Get.IngredientGroupsColorValues.ToArray());
        GPUBuffers.Get.IngredientGroupsColorRanges.SetData(CPUBuffers.Get.IngredientGroupsColorRanges.ToArray());
        GPUBuffers.Get.ProteinIngredientsRandomValues.SetData(CPUBuffers.Get.ProteinIngredientsRandomValues.ToArray());


        


    }



    private void setHueCircleColors()
    {
        CPUBuffers.Get.IngredientGroupsColor.Clear();
        CPUBuffers.Get.IngredientGroupsLerpFactors.Clear();
        CPUBuffers.Get.IngredientGroupsColorValues.Clear();
        CPUBuffers.Get.IngredientGroupsColorRanges.Clear();
        CPUBuffers.Get.ProteinIngredientsRandomValues.Clear();
        CPUBuffers.Get.ProteinIngredientsChainColors.Clear();
        CPUBuffers.Get.ProteinIngredientsColors.Clear();

        int[] numMembersIngredientGroups = new int[SceneManager.Get.IngredientGroups.Count];
        ArrayList numMembersIngredients = new ArrayList();
        for (int i = 0; i< SceneManager.Get.IngredientGroups.Count; i++)
        {
            numMembersIngredientGroups[i] = SceneManager.Get.IngredientGroups[i].Ingredients.Count;
            for (int j = 0; j< SceneManager.Get.IngredientGroups[i].Ingredients.Count; j++)
            {
                numMembersIngredients.Add(SceneManager.Get.IngredientGroups[i].Ingredients[j].nbChains);
            }
        }
        float[] anglefractions;
        float[] angleCentroids;
        float[] ingredientsAnglefractions;
        float[] ingredientsAngleCentroids;
        float startangle = 0;
        float endangle = 360;
        
        getFractionsAndCentroid(numMembersIngredientGroups, startangle, endangle, out anglefractions, out angleCentroids);
        getFractionsAndCentroid(numMembersIngredients.OfType<int>().ToArray(), startangle, endangle, out ingredientsAnglefractions, out ingredientsAngleCentroids);

       


        for (int i = 0; i< SceneManager.Get.IngredientGroups.Count; i++)
        {
            Debug.Log("anglecentroid i " + i + " " + angleCentroids[i]);
            Debug.Log("anglefractions i "+ i + " " + anglefractions[i]);
            CPUBuffers.Get.IngredientGroupsColor.Add(new Color(angleCentroids[i]/360f, 60f/100f,70f/100f));
            var group = SceneManager.Get.IngredientGroups[i];
            var offsetInc = 1.0f / group.Ingredients.Count;
            for (int j = 0; j<group.Ingredients.Count; j++)
            {
                Debug.Log("j loop i " + i);
                Debug.Log("loop anglecentroid i " + i + " " + angleCentroids[i]);
                CPUBuffers.Get.ProteinIngredientsColors.Add(new Vector4(angleCentroids[i] + anglefractions[i] * (j * offsetInc - 0.5f),60, 70));
                CPUBuffers.Get.IngredientGroupsLerpFactors.Add(0);
                CPUBuffers.Get.IngredientGroupsColorValues.Add(new Vector4(angleCentroids[i], 60, 90));// 15 + Random.value * 85));
                CPUBuffers.Get.IngredientGroupsColorRanges.Add(new Vector4(anglefractions[i], 0, 0));
                CPUBuffers.Get.ProteinIngredientsRandomValues.Add(new Vector4(j * offsetInc, 0, 0));

                
                var ingredient = group.Ingredients[j];
                var chainOffset = 1.0f / ingredient.nbChains;
                for (int k = 0; k< ingredient.nbChains; k++)
                {
                    float currentHue = ingredientsAngleCentroids[j] + (k * chainOffset - 0.5f)*ingredientsAnglefractions[j];
                    float currentChroma = 60f;
                    float currentLuminance = 60f;
                    CPUBuffers.Get.ProteinIngredientsChainColors.Add(new Vector4(Random.value * 360, currentChroma, 50 + Random.value * 20));

                }

            }
        }



        

    }





    private void getFractionsAndCentroid(int[] numMembers, float startangle, float endangle, out float[] anglefractions, out float[] angleCentroids)
    {
        anglefractions = new float[numMembers.Length];
        angleCentroids = new float[numMembers.Length];
        float sum = 0;
        for (int i = 0; i < numMembers.Length; i++)
        {
            sum += numMembers[i];
        }
        //inbetween angle (since otherwise there will be 2 proteins with the same color at every edge).
        float inbetweenangle = (endangle - startangle) / 100f; //temp placeholder




        for (int i = 0; i < numMembers.Length; i++)
        {

            anglefractions[i] = ((endangle-startangle)* (((float)numMembers[i]) / ((float)sum)) - inbetweenangle );

        }

        angleCentroids[0] = 0;
        float currentcentroid = 0;
        for (int i = 1; i< numMembers.Length; i++)
        {
            currentcentroid += anglefractions[i]/2 + anglefractions[i-1] / 2 + inbetweenangle;
            angleCentroids[i] = currentcentroid;
            
        }




    }

}
