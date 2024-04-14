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
    private Plane plane;
    Vector3 mousePos;
    Vector3 smothPos;
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

    }

    private void Start()
    {
        plane = new Plane(Vector3.up,transform.position);
        maps = new();
        CreateMap();
    }

    void GetMousePosOnGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(plane.Raycast(ray,out var enter))
        {
            mousePos = ray.GetPoint(enter);
            smothPos = mousePos;
            mousePos.y = 0;
            mousePos = Vector3Int.RoundToInt(mousePos);
            
        }
    }
    private void CreateMap()
    {
        if (hexagonPrefabs == null || mapRoot == null) return;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                maps.Add(new Vector2Int(i,j), Instantiate(hexagonPrefabs, new Vector3(i , 0, j), Quaternion.identity));
            }
        }
        //mapRoot.position = new Vector3(mapRoot.position.x, mapRoot.position.y, mapRoot.position.z - worldCenterPoint.z);
    }

    private void Update()
    {
        GetMousePosOnGrid();
    }
}
