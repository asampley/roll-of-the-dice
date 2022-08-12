using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DiceClass
{
    Pawn,
    Rook,
    Knight,
    Artisan,
    Trebuchet,
    ErrantKnight,
    Lich,
    King,
    TetraPawn,
    TestPawn,
}

// Notation: Shape FaceNumber FaceRotation (2 Digits each)
// C for Cube
public enum DiceOrientation
{
    INVALID,
    C0000,
    C0001,
    C0002,
    C0003,
    C0100,
    C0101,
    C0102,
    C0103,
    C0200,
    C0201,
    C0202,
    C0203,
    C0300,
    C0301,
    C0302,
    C0303,
    C0400,
    C0401,
    C0402,
    C0403,
    C0500,
    C0501,
    C0502,
    C0503,
}

[System.Serializable]
public class DiceSpawn
{
    public DiceClass diceClass;
    public Vector2Int tilePosition;
    public bool randomOrientation;
    public DiceOrientation diceOrientation;
}
