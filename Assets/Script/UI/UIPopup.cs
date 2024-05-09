using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup : MonoBehaviour
{
    [SerializeField] Transform popup;
    private void OnEnable()
    {
        popup.localScale = Vector3.zero;
        popup.DOScale(Vector3.one, 1f);
    }
}
