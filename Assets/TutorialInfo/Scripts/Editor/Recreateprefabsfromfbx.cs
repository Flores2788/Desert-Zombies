using UnityEngine;
using UnityEditor;
using System.IO;

public class RecreatePrefabsFromFBX
{
    [MenuItem("Tools/Recreate Prefabs From FBX")]
    static void Recreate()
    {
        string fbxFolder = EditorUtility.OpenFolderPanel("Select Folder With FBX Files", "Assets", "");
        if (string.IsNullOrEmpty(fbxFolder))
            return;

        if (fbxFolder.StartsWith(Application.dataPath))
            fbxFolder = "Assets" + fbxFolder.Substring(Application.dataPath.Length);

        // Create output folder for new prefabs
        string prefabFolder = fbxFolder + "/GeneratedPrefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder(fbxFolder, "GeneratedPrefabs");
        }

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { fbxFolder });
        int count = 0;

        foreach (string guid in guids)
        {
            string fbxPath = AssetDatabase.GUIDToAssetPath(guid);

            // Skip anything inside the GeneratedPrefabs subfolder
            if (fbxPath.Contains("GeneratedPrefabs"))
                continue;

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (model == null)
                continue;

            // Instantiate the model
            GameObject instance = Object.Instantiate(model);
            instance.name = model.name;

            // Save as new prefab
            string prefabPath = prefabFolder + "/" + model.name + ".prefab";

            // Overwrite if it already exists
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created {count} new prefabs in: {prefabFolder}");
        EditorUtility.DisplayDialog("Done", $"Created {count} prefabs in:\n{prefabFolder}\n\nReplace old prefab references in your scenes with these.", "OK");
    }
}