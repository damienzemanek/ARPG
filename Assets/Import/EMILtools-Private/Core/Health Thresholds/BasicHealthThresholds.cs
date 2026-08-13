using EMILtools.Core;
using UnityEngine;



[CreateAssetMenu(fileName = "BasicHealthThresholds", menuName = "ScriptableObjects/Health Thresholds/Basic")]
public class BasicHealthThresholds : Threshold<LivingEntity.BasicHealthThresholdEnum, PersistentAction<LivingEntity.BasicHealthThresholdEnum>>
{

}
