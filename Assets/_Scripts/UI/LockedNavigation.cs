using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedNavigation : MonoBehaviour
{
    public MenuButtons[] buttons;
    public int currentSelected;
    public ControlsManager controlsManager;
    public bool active;

    public bool xAxis;

    public void OnEnable()
    {
        controlsManager = GameManager.Instance.controlsManager;

        OnControllerInput(true);

        GameManager.Instance.controlsManager.lockedNavigation = this;
    }

    public void OnDisable()
    {
        active = false;
    }

    public void OnControllerInput(bool input)
    {
        Debug.Log("Locked nav enabled");
        if (input && !active)
            buttons[currentSelected].MouseEnter();
        else if (!input && active)
            buttons[currentSelected].MouseExit();

        active = input;
    }

    public void SelectButton()
    {
        if (gameObject.activeInHierarchy)
            buttons[currentSelected].OnClick();
    }

    float nextInput = 0f;

    public void Input(Vector2 Vector2Input)
    {
        if (!gameObject.activeInHierarchy)
            return;

        float input = xAxis ? Vector2Input.x : Vector2Input.y;

        if (Mathf.Abs(input) > 0f && Time.unscaledTime > nextInput)
        {
            nextInput = Time.unscaledTime + 0.6f;

            buttons[currentSelected].MouseExit();

            if (input > 0)
                currentSelected += 1;
            else if (input < 0)
                currentSelected -= 1;

            currentSelected = Mathf.Clamp(currentSelected, 0, buttons.Length - 1);

            buttons[currentSelected].MouseEnter();


        }
    }

}
