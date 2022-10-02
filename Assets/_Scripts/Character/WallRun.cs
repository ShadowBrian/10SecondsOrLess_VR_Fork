using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class WallRun : MonoBehaviour
{

    public float wallMaxDistance = 1;
    public float wallSpeedMultiplier = 1.2f;
    public float minimumHeight = 1.2f;
    public float maxAngleRoll = 20;
    [Range(0.0f, 1.0f)]
    public float normalizedAngleThreshold = 0.1f;

    public float jumpDuration = 1;
    public float wallBouncing = 3;
    public float cameraTransitionDuration = 1;

    public float wallGravityDownForce = 20f;

    public bool useSprint;

    PlayerController m_PlayerCharacterController; 
     
    Vector3[] directions;
    RaycastHit[] hits;

    bool isWallRunning = false;
    Vector3 lastWallPosition;
    Vector3 lastWallNormal;
    float elapsedTimeSinceJump = 0;
    float elapsedTimeSinceWallAttach = 0;
    float elapsedTimeSinceWallDetatch = 0;
    public bool jumping;
    float lastVolumeValue = 0;
    float noiseAmplitude;

    bool isPlayergrounded() => m_PlayerCharacterController.isGrounded;

    public bool IsWallRunning() => isWallRunning;

    bool CanWallRun()
    {
        float verticalAxis = GameManager.Instance.controlsManager.moveInput.y;
        bool isSprinting = true; 
        isSprinting = !useSprint ? true : isSprinting;

        return !isPlayergrounded() && verticalAxis > 0 && VerticalCheck() && isSprinting;
    }

    bool VerticalCheck()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumHeight);
    }


    void Start()
    {
        m_PlayerCharacterController = GetComponent<PlayerController>();

        directions = new Vector3[]{
            Vector3.right,
            Vector3.right + Vector3.forward,
            Vector3.forward,
            Vector3.left + Vector3.forward,
            Vector3.left
        };
    }


    public void LateUpdate()
    {
        isWallRunning = false; 

        jumping = GameManager.Instance.controlsManager.JumpInputDown(); 

        if (CanAttach())
        {
            hits = new RaycastHit[directions.Length];

            for (int i = 0; i < directions.Length; i++)
            {
                Vector3 dir = transform.TransformDirection(directions[i]);
                Physics.Raycast(transform.position, dir, out hits[i], wallMaxDistance);
                if (hits[i].collider != null)
                {
                    Debug.DrawRay(transform.position, dir * hits[i].distance, Color.green);
                }
                else
                {
                    Debug.DrawRay(transform.position, dir * wallMaxDistance, Color.red);
                }
            }

            if (CanWallRun())
            {
                hits = hits.ToList().Where(h => h.collider != null).OrderBy(h => h.distance).ToArray();
                if (hits.Length > 0)
                {
                    OnWall(hits[0]);
                    lastWallPosition = hits[0].point;
                    lastWallNormal = hits[0].normal;
                }
            }
        }

        if (isWallRunning)
        {
            elapsedTimeSinceWallDetatch = 0;

            elapsedTimeSinceWallAttach += Time.deltaTime;
            m_PlayerCharacterController.characterVelocity += Vector3.down * wallGravityDownForce * Time.deltaTime;
        }
        else
        {
            elapsedTimeSinceWallAttach = 0;
            elapsedTimeSinceWallDetatch += Time.deltaTime;
        }
    }

    bool CanAttach()
    {
        if (jumping)
        {
            elapsedTimeSinceJump += Time.deltaTime;
            if (elapsedTimeSinceJump > jumpDuration)
            {
                elapsedTimeSinceJump = 0;
                jumping = false;
            }
            return false; 
        }

        return true;
    }

    void OnWall(RaycastHit hit)
    {
        float d = Vector3.Dot(hit.normal, Vector3.up);
        if (d >= -normalizedAngleThreshold && d <= normalizedAngleThreshold)
        {
            // Vector3 alongWall = Vector3.Cross(hit.normal, Vector3.up); 
            float vertical = GameManager.Instance.controlsManager.moveInput.y;
            Vector3 alongWall = transform.TransformDirection(Vector3.forward); 
             
            Debug.DrawRay(transform.position, alongWall.normalized * 10, Color.green);
            Debug.DrawRay(transform.position, lastWallNormal * 10, Color.magenta);

            m_PlayerCharacterController.characterVelocity = alongWall * vertical * wallSpeedMultiplier;
            isWallRunning = true;
        }
    }

    float CalculateSide()
    {
        if (isWallRunning)
        {
            Vector3 heading = lastWallPosition - transform.position;
            Vector3 perp = Vector3.Cross(transform.forward, heading);
            float dir = Vector3.Dot(perp, transform.up);
            return dir;
        }
        return 0;
    }

    public float GetCameraRoll()
    {
        float dir = CalculateSide();
        float cameraAngle = m_PlayerCharacterController.playerCamera.transform.eulerAngles.z;
        float targetAngle = 0;
        if (dir != 0)
        {
            targetAngle = Mathf.Sign(dir) * maxAngleRoll;
        }
        return Mathf.LerpAngle(cameraAngle, targetAngle, Mathf.Max(elapsedTimeSinceWallAttach, elapsedTimeSinceWallDetatch) / cameraTransitionDuration);
    }

    public Vector3 GetWallJumpDirection()
    {
        if (isWallRunning)
        {
            return lastWallNormal * wallBouncing + Vector3.up;
        }
        return Vector3.zero;
    }
}
