using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    Normal,
    Blocking,
    Stopping,
    RotateClockwise,
    RotateCounterClockwise,
    ShovePosX,
    ShoveNegX,
    ShovePosY,
    ShoveNegY,
    RemoveFace,
    Randomize,
}

[CreateAssetMenu(fileName = "TileType", menuName = "Scriptable Objects/TileType", order = 3)]
public class TileData : ScriptableObject {
    public TileBase[] tiles;

    public TileType TileType;
}
