using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRotate : MonoBehaviour
{
   
    void Update()
    {
        //transform.localPosition = new Vector3(Mathf.Cos(Time.time), 0, Mathf.Sin(Time.time));
        transform.RotateAround(Vector3.zero, Vector3.up, 5 * Time.deltaTime);
    }
}
