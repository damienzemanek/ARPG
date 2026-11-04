using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FakeGodRay : MonoBehaviour
{
    [Header("Shape (meters)")]
    [Min(0.01f)] public float width = 2f;
    [Min(0.01f)] public float height = 8f;
    [Range(-1f, 1f)] public float topWidthRatio = 0.15f;

    [Header("Appearance")]
    [ColorUsage(true, true)] public Color rayColor = new Color(1f, 0.85f, 0.55f, 1f);
    [Range(0f, 1f)] public float opacity = 0.18f;
    [Range(0.5f, 8f)] public float softness = 2.5f;
    [Range(0f, 1f)] public float lengthFade = 0.6f;
    [Range(0f, 1f)] public float noiseStrength = 0.15f;
    [Range(0.1f, 30f)] public float noiseScale = 5f;
    [Range(-5f, 5f)] public float noiseSpeed = 0.2f;
    [Header("Animation")]
    public bool flicker = false;
    [Range(0f, 1f)] public float flickerAmount = 0.1f;
    [Min(0f)] public float flickerSpeed = 0.7f;
    [Header("Camera")]
    public bool faceCamera = true;
    public Camera targetCamera;

    Mesh mesh;
    MaterialPropertyBlock block;
    MeshRenderer meshRenderer;
    float seed;

    void OnEnable()
    {
        seed = Random.value * 100f;
        meshRenderer = GetComponent<MeshRenderer>();
        BuildMesh();
        UpdateMaterial();
    }
    void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        BuildMesh();
        UpdateMaterial();
    }
    void Update()
    {
        if (faceCamera)
        {
            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam != null)
            {
                // Rotate only around local Y, retaining the beam's tilt/height direction.
                Vector3 toCamera = cam.transform.position - transform.position;
                Vector3 local = transform.InverseTransformDirection(toCamera);
                local.y = 0f;
                if (local.sqrMagnitude > 0.0001f)
                    transform.Rotate(Vector3.up, Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, Space.Self);
            }
        }
        UpdateMaterial();
    }
    void BuildMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh { name = "Generated Fake God Ray" };
            mesh.hideFlags = HideFlags.DontSave;
        }
        float bottom = width * 0.5f;
        float top = bottom * Mathf.Max(0f, topWidthRatio);
        mesh.Clear();
        mesh.vertices = new[] {
            new Vector3(-top, 0, 0), new Vector3(top, 0, 0),
            new Vector3(-bottom, -height, 0), new Vector3(bottom, -height, 0)
        };
        mesh.uv = new[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0) };
        mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
    void UpdateMaterial()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (block == null) block = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(block);
        float pulse = flicker ? 1f - flickerAmount * (0.5f + 0.5f * Mathf.Sin(Time.realtimeSinceStartup * flickerSpeed * 6.28318f + seed)) : 1f;
        block.SetColor("_RayColor", rayColor);
        block.SetFloat("_Opacity", opacity * pulse);
        block.SetFloat("_Softness", softness);
        block.SetFloat("_LengthFade", lengthFade);
        block.SetFloat("_NoiseStrength", noiseStrength);
        block.SetFloat("_NoiseScale", noiseScale);
        block.SetFloat("_NoiseSpeed", noiseSpeed);
        meshRenderer.SetPropertyBlock(block);
    }
    void OnDisable()
    {
        if (mesh != null)
        {
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
            mesh = null;
        }
    }
}
