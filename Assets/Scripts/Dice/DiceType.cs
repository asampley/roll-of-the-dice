using System;
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

// Face number is on top, face rotation is how many times to rotate about that face
[Serializable]
public struct DiceOrientation {
    public static readonly DiceOrientation ZERO = new DiceOrientation();

    public int FaceNumber;
    public int FaceRotation;

    public DiceOrientation(int faceNumber, int faceRotation) {
        this.FaceNumber = faceNumber;
        this.FaceRotation = faceRotation;
    }

    public static bool operator==(DiceOrientation a, DiceOrientation b) {
        return a.FaceNumber == b.FaceNumber && a.FaceRotation == b.FaceRotation;
    }

    public static bool operator!=(DiceOrientation a, DiceOrientation b) {
        return !(a == b);
    }

    override public bool Equals(object? o) {
        if (o != null && o is DiceOrientation) {
            return this == (DiceOrientation)o;
        } else {
            return false;
        }
    }

    override public int GetHashCode() {
        int hash = 23;
        hash = hash * 31 + FaceNumber;
        hash = hash * 31 + FaceRotation;
        return hash;
    }

    override public string ToString() => "DiceOrientation(" + FaceNumber + "," + FaceRotation + ")";
}

[System.Serializable]
public class DiceSpawn
{
    public DiceClass diceClass;
    public Vector2Int tilePosition;
    public bool randomOrientation;
    public DiceOrientation diceOrientation;
}
