using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Joint
{
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 velocity; //joint velocity

    //For debug printing
    public string jointName;
    public int jointIndex;
    public Vector3 position; //world position
    public Quaternion rotation;
}
