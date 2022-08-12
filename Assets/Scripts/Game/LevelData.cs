using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    Square,
    Triangle,
    Hexagon,
}


[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData", order = 2)]
public class LevelData : ScriptableObject
{
    public string levelName;
    public string sceneName;
    public LevelData nextLevel;
    public Vector3 camStartPos = new(-0.25f, 1f, -200f);
    public float camStartDist = 2.5f;

    // Rules
    public GameRulesData gameRules;
    public GridType gridType;

    // Dice
    public DiceSpawn[] alliedDice;
    public DiceSpawn[] enemyDice;
    

    private void OnValidate()
    {
        List<Vector2Int> positions = new();
        foreach (DiceSpawn dice in alliedDice)
        {
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
