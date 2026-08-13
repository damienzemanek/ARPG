using UnityEngine;
using static EMILtools.Extensions.NumEX;

[CreateAssetMenu(fileName = "TurnSlowDown", menuName = "EMILtools/ScriptableObjects/Movement/TurnSlowDown")]
public class TurnSlowDown : ScriptableObject
{
    public AnimationCurve Curve(bool isGrounded) => (isGrounded ? curveGrounded : curveInAir);
    
    public AnimationCurve curveGrounded;
    public AnimationCurve curveInAir;
    public float duration = 0.1f;

    public float Eval(bool isGrounded, float prog, bool flip = true)
    {
        float potentialFlip = (flip) ? Flip01(prog) : prog;
        return Curve(isGrounded).Evaluate(potentialFlip);
    }
}