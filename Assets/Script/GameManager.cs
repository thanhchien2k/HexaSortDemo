using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private BaseHexagon[,] maps;
    [SerializeField] BaseHexagon hexagonPrefabs;
    public Vector3 offset {  get; private set; }
    [SerializeField] private Transform mapRoot;
    [SerializeField] ChipStackSpawner spawner;
    public List<ChipType> listType = new List<ChipType>();
    public List<GameObject> chips = new List<GameObject>();
    public List<HexagonCoordinate> neightborCoordinates;
    public List<ChipStack> StackToCheck = new List<ChipStack>();
    [SerializeField] Vector2Int size;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (hexagonPrefabs != null)
        {
            offset = hexagonPrefabs.GetComponent<Renderer>().bounds.size; // offset "0.42, 0.04, 0.38"
        }
    }

    private void Start()
    {
        maps = new BaseHexagon[4, 4];
        CreateMap();
    }

    private void CreateMap()
    {
        if (hexagonPrefabs == null || mapRoot == null) return;
        Vector3 worldPosition;
        if(size.x == 0 || size.y == 0)
        {
            Debug.Log("size is wrong!");
            return;
        }
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y< size.y; y++)
            {
                float zPos = y * offset.z;
                if (x % 2 == 1)
                {
                    zPos += offset.z / 2f;
                }
                worldPosition = new Vector3(x * offset.x * 3 / 4, 0, zPos);
                maps[x, y] = Instantiate(hexagonPrefabs, worldPosition, Quaternion.identity, mapRoot);
                maps[x, y].gameObject.name = "Cell " + x + "-" + y;
                maps[x, y].Coordinate = new Vector2Int(x, y);            }
        }
        //mapRoot.position = new Vector3(mapRoot.position.x, mapRoot.position.y, mapRoot.position.z - worldCenterPoint.z);
    }

    public void CheckSurroundingHexagon(BaseHexagon hexagon)
    {
        if(neightborCoordinates.Count <= 0)
        {
            Debug.Log("neightbor coordinate is null");
        }

        SetNeightborHexagon(hexagon);

        // get list stack neightbor have same top type
        ChipStack priority = null;

        foreach (var temp in hexagon.neightbors)
        {
            ChipStack check = temp.currentChipStack;
            if ( check != null)
            {
                if(check.GetTopType() == hexagon.currentChipStack.GetTopType())
                {
                    if (!StackToCheck.Contains(check))
                    {
                        if (check.IsOneTypeStack()) priority = check;
                        StackToCheck.Add(check);
                        Debug.Log("Check");
                    }
                }
            }
        }

        if (StackToCheck.Count != 0)
        {
            if (priority == null) priority = StackToCheck.First();
            if (priority != null)
            {
                //for(int i = 0; i < StackToCheck.Count; i++)
                //{
                //    if (StackToCheck[i] != priority)
                //    {

                //    }
                //}

                hexagon.currentChipStack.transform.SetParent(priority.transform);
                hexagon.currentChipStack.transform.position = priority.transform.position;
            }
        }


        // check spawn chip stack
        if (spawner.CheckSpawnPosIsEmpty())
        {
            spawner.SpawnChipStackInPos();
        }

    }

    private void MoveChip()
    {

    }

    private void SetNeightborHexagon(BaseHexagon hexagon)
    {
        if (hexagon.neightbors.Count <= 0)
        {
            Vector2Int[] check = neightborCoordinates[hexagon.Coordinate.x % 2].coordinate;
            for (int i = 0; i < check.Length; i++)
            {
                Vector2Int checkCoordinate = check[i] + hexagon.Coordinate;
                //Debug.Log(checkCoordinate);
                if ((checkCoordinate.x >= 0 && checkCoordinate.x < size.x) && (checkCoordinate.y >= 0 && checkCoordinate.y < size.y))
                {
                    if (maps[checkCoordinate.x, checkCoordinate.y] != null)
                    {
                        hexagon.neightbors.Add(maps[checkCoordinate.x, checkCoordinate.y]);
                    }
                }
            }
        }
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
    public List<GameObject> listChip;

    public ChipBlock(ChipType chipType, int chipCount, List<GameObject> listChip = null)
    {
        ChipType = chipType;
        ChipCount = chipCount;
        if(listChip == null)
        {
            this.listChip = new();
        }
        else
        {
            this.listChip = listChip;
        }
    }
}
[System.Serializable]

public class HexagonCoordinate
{
    public Vector2Int[] coordinate;
}