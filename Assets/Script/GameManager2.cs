using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance;
    private BaseHexagon[,] maps;
    [Header("Set Map")]
    [SerializeField] BaseHexagon hexagonPrefabs;
    [SerializeField] private Transform mapRoot;
    [SerializeField] Vector2Int size;
    [Header("Config")]
    [SerializeField] ChipStackSpawner spawner;
    public List<HexagonCoordinate> neightborCoordinates;
    public int numToRemoveBlock;
    public List<ChipType> listType = new List<ChipType>();
    public List<GameObject> chipsPrefabs = new List<GameObject>();
    public Vector3 offset { get; private set; }
    public List<BaseHexagon> StackToCheck { get; set; } = new List<BaseHexagon>();
    public bool isMoving { get; set; } = false;

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
                List<BaseHexagon> checkSecondHexagon = new List<BaseHexagon>();
                checkSecondHexagon = hexagon.CheckSecondType();

                if (checkSecondHexagon != null) 
                {
                    for (int i = 0; i< listCheck.Count; i++)
                    {
                        for(int j = i; j < checkSecondHexagon.Count; j++)
                        {
                            if (listCheck[i] != checkSecondHexagon[j])
                            {
                                priorityPutOn = listCheck[i];
                                listCheck.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    priorityPutOn = hexagon;
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
            DOVirtual.DelayedCall(0.2f, () =>
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
                ChipBlock tempBlock = medialHexagon.currentChipStack.listChipBlock.Last();
                Vector3 curPos = medialHexagon.currentChipStack.GetTopPosition();
                Vector3 newPos = putOnHexagon.currentChipStack.GetTopPosition();
                putOnHexagon.currentChipStack.AddChipBlock(tempBlock);
                Vector3 rotate2 = CaculatorRotate(medialHexagon, putOnHexagon);
                MoveChip(medialHexagon.currentChipStack.listChipBlock.Last().ListChip, medialHexagon ,putOnHexagon, curPos,newPos, rotate2 , 0, () =>
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
        Vector3 startPos = currentHexagon.currentChipStack.GetTopPosition();
        Vector3 targetPos = newHexagon.currentChipStack.GetTopPosition();
        ChipBlock moveChipBlock = currentHexagon.currentChipStack.listChipBlock.Last();
        newHexagon.currentChipStack.AddChipBlock(moveChipBlock);

        Vector3 rotate = CaculatorRotate(currentHexagon, putOnHexagon);

        MoveChip(moveChipBlock.ListChip, currentHexagon, newHexagon, startPos,targetPos, rotate,0, () =>
        {
            MoveListBlock(listHexagon, putOnHexagon, index + 1, medialHexagon);
        });
        
        currentHexagon.currentChipStack.RemoveTopChipBlock();
    }

    private void MoveChip(List<GameObject> chips, BaseHexagon currentHexagon, BaseHexagon putOnHexagon, Vector3 startPos,Vector3 targetPos, Vector3 rotate, int index, Action onComplete = null)
    {
        if (index > chips.Count - 1)
        {
            onComplete?.Invoke();
            return;
        }

        targetPos += new Vector3(0, offset.y, 0);
        float xMedial = (targetPos.x + startPos.x) / 2;
        float zMedial = (targetPos.z + startPos.z) / 2;
        float ymedial = (targetPos.y > startPos.y ? targetPos.y : startPos.y) + offset.y * 3;
        Vector3 medialPos = new Vector3(xMedial, ymedial, zMedial);

        if (index == chips.Count - 1)
        {
            chips[index].transform.DOMove(medialPos, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                chips[index].transform.DOMove(targetPos, 0.2f).SetEase(Ease.Linear);
            });
            chips[index].transform.DORotate(chips[index].transform.localEulerAngles + rotate, 0.4f, RotateMode.FastBeyond360).OnComplete(() =>
            {
                MoveChip(chips, currentHexagon, putOnHexagon, startPos, targetPos, rotate, index + 1, onComplete);
            });
        }
        else
        {
            chips[index].transform.DOMove(medialPos, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                chips[index].transform.DOMove(targetPos, 0.2f).SetEase(Ease.Linear);
            });
            chips[index].transform.DORotate(chips[index].transform.localEulerAngles + rotate, 0.4f, RotateMode.FastBeyond360);
            DOVirtual.DelayedCall(0.15f, () => { MoveChip(chips, currentHexagon, putOnHexagon, startPos, targetPos, rotate, index + 1, onComplete); });
        }
    }

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

    public Vector3 CaculatorRotate(BaseHexagon currentHexagon, BaseHexagon putOnHexagon)
    {
        float xRotate = 0f;
        float zRotate = 0f;
        if (currentHexagon.Coordinate.x == putOnHexagon.Coordinate.x)
        {
            if (currentHexagon.Coordinate.y > putOnHexagon.Coordinate.y)
            {
                xRotate = -180f;
            }
            else
            {
                xRotate = 180f;
            }
        }
        else
        {
            if (currentHexagon.Coordinate.x > putOnHexagon.Coordinate.x)
            {
                zRotate = 180f;
            }
            else
            {
                zRotate = -180f;
            }
        }

        return new Vector3(xRotate, 0f, zRotate);
    }

}
