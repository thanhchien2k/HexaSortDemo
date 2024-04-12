using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHexagon : MonoBehaviour
{
    [SerializeField] Material originalMaterial;

    private void OnTriggerEnter(Collider other)
    {
        ChipStack tileStack = other.GetComponent<ChipStack>();
        if(tileStack != null)
        {
            BaseHexagon currentHexagon = tileStack.GetCurrentHexagon();
            if (currentHexagon != null)
            {
                tileStack.SetCurrentHexagon(this);
            }
        }
    }
}
