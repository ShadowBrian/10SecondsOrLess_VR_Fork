using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void QuitGame()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }

    public void PlayGame()
    {
            StartCoroutine(PlayGameRoutine(0));
    }

    public void PlayLevel(int level)
    {
        StartCoroutine(PlayGameRoutine(level));
    }

    public void ExitToMenu()
    {
            StartCoroutine(ExitToMenuRoutine());
    }

    public IEnumerator ExitToMenuRoutine()
    {
        ScreenFader.Instance.Fade(1, 1.5f);
        yield return new WaitForSecondsRealtime(1.5f);


        SceneManager.LoadScene("MainMenu");
    }

    public void Resume()
    {
    }

    public void Restart()
    {
        GameManager.Instance.RestartLevel();
    }

    public void Continue()
    {
        GameManager.Instance.LoadNextLevel();
    }


    public IEnumerator PlayGameRoutine(int level = 0)
    {
        ScreenFader.Instance.Fade(1, 1.5f);
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene("Level " + (level + 1));
    }

  
}
