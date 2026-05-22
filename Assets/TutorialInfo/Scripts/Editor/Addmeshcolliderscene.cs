using UnityEngine;
using UnityEditor;

public class AddMeshCollidersToScene
{
    [MenuItem("Tools/Add Mesh Colliders To All Meshes In Scene")]
    static void AddColliders()
    {
        int count = 0;
        MeshFilter[] meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null)
                continue;

            // Skip if already has any collider
            if (mf.GetComponent<Collider>() != null)
                continue;

            MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
            EditorUtility.SetDirty(mf.gameObject);
            count++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"Added MeshCollider to {count} objects in scene.");
    }
}