using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    Square,
    Triangle,
    Hexagon,
}


[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/MapData", order = 2)]
public class LevelData : ScriptableObject
{
    public string levelName;
    public string sceneName;
    public LevelData nextLevel;
    public Vector3 camStartPos = new(-0.25f, 1f, -200f);
    public float camStartDistance = 2.5f;

    [Header("Rules")]
    public GameRulesData gameRules;
    public GridType gridType;

    [Header("Dice")]
    public DiceSpawn[] alliedDice;
    public DiceSpawn[] enemyDice;

    

    private void OnValidate()
    {
        List<Vector2Int> positions = new();
        foreach (DiceSpawn dice in alliedDice)
        {
            dice.isEnemy = false;
            if (positions.Contains(dice.tilePosition))
            {
                Debug.LogError("Identical spawn positions in a start position");
            }
            else
            {
                positions.Add(dice.tilePosition);
            }
        }
        foreach (DiceSpawn dice in enemyDice)
        {
            dice.isEnemy = true;
            if (positions.Contains(dice.tilePosition))
            {
                Debug.LogError("Identical spawn positions in a start position");
            }
            else
            {
                positions.Add(dice.tilePosition);
            }
        }
    }
}
