using EMILtools.Extensions;
using UnityEngine;

public class TeleportLoc : MonoBehaviour
{
    public void Teleport(GameObject teleportee) => NavEX.Teleport(transform, teleportee, out _);
}