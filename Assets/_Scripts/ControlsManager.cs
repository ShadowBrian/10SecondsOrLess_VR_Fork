using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsManager : MonoBehaviour
{
    public static ControlsManager instance;

    public Controls controls;

    public Vector2 moveInput;
    public Vector2 lookInput;

    public LockedNavigation lockedNavigation;

    public InputDevice currentInputDevice { get; private set; }
    public string currentScheme { get; private set; }

    public bool IsGamepad => currentScheme.Equals("Gamepad");


    public void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);


        controls = new Controls();

        //controls.Player.Jump.performed += JumpPerformed;
        //controls.Player.Fire.performed += FireGun;
        //controls.General.Restart.performed += Restart;
        // controls.General.LockedNavigationSelect.performed += LockedNavigationSelect;

        currentScheme = "Keyboard";
        InputSystem.onActionChange += OnActionChange;

        controls.Enable();
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            InputDevice lastDevice = ((InputAction)obj).activeControl.device;

            if (lastDevice != currentInputDevice || currentInputDevice == null)
            {
                if (lastDevice.ToString().Contains("Mouse") || lastDevice.ToString().Contains("Keyboard"))
                {
                    currentScheme = "Keyboard";

                    if (lockedNavigation)
                        lockedNavigation.OnControllerInput(false);

                }
                else
                {
                    currentScheme = "Gamepad";

                    if (lockedNavigation)
                        lockedNavigation.OnControllerInput(true);
                }
            }
        }
    }

    private void Restart()
    {
        GameManager.Instance.RestartLevel();
    }

    public void Update()
    {
        /*if (GameManager.Instance && GameManager.Instance.timerStarted)
            moveInput = controls.Player.Movement.ReadValue<Vector2>(); 
        else
            moveInput = Vector3.zero;*/

        if (GameManager.Instance && GameManager.Instance.timerStarted)
            moveInput = UnityXRInputBridge.instance.GetVec2(XR2DAxisMasks.primary2DAxis, XRHandSide.LeftHand);
        else
            moveInput = Vector3.zero;

        if (ScreenFader.Instance && !ScreenFader.Instance.IsFading)
        {
            /*if (!IsGamepad)
            {
                lookInput = controls.Player.Look.ReadValue<Vector2>() * 0.5f * 0.1f;
            }
            else
            {
                lookInput = controls.Player.Look.ReadValue<Vector2>() * Time.deltaTime;
            }*/

            lookInput = UnityXRInputBridge.instance.GetVec2(XR2DAxisMasks.primary2DAxis, XRHandSide.RightHand);
        }
        else
        {
            lookInput = Vector3.zero;
        }

        if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primaryButton, XRHandSide.RightHand))
        {
            JumpPerformed();
        }
        if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.triggerButton, XRHandSide.RightHand))
        {
            FireGun();
        }
        if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.secondaryButton, XRHandSide.RightHand))
        {
            Restart();
        }

        if (UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.primaryButton, XRHandSide.LeftHand) || UnityXRInputBridge.instance.GetButtonDown(XRButtonMasks.triggerButton, XRHandSide.LeftHand))
        {
            LockedNavigationSelect();
        }

        if (lockedNavigation && lockedNavigation.gameObject.activeInHierarchy)
            lockedNavigation.Input(UnityXRInputBridge.instance.GetVec2(XR2DAxisMasks.primary2DAxis, XRHandSide.LeftHand));
    }


    private void LockedNavigationSelect()
    {
        if (lockedNavigation != null)
            lockedNavigation.SelectButton();
    }

    public void JumpPerformed()
    {
        if (GameManager.Instance.timerStarted)
        {
            GameManager.Instance.newPlayerController.Jump();
        }
    }

    public void FireGun()
    {
        if (GameManager.Instance.timerStarted)
            GameManager.Instance.gun.Fire();
    }

    public bool CrouchInputDown()
    {
        return GameManager.Instance.timerStarted ? UnityXRInputBridge.instance.GetButton(XRButtonMasks.triggerButton, XRHandSide.LeftHand) : false;
    }

    public bool JumpInputDown()
    {
        return GameManager.Instance.timerStarted ? UnityXRInputBridge.instance.GetButton(XRButtonMasks.primaryButton, XRHandSide.RightHand) : false;
    }

    public bool FireInputDown()
    {
        return GameManager.Instance.timerStarted ? UnityXRInputBridge.instance.GetButton(XRButtonMasks.triggerButton, XRHandSide.RightHand) : false;
    }

}
