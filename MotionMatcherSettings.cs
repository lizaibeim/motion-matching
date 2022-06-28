using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MotionMatcherSettings", menuName = "MotionMatcher/MotionMatcherSettings")]
public class MotionMatcherSettings: ScriptableObject
{   
    [HideInInspector]
    public float[] predictFutureTrajectoryTimeList = {0, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f};

    [Tooltip("BakedTrajectoryTimeList")]
    public float[]  bakedTrajectoryTimeList = {0, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f, 1.2f, 1.4f, 1.6f, 1.8f, 2.0f};

    [Tooltip("fps of matching frames")]
    public int fps = 50;


    [Tooltip("AnalysedBonesNameList")]
    public string[] captureJointList = {"Root/Hips",
                                "Root/Hips/LeftThigh", "Root/Hips/LeftThigh/LeftShin", "Root/Hips/LeftThigh/LeftShin/LeftFoot",
                                "Root/Hips/RightThigh", "Root/Hips/RightThigh/RightShin", "Root/Hips/RightThigh/RightShin/RightFoot",
                                "Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm",
                                "Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm",
                                "Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand",
                                "Root/Hips/Spine1/Spine2/Spine3/Spine4/LeftShoulder/LeftArm",
                                "Root/Hips/Spine1/Spine2/Spine3/Spine4/LeftShoulder/LeftArm/LeftForeArm",
                                "Root/Hips/Spine1/Spine2/Spine3/Spine4/LeftShoulder/LeftArm/LeftForeArm/LeftHand",
                                "Root/Hips/Spine1/Spine2/Spine3/Spine4/Neck",
                                "Root/Hips/Spine1/Spine2/Spine3/Spine4/Neck/Head"                                
                                };

}