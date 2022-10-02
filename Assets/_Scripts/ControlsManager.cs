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

        controls.Player.Jump.performed += JumpPerformed;
        controls.Player.Fire.performed += FireGun;
        controls.General.Restart.performed += Restart;
        controls.General.LockedNavigationSelect.performed += LockedNavigationSelect;

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

                    if(lockedNavigation)
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

    private void Restart(InputAction.CallbackContext obj)
    {
        if (obj.performed)
            GameManager.Instance.RestartLevel();
    }

    public void Update()
	{

        if (GameManager.Instance && GameManager.Instance.timerStarted)
            moveInput = controls.Player.Movement.ReadValue<Vector2>(); 
        else
            moveInput = Vector3.zero;

        if (ScreenFader.Instance && !ScreenFader.Instance.IsFading)
        {
            if (!IsGamepad)
            {
                lookInput = controls.Player.Look.ReadValue<Vector2>() * 0.5f * 0.1f;
            }
            else
            {
                lookInput = controls.Player.Look.ReadValue<Vector2>() * Time.deltaTime;
            }
        }
        else
        {
            lookInput = Vector3.zero;
        }

        if (lockedNavigation && lockedNavigation.gameObject.activeInHierarchy)
            lockedNavigation.Input(controls.General.LockedNavigationInput.ReadValue<Vector2>());
    }


    private void LockedNavigationSelect(InputAction.CallbackContext obj)
    {
        if (obj.performed && lockedNavigation != null)
            lockedNavigation.SelectButton(); 
    } 

    public void JumpPerformed(InputAction.CallbackContext ctx)
	{
        if (ctx.performed && GameManager.Instance.timerStarted)
        { 
            GameManager.Instance.newPlayerController.Jump();
        }
	}

    public void FireGun(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && GameManager.Instance.timerStarted)
            GameManager.Instance.gun.Fire();
    }

    public bool CrouchInputDown()
    {
        return  GameManager.Instance.timerStarted ? controls.Player.Crouch.ReadValue<float>() > 0.5f : false; 
    }

    public bool JumpInputDown()
	{
        return   GameManager.Instance.timerStarted ?  controls.Player.Jump.ReadValue<float>() > 0.5f : false;
	}

	public bool FireInputDown()
	{
        return GameManager.Instance.timerStarted ? controls.Player.Fire.ReadValue<float>() > 0.5f : false;
	}

}
