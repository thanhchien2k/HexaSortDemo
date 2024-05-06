using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/GenConfig")]

public class GenStackConfig : ScriptableObject
{
    public List<GenConfig> genConfigs;
}
[System.Serializable]
public class GenConfig
{
    public List<Vector2> ColorRatio;
    public int maxType;
}

