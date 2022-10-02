using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;

    public float gravityDownForce = 20f;
    public LayerMask groundCheckLayers = -1;
    public float groundCheckDistance = 0.05f;
    public float maxSpeedOnGround = 10f;
    public float movementSharpnessOnGround = 15;

    [Range(0, 1)] public float maxSpeedCrouchedRatio = 0.5f;
    public float maxSpeedInAir = 10f;
    public float accelerationSpeedInAir = 25f;
    public float sprintSpeedModifier = 2f;


    public float rotationSpeed = 200f;
    [Range(0.1f, 1f)] public float aimingRotationMultiplier = 0.4f;

    public float jumpForce = 9f;

    public float cameraHeightRatio = 0.9f;
    public float capsuleHeightStanding = 1.8f;
    public float capsuleHeightCrouching = 0.9f;
    public float crouchingSharpness = 10f;

    public UnityAction<bool> onStanceChanged;

    public Vector3 characterVelocity { get; set; }
    public bool isGrounded { get; private set; }
    public bool hasJumpedThisFrame { get; private set; }
    public bool isDead { get; private set; }
    public bool isCrouching { get; private set; }
    public float RotationMultiplier
    { 
        get
        {
            return 1f;
        }
    }

    CharacterController m_Controller;
    Vector3 m_GroundNormal;
    Vector3 m_CharacterVelocity;
    Vector3 m_LatestImpactSpeed;
    float m_LastTimeJumped = 0f;
    float m_CameraVerticalAngle = 0f;
    float m_footstepDistanceCounter;
    float m_TargetCharacterHeight;

    const float k_JumpGroundingPreventionTime = 0.2f;
    public float groundCheckDistanceInAir = 0.07f; 

    WallRun wallRunComponent;

    bool usedDoubleJump = false;
     
    public AudioSource source;
    public AudioClip jumpClip; 

    void Start()
    {
        m_Controller = GetComponent<CharacterController>();

        wallRunComponent = GetComponent<WallRun>();

        m_Controller.enableOverlapRecovery = true;

        // force the crouch state to false when starting
        SetCrouchingState(false, true);
        UpdateCharacterHeight(true);
    }

    void Update()
    {
        if (GameManager.Instance.timerStarted && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }


        hasJumpedThisFrame = false;

        bool wasGrounded = isGrounded;
        GroundCheck();

        if (GameManager.Instance.controlsManager.CrouchInputDown())
        {
            SetCrouchingState(!isCrouching, false);

            if (isGrounded)
                usedDoubleJump = false;
        }

        UpdateCharacterHeight(false);

        HandleCharacterMovement();
    }

    void GroundCheck()
    {
        float chosenGroundCheckDistance = isGrounded ? (m_Controller.skinWidth + groundCheckDistance) : groundCheckDistanceInAir; 
        isGrounded = false;
        m_GroundNormal = Vector3.up;

        if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime) 
        {
            
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height), m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance, groundCheckLayers, QueryTriggerInteraction.Ignore))
            {
                m_GroundNormal = hit.normal;

                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(m_GroundNormal))
                {
                    isGrounded = true;
                    usedDoubleJump = false; 

                    if (hit.distance > m_Controller.skinWidth)
                    {
                        m_Controller.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }
    }

    public float GetCameraRoll()
    {
        float dir = -GameManager.Instance.controlsManager.moveInput.x;
        float cameraAngle = playerCamera.transform.eulerAngles.z;
        float targetAngle = 0;
        if (dir != 0)
        {
            targetAngle = Mathf.Sign(dir) * 5; 
        }
        return Mathf.LerpAngle(cameraAngle, targetAngle, 5 * Time.deltaTime);
    }

    void HandleCharacterMovement()
    {
        {
            transform.Rotate(new Vector3(0f, (GameManager.Instance.controlsManager.lookInput.x * rotationSpeed * RotationMultiplier), 0f), Space.Self);
        }

        {
            m_CameraVerticalAngle += -GameManager.Instance.controlsManager.lookInput.y * rotationSpeed * RotationMultiplier;
            m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);

            if (wallRunComponent != null) 
            {
                if (wallRunComponent.IsWallRunning())
                {
                    usedDoubleJump = false;
                    playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, wallRunComponent.GetCameraRoll());
                }
                else
                {
                    playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, GetCameraRoll());
                }
            }
            else
            {
                playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
            }
        }

        bool isSprinting = true;
        {
            if (isSprinting)
            {
                isSprinting = SetCrouchingState(false, false);
            }

            float speedModifier = isSprinting ? sprintSpeedModifier : 1f;
             
            Vector3 worldspaceMoveInput = transform.TransformVector(new Vector3(GameManager.Instance.controlsManager.moveInput.x, 0, GameManager.Instance.controlsManager.moveInput.y));
              
            if (isGrounded || (wallRunComponent != null && wallRunComponent.IsWallRunning()))
            {
                if (isGrounded)
                {
                    Vector3 targetVelocity = worldspaceMoveInput * maxSpeedOnGround * speedModifier;
                    if (isCrouching)
                        targetVelocity *= maxSpeedCrouchedRatio;
                    targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) * targetVelocity.magnitude;


                    characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, movementSharpnessOnGround * Time.deltaTime);
                }
            }
            else
            {
                if (wallRunComponent == null || (wallRunComponent != null && !wallRunComponent.IsWallRunning()))
                {
                    characterVelocity += worldspaceMoveInput * accelerationSpeedInAir * Time.deltaTime;

                    float verticalVelocity = characterVelocity.y;
                    Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * speedModifier);
                    characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                    characterVelocity += Vector3.down * gravityDownForce * Time.deltaTime;
                }
            }
        }

        Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
        Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_Controller.height);
        m_Controller.Move(characterVelocity * Time.deltaTime);

        m_LatestImpactSpeed = Vector3.zero;
        if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, m_Controller.radius, characterVelocity.normalized, out RaycastHit hit, characterVelocity.magnitude * Time.deltaTime, -1, QueryTriggerInteraction.Ignore))
        {
            m_LatestImpactSpeed = characterVelocity;

            characterVelocity = Vector3.ProjectOnPlane(characterVelocity, hit.normal);
        }
    }

    public void Jump()
    {
        bool groundedOrDouble = isGrounded || CanDoubleJump();

        if ((groundedOrDouble || (wallRunComponent != null && wallRunComponent.IsWallRunning())))
        {
            if (SetCrouchingState(false, false))
            { 
                if (!wallRunComponent.IsWallRunning() && groundedOrDouble)
                { 
                    if (!isGrounded)
                        usedDoubleJump = true; 

                    characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                    characterVelocity += Vector3.up * jumpForce;

                    source.volume = 0.5f;
                    source.pitch = Random.Range(0.95f, 1.05f);
                    source.PlayOneShot(jumpClip);
                    Debug.Log("PLAY JUMP");
                }
                else if(wallRunComponent.IsWallRunning()) 
                {
                    usedDoubleJump = false;
                    characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);
                    characterVelocity += wallRunComponent.GetWallJumpDirection() * jumpForce;

                    if (Time.time > m_LastTimeJumped + 0.3f)
                    { 
                        source.volume = 0.5f; 
                        source.pitch = Random.Range(0.95f, 1.05f);
                        source.PlayOneShot(jumpClip);
                    }
                }
                m_LastTimeJumped = Time.time;
                hasJumpedThisFrame = true;

                isGrounded = false;
                m_GroundNormal = Vector3.up;
            }
        }
         
        wallRunComponent.jumping = true; 
    }

    public bool CanDoubleJump() 
    {
        if (!usedDoubleJump && !isGrounded && !wallRunComponent.IsWallRunning() && Time.time > m_LastTimeJumped + 0.3f)
            return true;
        else
            return false;  
    }

    bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
    }

    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * m_Controller.radius);
    }

    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - m_Controller.radius));
    }

    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

    void UpdateCharacterHeight(bool force)
    {
        if (force)
        {
            m_Controller.height = m_TargetCharacterHeight;
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.up * m_TargetCharacterHeight * cameraHeightRatio; 
        }
        else if (m_Controller.height != m_TargetCharacterHeight)
        {
            m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight, crouchingSharpness * Time.deltaTime);
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, Vector3.up * m_TargetCharacterHeight * cameraHeightRatio, crouchingSharpness * Time.deltaTime);
        }
    }

    bool SetCrouchingState(bool crouched, bool ignoreObstructions)
    {
        if (crouched)
        {
            m_TargetCharacterHeight = capsuleHeightCrouching;
        }
        else
        {
            if (!ignoreObstructions)
            {
                Collider[] standingOverlaps = Physics.OverlapCapsule(
                    GetCapsuleBottomHemisphere(),
                    GetCapsuleTopHemisphere(capsuleHeightStanding),
                    m_Controller.radius,
                    -1,
                    QueryTriggerInteraction.Ignore);
                foreach (Collider c in standingOverlaps)
                {
                    if (c != m_Controller)
                    {
                        return false;
                    }
                }
            }

            m_TargetCharacterHeight = capsuleHeightStanding;
        }

        if (onStanceChanged != null)
        {
            onStanceChanged.Invoke(crouched);
        }

        isCrouching = crouched;
        return true;
    }
}
