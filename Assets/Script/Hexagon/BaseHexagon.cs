using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseHexagon : MonoBehaviour
{
    [SerializeField] Material originalMaterial;
    [SerializeField] Material hightlightMaterial;
    [SerializeField] Renderer Renderer;
    public ChipStack currentChipStack { get; set; }
    public bool isPlaceable = true;
    public List<BaseHexagon> neightbors { get; set; } = new List<BaseHexagon>();
    public Vector2Int Coordinate { get; set; }
    public bool IsRemoveStack  = false;
    public bool IsMoveStack  = false;
    public Vector3 GetWorldPosition() 
    { 
        return transform.position;
    }

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
        DOVirtual.DelayedCall(0.1f, () => 
        {
            if(currentChipStack == null) return;
            if (currentChipStack.listChipBlock.Last().ChipCount >= GameManager2.Instance.MaxToRemoveBlock)
            {
                if(!GameManager2.Instance.ListCheckSurroundHexagon.Contains(this)) GameManager2.Instance.ListCheckSurroundHexagon.Add(this);
                RemoveChipEffect(currentChipStack.listChipBlock.Last().ListChip, 0, () =>
                {
                    GameManager2.Instance.CheckRecallCheckSurround();
                });
                return;
            }
            else if ( currentChipStack.listChipBlock.Last().ChipCount >= GameManager2.Instance.MinToRemoveBlock)
            {
                if (!GameManager2.Instance.ListCheckBlockHexagon.Contains(this)) GameManager2.Instance.ListCheckBlockHexagon.Add(this);

            }

            GameManager2.Instance.CheckRecallCheckSurround();
        });

    }

    public void RemoveChipEffect(List<GameObject> chips, int index, Action completed = null)
    {
        if (IsRemoveStack == false)
        {
            IsRemoveStack = true;
        }
        if (index > chips.Count - 1)
        {
            // set isRemove first
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
        isPlaceable = true;
    }

    public List<BaseHexagon> CheckSecondType()
    {
        List<BaseHexagon> listSecondType = new List<BaseHexagon>();

        for (int i = 0; i < neightbors.Count; i++)
        {
            ChipStack check = neightbors[i].currentChipStack;
            if(check != null)
            {
                if (currentChipStack.GetSecondChipType() == check.GetTopType())
                {
                    listSecondType.Add(neightbors[i]);
                }
            }
        }

        if(listSecondType.Count >0) return listSecondType;
        else return null;
    }

    public bool IsNeightbor(BaseHexagon baseHexagon)
    {
        if(neightbors.Count == 0) return false;
        if(neightbors.Any(item => item == baseHexagon)) return true;
        else return false;
    }
}

