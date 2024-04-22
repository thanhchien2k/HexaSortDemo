using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance;
    private BaseHexagon[,] maps;
    [SerializeField] BaseHexagon hexagonPrefabs;
    public Vector3 offset { get; private set; }
    [SerializeField] private Transform mapRoot;
    [SerializeField] ChipStackSpawner spawner;
    public List<ChipType> listType = new List<ChipType>();
    public List<GameObject> chipsPrefabs = new List<GameObject>();
    public List<HexagonCoordinate> neightborCoordinates;
    public List<BaseHexagon> StackToCheck = new List<BaseHexagon>();
    [SerializeField] Vector2Int size;
    public int numToRemoveBlock;
    public bool isMoving = false;

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
        maps = new BaseHexagon[size.x, size.y];
        CreateMap();
    }

    private void CreateMap()
    {
        if (hexagonPrefabs == null || mapRoot == null) return;
        Vector3 worldPosition;
        if (size.x == 0 || size.y == 0)
        {
            Debug.Log("size is wrong!");
            return;
        }
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                float zPos = y * offset.z;
                if (x % 2 == 1)
                {
                    zPos += offset.z / 2f;
                }
                worldPosition = new Vector3(x * offset.x * 3 / 4, 0, zPos);
                maps[x, y] = Instantiate(hexagonPrefabs, worldPosition, Quaternion.identity, mapRoot);
                maps[x, y].gameObject.name = "Cell " + x + "-" + y;
                maps[x, y].Coordinate = new Vector2Int(x, y);
            }
        }
        mapRoot.position = new Vector3(mapRoot.position.x - size.x / 4 * offset.x, mapRoot.position.y, mapRoot.position.z);
    }

    public void CheckList(List<BaseHexagon> hexagons)
    {
        for (int i = 0; i < hexagons.Count; i++)
        {
            CheckSurroundingHexagon(hexagons[i]);
        }
    }

    public void CheckSurroundingHexagon(BaseHexagon hexagon)
    {
        isMoving = true;
        StackToCheck.RemoveAll(item => item == hexagon);
        if (neightborCoordinates.Count <= 0)
        {
            Debug.Log("neightbor coordinate is null");
        }

        if (hexagon.currentChipStack == null)
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
            if (check != null)
            {
                if (check.GetTopType() == hexagon.currentChipStack.GetTopType())
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

        if (listCheck.Count != 0 || (priorityPutOn != null && priorityPutOn != hexagon))
        {
            if (priorityPutOn == null)
            {
                for (int i = 0; i < listCheck.Count; i++)
                {
                    if (listCheck[i].neightbors.Count == 0) continue;
                    BaseHexagon checkHexagon = listCheck[i].CheckSecondType();
                    Debug.Log(checkHexagon);
                    if (checkHexagon != null)
                    {
                        // check sau co loi khi mau thu 2 trung voi no la hexagon
                        priorityPutOn = hexagon;
                        break;
                    }
                    else
                    {
                        if (hexagon.CheckSecondType() != null)
                        {
                            priorityPutOn = listCheck[i];
                            listCheck.RemoveAt(i);
                        }
                        else
                        {
                            priorityPutOn = hexagon;
                        }
                        break;
                    }
                }
            }


            if (priorityPutOn == hexagon)
            {
                MoveListBlock(listCheck, hexagon, 0);
            }
            else
            {
                if(listCheck.Count == 0)
                {
                    listCheck.Add(hexagon);
                    MoveListBlock(listCheck, priorityPutOn, 0);

                }
                else
                {
                    // dang loi o day
                    MoveListBlock(listCheck, priorityPutOn, 0, hexagon);
                }
            }

        }
        else
        {
            CheckRecallCheckSurround();
        }

        if (spawner.CheckSpawnPosIsEmpty())
        {
            spawner.SpawnChipStackInPos();
        }

    }
    private void CheckRecallCheckSurround()
    {
        if (StackToCheck.Count > 0)
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                CheckSurroundingHexagon(StackToCheck.First());
            });
        }
        else
        {
            isMoving = false;
        }
    }

    private void MoveListBlock(List<BaseHexagon> listHexagon, BaseHexagon putOnHexagon, int index, BaseHexagon medialHexagon = null)
    {
        if (index > listHexagon.Count - 1)
        {
            if (medialHexagon != null)
            {
                Debug.Log("move from hexagon to priproty");
                MoveChip(medialHexagon.currentChipStack.listChipBlock.Last().ListChip, medialHexagon ,putOnHexagon, medialHexagon.currentChipStack.GetTopPosition(), 0, () =>
                {
                    putOnHexagon.CheckChipStack();
                    CheckRecallCheckSurround();
                });
                if (!StackToCheck.Contains(medialHexagon)) StackToCheck.Add(medialHexagon);
                medialHexagon.currentChipStack.RemoveTopChipBlock();
            }
            else
            {
                CheckRecallCheckSurround();
                putOnHexagon.CheckChipStack();
            }

            return;
        }
        BaseHexagon newHexagon;
        BaseHexagon currentHexagon = listHexagon[index];

        newHexagon = medialHexagon != null ? medialHexagon : putOnHexagon;
        if (!StackToCheck.Contains(listHexagon[index])) StackToCheck.Add(listHexagon[index]);
        Vector3 beginPos = newHexagon.currentChipStack.GetTopPosition();
        ChipBlock moveChipBlock = currentHexagon.currentChipStack.listChipBlock.Last();
        newHexagon.currentChipStack.AddChipBlock(moveChipBlock);

        MoveChip(moveChipBlock.ListChip, currentHexagon, newHexagon, beginPos, 0, () =>
        {
            MoveListBlock(listHexagon, putOnHexagon, index + 1, medialHexagon);
        });
        
        currentHexagon.currentChipStack.RemoveTopChipBlock();
        Debug.Log("Remove");
    }

    private void MoveChip(List<GameObject> chips, BaseHexagon currentHexagon, BaseHexagon putOnHexagon, Vector3 position, int index, Action onComplete = null)
    {
        if (index > chips.Count - 1)
        {
            onComplete?.Invoke();
            return;
        }

        position += new Vector3(0, offset.y, 0);
        chips[index].transform.DOMove(position, 0.3f).SetEase(Ease.Linear);
        chips[index].transform.DORotate(chips[index].transform.localEulerAngles + new Vector3(0f, 0f, -180f), 0.3f, RotateMode.FastBeyond360).OnComplete(() =>
        {
            MoveChip(chips, currentHexagon, putOnHexagon, position, index + 1, onComplete);
        });
    }
    //private void MoveChipBlock(BaseHexagon currentHexagon, BaseHexagon newHexagon, bool isCheck = false)
    //{
    //    Vector3 beginPos = newHexagon.currentChipStack.GetTopPosition();
    //    ChipBlock moveChipBlock = currentHexagon.currentChipStack.listChipBlock.Last();
    //    newHexagon.currentChipStack.AddChipBlock(moveChipBlock);
    //    MoveChip(moveChipBlock.ListChip, currentHexagon, newHexagon, beginPos, 0);
    //    currentHexagon.currentChipStack.RemoveTopChipBlock();
    //}

    private void SetNeightborHexagon(BaseHexagon hexagon)
    {
        if (hexagon.neightbors.Count <= 0)
        {
            Vector2Int[] check = neightborCoordinates[hexagon.Coordinate.x % 2].coordinate;
            for (int i = 0; i < check.Length; i++)
            {
                Vector2Int checkCoordinate = check[i] + hexagon.Coordinate;
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
