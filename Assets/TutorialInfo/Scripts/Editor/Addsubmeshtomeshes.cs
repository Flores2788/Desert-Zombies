using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AddSubmeshToMeshes
{
    [MenuItem("Tools/Add Submesh To All Meshes In Scene")]
    static void AddSubmeshScene()
    {
        int count = 0;
        MeshFilter[] meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null)
                continue;

            Mesh original = mf.sharedMesh;

            // Mesh must be readable
            if (!original.isReadable)
            {
                Debug.LogWarning($"Mesh '{original.name}' on '{mf.gameObject.name}' is not Read/Write enabled. Skipping.");
                continue;
            }

            // Check how many materials the MeshRenderer expects
            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr == null)
                continue;

            int materialCount = mr.sharedMaterials.Length;

            // Skip if mesh already has enough submeshes
            if (original.subMeshCount >= materialCount)
                continue;

            Mesh newMesh = AddSubmeshes(original, materialCount);
            mf.sharedMesh = newMesh;
            EditorUtility.SetDirty(mf);
            count++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"Added submeshes to {count} meshes in scene.");
    }

    static Mesh AddSubmeshes(Mesh original, int targetCount)
    {
        Mesh mesh = Object.Instantiate(original);
        mesh.name = original.name + $"_{targetCount}sub";

        // Collect all existing submesh triangles
        List<int[]> existingSubmeshes = new List<int[]>();
        for (int i = 0; i < original.subMeshCount; i++)
        {
            existingSubmeshes.Add(original.GetTriangles(i));
        }

        // Get all triangles combined for new submeshes
        int[] allTriangles = original.GetTriangles(0);

        mesh.subMeshCount = targetCount;

        // Reassign existing submeshes
        for (int i = 0; i < existingSubmeshes.Count; i++)
        {
            mesh.SetTriangles(existingSubmeshes[i], i);
        }

        // Fill remaining slots with the full triangle list
        for (int i = existingSubmeshes.Count; i < targetCount; i++)
        {
            mesh.SetTriangles(allTriangles, i);
        }

        mesh.RecalculateBounds();
        return mesh;
    }
}