﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class PdbLoader
{
    public static string DefaultPdbDirectory = Application.dataPath + "/../Data/proteins/";

    public static List<Atom> LoadAtomSet(string fileName)
    {
        var path = GetPdbFile(fileName, DefaultPdbDirectory);
        return ReadAtomData(path);
    }

    public static List<Vector4> LoadAtomSpheres(string fileName)
    {
        return AtomHelper.GetAtomSpheres(ReadAtomData(GetPdbFile(fileName, DefaultPdbDirectory)));
    }

    public static List<Matrix4x4> LoadBiomtTransforms(string fileName)
    {
        var path = GetPdbFile(fileName, DefaultPdbDirectory);
        return ReadBiomtData(path);
    }

    public static List<Vector4> LoadAtomSpheresBiomt(string fileName)
    {
        var path = GetPdbFile(fileName, DefaultPdbDirectory);

        var atomData = ReadAtomData(path);
        var atomSpheres = AtomHelper.GetAtomSpheres(atomData);

        var biomtTransforms = ReadBiomtData(path);
        atomSpheres = AtomHelper.BuildBiomt(atomSpheres, biomtTransforms);

        return atomSpheres;
    }

    public static string GetFile(string directory, string fileName, string extention)
    {
        var filePath = directory + fileName + "." + extention;

        if (!File.Exists(filePath))
        {
            filePath = "";

            // Download from protein data bank
            if (fileName.Count() <= 4)
            {
                filePath = DownloadFile(fileName, "http://www.rcsb.org/pdb/download/downloadFile.do?fileFormat=pdb&compression=NO&structureId=", directory);
            }

            // Download from cellPACK repository
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = DownloadFile(fileName, "https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/other/", directory, "." + extention);
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("File not found: " + fileName);
            }
        }

        return filePath;
    }

    private static string GetPdbFile(string fileName, string directory)
    {
        var filePath = directory + fileName + ".pdb";

        if (!File.Exists(filePath))
        {
            filePath = "";

            // Download from protein data bank
            if (fileName.Count() <= 4)
            {
                filePath = DownloadFile(fileName, "http://www.rcsb.org/pdb/download/downloadFile.do?fileFormat=pdb&compression=NO&structureId=", directory);
            }

            // Download from cellPACK repository
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = DownloadFile(fileName, "https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/other/", directory, ".pdb");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("File not found: " + fileName);
            }
        }

        return filePath;
    }

    private static string DownloadFile(string fileName, string url, string directory, string extension = "")
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        url = url + WWW.EscapeURL(fileName + extension);

        Debug.Log("Downloading pdb file");
        var www = new WWW(url);

#if UNITY_EDITOR
        while (!www.isDone)
        {
            EditorUtility.DisplayProgressBar("Downloading " + fileName, "Downloading...", www.progress);
        }
        EditorUtility.ClearProgressBar();
