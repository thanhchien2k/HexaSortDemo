using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Level")]
public class LevelConfigs : ScriptableObject
{
    public List<int> PointComplete;
    public List<MapConfig> MapConfigs;
    public List<Vector2Int> LevelSize;
}

[System.Serializable]
public class MapConfig
{
    public List <HexgonConfig> HexagonConfigs;
}

[System.Serializable]
public class HexgonConfig
{
    public HexagonType HexagonType;
    public List<Vector2Int> Coordinate;
    public List<int> lockPoint;
}

[System.Serializable]
public enum HexagonType
{
    Null,
    Lock,
    Ads
}