using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameMode
{
    Destroy,
    Survive,
}

[CreateAssetMenu(fileName = "GameRules", menuName = "Scriptable Objects/GameRules", order = 1)]
public class GameRulesData : ScriptableObject
{
    public GameMode gameMode;

    [Header("Turns")]
    public bool turnLimit;
    public int maxTurns;
    [Header("Moves")]
    public bool canMoveAll;
    public int playerUnitsToMove;


    private void OnValidate()
    {
        if (playerUnitsToMove < 1)
            playerUnitsToMove = 1;
        if (maxTurns < 1)
            maxTurns = 1;
    }
}
