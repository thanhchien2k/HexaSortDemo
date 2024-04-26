
using UnityEngine;

public class Grabber : MonoBehaviour
{
    private ChipStack sellectGO;

    void Update()
    {
        //if (GameManager2.Instance.isMoving) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (sellectGO == null)
            {
                RaycastHit hit = CastRay();

                if(hit.collider != null)
                {
                    if (!hit.collider.gameObject.CompareTag("Drag")) return;

                    sellectGO = hit.collider.GetComponent<ChipStack>();
                    Cursor.visible = false;
                }
            }
        }

        if(sellectGO != null)
        {
            Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(sellectGO.gameObject.transform.position).z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(position);
            sellectGO.transform.position = new Vector3(worldPos.x, 0.2f, worldPos.z);
            sellectGO.GetBaseHexagon();
        }

        if (Input.GetMouseButtonUp(0))
        {
             if(sellectGO != null)
            {
                if(sellectGO.GetCurrentHexagon() != null)
                {
                    sellectGO.PutOnHexagonBase();
                }
                else
                {
                    sellectGO.MoveToOriginalPosition();
                }

                sellectGO = null;
                Cursor.visible = true;
            }
        }
        
    }

    private RaycastHit CastRay()
    {
        Vector3 screenMousePosFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane);
        Vector3 screenMousePosNear = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);

        Vector3 worldMousePosFar = Camera.main.ScreenToWorldPoint(screenMousePosFar);
        Vector3 worldMousePosNear = Camera.main.ScreenToWorldPoint(screenMousePosNear);

        RaycastHit hit;
        Physics.Raycast(worldMousePosNear, worldMousePosFar - worldMousePosNear, out hit);
        return hit;
    }
}