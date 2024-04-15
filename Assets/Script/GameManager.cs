using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private BaseHexagon[,] maps;
    [SerializeField] BaseHexagon hexagonPrefabs;
    [SerializeField] Vector3 offset;
    [SerializeField] private Transform mapRoot;

    public Vector3 smothPos;
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
           offset = hexagonPrefabs.GetComponent<Renderer>().bounds.size; // offset "0.42, 0.04, 0.38"
        }

    }

    private void Start()
    {
        maps = new BaseHexagon[4,4];
        CreateMap();
    }

    private void CreateMap()
    {
        if (hexagonPrefabs == null || mapRoot == null) return;
        Vector3 worldPosition;
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                float zPos = y * offset.z;
                if(x % 2 == 1)
                {
                    zPos += offset.z /2f;
                }
                worldPosition = new Vector3(x * offset.x * 3/4, 0, zPos);
                maps[x, y] = Instantiate(hexagonPrefabs, worldPosition, Quaternion.identity, mapRoot);
                maps[x, y].gameObject.name = "Cell " + x + "-" + y;
            }
        }
        //mapRoot.position = new Vector3(mapRoot.position.x, mapRoot.position.y, mapRoot.position.z - worldCenterPoint.z);
    }

}
