using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BaseHexagon : MonoBehaviour
{
    [SerializeField] Material originalMaterial;
    [SerializeField] Material hightlightMaterial;
    [SerializeField] Renderer Renderer;
    [SerializeField] TextMeshPro lockText;
    public ChipStack currentChipStack { get; set; }
    public bool IsPlaceable = true;

    private int lockPoint;
    public int LockPoint {
        get
        {
            return lockPoint;
        }
        set
        {
            if(lockText != null)
            {
                lockText.text = "Lock "+ value.ToString();
            }
            else
            {
                Debug.Log("Lock text is null");
            }
            IsPlaceable = false;
            lockPoint = value;
        }
    }
    public List<BaseHexagon> Neightbors { get; set; } = new List<BaseHexagon>();
    public Vector2Int Coordinate { get; set; }
    public bool IsRemoveStack { get; set; } = false;

    //public bool IsMoveStack { get; set; } = false;
    //public Vector3 GetWorldPosition() 
    //{ 
    //    return transform.position;
    //}

    public void SetHeightLight()
    {
        if(Renderer.material != hightlightMaterial) Renderer.material = hightlightMaterial;
    }

    public void SetOriginal()
    {
        if (Renderer.material != originalMaterial) Renderer.material = originalMaterial;
    }

    public void CheckTopStackOfHexagon()
    {
        DOVirtual.DelayedCall(0.05f, () => 
        {
            if(currentChipStack == null) return;
            if (currentChipStack.listChipBlock.Last().ChipCount >= GameManager2.Instance.MaxToRemoveBlock)
            {
                RemoveChipEffect(currentChipStack.listChipBlock.Last().ListChip, 0, () =>
                {
                    GameManager2.Instance.CheckRecallCheckSurround();
                });
                return;
            }
            else if ( currentChipStack.listChipBlock.Last().ChipCount >= GameManager2.Instance.MinToRemoveBlock)
            {
                if (!GameManager2.Instance.ListCheckBlockHexagon.Contains(this)) GameManager2.Instance.ListCheckBlockHexagon.Add(this);
                //if (GameManager2.Instance.ListCheckBlockHexagon.Contains(this)) GameManager2.Instance.ListCheckBlockHexagon.Add(this);
            }

            GameManager2.Instance.CheckRecallCheckSurround();
        });
    }

    public void RemoveChipEffect(List<GameObject> chips, int index, Action completed = null)
    {
        if (IsRemoveStack == false)
        {
            IsRemoveStack = true;
            GameManager2.Instance.CurrentPoint += chips.Count;
        }

        if (index > chips.Count - 1)
        {
            // set isRemove first
            IsRemoveStack = false;
            currentChipStack.RemoveTopChipBlock();
            completed?.Invoke();
            return;
        }

        if (chips[chips.Count - index - 1] == null)
        {
            IsRemoveStack = false;
            currentChipStack.RemoveTopChipBlock();
            completed?.Invoke();
            return;
        }
        chips[chips.Count - index - 1].transform.DOScale(Vector3.zero, GameManager2.Instance.TimeRemoveChip).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            RemoveChipEffect(chips, index + 1, completed);
        });
    }

    public void ResetHexagon()
    {
        Destroy(currentChipStack.gameObject);
        currentChipStack = null;
        IsPlaceable = true;
    }

    public void UnlockHexagon()
    {
        lockText.gameObject.SetActive(false);
        IsPlaceable = true;
    }
    public List<BaseHexagon> CheckSecondType()
    {
        List<BaseHexagon> listSecondType = new List<BaseHexagon>();

        for (int i = 0; i < Neightbors.Count; i++)
        {
            ChipStack check = Neightbors[i].currentChipStack;
            if(check != null)
            {
                if (currentChipStack.GetSecondChipType() == check.GetTopType())
                {
                    listSecondType.Add(Neightbors[i]);
                }
            }
        }

        if(listSecondType.Count >0) return listSecondType;
        else return null;
    }

}

