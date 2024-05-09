using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    private static int currentLevelID;
    public static int CurrentLevelID {
        get
        {
            currentLevelID = PlayerPrefs.GetInt("LevelID", 0);
            return currentLevelID;
        }
        set
        {
            PlayerPrefs.SetInt("LevelID", value);
            currentLevelID = PlayerPrefs.GetInt("LevelID",0);
        } 
    }
}
