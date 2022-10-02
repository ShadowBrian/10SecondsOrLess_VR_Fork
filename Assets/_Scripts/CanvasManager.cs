using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class CanvasManager : MonoBehaviour
{
    public MainMenu mainMenu;
    public GameObject gameplayHUD;
    public GameObject wonCanvas;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI devTimeText;
    public Animator winAnimator;

    public GameObject failCanvas;

    public GameObject menus;

    public GameObject levelSelect;
    public int currentLevelIndex = 0;
    public TextMeshProUGUI levelText;

    public TextMeshProUGUI TutorialText;

    public void Awake()
    {

        TutorialText.text = "";
    }

    public void Update()
    { 
        gameplayHUD.SetActive(GameManager.Instance.timerStarted);

    } 
    public void ShowWinScreen(float completionTime, float devTime)
    {
        devTimeText.text = devTime.ToString("n2" ) + "s";
        timeText.text = completionTime.ToString("n2") + "s"; 
        wonCanvas.SetActive(true);
        winAnimator.Rebind();
    }

    public void ShowFailScreen()
    { 
        failCanvas.SetActive(true); 
    }

    public void ShowLevelSelect()
    {
        menus.SetActive(false);
        levelSelect.SetActive(true);
    }

    public void ShowMenus()
    {
        menus.SetActive(true);
        levelSelect.SetActive(false); 
    }


    public void IncrementLevel()
    {
        currentLevelIndex++;
         
        if (currentLevelIndex >= SceneHelper.Levels.Length)
            currentLevelIndex = 0;

        levelText.text = "Level " + (currentLevelIndex + 1).ToString();
    }

    public void DecrementLevel()
    {
        currentLevelIndex--;

        if (currentLevelIndex < 0)
            currentLevelIndex = SceneHelper.Levels.Length - 1;

        levelText.text = "Level " + (currentLevelIndex + 1).ToString();
    }

    public void PlaySelectedLevel()
    {
        mainMenu.PlayLevel(currentLevelIndex );
    }

    public void DisplayTutorialText(string text, float duration)
    {
        StartCoroutine(DisplayTutorialTextForDuration(text, duration));
    }

    public IEnumerator DisplayTutorialTextForDuration(string text, float duration)
    {
        TutorialText.text = text;

        yield return new WaitForSeconds(duration);

        TutorialText.text = "";
    }
}