#endif

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log(www.error);
            return null;
        }

        var filePath = directory + fileName + ".pdb";
        File.WriteAllText(filePath, www.text);

        return filePath;
    }

    //http://deposit.rcsb.org/adit/docs/pdb_atom_format.html#ATOM
    public static List<Atom> ReadAtomData(string path)
    {
        if (!File.Exists(path)) throw new Exception("File not found at: " + path);
        var pdbName = Path.GetFileName(path);


        var chains = new List<string>();

        var atoms = new List<Atom>();

        foreach (var line in File.ReadAllLines(path))
        {
            //if (line.StartsWith("ATOM"))// || line.StartsWith("HETATM"))
            if (line.StartsWith("ATOM") || line.StartsWith("HETATM"))
            {
                var x = float.Parse(line.Substring(30, 8));
                var y = float.Parse(line.Substring(38, 8));
                var z = float.Parse(line.Substring(46, 8));

                var name = line.Substring(12, 4).Trim();
                var symbol = "";
                
                try
                {
                    symbol = line.Substring(76, 2).Trim();
                }
                catch (Exception)
                {
                    //Remove numbers from the name
                    //var t = Regex.Replace(name, @"[\d-]", string.Empty).Trim();
                    //symbol = t[0].ToString();
                }
                
                if (!AtomHelper.AtomSymbols.Contains(symbol))
                {
                    var t = Regex.Replace(name, @"[\d-]", string.Empty).Trim();
                    symbol = t[0].ToString();
                }

                var symbolId = Array.IndexOf(AtomHelper.AtomSymbols, symbol);
                if (symbolId < 0)
                {
                    Debug.Log(pdbName + " - Atom symbol not available at line: " + line) ;
                    symbol = "A";
                    symbolId = Array.IndexOf(AtomHelper.AtomSymbols, symbol);
                }

                var radius = AtomHelper.AtomRadii[symbolId];

                var residueName = line.Substring(17, 3).Trim();
                var residueId = Array.IndexOf(AtomHelper.ResidueNames, residueName);
                if (symbolId < 0)
                {
                    Debug.Log(pdbName + " - Residue symbol not available at line: " + line);
                }

                if (residueName == "HOH") continue;

                var residueIndex = int.Parse(line.Substring(22, 4));

                var chain = line[21].ToString();
                var chainId = chains.IndexOf(chain);
                if (chainId < 0)
                {
                    chains.Add(chain);
                    chainId = chains.IndexOf(chain);
                }

                var atom = new Atom
                {
                    radius = radius,
                    name = name,
                    symbol = symbol,
                    symbolId = symbolId,
                    residueName = residueName,
                    residueId = residueId,
                    residueIndex = residueIndex,
                    chain = chain,
                    chainId = chainId,
                    
                    position = new Vector3(-x, y, z)
                };

                atoms.Add(atom);
            }

            if (line.StartsWith("ENDMDL")) // Only parse first model of MDL files
            {
                break;
            }
        }

        return atoms;
    }


    //http://www.rcsb.org/pdb/101/static101.do?p=education_discussion/Looking-at-Structures/bioassembly_tutorial.html
    public static List<Matrix4x4> ReadBiomtData(string path)
    {
        if (!File.Exists(path)) throw new Exception("File not found at: " + path);

        var matrices = new List<Matrix4x4>();
        var matrix = new Matrix4x4();

        foreach (var line in File.ReadAllLines(path))
        {
            if (line.StartsWith("REMARK 350"))
            {
                if (line.Contains("BIOMT1"))
                {
                    matrix = Matrix4x4.identity;
                    var split = line.Substring(24).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    matrix[0, 0] = float.Parse(split[0]);
                    matrix[0, 1] = float.Parse(split[1]);
                    matrix[0, 2] = float.Parse(split[2]);
                    matrix[0, 3] = float.Parse(split[3]);
                }

                if (line.Contains("BIOMT2"))
                {
                    var split = line.Substring(24).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    matrix[1, 0] = float.Parse(split[0]);
                    matrix[1, 1] = float.Parse(split[1]);
                    matrix[1, 2] = float.Parse(split[2]);
                    matrix[1, 3] = float.Parse(split[3]);
                }

                if (line.Contains("BIOMT3"))
                {
                    var split = line.Substring(24).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    matrix[2, 0] = float.Parse(split[0]);
                    matrix[2, 1] = float.Parse(split[1]);
                    matrix[2, 2] = float.Parse(split[2]);
                    matrix[2, 3] = float.Parse(split[3]);

                    matrices.Add(matrix);
                }
            }
        }

        return matrices;
    }
}

public class Atom
{
    public Atom() {}

    public Atom(Atom atom)
    {
        this.position = atom.position;

        this.name = atom.name;
        this.chain = atom.chain;
        this.symbol = atom.symbol;
        this.residueName = atom.residueName;

        this.radius = atom.radius;
        this.chainId = atom.chainId;
        this.symbolId = atom.symbolId;
        this.residueId = atom.residueId;
        this.residueIndex = atom.residueIndex;
    }

    public float radius;

    public int chainId;
    public int symbolId;
    public int residueId;
    public int residueIndex;

    public string name;
    public string chain;
    public string symbol;
    public string residueName;
    
    public Vector3 position;
}

public static class AtomHelper
{
    public static float[] AtomRadii = { 1.548f, 1.100f, 1.400f, 1.348f, 1.880f, 1.808f, 1.5f };
    public static string[] AtomSymbols = { "C", "H", "N", "O", "P", "S", "A" };

