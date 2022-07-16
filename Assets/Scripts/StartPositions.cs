using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "StartPosition", menuName = "Scriptable Objects/StartPosition", order = 1)]
public class StartPosition : ScriptableObject
{
    public DiceSpawn[] alliedDice;

    public DiceSpawn[] enemyDice;

    private void OnValidate()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
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
