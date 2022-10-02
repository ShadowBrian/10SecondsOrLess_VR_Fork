using System;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    //Player View
    [SerializeField] Transform playerCamera;
    [SerializeField] float playerCameraYOffset = 0.72f;
    [SerializeField] float xMouseSensitivity = 30.0f;
    [SerializeField] float yMouseSensitivity = 30.0f;

    //Frame occuring factors
    [SerializeField] float gravity = 20.0f;
    [SerializeField] float friction = 6;

    //Movement
    [SerializeField] public float moveSpeed = 40;
    [SerializeField] float runAcceleration = 14;
    [SerializeField] float runDeacceleration = 10;
    [SerializeField] float airAcceleration = 2;
    [SerializeField] float airDeacceleration = 2;
    [SerializeField] float airControl = 0.3f;
    [SerializeField] float sideStrafeAcceleration = 50;
    [SerializeField] float sideStrafeSpeed = 1;
    [SerializeField] float jumpSpeed = 8;

    public CharacterController controller;

    //Camera Rotationals
    private float rotX = 0;
    private float rotY = 0;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 moveDirectionNorm = Vector3.zero;
    public Vector3 playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0;

    public bool grounded = false;

    private bool wishJump = false;

    float playerFriction;

    float camRotZ;

    AudioSource audioSource;

    float fallingTime;
    float lastVerticalVelocity;
    float lastJumpTime; 
    bool usedDoubleJump = false; 

    class Cmd
    {
        public float forwardmove;
        public float rightmove;
        public float upmove;
    }
     
    Cmd cmd;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //playerCamera = Camera.main.transform;
        Vector3 newPos = playerCamera.position;
        newPos.y = this.transform.position.y + playerCameraYOffset;
        playerCamera.position = newPos;

        cmd = new Cmd();
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (lastVerticalVelocity > 0 && playerVelocity.y < 0)
            fallingTime = Time.time; 

        lastVerticalVelocity = playerVelocity.y;

         if (!controller.isGrounded) 
        {
            AirMove();
            grounded = false;
        }
        else if (controller.isGrounded)
        {
            GroundMove();
            grounded = true;
        }

        controller.Move(playerVelocity * Time.deltaTime);

        Vector3 newPos = Vector3.zero;
        newPos.y = playerCameraYOffset;
        playerCamera.localPosition = newPos;

        Quaternion camRot = playerCamera.localRotation;

        if (cmd.rightmove != 0)
        {
            camRotZ += cmd.rightmove / 10 * -1;
            camRotZ = Mathf.Clamp(camRotZ, -0.7f, 0.7f);
            camRot.z += camRotZ * Mathf.Deg2Rad;
        }
        else
        {
            camRotZ = 0;
            if (camRot.z * Mathf.Rad2Deg != 0)
            {
                camRot.z = Mathf.Deg2Rad * Mathf.Lerp(camRot.z * Mathf.Rad2Deg, 0, 0.001f);
            }
        }

        //Camera rotation
        rotX -= GameManager.Instance.controlsManager.lookInput.y * xMouseSensitivity * 0.02f;
        rotY += GameManager.Instance.controlsManager.lookInput.x * yMouseSensitivity * 0.02f;

        // Clamp the X rotation
        if (rotX < -90)
            rotX = -90;
        else if (rotX > 90)
            rotX = 90;

        this.transform.rotation = Quaternion.Euler(0, rotY, 0); // Rotates the collider
        playerCamera.localRotation = Quaternion.Euler(rotX, 0, 0); // Rotates the camera 

        /* Calculate top velocity */
        Vector3 udp = playerVelocity;
        udp.y = 0.0f;
        if (udp.magnitude > playerTopVelocity)
            playerTopVelocity = udp.magnitude;
    }

    //Checks if player is moving into an object. 
    public bool CollisionCheck(Vector3 x, Vector3 y)
    {
        Vector3 raycastPos = transform.position;
        raycastPos.y -= 1;
        Debug.DrawRay(raycastPos, x + y, Color.red);

        Ray ray = new Ray(raycastPos, x + y);
        RaycastHit hit;
        float shotDistance = 2f;
        if (Physics.Raycast(ray, out hit, shotDistance) && hit.transform.gameObject.layer != 10)
        {
            shotDistance = hit.distance;
            Rigidbody target = hit.transform.gameObject.GetComponent<Rigidbody>();

            return true;
        }
        return false;
    }

    //Set movement direction based on player input
    void SetMovementDir()
    {
        cmd.forwardmove = GameManager.Instance.controlsManager.moveInput.y;
        cmd.rightmove = GameManager.Instance.controlsManager.moveInput.x;
    }

     
    public void JumpInput(bool performed)   
    {
        //Debug.Log(Time.time + " : " +  lastJumpTime + " : " +  fallingTime + " : " + usedDoubleJump);
        if (((playerVelocity.y > 0 && Time.time > lastJumpTime + 0.2f) ||  Time.time < fallingTime + 0.2f) && !usedDoubleJump)
        {
            wishJump = true; 
            usedDoubleJump = true;  
            CheckForJump();

            return; 
        }
        else if (playerVelocity.y > 0 ||  Time.time < fallingTime + 0.2f) 
            return;


        if (performed && !wishJump)
            wishJump = true;

        if (!performed)
            wishJump = false;
    }

    void AirMove()
    {
        Vector3 wishDir;
        float wishVal = airAcceleration;
        float accel;

        SetMovementDir();

        wishDir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);
        wishDir = transform.TransformDirection(wishDir);

        float wishSpeed = wishDir.magnitude;
        wishSpeed *= moveSpeed;

        wishDir.Normalize();
        Vector3 moveDirectionNorm = wishDir;

        float wishSpeed2 = wishSpeed;
        if (Vector3.Dot(playerVelocity, wishDir) > 0)
            accel = airDeacceleration;
        else
            accel = airAcceleration;

        if (cmd.forwardmove == 0 && cmd.rightmove != 0)
        {
            if (wishSpeed > sideStrafeSpeed)
                wishSpeed = sideStrafeSpeed;

            accel = sideStrafeAcceleration;
        }

        Accelerate(wishDir, wishSpeed, accel);
        if (airControl > 0)
            AirControl(wishDir, wishSpeed2);

        playerVelocity = Vector3.ClampMagnitude(playerVelocity, 60f);
        playerVelocity.z = Mathf.Clamp(playerVelocity.z, -40, 40);

        playerVelocity.y -= gravity * Time.deltaTime;
    }

    void AirControl(Vector3 wishDir, float wishSpeed)
    {
        float zSpeed;
        float speed;
        float dot;
        float k;
        int i;

        if (cmd.forwardmove == 0 || wishSpeed == 0)
            return;

        zSpeed = playerVelocity.y;
        playerVelocity.y = 0;

        speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        dot = Vector3.Dot(playerVelocity, wishDir);
        k = 32;
        k *= airControl * dot * dot * Time.deltaTime;

        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishDir.x * k;
            playerVelocity.y = playerVelocity.y * speed + wishDir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishDir.z * k;

            playerVelocity.Normalize();
            moveDirectionNorm = playerVelocity;
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zSpeed; 
        playerVelocity.z *= speed;


    }

    void GroundMove()
    { 
        bool crouching = false;
        usedDoubleJump = false; 

        if (GameManager.Instance.controlsManager.CrouchInputDown() && playerVelocity.y < 0.2f)
        {
            crouching = true; 
        }

        if (crouching)
        {
            controller.height = 0f;
            playerCameraYOffset = 0f;
        } 
        else
        {
            controller.height = 2f;
            playerCameraYOffset = 1f; 
        }

        Vector3 wishDir;

        if (!wishJump)
            ApplyFriction(crouching ? 0.2f : 1.0f); 
        else
            ApplyFriction(0);

        if (!crouching)
        {
            SetMovementDir();

            wishDir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);
            wishDir = transform.TransformDirection(wishDir);

            wishDir.Normalize();
            moveDirectionNorm = wishDir;

            var wishspeed = wishDir.magnitude;
            wishspeed *= moveSpeed;

            Accelerate(wishDir, wishspeed, runAcceleration);

            playerVelocity.y = 0;
        }

        CheckForJump();
    } 

    public  void CheckForJump()
    { 
        if (wishJump)
        {
            lastJumpTime = Time.time;
            playerVelocity.y = jumpSpeed;
            wishJump = false;
        }
    }

    void ApplyFriction(float t, bool preserveY = false)
    {
        Vector3 vec = playerVelocity;
        float vel;
        float speed;
        float newSpeed;
        float control;
        float drop;

        speed = vec.magnitude;
        drop = 0;


        control = speed < runDeacceleration ? runDeacceleration : speed;
        drop = control * friction * Time.deltaTime * t;

        newSpeed = speed - drop;
        playerFriction = newSpeed;

        if (newSpeed < 0)
            newSpeed = 0;
        if (speed > 0)
            newSpeed /= speed;

        playerVelocity.x *= newSpeed;
        playerVelocity.z *= newSpeed;
        playerVelocity.y *= newSpeed;
    }

    public void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
    {
        float addSpeed;
        float accelSpeed;
        float currentSpeed;

        currentSpeed = Vector3.Dot(playerVelocity, wishDir);
        addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0)
            return;

        accelSpeed = accel * Time.deltaTime * wishSpeed;

        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        playerVelocity.x += accelSpeed * wishDir.x;
        playerVelocity.z += accelSpeed * wishDir.z;
    }
}