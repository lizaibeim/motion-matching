using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Double;

public class MotionMatcher : MonoBehaviour
{
    public MotionLibrary motionLibrary;
    public MotionCostFactorSettings motionCostFactorSettings;
    public MotionMatcherSettings motionMatcherSettings;
    public GameObject avatar;
    public Animator animator;
    public LineRenderer lineRenderer;
    public PlayerMovementController movement;
    public string[] bestMatch;
    public float animateDuration = 1f;
    public float crossFadeTime;
    public bool applyPCA;
    public float pcaShreshold;
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public float inPlaySpeed;
    [HideInInspector] public bool candidateDebug = false;
    [HideInInspector] public bool finalDebug = false;
    float animatePlayedTime = 0;
    float discrepTime;
    int inPlayPriority = -1;
    double[,] trajectory2DArr;
    double[,] trajectoryDimReduceArrayT;
    DenseMatrix trajectoryMatrix;
    AnimationClip currentPlayingClip;
    AnimationClip[] clips;
    DenseMatrix pcaMatrix;
    Dictionary<string, BakedAnimationClip> bakedClipsDict = new Dictionary<string, BakedAnimationClip>();
    List<BakedAnimationClip> bakedClipsList = new List<BakedAnimationClip>();
    List<double[]> trajectoryArrList = new List<double[]>(); //store trajectory in double, used for PCA
    List<Trajectory> trajectoryList = new List<Trajectory>();
    List<string[]> trajectoryMapList = new List<string[]>();
    AttackScript attackScript;
    Dodge dodgeScript;
    private float fps;
    


    void Start()
    {
        attackScript = GetComponent<AttackScript>();
        attackScript.startupEvent += StartAttacking;
        attackScript.recoveryEvent += StopAttacking;

        lineRenderer.positionCount = motionLibrary.bakedAnimationClips[0].bakedClipPoses[0].trajectory.Length;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.15f;
        clips = animator.runtimeAnimatorController.animationClips;
        fps = motionMatcherSettings.fps;


        for (int i = 0; i < motionLibrary.bakedAnimationClips.Length; i++)
        {
            bakedClipsDict.Add(motionLibrary.bakedAnimationClips[i].clipName, motionLibrary.bakedAnimationClips[i]);
        }

        foreach (KeyValuePair<string, BakedAnimationClip> kvp in bakedClipsDict)
        {
            BakedAnimationClip bakedClip = kvp.Value;
            Pose[] bakedClipPoses = bakedClip.bakedClipPoses;
            int frame = 0;
            int totalFrame = (int)(bakedClipPoses.Length - (fps * 2.2f)); //simply trimming
            foreach (var pose in bakedClipPoses)
            {
                if (frame >= totalFrame)
                {
                    break;
                }

                trajectoryList.Add(pose.GroupTrajectory());
                string[] map = { bakedClip.clipName, frame.ToString() };
                trajectoryMapList.Add(map);

                List<double> doubleLst = new List<double>();
                for (int i = 0; i < pose.trajectory.Length; i++)
                {
                    doubleLst.Add(pose.trajectory[i].localPosition.x);
                    doubleLst.Add(pose.trajectory[i].localPosition.y);
                    doubleLst.Add(pose.trajectory[i].localPosition.z);
                    doubleLst.Add(pose.trajectory[i].velocity.x);
                    doubleLst.Add(pose.trajectory[i].velocity.y);
                    doubleLst.Add(pose.trajectory[i].velocity.z);
                    doubleLst.Add(pose.trajectory[i].orientation);
                }
                trajectoryArrList.Add(doubleLst.ToArray());

                frame++;

            }
        }


        if (applyPCA)
        {
            //construct a 2D array from motion library, first is the trajectory index, second is the feature vectors
            trajectory2DArr = new double[trajectoryMapList.Count, motionLibrary.bakedAnimationClips[0].bakedClipPoses[0].trajectory.Length * 7];
            for (int i = 0; i < trajectoryMapList.Count; i++)
            {
                for (int j = 0; j < motionLibrary.bakedAnimationClips[0].bakedClipPoses[0].trajectory.Length * 7; j++)
                {
                    trajectory2DArr[i, j] = trajectoryArrList[i][j];
                }
            }

            trajectoryMatrix = DenseMatrix.OfArray(trajectory2DArr);
            double[,] trajectoryDimReduceArray = PCA(pcaShreshold, trajectoryMatrix);
            trajectoryDimReduceArrayT = Transpose(trajectoryDimReduceArray);
            Debug.Log(string.Format("Trajectory dimension reduce array trajectory count {0} and feature vectors count {1}", trajectoryDimReduceArray.GetLength(0), trajectoryDimReduceArray.GetLength(1)));
            Debug.Log(string.Format("Trajectory dimension reduce transposed array trajectory count {0} and feature vectors {1}", trajectoryDimReduceArrayT.GetLength(0), trajectoryDimReduceArrayT.GetLength(1)));

        }
    }

