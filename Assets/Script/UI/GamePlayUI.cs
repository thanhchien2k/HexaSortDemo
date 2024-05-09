using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayUI : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI currentProgressText;
    [SerializeField]TextMeshProUGUI currentLevelText;
    [SerializeField]Image currentProgressImage;
    [SerializeField] Transform winPopup;
    [SerializeField] Transform losePopup;
    [SerializeField] Transform backPopup;

    private void Start()
    {
        currentLevelText.text = "LEVEL " + (GameData.CurrentLevelID + 1).ToString();
    }
    public void UpdateUI(int value, int complatePoint)
    {
        if(value >= complatePoint)
        {
            value = complatePoint;
            GameData.CurrentLevelID++;
        }
        currentProgressText.text = value.ToString() + "/" + complatePoint;
        currentProgressImage.fillAmount = value/(float)complatePoint;

        if(value >= complatePoint)
        {
            PopupTransition(winPopup);
        }
    }

    public void LoadHomeScene()
    {
        SceneTransition.Instance.LoadScene(0);
    }

    public void ReloadGamePlay()
    {
        SceneTransition.Instance.ReLoadScene();
    }

    public void PopupTransition(Transform popup)
    {
        GameManager2.Instance.HideMap();
        popup.gameObject.SetActive(true);
    }

    public void PopupLose()
    {
        PopupTransition(losePopup);
    }

    public void PopupBack()
    {
        PopupTransition(backPopup);
    }

    public void OnClickBackButton()
    {
        GameManager2.Instance.DisplayMap();
        backPopup.gameObject.SetActive(false);
    }
}
