using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Animator animator;
    public string animName = "Test";

    private void Start()
    {
        animator.CrossFade(animName, 0.2f);
    }
}
