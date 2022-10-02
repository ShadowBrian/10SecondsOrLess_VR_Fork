using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class SceneHelper
{
    public static string[]  Levels = { "Level 1", "Level 2", "Level 3", "Level 4", "Level 5", "Level 6", "Level 7", "Level 8", "Level 9"};

    public static string GetNextLevelString(string currentLevel)
    {
        if (int.TryParse(currentLevel.Last().ToString(), out int currnetLevelNumber))
        {
            if (currnetLevelNumber + 1 <= Levels.Length)
            {
                return "Level " + (currnetLevelNumber + 1);
            }
        } 

        return "MainMenu";
    }
}
