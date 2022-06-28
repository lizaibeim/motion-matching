[System.Serializable]
public class Trajectory
{
    //store the following 1s' trajectory
    public TrajectoryPoint[] points;

    public Trajectory(int i)
    {
        points = new TrajectoryPoint[i];
    }
    
}