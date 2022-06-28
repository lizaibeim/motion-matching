using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

///<summary> record the animation clips in certain framerate </summary>
public class BakeAnimationClips: MonoBehaviour
{   
    [SerializeField]
    AnimationClip[] clips;
    [SerializeField]
    public GameObject avatar;
    [SerializeField]
    public Animator animator;
    [SerializeField]
    public MotionMatcherSettings motionMatcherSettings;
    [SerializeField]
    public int fps;

    private MotionLibrary motionLibrary;
    private string[] captureJointList;
    private float[] bakedTrajectoryTimeList;
    public static int length = 0;
    private static string rootJoint = "Root";

    Dictionary<string, int> jointsMap = new Dictionary<string, int>();
    Transform[] joints = null;

    float timer = 0.0f;
    float time = 0.0f;
    int  frameIndex = 0;
    private int playIndex = 0;
    private bool isTrajectoryDone = false;


    void Awake()
    {
        clips = animator.runtimeAnimatorController.animationClips;
        InitSettings();
        InitAnimationJoints(avatar);
        motionLibrary = ScriptableObject.CreateInstance<MotionLibrary>();
        motionLibrary.bakedAnimationClips = new BakedAnimationClip[clips.Length];
    }

    // Start is called before the first frame update
    void Start()
    {
        fps = motionMatcherSettings.fps;
        //animator.Play("WalkToRun", 0, 0);
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        //frameIndex++;
        time += Time.deltaTime;
        int flag = Record();
        
        if ( flag == 1)
        {
            Debug.Log("All animation clips baked !");
            AssetDatabase.CreateAsset(motionLibrary, "Assets/Resources/avatar_motionLibrary.asset");
            string path = Application.persistentDataPath + "/motionLibrary" + ".json";
            File.WriteAllText(path, JsonUtility.ToJson(motionLibrary, true));
            Debug.Log("path : " + path);

        }
        

    }

    int Record()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        int totalFrames = Mathf.CeilToInt(clipInfo[0].clip.length * fps);

        
        //Debug.Log("info normalized time :" + info.normalizedTime);

        if (info.normalizedTime == 0.0f) //new clip play now, setting
        {
            isTrajectoryDone = false;
            motionLibrary.bakedAnimationClips[playIndex] = new BakedAnimationClip();
            BakedAnimationClip bakedClip = motionLibrary.bakedAnimationClips[playIndex];
            //Debug.Log("Total frames: " + totalFrames);
            bakedClip.clipName = clipInfo[0].clip.name;
            bakedClip.bakedClipPoses = new Pose[totalFrames];

            for ( int frame = 0; frame < totalFrames; frame++)
            {
                bakedClip.bakedClipPoses[frame] = new Pose();
                bakedClip.bakedClipPoses[frame].joints = new Joint[jointsMap.Count];
                bakedClip.bakedClipPoses[frame].trajectory = new TrajectoryPoint[bakedTrajectoryTimeList.Length];
                for ( int jointIndex = 0; jointIndex < jointsMap.Count; jointIndex++)
                {
                    bakedClip.bakedClipPoses[frame].joints[jointIndex] = new Joint();
                }
                for ( int trajectoryPointIndex = 0; trajectoryPointIndex < bakedTrajectoryTimeList.Length; trajectoryPointIndex++)
                {
                    bakedClip.bakedClipPoses[frame].trajectory[trajectoryPointIndex] = new TrajectoryPoint();
                }
            }
        }

        if (info.normalizedTime <= 1.0f && playIndex < clips.Length && frameIndex < totalFrames)
        {   
            BakedAnimationClip bakedClip = motionLibrary.bakedAnimationClips[playIndex];

            //record data
            timer = info.normalizedTime * clips[playIndex].length;
            //Debug.Log("FrameIndex " + frameIndex);
            Pose pose = bakedClip.bakedClipPoses[frameIndex];
            Pose prePose = frameIndex > 0 ? bakedClip.bakedClipPoses[frameIndex - 1] : null;
            BakePose(pose, prePose);
        }


