using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum DiceType
{
    Normal,
}
[System.Serializable]
public class DiceSpawn
{
    public DiceType diceClass;
    public Vector2Int tilePosition;
    [HideInInspector]
    public bool isEnemy;
}
