using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MotionCostFactorSettings", menuName = "MotionMatcher/MotionCostFactorSettings")]
public class MotionCostFactorSettings : ScriptableObject
{
    public float bonePosFactor = 1.0f;
    public float boneRotFactor = 1.0f;
    public float boneVelFactor = 1.0f;
    public float rootMotionVelFactor = 1.5f;
    public float rootMotionAngularVelFactor = 1.5f;
    public float candidateTrajectoryPosFactor = 1.5f;
    public float candidateTrajectoryVelFactor = 1.0f;
    public float candidateTrajectoryDirFactor = 1.5f;
    public float finalTrajectoryPosFactor = 2.0f;
    public float finalTrajectoryDirFactor = 2.0f;
}
