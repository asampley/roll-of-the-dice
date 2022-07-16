using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum DiceClass
{
    Normal,
    Rock,
}
[System.Serializable]
public class DiceSpawn
{
    public DiceClass diceClass;
    public Vector2Int tilePosition;
    [HideInInspector]
    public bool isEnemy;
}
