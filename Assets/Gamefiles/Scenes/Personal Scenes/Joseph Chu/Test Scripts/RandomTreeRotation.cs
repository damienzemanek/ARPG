
using UnityEngine;

public class RandomTreeRotation : MonoBehaviour
{
    [Header("Base Rotation")]
    [Tooltip("Original rotation of your tree")]
    public Vector3 baseRotation = new Vector3(90f, 0f, 0f);

    [Header("Random Tilt Settings")]
    [Range(0f, 45f)]
    public float tiltForwardBackward = 10f;

    [Range(0f, 45f)]
    public float tiltLeftRight = 10f;

    [Header("Random Y Rotation")]
    public bool randomizeY = true;

    [Header("Settings")]
    public bool randomizeOnStart = false;

    private void Start()
    {
        if (randomizeOnStart)
        {
            RandomizeRotation();
        }
    }

    [ContextMenu("Randomize Rotation")]
    public void RandomizeRotation()
    {
        // Random tilt angles
        float x = Random.Range(
            -tiltForwardBackward,
            tiltForwardBackward
        );

        float z = Random.Range(
            -tiltLeftRight,
            tiltLeftRight
        );

        float y = randomizeY
            ? Random.Range(0f, 360f)
            : 0f;

        // Original rotation (90 degrees, etc.)
        Quaternion original = Quaternion.Euler(baseRotation);

        // Random tilt around the tree's upright axis
        Quaternion tilt = Quaternion.Euler(x, 0f, z);

        // Random facing direction
        Quaternion facing = Quaternion.Euler(0f, y, 0f);

        // Preserve base rotation and add variation
        transform.localRotation = facing * tilt * original;
    }
}
