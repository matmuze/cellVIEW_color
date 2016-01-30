using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Loaders;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Loaders
{
    public class CellPackLoader2
    {
        public static void ReloadCellPackResults()
        {
            if (File.Exists(PersistantSettings.Get.LastSceneLoaded2))
            {
                LoadCellPackResults(PersistantSettings.Get.LastSceneLoaded2);
            }
            else
            {
                LoadCellPackResults();
            }
        }

        public static void LoadCellPackResults(string path = null)
        {
            if (path == null)
            {
                #if UNITY_EDITOR

                    var directory = "";
                    if (string.IsNullOrEmpty(PersistantSettings.Get.LastSceneLoaded2) || !Directory.Exists(Path.GetDirectoryName(PersistantSettings.Get.LastSceneLoaded2)))
                    {
                        directory = Application.dataPath;
                    }
                    else
                    {
                        directory = Path.GetDirectoryName(PersistantSettings.Get.LastSceneLoaded2);
                    }

                    path = EditorUtility.OpenFilePanel("Select .json", directory, "json");

                #endif
            }

            if(path == null || !File.Exists(path)) return;

            PersistantSettings.Get.LastSceneLoaded2 = path;

            var rootCompartment = CompartmentUtility.DeserializeJson(path);
            CompartmentUtility.PostProcessSceneGraph(rootCompartment);

            var ingredientGroups = CompartmentUtility.GetAllIngredientGroups(rootCompartment);
            
            //// Flatten all the hierarchy
            //var compartments = CompartmentUtility.GetAllCompartments(rootCompartment);
            //var ingredientGroups = CompartmentUtility.GetAllIngredientGroups(compartments);
            //var proteinIngredients = CompartmentUtility.GetAllProteinIngredients(ingredientGroups);

            SceneManager.Get.InitColors(ingredientGroups);
            SceneManager.Get.UploadAllData();
            //int aa = 0;
        }

        

        //public static void LoadProteinIngredientAtoms(Ingredient ingredient)
        //{
        //    var name = ingredient.name;
        //    var path = ingredient.path;
        //    var biomt = ingredient.source.biomt;
        //    var center = ingredient.source.transform.center;
        //    var pdbName = ingredient.source.pdb;

        //    if ((pdbName == "") || (pdbName == "null") || (pdbName == "None") || pdbName.StartsWith("EMDB"))
        //    {
        //        return;
        //    }

        //    var atomSet = PdbLoader.LoadAtomSet(pdbName);
        //    if (atomSet.Count == 0) return;

        //    var hasAlphaCardonOnly = AtomHelper.ContainsCarbonAlphaOnly(atomSet);

        //    var biomtTransforms = new List<Matrix4x4>();
            
        //    var centerPosition = AtomHelper.ComputeBounds(atomSet).center;


        //    //Vector3 biomtCenter = Vector3.zero;
        //    //bool containsACarbonOnly = false;
        //    //bool oneLOD = false;


        //    //else
        //    //{
        //    //    // Load atom set from pdb file
        //    //    atomSet = PdbLoader.LoadAtomSet(pdbName);

        //    //    // If the set is empty return
        //    //    if (atomSet.Count == 0) return;

        //    //    containsACarbonOnly = AtomHelper.ContainsCarbonAlphaOnly(atomSet);
        //    //}



        //    //// Center atoms
        //    //AtomHelper.OffsetAtoms(ref atomSet, centerPosition);

        //    //// Compute bounds
        //    ////var bounds = AtomHelper.ComputeBounds(atomSpheres);

        //    //biomtTransforms = biomt ? PdbLoader.LoadBiomtTransforms(pdbName) : new List<Matrix4x4>();
        //    //biomtCenter = AtomHelper.GetBiomtCenter(biomtTransforms, centerPosition);

        //    ////if (!pdbName.Contains("1TWT_1TWV")) return;

        //    //// Disable biomts until loading problem is resolved
        //    ////if (!biomt) return;

        //    //// Define cluster decimation levels
        //    //var clusterLevels = (containsACarbonOnly)
        //    //? new List<float>() { 0.85f, 0.25f, 0.1f }
        //    //    : new List<float>() { 0.15f, 0.10f, 0.05f };
        //    //if (oneLOD)
        //    //    clusterLevels = new List<float>() { 1, 1, 1 };
        //    //// Add ingredient type
        //    ////SceneManager.Get.AddIngredient(name, bounds, atomSpheres, color);

        //    //SceneManager.Get.AddProteinIngredient(path, atomSet, color, clusterLevels, oneLOD);
        //}
    }
}


