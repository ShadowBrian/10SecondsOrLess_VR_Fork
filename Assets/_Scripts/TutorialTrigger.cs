using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{

    public string text;
    public float duration = 5f;
    public bool ran;

    public void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Tutorial");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (ran)
            return; 

        PlayerController controller = other.GetComponent<PlayerController>();

        if (controller)
        {
            GameManager.Instance.canvasManager.DisplayTutorialText(text, duration);
            ran = true;
        }
    }
}
