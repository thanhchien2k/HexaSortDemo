using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private Dictionary<Vector2Int, BaseHexagon> maps;
    [SerializeField] BaseHexagon hexagonPrefabs;
    [SerializeField] Vector3 offset;
    [SerializeField] private Transform mapRoot;
    Vector3 worldCenterPoint;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if(hexagonPrefabs != null)
        {
           offset = hexagonPrefabs.GetComponent<Renderer>().bounds.size;
        }

        worldCenterPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, 0f));

    }

    private void Start()
    {
        CreateMap();
    }
    private void CreateMap()
    {
        if (hexagonPrefabs == null || mapRoot == null) return;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Instantiate(hexagonPrefabs, new Vector3(i * (offset.x + 0.1f), 0, j * (offset.z + 0.1f)), Quaternion.identity, mapRoot);
            }
        }
        //mapRoot.position = new Vector3(mapRoot.position.x, mapRoot.position.y, mapRoot.position.z - worldCenterPoint.z);
    }
}
