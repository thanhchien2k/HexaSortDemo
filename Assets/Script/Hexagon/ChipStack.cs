using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChipStack : MonoBehaviour
{
    [SerializeField] BoxCollider boxCollider;
    private BaseHexagon currentBaseHexagon = null;
    private Vector3 originalPosition;
    public List<ChipBlock> listChipBlock;


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

    public void PutOnHexagonBase()
    {
        if(currentBaseHexagon != null)
        {
            transform.SetParent(currentBaseHexagon.transform);
            transform.position = currentBaseHexagon.transform.position + (GameManager2.Instance.offset.y * Vector3.up);
            currentBaseHexagon.SetOriginal();
            currentBaseHexagon.isPlaceable = false;
            currentBaseHexagon.currentChipStack = this;

            if(boxCollider != null)
            {
                boxCollider.enabled = false;
            }
        }

        if (GameManager2.Instance.isMoving == true)
        {
            GameManager2.Instance.ListCheckSurroundHexagon.Add(currentBaseHexagon);
            GameManager2.Instance.listCheckBlockHexagon.RemoveAll(x => x == currentBaseHexagon);

        }
        else
        {
            DOVirtual.DelayedCall(0.05f, () => GameManager2.Instance.CheckSurroundingHexagon(currentBaseHexagon));
        }

        //DOVirtual.DelayedCall(0.05f, () => GameManager2.Instance.CheckSurroundingHexagon(currentBaseHexagon));
        //GameManager2.Instance.CheckSurroundingHexagon(currentBaseHexagon);

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
        return listChipBlock.Last().ChipType;
    }

    public bool IsOneTypeStack()
    {
        return listChipBlock.Count == 1;
    }

    public ChipType GetSecondChipType()
    {
        return listChipBlock[listChipBlock.Count - 2].ChipType;
    }

    public Vector3 GetTopPosition()
    {
        int allChip = 0;
        for (int i = 0; i < listChipBlock.Count; i++)
        {
            allChip += listChipBlock[i].ChipCount;
        }
        return transform.position + (Vector3.up * GameManager2.Instance.offset.y * (allChip - 1));
    }

    public void AddChipBlock(ChipBlock chipBlock)
    {
        if (chipBlock.ListChip.Count == 0) Debug.Log("0 chip move");
        ChipBlock topBlock = listChipBlock.Last();
        chipBlock.ListChip.Reverse();
        topBlock.ListChip.AddRange(chipBlock.ListChip);
        topBlock.ChipCount += chipBlock.ChipCount;
        chipBlock.ListChip.ForEach(x => x.transform.parent = topBlock.Block);
    }

    public void RemoveTopChipBlock()
    {
        if (currentBaseHexagon == null) return;

        if (listChipBlock.Count <= 1)
        {
            currentBaseHexagon.ReMoveChipStack();
            currentBaseHexagon = null;
        }
        else
        {
            Destroy(listChipBlock.Last().Block.gameObject);
            listChipBlock.RemoveAt(listChipBlock.Count - 1);

            if (!GameManager2.Instance.ListCheckSurroundHexagon.Contains(currentBaseHexagon))
            {
                GameManager2.Instance.ListCheckSurroundHexagon.Add(currentBaseHexagon);
            }
        }

        GameManager2.Instance.listCheckBlockHexagon.RemoveAll(x => x == currentBaseHexagon);

        if (GameManager2.Instance.isMoving == false)
        {
            if (GameManager2.Instance.ListCheckSurroundHexagon.Count > 0 || GameManager2.Instance.listCheckBlockHexagon.Count > 0)
            {
                GameManager2.Instance.CheckRecallCheckSurround();
            }
        }

    }

}
