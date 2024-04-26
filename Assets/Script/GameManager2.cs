using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance;
    private BaseHexagon[,] maps;

    [Header("Map Config")]
    [SerializeField] BaseHexagon hexagonPrefabs;
    [SerializeField] private Transform mapRoot;
    [SerializeField] Vector2Int size;

    [Header("Config")]
    [SerializeField] ChipStackSpawner spawner;
    [SerializeField] float timeChipMove;
    [SerializeField] float timeDelayChipMove;
    public float TimeRemoveChip;
    public int MinToRemoveBlock;
    public int MaxToRemoveBlock;
    public List<ChipType> ListType = new List<ChipType>();
    public List<GameObject> ChipsPrefabs = new List<GameObject>();
    public List<HexagonCoordinate> NeightborCoordinates;

    public Vector3 Offset { get; private set; }
    public List<BaseHexagon> ListCheckSurroundHexagon { get; set; } = new List<BaseHexagon>();
    public List<BaseHexagon> ListCheckBlockHexagon { get; set; } = new List<BaseHexagon> { };
    public bool IsMoving { get; set; } = false;

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
            Offset = hexagonPrefabs.GetComponent<Renderer>().bounds.size;
        }
    }

    private void Start()
    {
        maps = new BaseHexagon[size.x, size.y];
        CreateMap();
        //transform.DOLocalRotate(transform.localRotation.eulerAngles +  new Vector3(0, 120f, 0), 2f, RotateMode.Fast);
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
                float zPos = y * Offset.z;
                if (x % 2 == 1)
                {
                    zPos += Offset.z / 2f;
                }
                worldPosition = new Vector3(x * Offset.x * 3 / 4, 0, zPos);
                maps[x, y] = Instantiate(hexagonPrefabs, worldPosition, Quaternion.identity, mapRoot);
                maps[x, y].gameObject.name = "Cell " + x + "-" + y;
                maps[x, y].Coordinate = new Vector2Int(x, y);
            }
        }

        mapRoot.position = new Vector3(mapRoot.position.x - size.x / 4 * Offset.x, mapRoot.position.y, mapRoot.position.z);
    }

    public void CheckSurroundingHexagon(BaseHexagon hexagon)
    {
        //Debug.Log("check : " + hexagon);
        IsMoving = true;

        if (NeightborCoordinates.Count <= 0)
        {
            Debug.Log("neightbor coordinate is null");
        }

        ListCheckSurroundHexagon.RemoveAll(x => x == hexagon);

        if (hexagon.currentChipStack == null || hexagon == null)
        {
            ListCheckBlockHexagon.RemoveAll(x => x == hexagon);
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
            if( temp.IsRemoveStack) continue;

            ChipStack check = temp.currentChipStack;
            if (check != null)
            {
                if (check.GetTopType() == hexagon.currentChipStack.GetTopType())
                {
                    if (temp.IsMoveStack)
                    {
                        if (!ListCheckSurroundHexagon.Contains(hexagon)) ListCheckSurroundHexagon.Add(hexagon);
                        return;
                    }
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

        float timeDelay = 0f;

        if (listCheck.Count != 0 || (priorityPutOn != null && priorityPutOn != hexagon))
        {
            // move truoc cac hexagon co mau dinh trung voi cac hexagon hang xom cung mau dinh voi hexagon dang xet 
            if (listCheck.Count > 0)
            {
                for (int i = 0; i < listCheck.Count; i++)
                {
                    foreach (var temp in listCheck[i].neightbors)
                    {
                        if (temp == hexagon || temp == priorityPutOn || temp.IsRemoveStack || temp.IsMoveStack) continue;
                        ChipStack check = temp.currentChipStack;
                        if (check != null)
                        {
                            if (check.GetTopType() == hexagon.currentChipStack.GetTopType())
                            {
                                if (!listCheck.Contains(temp))
                                {
                                    MedialMoveChip(temp, listCheck[i]);
                                    timeDelay += 1f;
                                }
                            }
                        }
                    }
                }
            }

            // tim kiem hexagon de ci chuyen chip den
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
                            if (listCheck[i] != checkSecondHexagon[j] && !checkSecondHexagon[j].IsRemoveStack && !checkSecondHexagon[j].IsMoveStack)
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

            if (priorityPutOn.IsMoveStack)
            {
                Debug.Log("return check");
                if (!ListCheckSurroundHexagon.Contains(priorityPutOn)) ListCheckSurroundHexagon.Add(priorityPutOn);
                return;
            }

            if (priorityPutOn == hexagon)
            {
                DOVirtual.DelayedCall(timeDelay, () => { MoveListBlock(listCheck, hexagon, 0); }); 
            }
            else
            {
                DOVirtual.DelayedCall(timeDelay, () => { MoveListBlock(listCheck, priorityPutOn, 0, hexagon); });  
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
    public void CheckRecallCheckSurround()
    {
        if (ListCheckSurroundHexagon.Count > 0)
        {
  
            CheckSurroundingHexagon(ListCheckSurroundHexagon.Last());
        }
        else
        {
            // kiem tra removelist neu no khong rong thi se check stack tren do
            if(ListCheckBlockHexagon.Count > 0)
            {
                //Debug.Log(listCheckBlockHexagon.First());

                RemoveListTopStack(ListCheckBlockHexagon, 0);
            }
            else
            {
                IsMoving = false;
            }
        }
    }

    private void RemoveListTopStack(List<BaseHexagon> hexagons, int index)
    {
        if (index > hexagons.Count - 1)
        {
            ListCheckBlockHexagon.Clear();
            CheckRecallCheckSurround();
            return;
        }

        //if (hexagons[index] == null || hexagons[index].currentChipStack == null)
        //{
        //    RemoveListTopStack(hexagons, index + 1);
        //}
        Debug.Log(hexagons[index].name + " Remove");

        if (hexagons[index].currentChipStack == null)
        {
            RemoveListTopStack(hexagons, index + 1);
        }

        //Debug.Log(hexagons[index]);
        //Debug.Log(hexagons[index].currentChipStack);
        //Debug.Log(hexagons[index].currentChipStack.listChipBlock.Last());
        //Debug.Log(hexagons[index].currentChipStack.listChipBlock.Last().ListChip);

        hexagons[index].RemoveChipEffect(hexagons[index].currentChipStack.listChipBlock.Last().ListChip, 0);
        RemoveListTopStack(hexagons, index + 1);
        
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
                medialHexagon.IsMoveStack = true;
                putOnHexagon.IsMoveStack = true;

                MoveListChip(medialHexagon.currentChipStack.listChipBlock.Last().ListChip, medialHexagon ,putOnHexagon, curPos,newPos, rotate2 , 0, () =>
                {
                    medialHexagon.IsMoveStack = false;
                    putOnHexagon.IsMoveStack = false;
                    putOnHexagon.CheckTopStackOfHexagon();
                });

                if (!ListCheckSurroundHexagon.Contains(medialHexagon)) ListCheckSurroundHexagon.Add(medialHexagon);
                ListCheckBlockHexagon.RemoveAll(x => x == medialHexagon);

                medialHexagon.currentChipStack.RemoveTopChipBlock();
            }
            else
            {
                putOnHexagon.CheckTopStackOfHexagon();
            }

            return;
        }

        BaseHexagon newHexagon;
        BaseHexagon currentHexagon = listHexagon[index];

        if (!ListCheckSurroundHexagon.Contains(listHexagon[index]))
        {
            ListCheckSurroundHexagon.Add(listHexagon[index]);
        }
        else
        {
            MoveListBlock(listHexagon, putOnHexagon, index + 1, medialHexagon);
            return;
        }

        ListCheckBlockHexagon.RemoveAll(x => x == listHexagon[index]);

        newHexagon = medialHexagon != null ? medialHexagon : putOnHexagon;
        Vector3 startPos = currentHexagon.currentChipStack.GetTopPosition();
        Vector3 targetPos = newHexagon.currentChipStack.GetTopPosition();
        ChipBlock moveChipBlock = currentHexagon.currentChipStack.listChipBlock.Last();
        newHexagon.currentChipStack.AddChipBlock(moveChipBlock);
        Debug.Log("is moving");
        currentHexagon.IsMoveStack = true;
        putOnHexagon.IsMoveStack = true;


        Vector3 rotate = CaculatorRotate(currentHexagon, putOnHexagon);

        MoveListChip(moveChipBlock.ListChip, currentHexagon, newHexagon, startPos,targetPos, rotate,0, () =>
        {
            Debug.Log("stop moving");
            currentHexagon.IsMoveStack = false;
            putOnHexagon.IsMoveStack = false;
            MoveListBlock(listHexagon, putOnHexagon, index + 1, medialHexagon);
        });
        
        currentHexagon.currentChipStack.RemoveTopChipBlock();
    }

    private void MoveListChip(List<GameObject> chips, BaseHexagon currentHexagon, BaseHexagon putOnHexagon, Vector3 startPos, Vector3 targetPos, Vector3 rotate, int index, Action onComplete = null)
    {
        if (index > chips.Count - 1)
        {
            onComplete?.Invoke();
            return;
        }

        targetPos += new Vector3(0, Offset.y, 0);
        float xMedial = (targetPos.x + startPos.x) / 2;
        float zMedial = (targetPos.z + startPos.z) / 2;
        float ymedial = (targetPos.y > startPos.y ? targetPos.y : startPos.y) + Offset.y * 6;
        Vector3 medialPos = new Vector3(xMedial, ymedial, zMedial);
        medialPos += Vector3.up * Offset.y * index;

        if (index == chips.Count - 1)
        {
            MoveChip(chips[index], medialPos, targetPos, timeChipMove, rotate, () =>
            {
                DOVirtual.DelayedCall(timeDelayChipMove, () =>
                {
                    MoveListChip(chips, currentHexagon, putOnHexagon, startPos, targetPos, rotate, index + 1, onComplete);
                });
            });
        }
        else
        {
            MoveChip(chips[index], medialPos, targetPos, timeChipMove, rotate);
            DOVirtual.DelayedCall(timeDelayChipMove, () => { MoveListChip(chips, currentHexagon, putOnHexagon, startPos, targetPos, rotate, index + 1, onComplete); });
        }
    }

    private void MoveChip(GameObject chip, Vector3 medialPos, Vector3 targetPos, float timeMove, Vector3 rotate, Action completed = null)
    {
        //chip.transform.DOMove(medialPos, timeMove / 2).SetEase(Ease.InOutSine).OnComplete(() =>
        //{
        //    chip.transform.DOMove(targetPos, timeMove / 2).SetEase(Ease.InOutSine);
        //});

        Vector3[] path = { medialPos, targetPos };
        chip.transform.DOPath(path, timeMove, PathType.CatmullRom, PathMode.Full3D).SetEase(Ease.InOutSine);

        chip.transform.DORotate(chip.transform.localEulerAngles + rotate, timeMove, RotateMode.FastBeyond360).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            completed?.Invoke();
        });

    }

    private void MedialMoveChip(BaseHexagon curHexagon, BaseHexagon newHexagon)
    {
        if (!ListCheckSurroundHexagon.Contains(curHexagon)) ListCheckSurroundHexagon.Add(curHexagon);
        ListCheckBlockHexagon.RemoveAll(x => x == curHexagon);

        Vector3 startPos = curHexagon.currentChipStack.GetTopPosition();
        Vector3 targetPos = newHexagon.currentChipStack.GetTopPosition();
        ChipBlock moveChipBlock = curHexagon.currentChipStack.listChipBlock.Last();
        newHexagon.currentChipStack.AddChipBlock(moveChipBlock);
        Vector3 rotate = CaculatorRotate(curHexagon, newHexagon);

        MoveListChip(moveChipBlock.ListChip, curHexagon, newHexagon, startPos, targetPos, rotate, 0);

        curHexagon.currentChipStack.RemoveTopChipBlock();
    }

    private void SetNeightborHexagon(BaseHexagon hexagon)
    {
        if (hexagon.neightbors.Count <= 0)
        {
            Vector2Int[] check = NeightborCoordinates[hexagon.Coordinate.x % 2].coordinate;
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
