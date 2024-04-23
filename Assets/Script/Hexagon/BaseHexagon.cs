using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseHexagon : MonoBehaviour
{
    [SerializeField] Material originalMaterial;
    [SerializeField] Material hightlightMaterial;
    [SerializeField] Renderer Renderer;
    public ChipStack currentChipStack {  get; set; }
    public bool isPlaceable = true;
    public List<BaseHexagon> neightbors { get; set; } = new List<BaseHexagon>();
    public Vector2Int Coordinate { get; set; }

    public Vector3 GetWorldPosition() 
    { 
        return transform.position;
    }

    public void SetHeightLight()
    {
        if(Renderer.material != hightlightMaterial)
        Renderer.material = hightlightMaterial;
    }

    public void SetOriginal()
    {
        if (Renderer.material != originalMaterial)

            Renderer.material = originalMaterial;
    }

    public void ReMoveChipStack()
    {
        ResetHexagon();
    }

    public void CheckChipStack()
    {
        DOVirtual.DelayedCall(0.1f, () => 
        {
            if (currentChipStack.listChipBlock.Last().ChipCount >= GameManager2.Instance.numToRemoveBlock)
            {
                currentChipStack.RemoveTopChipBlock();
            }
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
}

