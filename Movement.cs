using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [HideInInspector] public Status status;
    [HideInInspector] public Rigidbody rb;
    public bool isMoving = false;



    [HeaderAttribute("Movement attributes")]
    public bool forwardOnly = true;
    public float walkSpeed = 3;
    public float runSpeed = 8;
    [HideInInspector] public bool run;

    public float currentVel;
    [HideInInspector] public float actualVelocity;
    public float smoothAcceleration = 0.5f;
    public float smoothDeacceleration = 0.5f;
    public float walkThreshold;

    [HeaderAttribute("Rotation attributes")]
    public bool smoothRotation = true;
    public float rotationDamp = 8;
    public float sharpRotationDamp = 16;
    public float deltaAngle;

    public delegate void MovementEvent();
    public event MovementEvent LandEvent;
    [HideInInspector] public float zeroFloat;
    [HideInInspector] public Vector3 direction;

    public CharacterSFX sfx;
    public Collider hurtbox;
    


    // Start is called before the first frame update
    void Start()
    {
        sfx = GetComponentInChildren<CharacterSFX>();
        rb = GetComponent<Rigidbody>();
        status = GetComponent<Status>();
        //hurtbox = GetComponent<Collider>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

    }

    private void Update()
    {
        if (status.isDead)
        {
            rb.drag = 1;
            return;
        }

        MovementProperties();

    }

    private void FixedUpdate()
    {
        if (status.isDead)
        {
            rb.velocity = Vector3.zero;
            direction = Vector3.zero;
            hurtbox.gameObject.SetActive(false);
            return;
        }
        //if (GameManager.isPaused) return;
        if (status.currentState == Status.State.Neutral)
        {

             Rotation();
            PlayerMovement();
        }

        if (direction != Vector3.zero)
        {
            isMoving = true;
        }
        else { isMoving = false; }
    }

    public virtual void Rotation()
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
                deltaAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
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
    public virtual void MovementProperties()
    {
        if (!isMoving)
        {
            currentVel = 0;
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
            actualVelocity = Mathf.SmoothDamp(actualVelocity, currentVel, ref zeroFloat, smoothAcceleration);
        }
    }


    public void PlayerMovement()
    {
        if (forwardOnly)
            rb.velocity = new Vector3(transform.forward.x * actualVelocity, rb.velocity.y, transform.forward.z * actualVelocity);
        else
            rb.velocity = new Vector3(direction.x * actualVelocity, rb.velocity.y, direction.z * actualVelocity);

    }

}
