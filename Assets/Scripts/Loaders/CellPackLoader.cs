using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Loaders;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Loaders
{
    public class CellPackLoader
    {
        public static string GetInputFile(string extension, string lastFileLoaded = null)
        {
            var directory = Application.dataPath;

            if (!string.IsNullOrEmpty(lastFileLoaded) && Directory.Exists(Path.GetDirectoryName(lastFileLoaded)))
            {
                directory = Path.GetDirectoryName(GlobalProperties.Get.LastRecipeFileLoaded);
            }

            return EditorUtility.OpenFilePanel("Select ." + extension, directory, extension);
        }

        //***************** Load cellPACK recipe *********************//

        public static void ReloadCellPackRecipe()
        {
            LoadCellPackRecipe(GlobalProperties.Get.LastRecipeFileLoaded);
        }

        public static void LoadCellPackRecipe(string path = null)
        {
            SceneManager.Get.ClearScene();

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                path = GetInputFile("json", GlobalProperties.Get.LastRecipeFileLoaded);
            }

            if (path == null || !File.Exists(path)) return;
            GlobalProperties.Get.LastRecipeFileLoaded = path;

            var rootCompartment = CompartmentUtility.DeserializeJson(path);
            CompartmentUtility.PostProcessSceneGraph(rootCompartment);

            LoadRecipe(rootCompartment);

            //CPUBuffers.Get.CopyDataToGPU();
        }

        public static void LoadRecipe(Compartment rootCompartment)
        {
            // Flatten all the hierarchy
            //SceneManager.Get.Compartments = CompartmentUtility.GetCompartments(rootCompartment);
            SceneManager.Get.IngredientGroups = CompartmentUtility.GetIngredientGroups(rootCompartment);
            //SceneManager.Get.ProteinIngredients = CompartmentUtility.GetProteinIngredients(rootCompartment);

            for (var i = 0; i < SceneManager.Get.IngredientGroups.Count; i++)
            {
                for (var j = 0; j < SceneManager.Get.IngredientGroups[i].Ingredients.Count; j++)
                {
                    var ingredient = SceneManager.Get.IngredientGroups[i].Ingredients[j];

                    AddProteinIngredient(ref ingredient);
                    SceneManager.Get.ProteinInstanceCount += SceneManager.Get.IngredientGroups[i].Ingredients[j].nbMol;
                }
            }

            ColorManager.Get.InitColors();

            int a = 0;
        }

        public static List<Atom> GetAtoms(Ingredient ingredient)
        {
            var name = ingredient.name;
            var pdbName = ingredient.source.pdb.Replace(".pdb", "");

            var atoms = new List<Atom>();

            if ((pdbName == "") || (pdbName == "null") || (pdbName == "None") || pdbName.StartsWith("EMDB"))
            {
                var filePath = PdbLoader.GetFile(PdbLoader.DefaultPdbDirectory, name, "bin");

                if (File.Exists(filePath))
                {
                    var points = MyUtility.ReadBytesAsFloats(filePath);
                    for (var i = 0; i < points.Length; i += 4)
                    {
                        var currentAtom = new Atom
                        {
                            position = new Vector3(points[i], points[i + 1], points[i + 2]),
                            radius = points[i + 3],
                            symbolId = -1,
                            chainId = 0
                        };
                        atoms.Add(currentAtom);
                    }
                }
            }
            else
            {
                // Load atom set from pdb file
                atoms = PdbLoader.LoadAtomSet(pdbName);
            }

            // If the set is empty return
            if (atoms.Count == 0) throw new Exception("Atom list empty: " + name);

            return atoms; 
        }

        public static void AddProteinIngredient(ref Ingredient ingredient)
        {
            var path = ingredient.path;
            var biomt = ingredient.source.biomt;
            var pdbName = ingredient.source.pdb.Replace(".pdb", "");

            Debug.Log("*****");
            Debug.Log("Ingredient: " + ingredient.ingredient_id);
            Debug.Log("Name: " + ingredient.path);
            Debug.Log("Pdb id: " + ingredient.source.pdb);


            // ***** Load atoms *****//
            
            var atoms = GetAtoms(ingredient);

            var numChains = AtomHelper.GetNumChains(atoms);
            Debug.Log("Num chains: " + numChains);

            ingredient.nbChains = numChains;

            var isFromCustomStructureFile = AtomHelper.IsFromCustomStructureFile(atoms);
            if (isFromCustomStructureFile)
                Debug.Log("From custom structure file");

            var alphaCarbonsOnly = AtomHelper.ContainsCarbonAlphaOnly(atoms);
            if (alphaCarbonsOnly) AtomHelper.OverwriteRadii(ref atoms, 3);
            if(alphaCarbonsOnly)
                Debug.Log("Alpha carbons only");
            

            // ***** Compute lod proxies *****//
            
            var lodProxies = new List<List<Vector4>>();

            // Define cluster decimation levels
            var clusterLevelFactors = new List<float>() { 0.15f, 0.10f, 0.05f };
            if (alphaCarbonsOnly || isFromCustomStructureFile) clusterLevelFactors = new List<float>() { 1, 1, 1 };
            
            if (!biomt)
            {
                // Center atoms before computing the lod proxies
                AtomHelper.CenterAtoms(ref atoms);

                var atomSpheres = AtomHelper.GetAtomSpheres(atoms);
                lodProxies = AtomHelper.ComputeLodProxies(atomSpheres, clusterLevelFactors);
            }
            else
            {
                var atomSpheres = AtomHelper.GetAtomSpheres(atoms);
                var biomtTransforms = PdbLoader.LoadBiomtTransforms(pdbName);

                // Compute centered lod proxies
                lodProxies = AtomHelper.ComputeLodProxiesBiomt(atomSpheres, biomtTransforms, clusterLevelFactors);

                // Assemble the atom set from biomt transforms and center
                atoms = AtomHelper.BuildBiomt(atoms, biomtTransforms);

                var centerPosition = AtomHelper.ComputeBounds(atoms).center;

                // Center atoms
                AtomHelper.OffsetAtoms(ref atoms, centerPosition);

                // Center proxies
                for (int i = 0; i < lodProxies.Count; i++)
                {
                    var t = lodProxies[i];
                    AtomHelper.OffsetSpheres(ref t, centerPosition);
                }
            }
            
            SceneManager.Get.AddProteinIngredient(path, atoms, lodProxies);
            Debug.Log("Ingredient added succesfully");
        }

        //***************** Load cellPACK positions *********************//

        public static void ReloadCellPackPositions()
        {
            LoadCellPackPositions(GlobalProperties.Get.LastPositionsFileLoaded);
        }

        public static void LoadCellPackPositions(string path = null)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                path = GetInputFile("bin", GlobalProperties.Get.LastPositionsFileLoaded);
            }

            if (string.IsNullOrEmpty(path)) return;
            GlobalProperties.Get.LastPositionsFileLoaded = path;

            // Load position from .bin file
            LoadPositions(path);

            // Upload scene data to the GPU
            CPUBuffers.Get.CopyDataToGPU();
        }

        private static void LoadPositions(string path)
        {
            var numInstances = SceneManager.Get.ProteinInstanceCount;

            if (numInstances == 0)
            {
                throw new Exception("Ingredient description empty, load a cellPACK recipe first.");
            }

            var arrayByteSize = numInstances*sizeof (float)*4;
            var arrayFloatSize = numInstances*4;

            var positionByteArray = new byte[arrayByteSize];
            var rotationByteArray = new byte[arrayByteSize];

            using (var fs = File.OpenRead(path))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(positionByteArray, 0, arrayByteSize);
                fs.Read(rotationByteArray, 0, arrayByteSize);
            }

            var positionFloatArray = new float[arrayFloatSize];
            var rotationFloatArray = new float[arrayFloatSize];

            Buffer.BlockCopy(positionByteArray, 0, positionFloatArray, 0, positionByteArray.Length);
            Buffer.BlockCopy(rotationByteArray, 0, rotationFloatArray, 0, rotationByteArray.Length);

            CPUBuffers.Get.ProteinInstanceInfos.Clear();
            CPUBuffers.Get.ProteinInstancePositions.Clear();
            CPUBuffers.Get.ProteinInstanceRotations.Clear();

            for (int i = 0; i < numInstances; i++)
            {
                CPUBuffers.Get.ProteinInstanceInfos.Add(new Vector4(positionFloatArray[i*4 + 3],
                    (int) InstanceState.Normal, 0));

                var position = new Vector4();
                position.x = positionFloatArray[i*4 + 0];
                position.y = positionFloatArray[i*4 + 1];
                position.z = positionFloatArray[i*4 + 2];
                position.w = positionFloatArray[i*4 + 3];
                CPUBuffers.Get.ProteinInstancePositions.Add(position);

                var rotation = new Vector4();
                rotation.x = rotationFloatArray[i*4 + 0];
                rotation.y = rotationFloatArray[i*4 + 1];
                rotation.z = rotationFloatArray[i*4 + 2];
                rotation.w = rotationFloatArray[i*4 + 3];
                CPUBuffers.Get.ProteinInstanceRotations.Add(rotation);
            }

            //GPUBuffers.Get.ProteinInstanceInfo.SetData(CPUBuffers.Get.ProteinInstanceInfos.ToArray());
            //GPUBuffers.Get.ProteinInstancePositions.SetData(CPUBuffers.Get.ProteinInstancePositions.ToArray());
            //GPUBuffers.Get.ProteinInstanceRotations.SetData(CPUBuffers.Get.ProteinInstanceRotations.ToArray());

            Debug.Log("*****");
            Debug.Log("Positions loaded succesfully");
            Debug.Log("*****");
        }
    }
}


