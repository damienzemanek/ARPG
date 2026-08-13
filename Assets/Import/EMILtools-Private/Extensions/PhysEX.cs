using System;
using EMILtools.Core;
using Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using EMILtools.Signals;
using EMILtools.Timers;
using UnityEngine.Serialization;
using static EMILtools.Signals.ModifierStrategies;
using static EMILtools.Extensions.NumEX;
using static EMILtools.Signals.StatTags;

namespace EMILtools.Extensions
{
    public static class PhysEX
    {
        [Serializable]
        public struct MoveSettings
        {
            public Stat<float, Speed> speed;
        }
        [Serializable]
        public struct GroundedSettings
        {
            public Transform feetPoint;
            public float checkDist;
            public LayerMask mask;
        }

        [Serializable]
        public struct FallSettings
        {
            public ForceMode forceMode;
            public float mult;
            public Vector3 dir;
            public float inAirMoveScalar;
        }
        
        [Serializable]
        public struct FallSettings2D
        {
            public ForceMode2D forceMode;
             public float mult;
            public Vector2 dir;
            public float inAirMoveScalar;
            
            public void SetMult(float mult) => this.mult = mult;
            public void SetInAirMoveScalar(float inairmovescalar) => this.inAirMoveScalar = inairmovescalar;
        }

        [Serializable]
        public struct JumpSettings
        {
            public ForceMode forceMode;
            public bool useGlobal;
            [ShowIf("useGlobal")] [FormerlySerializedAs("direction")] public Vector3 globalDirection;
            [SerializeField] public ClassRef<float> cooldown;
            public bool complexJump;
            [ShowInInspector, InlineProperty, ShowIf("complexJump")] public AnimationCurve forceCurve;
            [SerializeField, ShowIf("complexJump")]                  public ClassRef<float> inputMaxDuration;
        }
        
        [Serializable]
        public struct JumpSettings2D
        {
            public ForceMode2D forceMode;
            public Vector2 direction;
            [SerializeField] public ClassRef<float> cooldown;
            public ClassRef<float> inputMaxDuration;
        }

        [Serializable]
        public struct Explode
        {
            public Rigidbody[] rbs;
            public float strength;
            public float radius;
            public bool individualOrigin;
            bool NotIndividualOrigin => !individualOrigin;
            [ShowIf("individualOrigin")] public Transform origin;
            [ShowIf("NotIndividualOrigin")] public Transform[] origins;

            public ForceMode forceMode;
        }
        
        public static void Blast(this Explode explode)
        {
            Transform origin = null;

            for (int i = 0; i < explode.rbs.Length; i++)
            {
                Rigidbody rb = explode.rbs[i];
                if (explode.individualOrigin) origin = explode.origin;
                else origin = explode.origins.Rand();

                rb.AddExplosionForce(explode.strength, origin.position, explode.radius);
            }
            origin.Log("Exploding");
            
        }

        static void GroundDefaultCheck(this Transform t, ref GroundedSettings ground)
        {
            if (!ground.feetPoint)
            {
                var newFeetPoint = new GameObject("Feet Point Auto-Generated");
                newFeetPoint.transform.parent = t;
                newFeetPoint.transform.localPosition = t.position.With(y: t.position.y + 0.02f);
                ground.feetPoint = newFeetPoint.transform;
            }

            if (ground.checkDist == 0) ground.checkDist = 0.08f;
        }

        public static bool IsGrounded(this Transform transform, ref GroundedSettings ground)
        {
            transform.GroundDefaultCheck(ref ground);

            bool isGrounded = Physics.Raycast(ground.feetPoint.position,
                                        -transform.up,
                                        out RaycastHit hit,
                                        ground.checkDist,
                                        ground.mask);

            return isGrounded;
        }
        
        public static bool IsGrounded2D(this Transform transform, ref GroundedSettings ground)
        {
            transform.GroundDefaultCheck(ref ground);

            bool isGrounded = Physics2D.Raycast(
                ground.feetPoint.position,
                -transform.up,
                ground.checkDist,
                ground.mask);

            return isGrounded;
        }

        public static void FallFaster(this Rigidbody rb, FallSettings fall)
        {
            rb.AddForce(fall.dir * fall.mult, fall.forceMode);
        }
        
        public static void FallFaster2D(this Rigidbody2D rb, FallSettings2D fall)
        {
            rb.AddForce(fall.dir * fall.mult, fall.forceMode);
        }

        public static void Jump(this Rigidbody rb, JumpSettings jump)
        {
            Vector3 force = Vector3.zero;
            force += jump.globalDirection;
            rb.AddForce(force, jump.forceMode);
        }
        
        public static void Jump2D(this Rigidbody2D rb, JumpSettings2D jump)
        {
            Vector2 force = Vector2.zero;
            force += jump.direction;
            rb.AddForce(force, jump.forceMode);
        }

        /// <summary>
        /// Broken atm
        /// </summary>
        /// <param name="rb"></param>
        /// <param name="jump"></param>
        /// <param name="progress"></param>
        public static void JumpComplex(this Rigidbody rb, JumpSettings jump, float progress)
        {
            float mult = ZeroF;
             mult = jump.forceCurve.Evaluate(Flip01(progress));
           
            Vector3 dir = jump.globalDirection;
            Debug.Log(jump.globalDirection);
            rb.AddForce(dir, jump.forceMode);
        }

        public static void JumpScaled2D(this Rigidbody2D rb, JumpSettings2D jump, float progress)
        {
            progress = Mathf.Clamp01(progress);
            float mult = progress;
            Vector2 dir = jump.direction;
            rb.AddForce(dir * mult, jump.forceMode);
        }
        

        public static void InputDirectionalMove(this Rigidbody rb, Vector2 moveInput, MoveSettings move)
        {
            if (moveInput == Vector2.zero) return;

            float speedMult = move.speed;

            // if diagnally inputting
            if (Mathf.Abs(moveInput.x) > 0.5f && Mathf.Abs(moveInput.y) > 0.5f) 
                speedMult = speedMult * 0.7071f;

            if (moveInput.x != 0)
            {
                if (moveInput.x > 0.5)
                    rb.AddForce(rb.transform.right * (speedMult * 100), ForceMode.Force);
                if (moveInput.x < -0.5)
                    rb.AddForce(-rb.transform.right * (speedMult * 100), ForceMode.Force);
            }

            if (moveInput.y != 0)
            {
                if (moveInput.y > 0.5)
                    rb.AddForce(rb.transform.forward * (speedMult * 100), ForceMode.Force);
                if (moveInput.y < -0.5)
                    rb.AddForce(-rb.transform.forward * (speedMult * 100), ForceMode.Force);
            }
        }

    }
}
