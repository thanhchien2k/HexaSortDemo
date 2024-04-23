
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChipStackSpawner : MonoBehaviour
{
    [SerializeField] float ChipSpacing;
    [SerializeField] ChipStack chipStackPrefabs;
    [SerializeField] int maxChipOfStack;
    [SerializeField] Transform[] listPosSpawn;
    private int currentCount;

    private void Awake()
    {
        //listPosSpawn = new Transform[transform.childCount];
        //for(int i =0; i< transform.childCount; i++)
        //{
        //    listPosSpawn[i] = transform.GetChild(i).transform;
        //}
    }
    private void Start()
    {
       SpawnChipStackInPos();
    }

    public void SpawnChipStackInPos()
    {
        for (int i = 0; i < listPosSpawn.Length; i++)
        {
            SpawnChipStack(listPosSpawn[i]);
        }
    }
    private void SpawnChipStack(Transform parent)
    {
        currentCount = 0;
        ChipStack chip = Instantiate(chipStackPrefabs, parent);
        List<ChipType> chipTypes = new List<ChipType>(GameManager2.Instance.listType);
        Shuffle(chipTypes);
        int types = Random.Range(1, chipTypes.Count);
        for (int i = 0; i < types; i++)
        {
            CreateBlock(chip, chipTypes[i]);
        }

        chip.SetHeightCollider(currentCount * 0.1f);

    }

    private void CreateBlock(ChipStack stack, ChipType _type)
    {
        int maxChipOfBlock = maxChipOfStack - currentCount;
        if (maxChipOfBlock <= 0) return;
        int count = Random.Range(2, maxChipOfBlock + 1);
        //Debug.Log(count);
        GameObject chipBlock = new GameObject(nameof(_type)+" Block");
        chipBlock.transform.parent = stack.transform;
        chipBlock.transform.position = stack.transform.position;
        Vector3 blockPos = chipBlock.transform.position;

        ChipBlock block = new ChipBlock(chipBlock.transform, _type, count);

        while (count >0) 
        {
            GameObject newChip = Instantiate(GameManager2.Instance.chipsPrefabs[(int)_type], blockPos + Vector3.up * GameManager2.Instance.offset.y * currentCount, Quaternion.identity , chipBlock.transform);
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
            Debug.Log("list position is empty");
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
