using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class ColliderEX
    {
        public static bool TagIs(this Collider c, string tag) => (c.tag == tag);
        public static bool TagIs(this Collision c, string tag) => (c.transform.tag == tag);
    }
}
