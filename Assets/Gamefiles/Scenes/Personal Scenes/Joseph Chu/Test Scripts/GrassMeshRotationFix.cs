
using UnityEngine;

public class GrassMeshRotationFix : MonoBehaviour
{
    [Header("Mesh Rotation Correction")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    [ContextMenu("Bake Rotation Into Mesh")]
    public void BakeRotation()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogError("Missing MeshFilter or Mesh!");
            return;
        }

        Mesh original = meshFilter.sharedMesh;
        Mesh corrected = Instantiate(original);
        corrected.name = original.name + "_Corrected";

        Quaternion rotation = Quaternion.Euler(rotationOffset);

        Vector3[] vertices = corrected.vertices;
        Vector3[] normals = corrected.normals;
        Vector4[] tangents = corrected.tangents;

        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = rotation * vertices[i];

        if (normals.Length == vertices.Length)
        {
            for (int i = 0; i < normals.Length; i++)
                normals[i] = rotation * normals[i];
        }

        if (tangents.Length == vertices.Length)
        {
            for (int i = 0; i < tangents.Length; i++)
            {
                Vector3 direction = rotation *
                    new Vector3(tangents[i].x, tangents[i].y, tangents[i].z);

                tangents[i] = new Vector4(
                    direction.x, direction.y, direction.z, tangents[i].w);
            }
        }

        corrected.vertices = vertices;
        corrected.normals = normals;
        corrected.tangents = tangents;
        corrected.RecalculateBounds();

#if UNITY_EDITOR
        string path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(
            "Assets/" + corrected.name + ".asset");

        UnityEditor.AssetDatabase.CreateAsset(corrected, path);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        meshFilter.sharedMesh = corrected;

        Debug.Log("Grass mesh rotation baked successfully!");
    }
}
