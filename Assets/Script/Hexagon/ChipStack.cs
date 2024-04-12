
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class ChipStack : MonoBehaviour
{
    private BaseHexagon currentBaseHexagon = null;
    private Vector3 originalPosition;
    private Stack<ChipBlock> stackChipBlock;
    private Vector3 mOffset;
    private float mZCoord;
    private bool isDragging = false;

    private void Awake()
    {
        originalPosition = transform.position;
    }

    private void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(transform.position).y;
        mOffset = transform.position - GetMouseAsWorldPoint();
    }

    private void OnMouseDrag()
    {
        isDragging = true;

        Ray ray = new Ray(transform.position, -Vector3.up);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            BaseHexagon hexagon = hit.collider.GetComponent<BaseHexagon>();
            if (hexagon != null)
            {
                
            }
   
        }

        isDragging = true;
        //Vector3 newPosition = GetMouseAsWorldPoint() + mOffset;
        Vector3 newPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,1f)); 
        transform.position = newPos;
    }

    private void OnMouseUp()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, -Vector3.up);

        if(Physics.Raycast(ray, out hit))
        {
            BaseHexagon hexagon = hit.collider.GetComponent<BaseHexagon>();
            if(hexagon != null)
            {
                currentBaseHexagon = hexagon;
                SnapToHexagon(currentBaseHexagon);
                return;
            }
        }

        MoveToOriginalPosition();

    }

    private void SnapToHexagon(BaseHexagon hexagon)
    {
        transform.position = hexagon.transform.position + new Vector3(0, 0.2f, 0);
        currentBaseHexagon = hexagon;
        hexagon.GetComponent<BoxCollider>().enabled = false;
        transform.SetParent(hexagon.transform);
    }
    private Vector3 GetMouseAsWorldPoint()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
    public BaseHexagon GetCurrentHexagon()
    {
        return this.currentBaseHexagon;
    }

    public void SetCurrentHexagon(BaseHexagon hexagon)
    {
        transform.position = hexagon.transform.position;
        transform.SetParent(hexagon.transform);
    }

    public void MoveToOriginalPosition()
    {
        transform.DOMove(originalPosition, 1f).SetEase(Ease.OutExpo);
    }
}
[System.Serializable]
public enum ChipType
{
    Red,
    Yellow,
    Green,
    Blue
}
[System.Serializable]
public class ChipBlock
{
    public ChipType ChipType;
    public int ChipCount;
}