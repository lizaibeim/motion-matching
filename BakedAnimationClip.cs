using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class BakedAnimationClip
{
    public string clipName;
    public Pose[] bakedClipPoses;

    public void Reset()
    {
        clipName = "";
        bakedClipPoses = null;
    }

}