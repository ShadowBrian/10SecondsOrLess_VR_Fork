using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EndOfLevel : MonoBehaviour
{
    public Animator endOfLevel;
    public Collider trigger;
    public Volume volume;

    public bool beamOut; 

    public void TriggerEndOfLevel()
    {
        if (!beamOut)
        {
            trigger.enabled = true;

            //StartCoroutine(LerpVolumeWeight(0.2f));
            endOfLevel.SetTrigger("ReleaseBeam");
        }
    }

    public IEnumerator LerpVolumeWeight(float duration)
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            volume.weight = Mathf.Lerp(0, 1, t/duration);
            yield return new WaitForEndOfFrame();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();

        if (controller && GameManager.Instance.timerStarted)
        {
            GameManager.Instance.CompleteLevel();
        }
    }


}
