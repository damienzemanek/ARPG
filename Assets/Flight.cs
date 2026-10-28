using System;
using System.Collections;
using UnityEngine;

public class RepeatAnimation : MonoBehaviour
{
    private Animator anim;

    // Replace with the exact name of your animation state in the Animator window
    [SerializeField] private string animationStateName = "BirdAnim";

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(PlayAnimationLoop());
    }

    IEnumerator PlayAnimationLoop()
    {   
        while (true)
        {
            // Play the animation state from the beginning (layer 0, normalized time 0f)
            anim.Play(animationStateName, 0, 5f);

            // Wait for 5 seconds before repeating
            yield return new WaitForSeconds(5f);
        }
    }
}

public class Flight : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        }
}

