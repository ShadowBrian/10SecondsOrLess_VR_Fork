using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Watch : MonoBehaviour
{
    public Image stopWatchImage;

    public void Update()
    {
        float stopWatchFill = GameManager.Instance.timerStarted ?  1 - Mathf.Clamp01((Time.time - GameManager.Instance.levelStartTime)/10f) : 1f;
        stopWatchImage.fillAmount = stopWatchFill;
    }
}
