using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    Destroy,
    Survive,
}

public enum GridType
{
    Square,
    Triangle,
    Hexagon,
}

[CreateAssetMenu(fileName = "GameRules", menuName = "Scriptable Objects/GameRules", order = 1)]
public class GameRulesData : ScriptableObject
{
    public int maxTurns;
    public int playerUnitsToMove;
    public GameMode gameMode;
    public GridType gridType;
}
