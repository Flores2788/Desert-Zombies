using UnityEngine;
using UnityEditor;
using System.IO;

public class RefreshPrefabMeshes
{
    [MenuItem("Tools/Refresh All Prefab Meshes")]
    static void Refresh()
    {
        string folder = EditorUtility.OpenFolderPanel("Select Prefab Folder", "Assets", "");
        if (string.IsNullOrEmpty(folder))
            return;

        // Convert absolute path to relative Assets path
        if (folder.StartsWith(Application.dataPath))
            folder = "Assets" + folder.Substring(Application.dataPath.Length);

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
        int prefabCount = 0;
        int meshCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            bool modified = false;

            // Open prefab for editing
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            // Refresh MeshFilters
            MeshFilter[] meshFilters = prefabRoot.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter mf in meshFilters)
            {
                if (mf.sharedMesh != null)
                {
                    Mesh mesh = mf.sharedMesh;
                    mf.sharedMesh = null;
                    mf.sharedMesh = mesh;
                    meshCount++;
                    modified = true;
                }
            }

            // Refresh SkinnedMeshRenderers
            SkinnedMeshRenderer[] skinned = prefabRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer smr in skinned)
            {
                if (smr.sharedMesh != null)
                {
                    Mesh mesh = smr.sharedMesh;
                    smr.sharedMesh = null;
                    smr.sharedMesh = mesh;
                    meshCount++;
                    modified = true;
                }
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                prefabCount++;
            }

            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Refreshed {meshCount} meshes across {prefabCount} prefabs in: {folder}");
    }
}