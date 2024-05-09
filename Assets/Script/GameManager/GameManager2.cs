using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    public static GameManager2 Instance;
    private BaseHexagon[,] maps;
    public List<BaseHexagon> listLockHexagon { get; private set; }
    [Header("Hexagon Prefabs")]
    [SerializeField] BaseHexagon hexagonPrefabs;
    [SerializeField] BaseHexagon LockhexagonPrefabs;
    [SerializeField] BaseHexagon AdshexagonPrefabs;

    [Header("Map Config")]
    [SerializeField] private Transform mapRoot;
    Vector2Int mapSize;

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
    public List<Material> ListMaterials;

    [Header("UI")]
    [SerializeField] GamePlayUI gamePlayUI;

    [Header("Level")]
    public int CurrentLevelID;
    [SerializeField] LevelConfigs LevelConfig;
    MapConfig mapConfig;
    private int completedPoint;
    private int currentPoint;
    public int CurrentPoint
    {
        get
        {
            return currentPoint;
        }

        set
        {
            gamePlayUI.UpdateUI(value, completedPoint);
            currentPoint = value;
            CheckLockHexagon();
        }
    }

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
        //PlayerPrefs.SetInt("LevelID", 0);
        CurrentLevelID = GameData.CurrentLevelID;
        listLockHexagon = new();
        completedPoint = LevelConfig.PointComplete[CurrentLevelID];
        mapConfig = LevelConfig.MapConfigs[CurrentLevelID];
        mapSize = LevelConfig.LevelSize[CurrentLevelID];
        maps = new BaseHexagon[mapSize.x, mapSize.y];
        CurrentPoint = 0;

        spawner.SpawnChipStackInPos();
        CreateMap();
        //transform.DOLocalRotate(transform.localRotation.eulerAngles +  new Vector3(0, 120f, 0), 2f, RotateMode.Fast);
    }

    private void CreateMap()
    {
        if (hexagonPrefabs == null || mapRoot == null) return;
        Vector3 worldPosition;

        List<Vector2Int> nullHexgon = mapConfig.HexagonConfigs.Find(item => item.HexagonType == HexagonType.Null)?.Coordinate;
        int nullHexCount = nullHexgon != null ? nullHexgon.Count : 0;

        List<Vector2Int> adsHexagon = mapConfig.HexagonConfigs.Find(item => item.HexagonType == HexagonType.Ads)?.Coordinate;
        int adsHexCount = adsHexagon != null ? adsHexagon.Count : 0;
        

        List<Vector2Int> lockHexagon = mapConfig.HexagonConfigs.Find(item => item.HexagonType == HexagonType.Lock)?.Coordinate;
        List<int> lockPoint = mapConfig.HexagonConfigs.Find(item => item.HexagonType == HexagonType.Lock)?.lockPoint;
        int lockHexCount = lockHexagon != null ? lockHexagon.Count : 0;

        if (mapSize.x == 0 || mapSize.y == 0)
        {
            Debug.Log("size is wrong!");
            return;
        }

        float xStartPost = (mapRoot.position.x - (mapSize.x - 1) * Offset.x * 3/4f) / 2;
        float zStartPos = (mapRoot.position.z - (mapSize.y - 0.5f) * Offset.z) / 2;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {   
                Vector2Int curPos = new Vector2Int(x, y);
                if(nullHexCount > 0)
                {
                    if(nullHexgon.Contains(curPos))
                    {
                        nullHexCount--;
                        continue;
                    }
                }

                float zPos = y * Offset.z;
                if (x % 2 == 1)
                {
                    zPos += Offset.z / 2f;
                }

                worldPosition = new Vector3(xStartPost + x * Offset.x * 3 /4 , 0, zStartPos + zPos);

                BaseHexagon baseHexagon = null;
                int lockIndex = -1;

                if(adsHexCount > 0)
                {
                    if (adsHexagon.Contains(curPos))
                    {
                        baseHexagon = AdshexagonPrefabs;
                        adsHexCount--;
                    }
                } 

                if(lockHexCount > 0)
                {
                    if (lockHexagon.Contains(curPos))
                    {
                        baseHexagon = LockhexagonPrefabs;
                        lockIndex = lockHexagon.IndexOf(curPos);
                        lockHexCount--;
                    }
                }

                if( baseHexagon == null)
                {
                    baseHexagon = hexagonPrefabs;
                }

                maps[x, y] = Instantiate(baseHexagon, worldPosition, Quaternion.identity, mapRoot);

                if(lockIndex != -1)
                {
                    Debug.Log("add");
                    maps[x, y].LockPoint = lockPoint[lockIndex];
                    listLockHexagon.Add(maps[x, y]);
                }

                maps[x, y].gameObject.name = "Cell " + x + "-" + y;
                maps[x, y].Coordinate = new Vector2Int(x, y);
            }
        }

        //float xCenter = (maps[size.x - 1, 0].transform.position.x - maps[0, 0].transform.position.x ) / 2f;
        //float zCenter = (maps[1, size.x - 1].transform.position.x - maps[0, 0].transform.position.x) / 2f;
        //mapRoot.position = new Vector3(mapRoot.position.x - xCenter , mapRoot.position.y, mapRoot.position.z - zCenter);
        //transform.RotateAround(transform.position, Vector3.up, 180f);

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

        foreach (var temp in hexagon.Neightbors)
        {
            if( temp.IsRemoveStack)
            {
                if (listCheck.Contains(temp))
                {
                    listCheck.Remove(temp);
                }
                continue;
            }

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

        float timeDelay = 0f;
        
        if (listCheck.Count != 0 || (priorityPutOn != null && priorityPutOn != hexagon))
        {
            if (listCheck.Count > 0)
            {
                for (int i = 0; i < listCheck.Count; i++)
                {
                    foreach (var temp in listCheck[i].Neightbors)
                    {
                        if (temp == hexagon || temp == priorityPutOn || temp.IsRemoveStack) continue;
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
                            if (listCheck[i] != checkSecondHexagon[j] && !checkSecondHexagon[j].IsRemoveStack)
                            {
                                priorityPutOn = listCheck[i];
                                listCheck.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                //Debug.Log("xet puton");
                //priorityPutOn = hexagon;
                if (priorityPutOn == null)
                {
                    priorityPutOn = hexagon;
                }
            }



            //if (priorityPutOn.IsMoveStack)
            //{
            //    if (!ListCheckSurroundHexagon.Contains(priorityPutOn)) ListCheckSurroundHexagon.Add(priorityPutOn);
            //    return;
            //}

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
        Debug.Log("Check recall");
        if (ListCheckSurroundHexagon.Count > 0)
        {
            CheckSurroundingHexagon(ListCheckSurroundHexagon.Last());
        }
        else
        {
            // kiem tra removelist neu no khong rong thi se check stack tren do
            if(ListCheckBlockHexagon.Count > 0)
            {
                RemoveListTopStack(ListCheckBlockHexagon, 0);
            }

            IsMoving = false;
            
            if(IsMoving == false)
            {
                if (!IsCanPutHexagon())
                {
                    gamePlayUI.PopupLose();
                }
            }
        }
    }

    public void HideMap()
    {
        mapRoot.gameObject.SetActive(false);
        spawner.gameObject.SetActive(false);
    }

    public void DisplayMap()
    {
        mapRoot.gameObject.SetActive(true);
        spawner.gameObject.SetActive(true);
    }

    private void RemoveListTopStack(List<BaseHexagon> hexagons, int index)
    {
        if (index > hexagons.Count - 1)
        {
            ListCheckBlockHexagon.Clear();
            CheckRecallCheckSurround();
            return;
        }

        if (hexagons[index].currentChipStack == null)
        {
            RemoveListTopStack(hexagons, index + 1);
        }

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

                MoveListChip(medialHexagon.currentChipStack.listChipBlock.Last().ListChip, medialHexagon ,putOnHexagon, curPos,newPos, rotate2 , 0, () =>
                {
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
        //else
        //{
        //    //MoveListBlock(listHexagon, putOnHexagon, index + 1, medialHexagon);
        //    Debug.Log("return nn n n n n n n n n n n n n n n n ");
        //    //return;
        //}

        ListCheckBlockHexagon.RemoveAll(x => x == listHexagon[index]);

        newHexagon = medialHexagon != null ? medialHexagon : putOnHexagon;
        Vector3 startPos = currentHexagon.currentChipStack.GetTopPosition();
        Vector3 targetPos = newHexagon.currentChipStack.GetTopPosition();
        ChipBlock moveChipBlock = currentHexagon.currentChipStack.listChipBlock.Last();
        newHexagon.currentChipStack.AddChipBlock(moveChipBlock);


        Vector3 rotate = CaculatorRotate(currentHexagon, putOnHexagon);

        MoveListChip(moveChipBlock.ListChip, currentHexagon, newHexagon, startPos,targetPos, rotate,0, () =>
        {
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
        if (hexagon.Neightbors.Count <= 0)
        {
            Vector2Int[] check = NeightborCoordinates[hexagon.Coordinate.x % 2].coordinate;
            for (int i = 0; i < check.Length; i++)
            {
                Vector2Int checkCoordinate = check[i] + hexagon.Coordinate;
                if ((checkCoordinate.x >= 0 && checkCoordinate.x < mapSize.x) && (checkCoordinate.y >= 0 && checkCoordinate.y < mapSize.y))
                {
                    if (maps[checkCoordinate.x, checkCoordinate.y] != null)
                    {
                        hexagon.Neightbors.Add(maps[checkCoordinate.x, checkCoordinate.y]);
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

    public float GetCurrentProgress()
    {
        float current = currentPoint;
        return current / completedPoint;
    }

    private void CheckLockHexagon()
    {
        if (listLockHexagon.Count == 0) return;

        for (int i = 0; i< listLockHexagon.Count; i++)
        {
            if (listLockHexagon[i].LockPoint <= currentPoint)
            {
                listLockHexagon[i].UnlockHexagon();
            }
        }
    }

    private bool IsCanPutHexagon()
    {
        foreach (var item in maps)
        {
            if (item == null) continue;
            if (item.IsPlaceable == false) continue;
            else
            {
                return true;
            }
        }

        return false;
    }

}


[System.Serializable]
public enum ChipType
{
    Red,
    Yellow,
    Green,
    Blue,
    Purple,
    AquaBlue,
    White,
    Black,
    Null
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
        if (listChip == null)
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