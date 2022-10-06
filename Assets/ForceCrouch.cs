using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceCrouch : MonoBehaviour
{
    public CharacterController controller;
    void Start()
    {
        controller = GetComponentInParent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Crouch state:" + (controller.height < 0.5f));
        if (controller != null && controller.height < 0.5f)
        {
            transform.localPosition = -Vector3.up * 0.5f;
        }
        else
        {
            transform.localPosition = Vector3.zero;
        }
    }
}