        if (info.normalizedTime >= 1.0f && playIndex < clips.Length && !isTrajectoryDone)
        {  
             //Bake trajectory for this clip, and then reset all
            BakedAnimationClip bakedClip = motionLibrary.bakedAnimationClips[playIndex];
           
            for (int frame = 0; frame < totalFrames; frame++)
            {
                BakeTrajectory(bakedClip, frame, totalFrames);
            }
            
            //reset
            isTrajectoryDone = true;
            avatar.transform.position = Vector3.zero;
            avatar.transform.rotation= Quaternion.identity;
            frameIndex = 0;

            //play the next animation clip
            playIndex++;
            if (playIndex < clips.Length)
            {
                animator.Play(clips[playIndex].name, 0);
                animator.speed = 0f;
                return 0; //flag as bake next clip
            }

            if (playIndex == clips.Length)
            {
                return 1; //flag as complete all clips' bake
            }

            

        }

        animator.speed = 1.0f;
        float deltaTimeStamp = 1 / (float)fps;
        animator.Update(deltaTimeStamp);
        animator.speed = 0f;
        //Debug.Log("Frame: " + frameIndex + " Time : " + time + " clip frame: " + clipInfo[0].clip.length * info.normalizedTime * clipInfo[0].clip.frameRate);
        frameIndex++;

        if (playIndex < clips.Length)
        {
            return -1; //flag as bake next frame of the same clip
        }

