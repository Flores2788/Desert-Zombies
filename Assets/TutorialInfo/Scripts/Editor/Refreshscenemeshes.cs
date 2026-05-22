using UnityEngine;
using UnityEditor;

public class RefreshSceneMeshes
{
    [MenuItem("Tools/Refresh All Scene Meshes")]
    static void Refresh()
    {
        int count = 0;

        // Refresh MeshFilter references
        MeshFilter[] meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh != null)
            {
                Mesh mesh = mf.sharedMesh;
                mf.sharedMesh = null;
                mf.sharedMesh = mesh;
                EditorUtility.SetDirty(mf);
                count++;
            }
        }

        // Refresh SkinnedMeshRenderer references
        SkinnedMeshRenderer[] skinned = Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);
        foreach (SkinnedMeshRenderer smr in skinned)
        {
            if (smr.sharedMesh != null)
            {
                Mesh mesh = smr.sharedMesh;
                smr.sharedMesh = null;
                smr.sharedMesh = mesh;
                EditorUtility.SetDirty(smr);
                count++;
            }
        }

        // Mark scene as dirty so changes are saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"Refreshed {count} mesh references in scene.");
    }
}
