using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    
    //private void OnTriggerEnter(Collider other)
    //{
    //    ChipStack tileStack = other.GetComponent<ChipStack>();
    //    if(tileStack != null)
    //    {
    //        BaseHexagon currentHexagon = tileStack.GetCurrentHexagon();
    //        if (currentHexagon != null)
    //        {
    //            tileStack.SetCurrentHexagon(this);
    //        }
    //    }
    //}

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
        if(currentChipStack.listChipBlock.Last().ChipCount >= 8)
        {
            ResetHexagon();
        }
    }

    private void ResetHexagon()
    {
        Destroy(currentChipStack.gameObject);
        currentChipStack = null;
        isPlaceable = true;
    }

    public BaseHexagon CheckSecondType()
    {
        for (int i = 0; i < neightbors.Count; i++)
        {
            ChipStack check = neightbors[i].currentChipStack;
            if(check != null)
            {
                if (currentChipStack.GetSecondChipType() == check.GetTopType())
                {
                    return neightbors[i];
                }
            }
        }
        return null;
    }
}