        return -2; //flag as stop baking

    }

    private void BakePose(Pose pose, Pose prePose)
    {
        float deltaTimeStamp = 1 / (float)fps;
        int index = 0;

        // root transform
        TrajectoryPoint trajectoryPoint = pose.trajectory[0];
        trajectoryPoint.position = avatar.transform.position;
        trajectoryPoint.rotation = avatar.transform.rotation;
        trajectoryPoint.direction = avatar.transform.forward;
        trajectoryPoint.velocity = Vector3.zero;

        foreach (string jointName in jointsMap.Keys)
        {
            //retrive joints' data
            int jointIndex = jointsMap[jointName];
            Transform jointObj = joints[jointIndex];
            Joint joint = pose.joints[index];
            joint.position = jointObj.position;
            joint.rotation = jointObj.rotation;
            KeyValuePair<Vector3, Quaternion> kvp = TRS_WtL(joint.position, joint.rotation, trajectoryPoint.position, trajectoryPoint.rotation);
            joint.localPosition = kvp.Key;
            joint.localRotation = kvp.Value;
            joint.velocity = Vector3.zero;
            joint.jointName = jointObj.name;
            joint.jointIndex = jointIndex;

            //calculate previous pose's joints' velocity
            if (prePose != null)
            {
                Joint prePoseJoint = prePose.joints[index];
                prePoseJoint.velocity = (joint.localPosition - prePoseJoint.localPosition) / deltaTimeStamp;
            }
            index++;
        }
    }


    private void BakeTrajectory(BakedAnimationClip bakedClip, int frame, int totalFrames)
    {
        //Debug.Log("frame " + frame + "total frames" + totalFrames);
        Pose pose = bakedClip.bakedClipPoses[frame];
        Pose prePose = frame > 0 ? bakedClip.bakedClipPoses[frame - 1] : null;
        float deltaTimeStamp = 1 /(float)fps;
        for (int i = 1; i < bakedTrajectoryTimeList.Length; i++)
        {
            //calculate trajectory points after the first point
            float predictTrajectoryTime = bakedTrajectoryTimeList[i];
            int nextFrame = frame + (int)(predictTrajectoryTime * fps);

            if (nextFrame >= totalFrames)
            {
                nextFrame = totalFrames - 1;
            }
            TrajectoryPoint trajectoryPoint = pose.trajectory[i];
            trajectoryPoint.position = bakedClip.bakedClipPoses[nextFrame].trajectory[0].position;
            trajectoryPoint.rotation = bakedClip.bakedClipPoses[nextFrame].trajectory[0].rotation;
            trajectoryPoint.direction = bakedClip.bakedClipPoses[nextFrame].trajectory[0].direction;
            trajectoryPoint.velocity = Vector3.zero;


        }

        for (int i = 0; i < bakedTrajectoryTimeList.Length; i++)
        {
            pose.trajectory[i].localPosition = TRS_WtL(pose.trajectory[i].position, pose.trajectory[0].position, pose.trajectory[0].rotation);
            if (i == 0){
                pose.trajectory[i].orientation = 0;
            }
            else
            {
                float deltaRadian = Mathf.Deg2Rad * Vector3.SignedAngle(pose.trajectory[i - 1].direction, pose.trajectory[i].direction, Vector3.up);
                pose.trajectory[i].orientation = pose.trajectory[i-1].orientation + deltaRadian;
            }
                
        }

        for (int i = 0; i < bakedTrajectoryTimeList.Length; i++)
        {
            if (i == 0)
            {
                pose.trajectory[i].velocity = Vector3.zero;
            }
            else
            {
                pose.trajectory[i].velocity = (pose.trajectory[i].localPosition - pose.trajectory[i-1].localPosition) / (bakedTrajectoryTimeList[i] - bakedTrajectoryTimeList[i-1]);
            }
        }

        //calculate pose velocity magnitude
        float trajectoryTime = bakedTrajectoryTimeList[bakedTrajectoryTimeList.Length - 1];
        TrajectoryPoint first = pose.trajectory[0];
        TrajectoryPoint last = pose.trajectory[pose.trajectory.Length - 1];
        Vector3 velocity = (last.position - first.position) / trajectoryTime;
        pose.velocity = velocity.magnitude;

        //calculate pose angular velocity magnitude(radians / seconds)
        float angularVelocity = (float)last.orientation / trajectoryTime;
        pose.angularVelocity = angularVelocity;

    }

    private void InitSettings()
    {
        if (motionMatcherSettings)
        {
            bakedTrajectoryTimeList = motionMatcherSettings.bakedTrajectoryTimeList;
            captureJointList = motionMatcherSettings.captureJointList;
        }
    }
    
    private void InitAnimationJoints(GameObject avatar)
    {
        jointsMap.Clear();
        Transform child = avatar.transform.Find(rootJoint);
        joints = child.GetComponentsInChildren<Transform>(); //an array of joint transform
        List<Transform> jointList = joints.ToList();
        for (int i = 0; i < captureJointList.Length; i++)
        {
            string jointName = captureJointList[i];
            Transform joint = avatar.transform.Find(jointName);
            int index = jointList.IndexOf(joint);
            jointsMap.Add(joint.name, index);
        }
    }

    ///<summary> convert the world space position into a specific character space position </summary>
    public Vector3 TRS_WtL(Vector3 worldPosition, Vector3 basedCoordinatePos, Quaternion basedCoordinateRot)
    {
        Matrix4x4 m = Matrix4x4.TRS(basedCoordinatePos, basedCoordinateRot, Vector3.one);
        Vector3 convertedPosition = m.inverse.MultiplyPoint(worldPosition);
        return convertedPosition;
    }
    

    ///<summary> convert the world space position into a specific character space position </summary>
    public KeyValuePair<Vector3, Quaternion> TRS_WtL(Vector3 worldPosition, Quaternion worldRotation, Vector3 basedCoordinatePos, Quaternion basedCoordinateRot)
    {
        Matrix4x4 m = Matrix4x4.TRS(basedCoordinatePos, basedCoordinateRot, Vector3.one);
        Vector3 convertedPosition = m.inverse.MultiplyPoint(worldPosition);
        Quaternion convertedRotation = Quaternion.Inverse(basedCoordinateRot) * worldRotation;

        KeyValuePair<Vector3, Quaternion> result = new KeyValuePair<Vector3, Quaternion> (convertedPosition, convertedRotation);
        return result; 
    }


}