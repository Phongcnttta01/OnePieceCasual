using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSO", menuName = "LevelSO")]
public class LevelSO : ScriptableObject
{
    public int level;
    public int size;
    public GameObject textrue;
    public Sprite image;
    public string name;
    public string time;
    public int step;
    public LevelType levelType;
}

public enum LevelType
{
    Level1,
    Level2,
    Level3,
    Level4,
    Level5
}