    void StartAttacking()
    {
        isAttacking = true;
    }

    void StopAttacking()
    {
        isAttacking = false;
    }

    void Update()
    {

        // TrajectoryPoint[] result = bakedClipsDict[bestMatch[0]].bakedClipPoses[int.Parse(bestMatch[1])].trajectory;
        // if (movement.preTrajectoryOn)
        // {
        //    for (int i = 0; i < result.Length; i++)
        //    {
        //        Vector3 point = TRS_WtL(result[i].position, result[5].position, result[5].rotation);
        //        lineRenderer.SetPosition(i, new Vector3(point.x, 0, point.z));
        //    }
        // }
        // else
        // {
        //    for (int i = 0; i < result.Length; i++)
        //    {
        //        lineRenderer.SetPosition(i, result[i].localPosition);
        //    }
        // }


    }

    public void RegularStartMotionMatcher()
    {
        PreStartMotionMatcher(0, 1);
    }

    public void PreStartMotionMatcher(int priority, float speed)
    {
        if (priority > inPlayPriority)
        {
            StartMotionMatcher(priority, speed);
        }
        else if (priority <= inPlayPriority && ((Time.time - animatePlayedTime) * inPlaySpeed) >= animateDuration)
        {
             StartMotionMatcher(priority, speed);
        }
    }

    public void StartMotionMatcher(int priority, float speed)
    {

        Vector3[] positions = movement.changedPos;
        Vector3[] velocities = movement.velocities;
        float[] orientations = movement.offRadians;

        Trajectory goal = new Trajectory(positions.Length);
        for (int i = 0; i < goal.points.Length; i++)
        {
            goal.points[i] = new TrajectoryPoint();
            goal.points[i].localPosition = positions[i];
            goal.points[i].velocity = velocities[i];
            goal.points[i].orientation = (double)orientations[i];
        }


        AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);

        if (clipInfos.Length != 0)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float ratio = stateInfo.normalizedTime % 1;
            currentPlayingClip = clipInfos[0].clip;

