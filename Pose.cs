using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Pose
{
    public float velocity;  //root motion velocity
    public float angularVelocity;
    public Joint[] joints;
    public TrajectoryPoint[] trajectory;

    public void Reset()
    {
        velocity = 0;
        joints = null;
        trajectory = null;
    }

    public Trajectory GroupTrajectory()
    {
        Trajectory group = new Trajectory(trajectory.Length);

        for (int i = 0; i < trajectory.Length; i++)
        {
            group.points[i] = trajectory[i];
        }

        return group;
    }

}