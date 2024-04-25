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
    public bool StackIsRemove { get; set; } = false;
    public bool StackIsMoving {get; set; } = false;
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

    public void ReMoveChipStack()
    {
        ResetHexagon();
    }

    public void CheckTopStackOfHexagon()
    {
        DOVirtual.DelayedCall(0.1f, () => 
        {
            if(currentChipStack == null) return;
            if (currentChipStack.listChipBlock.Last().ChipCount >= GameManager2.Instance.numToRemoveBlock)
            {
                //RemoveChipEffect(currentChipStack.listChipBlock.Last().ListChip, 0, () =>
                //{
                //    currentChipStack.RemoveTopChipBlock();
                //    GameManager2.Instance.CheckRecallCheckSurround();
                //});
                if(!GameManager2.Instance.listCheckBlockHexagon.Contains(this)) GameManager2.Instance.listCheckBlockHexagon.Add(this);
            }
            
            GameManager2.Instance.CheckRecallCheckSurround();
        });

    }

    public void RemoveChipEffect(List<GameObject> chips, int index, Action completed = null)
    {
        if (StackIsRemove == false)
        {
            StackIsRemove = true;
        }
        if (index > chips.Count - 1)
        {
            currentChipStack.RemoveTopChipBlock();
            completed?.Invoke();
            StackIsRemove = false;
            return;
        }

        chips[chips.Count - index - 1].transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            RemoveChipEffect(chips, index + 1, completed);
        });
    }

    private void ResetHexagon()
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

