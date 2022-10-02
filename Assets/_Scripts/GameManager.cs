using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class GameManager : Singleton<GameManager>
{
    public ControlsManager controlsManager;

    public PlayerController newPlayerController; 
    public CharacterMovement playerController;
    public Gun gun;
    public EndOfLevel endOfLevel;
    public CanvasManager canvasManager; 

    public AudioSource globalSource;

    public Enemy[] allEnemies;

    public float levelStartTime;
    public bool timerStarted;

    bool loadingScene;

    public TextMeshProUGUI remainingEnemiesText;

    public float levelDeveloperTime = 10f;

    public AudioClip levelStartClip;

    public bool menus; 

    public float RemainingEnemies
    {
        get
        {
            int count = 0;

            if (allEnemies != null)
            {
                foreach (Enemy enemy in allEnemies)
                {
                    if (enemy != null) 
                    {
                        if (!enemy.killed)
                            count++;
                    }
                }
            }

            return count;  
        } 
    }

    public void Awake()  
    {
        controlsManager = ControlsManager.instance;

        Initialise();
    } 

    public void Initialise()
    {
        if (!menus)
        {
            allEnemies = FindObjectsOfType<Enemy>();
            endOfLevel = FindObjectOfType<EndOfLevel>();

            StartCoroutine(GameRoutine()); 
        }
        else
        {
            canvasManager.menus.SetActive(true);
        }
    }
     
    public void Update()  
    {
        remainingEnemiesText.text = RemainingEnemies.ToString() ;
    }

    public void CompleteLevel()
    {
        if (timerStarted) 
        {
            Debug.Log("Player Completed Level in: " + (Time.time - levelStartTime));
            timerStarted = false;
            canvasManager.ShowWinScreen((Time.time - levelStartTime), levelDeveloperTime);
              
            controlsManager.controls.Player.Disable();

            Cursor.lockState = CursorLockMode.None;
        }
    }

    public IEnumerator GameRoutine() 
    {
        while (ScreenFader.Instance.IsFading)
        {
            controlsManager.controls.Player.Disable();
            yield return null; 
        }


        controlsManager.controls.Player.Enable();

        globalSource.PlayOneShot(levelStartClip );

        yield return new WaitForSeconds(3f);

        levelStartTime = Time.time;
        timerStarted = true;

        while (Time.time < levelStartTime + 10f)
        {
            yield return null;

            if (RemainingEnemies == 0)
                endOfLevel.TriggerEndOfLevel(); 
        }

        if (timerStarted == true) 
        {
            timerStarted = false;
            controlsManager.controls.Player.Disable();
            canvasManager.ShowFailScreen(); 
        }
    }

    public void RestartLevel()   
    {
        if (!loadingScene)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            loadingScene = true;
        }
    }

    public void LoadNextLevel() 
    {
        if (!loadingScene)
        { 
            SceneManager.LoadScene(SceneHelper.GetNextLevelString(SceneManager.GetActiveScene().name));
            loadingScene = true;
        }
    }

}