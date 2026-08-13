using EMILtools.Core;
using UnityEngine;

[CreateAssetMenu(fileName = "PhasedHealthThresholds", menuName = "ScriptableObjects/Health Thresholds/Phased")]
public class PhasedHealthThresholds : Threshold<LivingEntity.PhasedHealthThresholdEnum, PersistentAction<LivingEntity.PhasedHealthThresholdEnum>>
{

}
