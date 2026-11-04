using DG.Tweening;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class BirdMove : MonoBehaviour
{

    public Transform b;
    public Vector3 finalrot;
    public float speed = 5f;
    
    public void Move()
    {
        transform.DOMove(b.position, speed);
    }

    [Button]
    public void Rotate() 
    {
        transform.DORotate(new Vector3(0, 360, 0), speed, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart) // Infinite loops
            .SetEase(Ease.Linear);          // Smooth, constant speed     
    }
}
