using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private BaseHexagon[,] maps;
    [SerializeField] BaseHexagon hexagonPrefabs;
    public Vector3 offset {  get; private set; }
    [SerializeField] private Transform mapRoot;
    [SerializeField] ChipStackSpawner spawner;
    public List<ChipType> listType = new List<ChipType>();
    public List<GameObject> chipsPrefabs = new List<GameObject>();
    public List<HexagonCoordinate> neightborCoordinates;
    public List<BaseHexagon> StackToCheck = new List<BaseHexagon>();
    [SerializeField] Vector2Int size;
    public bool isMoveBlock = false;
    int addCount;

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
        mapRoot.position = new Vector3(mapRoot.position.x - size.x/4 * offset.x, mapRoot.position.y, mapRoot.position.z);
    }

    public void CheckList(List<BaseHexagon> hexagons)
    {
        for(int i = 0; i < hexagons.Count; i++)
        {
            CheckSurroundingHexagon(hexagons[i]);
        }
    }

    public void CheckSurroundingHexagon(BaseHexagon hexagon)
    {
        Debug.Log("Check");
        Debug.Log(hexagon);
        StackToCheck.RemoveAll(item => item == hexagon);
        if (neightborCoordinates.Count <= 0)
        {
            Debug.Log("neightbor coordinate is null");
        }

        if(hexagon.currentChipStack == null)
        {
            CheckRecallCheckSurround();
            return;
        }
        // Search neightbor for hexagon 
        SetNeightborHexagon(hexagon);


        // get list stack neightbor have same top type
        BaseHexagon priorityPutOn = null;
        List<BaseHexagon> listCheck = new List<BaseHexagon>();

        if (hexagon.currentChipStack.IsOneTypeStack())
        {
            priorityPutOn = hexagon;
        }

        foreach (var temp in hexagon.neightbors)
        {
            ChipStack check = temp.currentChipStack;
            if ( check != null)
            {
                if(check.GetTopType() == hexagon.currentChipStack.GetTopType())
                {
                    if (!listCheck.Contains(temp))
                    {
                        if (check.IsOneTypeStack() && priorityPutOn == null)
                        {
                            priorityPutOn = temp;
                        }
                        else
                        {
                            listCheck.Add(temp);
                        }
                    }
                }
            }
        }

        if (listCheck.Count != 0 || priorityPutOn != null)
        {
            if(priorityPutOn == null)
            {
                for (int i = 0; i < listCheck.Count; i++)
                {
                    
                    MoveChip(listCheck[i], hexagon);
                    if (!StackToCheck.Contains(listCheck[i])) StackToCheck.Add(listCheck[i]);
                }

                DOVirtual.DelayedCall(1f, () =>
                {
                    hexagon.CheckChipStack();
                });
            }
            else
            {
                if(priorityPutOn == hexagon)
                {
                    for (int i = 0; i < listCheck.Count; i++)
                    {
                        MoveChip(listCheck[i], hexagon);
                        if (!StackToCheck.Contains(listCheck[i])) StackToCheck.Add(listCheck[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < listCheck.Count; i++)
                    {
                       if(priorityPutOn != listCheck[i])
                        {
                            MoveChip(listCheck[i], hexagon);
                            if (!StackToCheck.Contains(listCheck[i])) StackToCheck.Add(listCheck[i]);
                        }
                    }

                    if (!StackToCheck.Contains(hexagon)) StackToCheck.Add(hexagon);
                    MoveChip(hexagon, priorityPutOn);
                }

                DOVirtual.DelayedCall(1f, () =>
                {
                    priorityPutOn.CheckChipStack();
                });
            }

        }

        CheckRecallCheckSurround();

        //if(StackToCheck.Count > 10)
        //{
        //    foreach (var item in StackToCheck)
        //    {
        //        Debug.Log(item.name);
        //    }
        //}

        if (spawner.CheckSpawnPosIsEmpty())
        {
            spawner.SpawnChipStackInPos();
        }

    }
    private void CheckRecallCheckSurround()
    {
        if (StackToCheck.Count > 0)
        {

            DOVirtual.DelayedCall(1f, () =>
            {
                foreach (var item in StackToCheck) { { Debug.Log(item.name); } }
                Debug.Log(StackToCheck.First() + " check");
                CheckSurroundingHexagon(StackToCheck.First());
            });
        }
    }
    //public void MoveListChipBlock(List<BaseHexagon> listHexagon, BaseHexagon newHexagon)
    //{
    //    isMoveBlock = true;
    //    int nums = 0;
    //    MoveChip(listHexagon[0], newHexagon);
    //    nums++;
    //    Debug.Log("start");
    //    DOTween.Sequence().SetDelay(1f).OnComplete(() =>
    //    {
    //        Debug.Log("tween");
    //    });
    //}
    // move chip from to currentHexagon to newHexagon
    private void MoveChip(BaseHexagon currentHexagon, BaseHexagon newHexagon)
    {
        Vector3 beginPos = newHexagon.currentChipStack.GetTopPosition();
        ChipBlock moveChipBlock = currentHexagon.currentChipStack.listChipBlock.Last();
        newHexagon.currentChipStack.AddChipBlock(moveChipBlock);

        for (int i = 0; i< moveChipBlock.ChipCount; i++)
        {
            beginPos += new Vector3(0, 0.04f, 0);
            moveChipBlock.ListChip[i].transform.DOMove(beginPos, 0.3f).SetEase(Ease.Linear);
        }
            currentHexagon.currentChipStack.RemoveTopChipBlock();
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

    public void RemoveFromStack(BaseHexagon baseHexagon)
    {
        StackToCheck.RemoveAll(x => x == baseHexagon);
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
    public Transform Block;
    public ChipType ChipType;
    public int ChipCount;
    public List<GameObject> ListChip;

    public ChipBlock(Transform block, ChipType chipType, int chipCount, List<GameObject> listChip = null)
    {
        this.Block = block;
        this.ChipType = chipType;
        this.ChipCount = chipCount;
        if(listChip == null)
        {
            this.ListChip = new();
        }
        else
        {
            this.ListChip = listChip;
        }
    }
}
[System.Serializable]

public class HexagonCoordinate
{
    public Vector2Int[] coordinate;
}