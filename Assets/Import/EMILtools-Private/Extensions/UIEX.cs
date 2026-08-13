using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class UIEX
    {
        [Serializable]
        public struct ChangePosSettings
        {
            public Transform newParent;
            public Transform oldParent;

            public GameObject Obj;
        }


        public static void ChangePos(this MonoBehaviour host, bool goToNew, ChangePosSettings change)
        {
            Transform parent = change.newParent;
            if (!goToNew) parent = change.oldParent;

            change.Obj.transform.SetParent(parent);
            change.Obj.transform.Lerp(Vector3.zero, 1f, host, local: true);
            change.Obj.transform.LerpRot(Quaternion.identity, 1f, host, local: true);
        }
    }
}
