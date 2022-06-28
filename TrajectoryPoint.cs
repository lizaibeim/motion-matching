using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrajectoryPoint
{
   
    public Vector3 position;    //world position
    public Quaternion rotation; //world rotation
    public Vector3 direction;   //world direction
    
    ///following attributes used in trajectory comparison
    public Vector3 velocity;    //local velocity relative to the coordinates of first trajectory point
    public Vector3 localPosition;   //the local positon relative to the first trajectory point
    public double orientation; //the direction offset

}