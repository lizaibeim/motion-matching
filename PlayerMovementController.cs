using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : Movement
{
    [HeaderAttribute("MotionMatching attributes")]
    public LineRenderer lineRenderer;
    public GameObject motionMatcher;
    public MotionMatcherSettings motionMatcherSettings;
    public Vector3[] changedPos;
    public float[] offRadians;
    public Vector3[] velocities;
    public bool sharpTurn = false;
    [HideInInspector]public bool preTrajectoryOn = false;
    [HideInInspector]public Queue<Vector3> previousTrajectoryPointsPos;
    [HideInInspector]public Queue<Quaternion> previousTrajectoryPointsRot;
    [HideInInspector]public Queue<Vector3> previousTrajectoryPointsDir;
    MotionMatcher runtimeMotionMatcher;
    Vector3[] desiredDir;
    Vector3[] desiredPos;
    Quaternion[] changedRot;
    float previousVelocity = 0;
    bool isTurn;
    bool isStop;
    private int framesAccumulated = 0;
    private float[] predictFutureTrajectoryTimeList;
    private float[] bakedTrajectoryTimeList;


    void Awake()
    {
        predictFutureTrajectoryTimeList = motionMatcherSettings.predictFutureTrajectoryTimeList;
        bakedTrajectoryTimeList = motionMatcherSettings.bakedTrajectoryTimeList;
        desiredPos = new Vector3[bakedTrajectoryTimeList.Length];
        desiredDir = new Vector3[bakedTrajectoryTimeList.Length];
        changedPos = new Vector3[bakedTrajectoryTimeList.Length];
        changedRot = new Quaternion[bakedTrajectoryTimeList.Length];
        offRadians = new float[bakedTrajectoryTimeList.Length];
        velocities = new Vector3[bakedTrajectoryTimeList.Length];
       
        previousTrajectoryPointsPos = new Queue<Vector3>();
        previousTrajectoryPointsRot = new Queue<Quaternion>();
        previousTrajectoryPointsDir = new Queue<Vector3>();

        for (int i = 0 ; i <= 50; i++)
        {
            previousTrajectoryPointsPos.Enqueue(transform.position);
            previousTrajectoryPointsRot.Enqueue(transform.rotation);
            previousTrajectoryPointsDir.Enqueue(transform.forward);
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        sfx = GetComponentInChildren<CharacterSFX>();
        rb = GetComponent<Rigidbody>();
        status = GetComponent<Status>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        lineRenderer.positionCount = bakedTrajectoryTimeList.Length;
        runtimeMotionMatcher = motionMatcher.GetComponent(typeof(MotionMatcher)) as MotionMatcher;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.startWidth = 0.15f;
        isStop = true;
        isTurn = false;

    }

    private void Update()
    {
        if (status.isDead)
        {
            rb.drag = 1;
            return;
        }

        this.MovementProperties();

    }

    private void FixedUpdate()
    {
        
        PreviousTrajectory();
        if (!isStop)
        {
            framesAccumulated ++;
        }
        else
        {
            framesAccumulated = 0;
        }


        if (status.isDead)
        {
            return;
        }

        if (direction != Vector3.zero)
        {
            isMoving = true;
        }
        else { isMoving = false; }

        float tempVelocity = actualVelocity > walkSpeed ? runSpeed : actualVelocity;
        Vector3 rb_velocity = forwardOnly ? new Vector3(transform.forward.x * tempVelocity, rb.velocity.y, transform.forward.z * tempVelocity) : new Vector3(direction.x * tempVelocity, rb.velocity.y, direction.z * tempVelocity);
        deltaAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

        sharpTurn = false;
        preTrajectoryOn = (framesAccumulated > 50) ? true : false;
        Trajectory(rb_velocity);
        
        if (status.currentState == Status.State.Neutral)
        {
            runtimeMotionMatcher.animator.speed = (actualVelocity > walkSpeed) ? (actualVelocity / runSpeed * 0.9f) : 1f;
            float speed;

            if (deltaAngle > 100 || deltaAngle < -100)
            {
                sharpTurn = true;
                Trajectory(rb_velocity);
                speed = runtimeMotionMatcher.animateDuration * sharpRotationDamp;
            }                                                 
            else if ((deltaAngle > 0.2  && deltaAngle <=100) || (deltaAngle >= -100 && deltaAngle <= -0.2))
            {
                speed = runtimeMotionMatcher.animateDuration * rotationDamp;
            }
            else
            {
                speed = 1;
            }

            if (Mathf.Abs(deltaAngle) < 0.2f) {
                preTrajectoryOn = false;
                Trajectory(rb_velocity);
            }
     
            //turning with prority 1
            if (previousVelocity <= walkSpeed && actualVelocity <= walkSpeed && isMoving)
            {
                isStop = false;
                runtimeMotionMatcher.PreStartMotionMatcher(1, speed);
            }
            
            //uniform motion
            else if (previousVelocity == actualVelocity && actualVelocity >= walkSpeed && isMoving)
            {
                isStop = false;
                runtimeMotionMatcher.PreStartMotionMatcher(3, speed);
            }

            //acceleration
            else if (previousVelocity > actualVelocity && isMoving)
            {
                isStop = false;
                runtimeMotionMatcher.PreStartMotionMatcher(3, speed);
            }

            //deacceleration
            else if (previousVelocity < actualVelocity && isMoving)
            {
                isStop = false;
                runtimeMotionMatcher.PreStartMotionMatcher(3, speed);
            }

            //stop
            else if (!isMoving)
            {
                isStop = true;
                preTrajectoryOn = false;
                Trajectory(new Vector3(0, 0, 0));
                //runtimeMotionMatcher.finalDebug = true;
                runtimeMotionMatcher.PreStartMotionMatcher(4, speed);
            }
            
            Rotation();    
            PlayerMovement();

        }
         
    }

    void Trajectory(Vector3 rb_velocity)
    {
        for (int i = 0; i < bakedTrajectoryTimeList.Length; i++)
        {    
                if (i == 0)
                {  
                    desiredPos[i] = transform.position;
                    desiredDir[i] = transform.forward;
                }                
                else
                {
                    float damp = sharpTurn ? sharpRotationDamp : rotationDamp;
                    float ratio = 1f / (bakedTrajectoryTimeList.Length - 1);
                    if (actualVelocity > previousVelocity && isMoving)
                    {
                        float r = i / 3;
                        desiredPos[i] = Vector3.Lerp(desiredPos[i - 1], desiredPos[i - 1] + RotateVectorByDegrees(rb_velocity, Mathf.Lerp(0, deltaAngle, Time.fixedDeltaTime * damp * i) ), ratio * r);
                    }
                    else if (actualVelocity < previousVelocity && isMoving)
                    {
                        float r = 1 / i;
                        desiredPos[i] = Vector3.Lerp(desiredPos[i - 1], desiredPos[i - 1] + RotateVectorByDegrees(rb_velocity, Mathf.Lerp(0, deltaAngle, Time.fixedDeltaTime * damp * i) ),  ratio * r); 
                    }
                    else if (actualVelocity == previousVelocity && actualVelocity >= walkSpeed && actualVelocity < runSpeed && isMoving)
                    {
                        float r = 1.2f;
                        desiredPos[i] = Vector3.Lerp(desiredPos[i - 1], desiredPos[i - 1] + RotateVectorByDegrees(rb_velocity, Mathf.Lerp(0, deltaAngle, Time.fixedDeltaTime * damp * i) ), ratio * r);
                    }
                    else if (actualVelocity == previousVelocity && actualVelocity >= runSpeed && isMoving)
                    {
                        float r = 1f;
                        desiredPos[i] = Vector3.Lerp(desiredPos[i - 1], desiredPos[i - 1] + RotateVectorByDegrees(rb_velocity, Mathf.Lerp(0, deltaAngle, Time.fixedDeltaTime * damp * i) ), ratio * r);
                    
                    }
                    else
                    {
                        desiredPos[i] = desiredPos[i-1];
                    }
                    desiredDir[i] = RotateVectorByDegrees(desiredDir[i - 1], Mathf.Lerp(0, deltaAngle, ratio)); 

                }
            
        }

        ConvertToCharacterSpace();
        TrajectoryPointAngleOffset();
        VelocityCalculator();

        // for (int i = 0; i < bakedTrajectoryTimeList.Length; i++)
        // {

        //     if (preTrajectoryOn)
        //     {
        //         if (i < 5)
        //         {
        //             int index = i * 5 + 25;
        //             lineRenderer.SetPosition(i, previousTrajectoryPointsPos.ToArray()[index]);
        //         }
        //         else 
        //         {
        //             lineRenderer.SetPosition(i, desiredPos[i-5]);
        //         }
        //     }
        //     else
        //     {
        //         lineRenderer.SetPosition(i, desiredPos[i]);
        //     }
        // }
       
        // if (sharpTurn)
        // {
        //     //Debug.Log(string.Format("Offset angles are: {0}, {1}... {2}", offRadians[0], offRadians[1], offRadians[10]));
        //     //Debug.Log(string.Format("Desired Position is {0} , {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}", changedPos[0], changedPos[1], changedPos[2], changedPos[3], changedPos[4], changedPos[5], changedPos[6], changedPos[7], changedPos[8], changedPos[9], changedPos[10]));
        // }

    

    }

    public override void Rotation()
    {
        if (!smoothRotation)
        {
            //HARD INSTANT ROTATION
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, new Vector3(direction.x, 0, direction.z), Vector3.up), 0);
        }
        else
        {
            Quaternion desiredRotation = Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, new Vector3(direction.x, 0, direction.z), Vector3.up), 0);

            if (direction != Vector3.zero)
            {
                //Desired rotation, updated every (fixed) frame
                if (Mathf.Abs(deltaAngle) < 90)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationDamp);
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * sharpRotationDamp);
                }

            }
        }
    }

    public override void MovementProperties()
    {
        if (!isMoving)
        {
            currentVel = 0;
            previousVelocity = actualVelocity;
            actualVelocity = Mathf.SmoothDamp(actualVelocity, currentVel, ref zeroFloat, smoothDeacceleration);
        }
        else if (isMoving)
        {
            if (direction.magnitude > walkThreshold) { run = true; }
            else { run = false; }

            if (run)
            {
                currentVel = runSpeed * status.rawStats.movementSpeedModifier;
            }
            else { currentVel = walkSpeed; }
            previousVelocity = actualVelocity;
            actualVelocity = Mathf.SmoothDamp(actualVelocity, currentVel, ref zeroFloat, smoothAcceleration);
        }
    }

    public Vector3 Magnitude2Vector3(float velocityMagnitude)
    {
        if (forwardOnly)
            return new Vector3(transform.forward.x * velocityMagnitude, rb.velocity.y, transform.forward.z * velocityMagnitude);
        else
            return new Vector3(direction.x * velocityMagnitude, rb.velocity.y, direction.z * velocityMagnitude);

    }
    
    Vector3 RotateVectorByDegrees(Vector3 startPos, float rotationDegree)
    {
        Vector3 rotatedVector = Quaternion.Euler(0, rotationDegree, 0) * startPos;
        return rotatedVector;
    }

    void VelocityCalculator()
    {   
        for (int i = 0; i < bakedTrajectoryTimeList.Length; i++)
        {
            if (i == 0)
            {
                velocities[i] = Vector3.zero;
            }
            else
            {
                velocities[i] = (changedPos[i] - changedPos[i - 1]) / (bakedTrajectoryTimeList[i] - bakedTrajectoryTimeList[i - 1]);
            }
            
        }
    }

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

    public void ConvertToCharacterSpace()
    {
        if (preTrajectoryOn)
        {
            Vector3 basedPos = previousTrajectoryPointsPos.ToArray()[0];
            Quaternion basedRot = previousTrajectoryPointsRot.ToArray()[0];

            for (int i = 0; i < bakedTrajectoryTimeList.Length; i++)
            {
                if (i < 5)
                {
                    int index = i * 5 + 25;
                    changedPos[i] = TRS_WtL(previousTrajectoryPointsPos.ToArray()[index], basedPos, basedRot);
                }
                else 
                {
                    changedPos[i] = TRS_WtL(desiredPos[i-5], basedPos, basedRot);
                }
            }
        }
        else
        {
            Vector3 basedPos = transform.position;
            Quaternion basedRot = transform.rotation;

            for (int i = 0; i < bakedTrajectoryTimeList.Length; i++)
            {
                changedPos[i] = TRS_WtL(desiredPos[i], basedPos, basedRot);
            }
        }
        
    }

    public void TrajectoryPointAngleOffset()
    {
        if (preTrajectoryOn)
        {      
            for (int i = 0; i < bakedTrajectoryTimeList.Length; i++)
            {
                if (i == 0)
                {
                    offRadians[i] = 0;
                }
                else if (i < 6)
                {
                    int index = (i - 1) * 5 + 25;
                    int index2 = i * 5 + 25;
                    float deltaRadian = Mathf.Deg2Rad * Vector3.SignedAngle(previousTrajectoryPointsDir.ToArray()[index], previousTrajectoryPointsDir.ToArray()[index2], Vector3.up);
                    offRadians[i] = offRadians[i-1] + deltaRadian;
                }
                else
                {
                    float deltaRadian = Mathf.Deg2Rad * Vector3.SignedAngle(desiredDir[i-6], desiredDir[i-5], Vector3.up);
                    offRadians[i] = offRadians[i-1] + deltaRadian;
                }
            }
        }
        else
        {
            for (int i = 0; i < bakedTrajectoryTimeList.Length; i++)
            {
                if (i == 0)
                {
                    offRadians[i] = 0;
                }
                else
                {
                    float deltaRadian = Mathf.Deg2Rad * Vector3.SignedAngle(desiredDir[i-1], desiredDir[i], Vector3.up);
                    offRadians[i] = offRadians[i-1] + deltaRadian;
                }
            }
        }

    }

    public void PreviousTrajectory()
    {
            previousTrajectoryPointsPos.Dequeue();
            previousTrajectoryPointsPos.Enqueue(transform.position);
            previousTrajectoryPointsRot.Dequeue();
            previousTrajectoryPointsRot.Enqueue(transform.rotation);
            previousTrajectoryPointsDir.Dequeue();
            previousTrajectoryPointsDir.Enqueue(transform.forward);
            
            //Debug.Log("length of previous history " + previousTrajectoryPointsPos.Count + "current pos" + previousTrajectoryPointsPos.ToArray()[49] + " rot" + previousTrajectoryPointsRot.ToArray()[49]);
    }
}