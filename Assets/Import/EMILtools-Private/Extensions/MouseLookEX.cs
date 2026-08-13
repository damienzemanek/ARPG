using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace EMILtools.Extensions
{
    public static class MouseLookEX
    {
        const int SENSITIVITY_ADJUSTMENT = 5;
        
        [Serializable]
        public class MouseLookSettings
        {
            [BoxGroup("ReadOnly")] [SerializeField, ReadOnly] Vector2 look;
            [BoxGroup("ReadOnly")] [SerializeField, ReadOnly] Vector2 rot;
            
            [BoxGroup("References")] [SerializeField] [ShowIf("useBody")] Transform body;
            [BoxGroup("References")] [SerializeField] Transform head;

            [field: BoxGroup("References")] [field: ShowInInspector] [field: ReadOnly] public IInputMouseLook Input { get; set; }
            
            [BoxGroup("Settings")] [SerializeField] public bool useBody = true;
            [BoxGroup("Settings")] [SerializeField] public bool clampXRotation = false;
            [BoxGroup("Settings")] [SerializeField] public bool clampYRotation = true;
            [BoxGroup("Settings")] [SerializeField] public bool updateMouseLook = true;
            [BoxGroup("Settings")] [SerializeField] bool invertX = false;
            [BoxGroup("Settings")] [SerializeField] bool invertY = true;
            
            [BoxGroup("Settings")] [SerializeField] Vector2 sensitivity = new Vector2(1, 1);
            [BoxGroup("Settings")] [SerializeField] [ShowIf("clampXRotation")] Vector2 clampX = new Vector2(-90f, 90f);
            [BoxGroup("Settings")] [SerializeField] [ShowIf("clampYRotation")] Vector2 clampY = new Vector2(-90f, 90f);

            public void UpdateMouseLook()
            {
                if (!updateMouseLook) return;
                
                // Grab the input
                Vector2 mouseInput = Input.mouse; //Debug.Log("Mouse Input: " + mouseInput);
                
                // Apply sensitivity
                look = mouseInput * sensitivity / SENSITIVITY_ADJUSTMENT;

                look.x *= invertX ? -1f : 1f;
                look.y *= invertY ? -1f : 1f;

                // Apply the rotation to the variable
                rot.x += look.x;
                rot.y += look.y;
                if(clampXRotation) rot.x = Mathf.Clamp(rot.x, clampX.x, clampX.y);
                if(clampYRotation) rot.y = Mathf.Clamp(rot.y, clampY.x, clampY.y);
                
                // Use the variable on the transforms
                if(useBody) body.localRotation = Quaternion.Euler(0, rot.x, 0);
                head.transform.localRotation = Quaternion.Euler(rot.y, rot.x, 0);
            }
        }
        
        

         
        [Serializable]
        public class PositionToMouseWorldSpace
        {
            public Camera cam;
            public MouseToWorldSpace core;
            
            bool isLerp => moveOptions == MoveOptions.Lerp;
            
            [BoxGroup("References")] public Transform objectToMove;
            public enum MoveOptions { Teleport, Lerp }

            public Vector3 offset = Vector3.zero;
            public bool lockX;
            public bool lockY;
            public bool lockZ;
            [ShowIf("lockX")] public float lockPosX;
            [ShowIf("lockY")]public float lockPosY;
            [ShowIf("lockZ")]public float lockPosZ;
            [ShowIf("isLerp")] public float lerpSpeed = 5f;

            public MoveOptions moveOptions;
            
            public void Execute()
            {
                switch (moveOptions)
                {
                    case MoveOptions.Teleport: objectToMove.position = PositionalInteroggative(); break;
                    case MoveOptions.Lerp: objectToMove.position = Vector3.Lerp(objectToMove.position, PositionalInteroggative() + offset, Time.deltaTime * lerpSpeed); break;
                }
            }

            Vector3 PositionalInteroggative()
            {
                Vector3 ret = core.GetHitPoint(cam) + offset;
                if(lockX) ret.x = lockPosX;
                if(lockY) ret.y = lockPosY;
                if(lockZ) ret.z = lockPosZ;
                return ret;
            }
        }


        [Serializable]
        public class RotateToMouseWorldSpace
        {
            
            [Serializable]
            public struct RotatingObject
            {
                public float rotateScaler;
                
                public Transform transform;

                public bool swapXWithZ;
                
                public bool rotateX;
                public bool rotateY;
                public bool rotateZ;
                
                public bool flipX;
                public bool flipY;
                public bool flipZ;

                public bool clampX;
                public bool clampY;
                public bool clampZ;

                [ShowIf("clampX")] public Vector2 clampXrot;
                [ShowIf("clampY")] public Vector2 clampYrot;
                [ShowIf("clampZ")] public Vector2 clampZrot;
            }

            [ReadOnly] public Camera cam;
            public MouseToWorldSpace core;
            [BoxGroup("References")] public RotatingObject[] rotatingObjects;
            [BoxGroup("ReadOnly")] [ShowInInspector, ReadOnly] public Quaternion rotation;



            public void Execute(Transform center = null)
            => RotateToMouseDirection(rotatingObjects, core.GetHitPoint(cam, center));

            void RotateToMouseDirection (RotatingObject[] transform, Vector3 destination)
            {
                if (rotatingObjects == null) return;
                
                foreach (var ro in transform)
                {
                    Vector3 prevEuler = ro.transform.eulerAngles;
                    ro.transform.LookAt(destination);
                    Vector3 lookAtEuler = ro.transform.eulerAngles;

                    if (ro.swapXWithZ)
                    {
                        float save = lookAtEuler.x;
                        lookAtEuler.x = lookAtEuler.z;
                        lookAtEuler.z = save; 
                    }

                    if (!ro.rotateX) lookAtEuler.x = prevEuler.x;
                    if (!ro.rotateY) lookAtEuler.y = prevEuler.y;
                    if (!ro.rotateZ) lookAtEuler.z = prevEuler.z;
                    
                    if (ro.flipX) lookAtEuler.x *= -1;
                    if (ro.flipY) lookAtEuler.y *= -1;
                    if (ro.flipZ) lookAtEuler.z *= -1f;
                                        
                    if (ro.clampX) lookAtEuler.x = Mathf.Clamp(lookAtEuler.x, ro.clampXrot.x, ro.clampXrot.y);
                    if (ro.clampY) lookAtEuler.y = Mathf.Clamp(lookAtEuler.y, ro.clampXrot.x, ro.clampXrot.y);
                    if (ro.clampZ) lookAtEuler.z = Mathf.Clamp(lookAtEuler.z, ro.clampXrot.x, ro.clampXrot.y);
                    

                    ro.transform.rotation = Quaternion.Euler(lookAtEuler);
                    //ro.transform.localEulerAngles = new Vector3(ro.transform.localEulerAngles.x * ro.rotateScaler, 0, ro.transform.localEulerAngles.z);


                }
                
                // foreach (var ro in transform)
                // {
                //                         Vector3 current = ro.transform.localEulerAngles;
                //
                //     core.direction = destination - ro.transform.position;
                //
                //     if (ro.flipX) core.direction.x *= -1;
                //     if (ro.flipY) core.direction.y *= -1;
                //     if (ro.flipZ) core.direction.z *= -1;
                //
                //     // Base rotation
                //     Vector3 localDir = ro.transform.parent.InverseTransformDirection(core.direction);
                //     rotation = Quaternion.LookRotation(localDir, Vector3.up);
                //     // Convert to euler
                //     Vector3 euler = rotation.eulerAngles;
                //
                //     // Normalize for clamping
                //     euler.x = NormalizeAngle(euler.x);
                //     euler.y = NormalizeAngle(euler.y);
                //     euler.z = NormalizeAngle(euler.z);
                //
                //     // Clamp
                //     if (ro.clampX) euler.x = Mathf.Clamp(euler.x, ro.clampXrot.x, ro.clampXrot.y);
                //     if (ro.clampY) euler.y = Mathf.Clamp(euler.y, ro.clampYrot.x, ro.clampYrot.y);
                //     if (ro.clampZ) euler.z = Mathf.Clamp(euler.z, ro.clampZrot.x, ro.clampZrot.y);
                //     
                //     
                //     // Rotate Angle toggles
                //     Vector3 current = ro.transform.localEulerAngles;
                //     current.x = NormalizeAngle(current.x);
                //     current.y = NormalizeAngle(current.y);
                //     current.z = NormalizeAngle(current.z);
                //     if(!ro.rotateX) euler.x = current.x;
                //     if(!ro.rotateY) euler.y = current.y;
                //     if(!ro.rotateZ) euler.z = current.z;
                //
                //     // Rebuild quaternion & apply
                //     ro.transform.localRotation = Quaternion.Euler(euler);
                //     
                //     Debug.Log("Rotating");
                // }
            }
            
            public static implicit operator MouseToWorldSpace(RotateToMouseWorldSpace self) => self.core;
            
            
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

    }
    
    

}