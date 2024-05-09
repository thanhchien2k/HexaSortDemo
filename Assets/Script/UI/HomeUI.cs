using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class HomeUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI levelText;

    private void OnEnable()
    {
        SetUpLevelText(GameData.CurrentLevelID);
    }
    public void LoadGameplayScene()
    {
        SceneTransition.Instance.LoadScene(1);
    }

    private void SetUpLevelText(int _index)
    {
        levelText.text = "LEVEL " + (_index + 1).ToString();
    }
}
