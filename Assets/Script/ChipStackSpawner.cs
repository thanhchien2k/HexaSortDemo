using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChipStackSpawner : MonoBehaviour
{
    [SerializeField] GenStackConfig genStackConfig;
    [SerializeField] ChipStack chipStackPrefabs;
    [SerializeField] int maxChipOfStack;
    [SerializeField] Transform[] listPosSpawn;
    [SerializeField] int genFewTypeTime; 
    private int currentCount;
    private float currentProgress;
    int curLevel;

    public void SpawnChipStackInPos()
    {
        currentProgress = GameManager2.Instance.GetCurrentProgress();
        curLevel = GameManager2.Instance.CurrentLevelID;
        int indexOfProgres = GetIndexOfProgress(currentProgress);

        for (int i = 0; i < listPosSpawn.Length; i++)
        {
            SpawnChipStack(listPosSpawn[i], indexOfProgres);
        }
    }
    private void SpawnChipStack(Transform parent, int indexOfProgres)
    {
        currentCount = 0;
        ChipStack chip = Instantiate(chipStackPrefabs, parent);
        int chipCount = Random.Range(2, maxChipOfStack + 1);
        int colorRatioIndex = chipCount == 2 ? 1 : 0;
        int numTypes = GetNumOfTypeChip(colorRatioIndex, indexOfProgres);
        int maxType;
        //maxType = genStackConfig.genConfigs[indexOfProgres].maxType;
        //Debug.Log(maxType);

        if (curLevel == 0) maxType = 4;
        else
        {
            maxType = genStackConfig.genConfigs[indexOfProgres].maxType;

            if (curLevel >= 1 && curLevel < 5 && maxType > 6)
            {
                maxType = 6;
            }
        }

        //Debug.Log(chipCount + " " + indexOfProgres + " " + colorRatioIndex + " " + numTypes);

        List<ChipType> chipTypes = new List<ChipType>(GameManager2.Instance.ListType.GetRange(0, maxType));

        Shuffle(chipTypes);

        switch (numTypes)
        {
            case 1:
                CreateBlock(chip, chipTypes[0], chipCount);
                break;
            case 2:
                int fistCount = Random.Range(1, chipCount);
                CreateBlock(chip, chipTypes[0], fistCount);
                CreateBlock(chip, chipTypes[1], chipCount - fistCount);
                break;
            case 3:
                int fistCount1 = Random.Range(1, chipCount - 1);
                int secondBlockCount = Random.Range(1, chipCount - fistCount1);
                CreateBlock(chip, chipTypes[0], fistCount1);
                CreateBlock(chip, chipTypes[1], chipCount - fistCount1);
                CreateBlock(chip, chipTypes[2], chipCount - fistCount1 - secondBlockCount);
                break;
        }

        chip.SetHeightCollider(currentCount * 0.1f);

    }

    private int GetNumOfTypeChip(int colorRatioIndex, int progressIndex)
    {
        if(genFewTypeTime > 0)
        {
            genFewTypeTime--;
        }

        float typeRatio = Random.value;
        Vector2 configRatio = genStackConfig.genConfigs[progressIndex].ColorRatio[colorRatioIndex];

        if (typeRatio < configRatio.x)
        {
            return 1;
        }
        else if(typeRatio >= configRatio.x && typeRatio < configRatio.x + configRatio.y)
        {
            return 2;
        }

        if(genFewTypeTime > 0)
        {
            return 2;
        }

        if (curLevel == 1)
        {
            return 1;
        }

        if (colorRatioIndex == 1 )
        {
            Debug.Log("2 chip nhung lai chon 3 mau");
            return 2;
        }


        return 3;
    }

    private int GetIndexOfProgress(float value)
    {
        if (value < 0.15) return 0;
        else if (value >= 0.15 && value < 0.3) return 1;
        else if (value >= 0.3 && value < 0.45) return 2;
        else if (value > 0.45 && value < 0.7) return 3;
        else return 4;
    }
    private void CreateBlock(ChipStack stack, ChipType _type, int maxCount)
    {
        int count = maxCount;

        GameObject chipBlock = new GameObject(nameof(_type)+" Block");
        chipBlock.transform.parent = stack.transform;
        chipBlock.transform.position = stack.transform.position;
        Vector3 blockPos = chipBlock.transform.position;

        ChipBlock block = new ChipBlock(chipBlock.transform, _type, count);

        while (count > 0) 
        {
            GameObject newChip = Instantiate(GameManager2.Instance.ChipsPrefabs[(int)_type], blockPos + Vector3.up * GameManager2.Instance.Offset.y * currentCount, Quaternion.identity , chipBlock.transform);
            newChip.name = nameof(_type);
            block.ListChip.Add(newChip);
            currentCount++;
            count--;
        }

        stack.listChipBlock.Add(block);
    }



    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public bool CheckSpawnPosIsEmpty()
    {
        if (listPosSpawn.Length <= 0)
        {
            return false;
        }

        for (int i = 0; i < listPosSpawn.Length; i++)
        {
            if (listPosSpawn[i].childCount > 0)
            {
                return false;
            }
        }
        return true;
    }
}
