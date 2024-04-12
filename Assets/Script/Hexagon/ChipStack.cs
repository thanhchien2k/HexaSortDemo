using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipStack : MonoBehaviour
{
    private BaseHexagon baseHexagon = null;
    private Vector3 originalPosition;
    private Stack<ChipBlock> stackChipBlock;
    private float mZCoord;
    private Vector3 mOffset;
    private bool isDragging = false;

    private void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(transform.position).y;
        mOffset = transform.position - GetMouseAsWorldPoint();
    }

    private void OnMouseDrag()
    {
        //Ray ray = new Ray(transform.position, -Vector3.up);
        isDragging = true;
        Vector3 newPosition = GetMouseAsWorldPoint() + mOffset;

        float distanceFromInitialTouch = Mathf.Abs(newPosition.z - transform.position.z);

        AnimationCurve lerpCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 2f);

        float lerpFactor = lerpCurve.Evaluate(Mathf.Clamp01(distanceFromInitialTouch / 10f)); 

        newPosition.z = Mathf.Lerp(transform.position.z, newPosition.z, lerpFactor);

        transform.position = new Vector3(newPosition.x, 5, newPosition.z);
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mZCoord;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
    public BaseHexagon GetCurrentHexagon()
    {
        return this.baseHexagon;
    }

    public void SetCurrentHexagon(BaseHexagon hexagon)
    {
        transform.position = hexagon.transform.position;
        transform.SetParent(hexagon.transform);
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