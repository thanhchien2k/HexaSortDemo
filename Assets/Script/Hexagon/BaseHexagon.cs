using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHexagon : MonoBehaviour
{
    [SerializeField] Material originalMaterial;
    [SerializeField] Material hightlightMaterial;
    [SerializeField] Renderer Renderer;
    public bool isPlaceable = true;
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
}
