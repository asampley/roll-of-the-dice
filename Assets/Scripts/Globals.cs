using System;
using System.Linq;
using UnityEngine;

public static class Globals
{
    public static float STEP_TIME = 1f;
    public static UnitManager SELECTED_UNIT;
    public static Vector2 TILE_SIZE = new Vector2(1.0f, 0.5f);
    public static float MOVEMENT_TIME = 0.4f;

    public static float OVERLAY_TILE_Z_OFFSET = -1e-2f;

    public static float MIN_CAMERA_SIZE = 1;
    public static float MAX_CAMERA_SIZE = 10;

    public static readonly uint DICE_STATES = Enum.GetValues(typeof(DiceState)).Cast<uint>().Max() + 1;
    public static UnitData[] UNIT_DATA;


    // FILE PATHS
    public static string DICE_CLASS_SO = "ScriptableObjects/DiceClasses";
    public static string LEVEL_DATA_SO = "ScriptableObjects/LevelData";
}
