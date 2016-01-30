using UnityEngine;
using UnityEditor;

class MyWindow : EditorWindow
{
    private readonly string[] _contourOptionsLabels = { "Show Contour", "Hide Contour", "Contour Only" };

    void OnGUI()
    {
        //GUIStyle style_1 = new GUIStyle();
        //style_1.margin = new RectOffset(5, 5, 5, 5);
        
        EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        PersistantSettings.Get.Scale = EditorGUILayout.Slider("Global scale", PersistantSettings.Get.Scale,
            0.0001f, 1);
        PersistantSettings.Get.ContourStrength = EditorGUILayout.Slider("Contour strength",
            PersistantSettings.Get.ContourStrength, 0, 1);
        PersistantSettings.Get.ContourOptions = EditorGUILayout.Popup("Contours Options",
            PersistantSettings.Get.ContourOptions, _contourOptionsLabels);
        //DisplaySettings.Get.EnableOcclusionCulling = EditorGUILayout.Toggle("Enable Culling", DisplaySettings.Get.EnableOcclusionCulling);
        PersistantSettings.Get.DebugObjectCulling = EditorGUILayout.Toggle("Debug Culling",
            PersistantSettings.Get.DebugObjectCulling);
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        PersistantSettings.Get.EnableLod = EditorGUILayout.BeginToggleGroup("Level of Detail",
            PersistantSettings.Get.EnableLod);
        {
            PersistantSettings.Get.FirstLevelOffset = EditorGUILayout.FloatField("First Level Being Range",
                PersistantSettings.Get.FirstLevelOffset);

            EditorGUI.indentLevel++;
            for (int i = 0; i <= SceneManager.Get.NumLodLevels; i++)
            {
                EditorGUILayout.LabelField("Level " + i, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                var x = EditorGUILayout.FloatField("End Range", PersistantSettings.Get.LodLevels[i].x);
                var y = EditorGUILayout.FloatField("Min Radius", PersistantSettings.Get.LodLevels[i].y);
                var z = EditorGUILayout.FloatField("Max Radius", PersistantSettings.Get.LodLevels[i].z);

                var lodInfo = new Vector4(x, y, z, 1);

                if (PersistantSettings.Get.LodLevels[i] != lodInfo)
                {
                    PersistantSettings.Get.LodLevels[i] = lodInfo;
                    GPUBuffers.Instance.LodInfo.SetData(PersistantSettings.Get.LodLevels);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndToggleGroup();
    }
}