            if (bakedClipsDict.ContainsKey(currentPlayingClip.name))
            {
                bestMatch = CostCompute(goal, currentPlayingClip.name, ratio * currentPlayingClip.length);
                float currentTime = stateInfo.normalizedTime * clipInfos[0].clip.length;
                float matchTime = movement.preTrajectoryOn ? (float.Parse(bestMatch[1]) / fps + 1.1f) : (float.Parse(bestMatch[1]) / fps);

                //Debug.Log("Best match animation " + bestMatch[0] + "best match frame " + bestMatch[1] + " best match time" + matchTime);
                discrepTime = Cycle.GetDiscrepTime(bestMatch[0]);
                if (string.Equals(currentPlayingClip.name, bestMatch[0]) && string.Equals(bestMatch[0], "Idle"))
                {
                    discrepTime = 3f;
                }

                
                if (!string.Equals(currentPlayingClip.name, bestMatch[0]) || (string.Equals(currentPlayingClip.name, bestMatch[0]) && currentTime - matchTime > discrepTime) || ratio > 0.9f)
                {
                    if (string.Equals(bestMatch[0], "Idle") || priority == 4)
                    {
                        inPlayPriority = 0;
                    }
                    else if (priority == 3)
                    {
                        inPlayPriority = 2;
                    } 
                    else
                    {
                        inPlayPriority = priority;
                    }

                    inPlaySpeed = speed;
                    animator.speed *= Cycle.GetSpeed(bestMatch[0]);
                    if (currentPlayingClip.name.Equals("Idle"))
                    {
                        animator.PlayInFixedTime(bestMatch[0], 0, matchTime);
                    }
                    else
                    {
                        animator.CrossFadeInFixedTime(bestMatch[0], crossFadeTime, 0, matchTime);
                    }
                    animatePlayedTime = Time.time;
                }
            }
        }
    }


    ///<summary> search and match next best animation clip</summary>
    string[] CostCompute(Trajectory goal, string currentAnimName, float currentAnimTime)
    {
        //find out the current pose inforamtion, use the interpolate value of the nereast two pose
        Pose lerpedPose = EvaluateLerpedPose(currentAnimName, currentAnimTime);
        Pose bestPose = lerpedPose;
        double bestCost = double.MaxValue;
        int bestIndex = -1;
        Dictionary<int, double> costDict1 = new Dictionary<int, double>();
        Dictionary<int, double> costDict2 = new Dictionary<int, double>();

        if (!applyPCA)
        {
            //search the most matching frame among the motion library
            for (int i = 0; i < trajectoryList.Count; i++)
            {
                double trjCost = CandidateTrajectoryCost(goal, trajectoryList[i]);
                costDict1.Add(i, trjCost);
            }
        }
        else
        {
            double[,] goalArr = new double[1, goal.points.Length * 7];
            List<double> dL =  new List<double>();

            for (int x = 0; x < goal.points.Length; x++)
            {
                dL.Add(goal.points[x].localPosition.x);
                dL.Add(goal.points[x].localPosition.y);                    
                dL.Add(goal.points[x].localPosition.z);
                dL.Add(goal.points[x].velocity.x);
                dL.Add(goal.points[x].velocity.y);
                dL.Add(goal.points[x].velocity.z);
                dL.Add(goal.points[x].orientation);
            }

            double[] dLArr = dL.ToArray();

            for (int y = 0; y < goal.points.Length * 7; y++)
                goalArr[0, y] = dLArr[y];                

            DenseMatrix goalMatrix = DenseMatrix.OfArray(goalArr);
            double[,] goalReducedArr = PCA(pcaMatrix, goalMatrix);
            double[] goalTrj = GetRow(goalReducedArr, 0);

            for (int i = 0; i < trajectoryDimReduceArrayT.GetLength(0); i++)
            {
                double[] candidateTrajectory = GetRow(trajectoryDimReduceArrayT, i);
                double trjCost = ComputeDimensionReduceTrajectoryCost(goalTrj, candidateTrajectory);
                costDict1.Add(i, trjCost);
            }
        }

        if (candidateDebug)
        {
            candidateDebug = false;
            foreach (var cost in costDict1.OrderBy(i => i.Value))
            {
                Debug.Log(string.Format("Candidate process: clips {0} at frame {1} has cost {2}", trajectoryMapList[cost.Key][0], trajectoryMapList[cost.Key][1], cost.Value));
            }
        }
        
        //search among the candidates
        int count = 0;
        int flag = applyPCA ? 500 : 20;
        
        foreach (var cost in costDict1.OrderBy(i => i.Value))
        {
            BakedAnimationClip candidateClip = bakedClipsDict[trajectoryMapList[cost.Key][0]];
            int poseIndex = Int16.Parse(trajectoryMapList[cost.Key][1]);

            // 这里的cnadiate pose应该要考虑是否应用了历史轨迹，如果应用了，则相应的poseIndex得改变
            double poseCost = ComputePoseCost(lerpedPose, candidateClip.bakedClipPoses[poseIndex]);
            double trajectoryCost;

            if (!applyPCA)
                trajectoryCost = ComputeTrajectoryCost(goal, candidateClip.bakedClipPoses[poseIndex].GroupTrajectory());
            else
                trajectoryCost = CandidateTrajectoryCost(goal, candidateClip.bakedClipPoses[poseIndex].GroupTrajectory());


            if (poseCost + trajectoryCost < bestCost)
            {
                bestCost = poseCost + trajectoryCost;
                bestPose = candidateClip.bakedClipPoses[poseIndex];
                bestIndex = cost.Key;
            }

            costDict2.Add(cost.Key, poseCost + trajectoryCost);

            if (count == flag) 
            {
                break;
            }

            count++;
        }

        if (finalDebug)
        {
            finalDebug = false;
            foreach (var cost in costDict2.OrderBy(i => i.Value))
            {
                Debug.Log(string.Format("Final process: clips {0} at time {1} has cost {2}", trajectoryMapList[cost.Key][0], trajectoryMapList[cost.Key][1], cost.Value));
            }
            Debug.Log("Best cost" + bestCost + " best pose " + trajectoryMapList[bestIndex][0] + " at frame " + trajectoryMapList[bestIndex][1]);
        }
        
        string[] bestMatch = new string[] { trajectoryMapList[bestIndex][0], trajectoryMapList[bestIndex][1] };
        return bestMatch;
    }

    ///<summary> find out the pose information </summary>
    Pose EvaluateLerpedPose(string currentAnimName, double currentAnimTime) //actual time
    {
        string name = currentAnimName;
        int poseIndex = Mathf.FloorToInt((float)(currentAnimTime * fps));
        BakedAnimationClip clip = bakedClipsDict[name];
        return clip.bakedClipPoses[poseIndex];
    }

    ///<summary> An PCA to reduce the data dimensions</summary>
    double[,] PCA(double threshold, DenseMatrix matrix)
    {
        var svd = matrix.Svd(true);
        double[,] singular = svd.W.ToArray();

        string path = Application.persistentDataPath + "/" + "singularValue.txt";
        Write(Transpose(singular), path);

        int rank = svd.Rank;
        double[] singularArr = new double[rank];
        double sum = 0;

        for (int i = 0; i < rank; i++)
        {
            singularArr[i] = singular[i, i];
            sum += singularArr[i];
        }

        double variance_ratio = 0;
        int k = 0;

        while (variance_ratio < threshold)
        {
            variance_ratio += singularArr[k] / sum;
            k++;
        }

        //Debug.Log("k is " + k + "sum is " + sum + "variance ratio is " + variance_ratio);
        pcaMatrix = (DenseMatrix)svd.VT.Transpose().SubMatrix(0, k, 0, svd.VT.Transpose().RowCount);
        //Debug.Log("pcaMatrix is:" + pcaMatrix);
        var dimensionReducedMatrix = pcaMatrix.Multiply(matrix.Transpose());
        //Debug.Log("result matrix is: " + dimensionReducedMatrix);
        double[,] dimensionReducedArr = dimensionReducedMatrix.ToArray();
        string path2 = Application.persistentDataPath + "/" + "DimensionReduceTrajectory.txt";
        //Debug.Log("dimensionReducedArr row" + dimensionReducedArr.GetLength(0) + "dimensionReducedArr col" + dimensionReducedArr.GetLength(1));
        Write(Transpose(dimensionReducedArr), path2);
        return dimensionReducedArr;
    }

    double[,] PCA(DenseMatrix pcaMatrix, DenseMatrix goalMatrix)
    {
        var dimensionReducedMatrix = pcaMatrix.Multiply(goalMatrix.Transpose());
        double[,] dimensionReducedArr = dimensionReducedMatrix.ToArray();
        return dimensionReducedArr;
    }

    public double[] GetRow(double[,] matrix, int rowNumber)
    {
        return Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[rowNumber, x]).ToArray();
    }

    double[,] Transpose(double[,] arr)
    {
        int row = arr.GetLength(1);
        int col = arr.GetLength(0);
        double[,] tp = new double[row, col];

        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < col; c++)
            {
                tp[r, c] = arr[c, r];
            }
        }

        return tp;
    }

    void Write(double[,] arr, string path)
    {
        int row = arr.GetLength(0);
        int col = arr.GetLength(1);
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < col; c++)
            {
                //add space for each dimension
                sw.Write(arr[r, c] + " ");
            }
            sw.WriteLine();
        }
        //flush buffer
        sw.Flush();
        //close buffer
        sw.Close();
        fs.Close();
    }

    void Write(string[][] arr, string path)
    {
        int row = arr.Length;
        int col = arr[0].Length;
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < col; c++)
            {
                sw.Write(arr[r][c] + " ");
            }
            sw.WriteLine();
        }

        sw.Flush();
        sw.Close();
        fs.Close();
    }

    ///<summary> calculate the Euclidean distance of two array, which is the trajectory reduced by PCA </summary>
    double ComputeDimensionReduceTrajectoryCost(double[] goal, double[] candidate)
    {
        int l = goal.Length;
        double sum = 0;

        for (int i = 0; i < l; i++)
        {
            double diff = goal[i] - candidate[i];
            sum += Math.Pow(diff, 2);

        }

        double cost = Math.Sqrt(sum);
        return cost;
    }

    ///<summary> detailed trajectory cost comparison when do the precomputation </summary>
    double CandidateTrajectoryCost(Trajectory goal, Trajectory candidate)
    {
        double trajectoryCost = 0f;

        for (int i = 0; i < goal.points.Length; i++)
        {
            trajectoryCost = trajectoryCost + Vector3.Distance(goal.points[i].localPosition, candidate.points[i].localPosition) * motionCostFactorSettings.candidateTrajectoryPosFactor + Vector3.Distance(goal.points[i].velocity, candidate.points[i].velocity) * motionCostFactorSettings.candidateTrajectoryVelFactor + Mathf.Abs((float)goal.points[i].orientation - (float)candidate.points[i].orientation) * motionCostFactorSettings.candidateTrajectoryDirFactor;
        }

        return trajectoryCost;
    }

    ///<summary> calculate the trajectory cost in the second search among the specific candidates </summary>
    double ComputeTrajectoryCost(Trajectory goal, Trajectory candidate)
    {
        int length = goal.points.Length;
        double trajectoryPosCost = Vector3.SqrMagnitude(goal.points[length - 1].localPosition - candidate.points[length - 1].localPosition) * motionCostFactorSettings.finalTrajectoryPosFactor;
        double trajectoryDirCost = Math.Abs(goal.points[length - 1].orientation - candidate.points[length - 1].orientation) * motionCostFactorSettings.finalTrajectoryDirFactor;
        double trajectoryCost = trajectoryPosCost + trajectoryDirCost;
        return trajectoryCost;
    }

    ///<summary> calculate the Euclidean distance of two pose with weight factor </summary>
    double ComputePoseCost(Pose lerpedPose, Pose candidate)
    {
        float PoseCost = 0;
        float BonePosCost = 0f;
        float BoneRotCost = 0f;
        float BoneVelCost = 0f;
        float RootMotionVelCost = 0f;
        float RootMotionAngularVelCost = 0f;

        for (int i = 0; i < candidate.joints.Length; i++)
        {
            float bonePosCost = Vector3.SqrMagnitude(candidate.joints[i].localPosition - lerpedPose.joints[i].localPosition);
            Quaternion boneRotDiff = Quaternion.Inverse(candidate.joints[i].localRotation) * lerpedPose.joints[i].localRotation;
            float boneRotCost = Mathf.Abs(boneRotDiff.x) + Mathf.Abs(boneRotDiff.y) + Mathf.Abs(boneRotDiff.z) + (1 - Mathf.Abs(boneRotDiff.w));
            float boneVelCost = Vector3.SqrMagnitude(candidate.joints[i].velocity - lerpedPose.joints[i].velocity);
            BonePosCost += bonePosCost * motionCostFactorSettings.bonePosFactor;
            BoneRotCost += boneRotCost * motionCostFactorSettings.boneRotFactor;
            BoneVelCost += boneVelCost * motionCostFactorSettings.boneVelFactor;
        }

        RootMotionVelCost = Mathf.Abs(candidate.velocity - lerpedPose.velocity) * motionCostFactorSettings.rootMotionVelFactor;
        RootMotionAngularVelCost = Mathf.Abs(candidate.angularVelocity - lerpedPose.angularVelocity) * motionCostFactorSettings.rootMotionAngularVelFactor;
        PoseCost = BonePosCost + BoneRotCost + BoneVelCost + RootMotionAngularVelCost + RootMotionVelCost;
        return PoseCost;
    }

    public Vector3 TRS_WtL(Vector3 worldPosition, Vector3 basedCoordinatePos, Quaternion basedCoordinateRot)
    {
        Matrix4x4 m = Matrix4x4.TRS(basedCoordinatePos, basedCoordinateRot, Vector3.one);
        Vector3 convertedPosition = m.inverse.MultiplyPoint(worldPosition);
        return convertedPosition;
    }

}