    // Color scheme taken from http://life.nthu.edu.tw/~fmhsu/rasframe/CPKCLRS.HTM
    // not used
    public static Color[] AtomColors =
    {
        new Color32(200,200,200,255),       // C        light grey
        new Color32(255,255,255,255),       // H        white       
        new Color32(143,143,255,255),       // N        light blue
        new Color32(240,0,0,255),           // O        red         
        new Color32(255,165,0, 255),        // P        orange      
        new Color32(255,200,50, 255) ,      // S        yellow    
		new Color32(255, 0,255, 255)         // A        purple   
    };


    public static string[] ResidueNames = { "ALA", "ARG", "ASN", "ASP", "CYS", "GLN", "GLU", "GLY", "HID", "HIE", "HIP", "HIS", "ILE", "LEU", "LYS", "MET", "PHE", "PRO", "SER", "THR", "TRP", "TYR", "VAL" };
    
    // Color scheme taken from http://life.nthu.edu.tw/~fmhsu/rasframe/COLORS.HTM
    public static Color[] ResidueColors =
    {
        new Color(200,200,200) / 255,     // ALA      dark grey
        new Color(20,90,255) / 255,       // ARG      blue       
        new Color(0,220,220) / 255,       // ASN      cyan   
        new Color(230,10,10) / 255,       // ASP      bright red
        new Color(255,200,50) / 255,      // CYS      yellow 
        new Color(0,220,220) / 255,       // GLN      cyan   
        new Color(230,10,10) / 255,       // GLU      bright red
        new Color(235,235,235) / 255,     // GLY      light grey
        new Color(130,130,210) / 255,     // HID      pale blue
        new Color(130,130,210) / 255,     // HIE      pale blue
        new Color(130,130,210) / 255,     // HIP      pale blue
        new Color(130,130,210) / 255,     // HIS      pale blue
        new Color(15,130,15) / 255,       // ILE      green  
        new Color(15,130,15) / 255,       // LEU      green  
        new Color(20,90,255) / 255,       // LYS      blue       
        new Color(255,200,50) / 255,      // MET      yellow 
        new Color(50,50,170) / 255,       // PHE      mid blue
        new Color(220,150,130) / 255,     // PRO      flesh  
        new Color(250,150,0) / 255,       // SER      orange 
        new Color(250,150,0) / 255,       // THR      orange 
        new Color(180,90,180) / 255,      // TRP      pink   
        new Color(50,50,170) / 255,       // TYR      mid blue
        new Color(15,130,15) / 255        // VAL      green  
    };

    public static int GetNumChains(List<Atom> atoms)
    {
        var lastChainId = -2;
        var chainCount = 0;

        foreach (var atom in atoms.Where(atom => atom.chainId != lastChainId))
        {
            chainCount ++;
            lastChainId = atom.chainId;
        }

        return chainCount;
    }

    public static bool IsFromCustomStructureFile(List<Atom> atoms)
    {
        return atoms.All(atom => atom.symbolId == -1);
    }

    public static bool ContainsCarbonAlphaOnly(List<Atom> atoms)
    {
        return atoms.All(atom => String.CompareOrdinal(atom.name, "CA") == 0);
    }

    public static List<Vector3> GetAtomPoints(List<Atom> atoms)
    {
        var points = new List<Vector3>();
        for (int i = 0; i < atoms.Count; i++)
        {
            points.Add(new Vector3(atoms[i].position.x, atoms[i].position.y, atoms[i].position.z));
        }
        return points;
    }

    public static List<Vector4> GetAtomSpheres(List<Atom> atoms)
    {
        var spheres = new List<Vector4>();
        for (int i = 0; i < atoms.Count; i++)
        {
            var symbolId = Array.IndexOf(AtomSymbols, atoms[i].symbol);
            if (symbolId < 0) symbolId = 0;

            spheres.Add(new Vector4(atoms[i].position.x, atoms[i].position.y, atoms[i].position.z, atoms[i].radius));
        }

        return spheres;
    }

