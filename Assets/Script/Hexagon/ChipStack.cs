
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class ChipStack : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    [SerializeField] BoxCollider boxCollider;
    private BaseHexagon currentBaseHexagon = null;
    private Vector3 originalPosition;
    public List<ChipBlock> listChipBlock;
    public int chipCount;


    private void Awake()
    {
        listChipBlock = new List<ChipBlock>();
        originalPosition = transform.position;
    }

    public void GetBaseHexagon()
    {
        Ray ray = new Ray(transform.position, -Vector3.up);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            BaseHexagon hexagon = hit.collider.GetComponent<BaseHexagon>();
            if (hexagon != null)
            {
                if (!hexagon.isPlaceable) {
                    if (currentBaseHexagon != null)
                    {
                        currentBaseHexagon.SetOriginal();
                        currentBaseHexagon = null;
                    }
                    return;
                } 
                
                if(currentBaseHexagon == null)
                {
                    currentBaseHexagon = hexagon;
                    currentBaseHexagon.SetHeightLight();
                }
                else if(currentBaseHexagon != hexagon)
                {
                    currentBaseHexagon.SetOriginal();
                    currentBaseHexagon = hexagon;
                    currentBaseHexagon.SetHeightLight();
                }
            }
            else
            {
                if (currentBaseHexagon != null)
                {
                    currentBaseHexagon.SetOriginal();
                    currentBaseHexagon = null;
                }
            }
        }


    }

    public BaseHexagon GetCurrentHexagon()
    {
        return this.currentBaseHexagon;
    }

    public void MoveToOriginalPosition()
    {
        transform.DOMove(originalPosition, 1f).SetEase(Ease.OutExpo);
    }

    public void DrawRay()
    {
        Ray ray = new Ray(transform.position, -Vector3.up);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
    }

    public void PutOnHexagonBase()
    {
        if(currentBaseHexagon != null)
        {
            transform.SetParent(currentBaseHexagon.transform);
            transform.position = currentBaseHexagon.transform.position + offset;
            currentBaseHexagon.SetOriginal();
            currentBaseHexagon.isPlaceable = false;
            currentBaseHexagon.currentChipStack = this;
            if(boxCollider != null)
            {
                boxCollider.enabled = false;
            }
            else
            {
                Debug.Log("Collider is null" + gameObject);
            }

        }

        GameManager.Instance.CheckSurroundingHexagon(currentBaseHexagon);
    }

    public void SetHeightCollider(float size)
    {
        if (boxCollider != null)
        {
            boxCollider.size = boxCollider.size + Vector3.up * size;
        }
    }

    public ChipType GetTopType()
    {
        return listChipBlock[listChipBlock.Count - 1].ChipType;
    }

    public bool IsOneTypeStack()
    {
        return listChipBlock.Count == 1;
    }
}
