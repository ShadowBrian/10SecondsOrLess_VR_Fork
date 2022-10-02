using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events; 

public class MenuButtons : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    bool hovering;

    public UnityEvent clickEvent;
    public AudioSource audioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;

    float fontSize;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        fontSize = text.fontSize;
    }

    public void OnClick()
    {
        clickEvent.Invoke();
        Debug.Log(gameObject.name);
        audioSource.clip = clickClip;
        audioSource.Play();
    }


    public void MouseEnter()
    {
        if(fontSize == 0)
            fontSize = text.fontSize;
        text.color = Color.white * 0.7f;
        hovering = true;
        text.fontSize = fontSize * 1.02f;
        audioSource.clip = hoverClip;
        audioSource.Play();
    }


    public void MouseExit()
    {
        Debug.Log("Exit");
        hovering = false;

        text.color = Color.white;

        if (fontSize == 0) 
            fontSize = text.fontSize;

        text.fontSize = fontSize;
    }
}