    public static void CenterAtoms(ref List<Atom> atoms)
    {
        var centerPosition = ComputeBounds(atoms).center;

        // Center atoms
        OffsetAtoms(ref atoms, centerPosition);
    }

    public static void CenterSpheres(ref List<Vector4> spheres)
    {
        var centerPosition = ComputeBounds(spheres).center;

        // Center atoms
        OffsetSpheres(ref spheres, centerPosition);
    }

    public static void OffsetAtoms(ref List<Atom> atoms, Vector3 offset)
    {
        for (var i = 0; i < atoms.Count(); i++)
        {
            atoms[i].position = atoms[i].position - offset;
        }
    }

    public static void OverwriteRadii(ref List<Atom> atoms, float radius)
    {
        for (var i = 0; i < atoms.Count(); i++)
        {
            atoms[i].radius = radius;
        }
    }

    public static void OverwriteRadii(ref List<Vector4> spheres, float radius)
    {
        for (var i = 0; i < spheres.Count(); i++)
        {
            spheres[i] = new Vector4(spheres[i].x, spheres[i].y, spheres[i].z, radius);
        }
    }

    public static void OffsetSpheres(ref List<Vector4> spheres, Vector3 offset)
    {
        var offsetVector = new Vector4(offset.x, offset.y, offset.z, 0);

        for (var i = 0; i < spheres.Count(); i++)
        {
            spheres[i] -= offsetVector;
        }
    }

    public static void OffsetPoints(ref List<Vector3> points, Vector3 offset)
    {
        //var offsetVector = new Vector4(offset.x, offset.y, offset.z, 0);

        for (var i = 0; i < points.Count(); i++)
        {
            points[i] -= offset;
        }
    }

    //public static Vector3 CenterAtoms(ref List<Atom> atoms)
    //{
    //    var bounds = ComputeBounds(atoms);

    //    for (var i = 0; i < atoms.Count(); i++)
    //    {
    //        atoms[i].position = new Vector3(atoms[i].position.x, atoms[i].position.y, atoms[i].position.z) - bounds.center;
    //    }

    //    return bounds.center;
    //}

    public static float ComputeRadius(List<Atom> atoms)
    {
        return atoms.Select(atom => Vector3.Magnitude(atom.position)).Concat(new float[] {0}).Max();
    }

    public static float ComputeRadius(List<Vector4> spheres)
    {
        return spheres.Select(sphere => Vector3.Magnitude(sphere)).Concat(new float[] {0}).Max();
    }

    public static Bounds ComputeBounds(List<Atom> atoms)
    {
        var bbMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var bbMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var atom in atoms)
        {
            bbMin = Vector3.Min(bbMin, new Vector3(atom.position.x, atom.position.y, atom.position.z));
            bbMax = Vector3.Max(bbMax, new Vector3(atom.position.x, atom.position.y, atom.position.z));
        }

        var bbSize = bbMax - bbMin;
        var bbCenter = bbMin + bbSize * 0.5f;

