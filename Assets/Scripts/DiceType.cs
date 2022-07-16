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
    public DiceState state;
    public Vector2Int tilePosition;
    public bool isEnemy;
}
