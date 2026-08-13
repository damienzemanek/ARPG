using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] Transform follow;
    [SerializeField] bool releaseFromParentOnSpawn = true;

    protected virtual void Start()
    {
        if (releaseFromParentOnSpawn)
        {
            name = name + $" [ Following {transform.parent.name} ]";
            transform.parent = null;
        }
    }

    protected virtual void Update()
    {
        if(follow) transform.position = follow.position;
    }
}