        return new Bounds(bbCenter, bbSize);
    }

    public static Bounds ComputeBounds(List<Vector4> spheres)
    {
        var bbMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var bbMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var sphere in spheres)
        {
            bbMin = Vector3.Min(bbMin, sphere);
            bbMax = Vector3.Max(bbMax, sphere);
        }

        var bbSize = bbMax - bbMin;
        var bbCenter = bbMin + bbSize * 0.5f;
		if (spheres.Count == 1) {
			bbSize = new Vector3(spheres[0].w-0.5f,spheres[0].w-0.5f,spheres[0].w-0.5f);
			bbCenter = new Vector3(spheres[0].x,spheres[0].y,spheres[0].z);
		}
        return new Bounds(bbCenter, bbSize);
    }

    public static Bounds ComputeBounds(List<Vector3> points)
    {
        var bbMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var bbMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var point in points)
        {
            bbMin = Vector3.Min(bbMin, point);
            bbMax = Vector3.Max(bbMax, point);
        }

        var bbSize = bbMax - bbMin;
        var bbCenter = bbMin + bbSize * 0.5f;

        return new Bounds(bbCenter, bbSize);
    }

    public static List<Atom> BuildBiomt(List<Atom> atoms, List<Matrix4x4> transforms)
    {
        // Code de debug, permet de comparer avec un resultat valide
        // La je load tous les atoms d'un coup et je les transform individuelement
        var biomtAtoms = new List<Atom>();

        foreach (var transform in transforms)
        {
            var posBiomt = new Vector3(transform.m03, transform.m13, transform.m23);
            var rotBiomt = MyUtility.RotationMatrixToQuaternion(transform);

            foreach (var atom in atoms)
            {
                var newAtom = new Atom(atom);
                newAtom.position = transform.MultiplyVector(atom.position) + posBiomt;
                biomtAtoms.Add(newAtom);
            }
        }

        return biomtAtoms;
    }

    public static List<Vector4> BuildBiomt(List<Vector4> atomSpheres, List<Matrix4x4> transforms)
    {
        // Code de debug, permet de comparer avec un resultat valide
        // La je load tous les atoms d'un coup et je les transform individuelement
        var biomtSpheres = new List<Vector4>();

        foreach (var transform in transforms)
        {
            var posBiomt = new Vector3(transform.m03, transform.m13, transform.m23);
            var rotBiomt = MyUtility.RotationMatrixToQuaternion(transform);

            foreach (var sphere in atomSpheres)
            {
                //var atomPos = Helper.QuaternionTransform(rotBiomt, sphere) + posBiomt;
                var atomPos = transform.MultiplyVector(sphere) + posBiomt;
                biomtSpheres.Add(new Vector4(atomPos.x, atomPos.y, atomPos.z, sphere.w));
            }
        }

        return biomtSpheres;
    }

   // public static Vector3 GetBiomtCenter(List<Matrix4x4> transforms, Vector3 center)
   // {
   //     if (transforms.Count <= 0) return Vector3.zero;

   //     var bbMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
   //     var bbMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

   //     foreach (var transform in transforms)
   //     {
			//var euler = MyUtility.euler_from_matrix(transform);
			//var rotBiomt = MyUtility.MayaRotationToUnity(euler);
			//var offset = MyUtility.QuaternionTransform(rotBiomt,center);//Helper.RotationMatrixToQuaternion(matBiomt), GetCenter());
			//var posBiomt = new Vector3(-transform.m03, transform.m13, transform.m23);

   //         bbMin = Vector3.Min(bbMin, new Vector3(posBiomt.x, posBiomt.y, posBiomt.z));
   //         bbMax = Vector3.Max(bbMax, new Vector3(posBiomt.x, posBiomt.y, posBiomt.z));
   //     }

   //     var bbSize = bbMax - bbMin;
   //     var bbCenter = bbMin + bbSize * 0.5f;
   //     var bounds = new Bounds(bbCenter, bbSize);

   //     return bounds.center;
   // }

    public static List<List<Vector4>> ComputeLodProxies(List<Vector4> atomSpheres, List<float> clusterLevelFactors)
    {
        var lodProxies = new List<List<Vector4>>();

        foreach (var level in clusterLevelFactors)
        {
            var numClusters = Math.Max(atomSpheres.Count * level, 5);
            var loxProxySpheres = (level >= 1) ? new List<Vector4>(atomSpheres) : KMeansClustering.GetClusters(atomSpheres, (int)numClusters);
            if (level < 1) OverwriteRadii(ref loxProxySpheres, 0);
            lodProxies.Add(loxProxySpheres);
        }

        return lodProxies;
    }

    public static List<List<Vector4>> ComputeLodProxiesBiomt(List<Vector4> atomSpheres, List<Matrix4x4> biomtTransforms, List<float> decimationFactors)
    {
        var clusterLevelsBiomt = new List<List<Vector4>>();
        var clusterLevels = ComputeLodProxies(atomSpheres, decimationFactors);

        foreach (var level in clusterLevels)
        {
            var biomtSpheres = AtomHelper.BuildBiomt(level, biomtTransforms);
            //AtomHelper.CenterSpheres(ref biomtSpheres);
            clusterLevelsBiomt.Add(biomtSpheres);
        }

        return clusterLevelsBiomt;
    }
}