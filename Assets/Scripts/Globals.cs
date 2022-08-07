using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Globals
{
    public static float STEP_TIME = 1f;
    public static UnitManager SELECTED_UNIT;
    public static Vector2 TILE_SIZE = new(1.0f, 0.5f);
    public static float MOVEMENT_TIME = 0.4f;

    public static float OVERLAY_TILE_Z_OFFSET = -1e-2f;
    public static float OVERLAY_LINE_Z_OFFSET = -0.5f;

    public static float MIN_CAMERA_SIZE = 1;
    public static float MAX_CAMERA_SIZE = 10;

    public static readonly uint DICE_STATES = Enum.GetValues(typeof(DiceState)).Cast<uint>().Max() + 1;
    public static UnitData[] UNIT_DATA;

    public static Dictionary<(DiceClass, bool), Material> DICE_MATERIALS;
    public static Dictionary<(DiceClass, bool), Material> GHOST_MATERIALS;


    // DEBUGGING
    public static bool DEBUG_GAME_SETUP;
    public static bool DEBUG_UNIT_SPAWN;
    public static bool DEBUG_SERIALIZATION;
    public static bool DEBUG_PHASES;
    public static bool DEBUG_AI;


    // FILE PATHS
    public static string DICE_CLASS_SO = "ScriptableObjects/DiceClasses";
    public static string LEVEL_DATA_SO = "ScriptableObjects/LevelData";
#if UNITY_EDITOR
    public static string DATA_DIRECTORY = "Data_Dev";
#else
    public static string DATA_DIRECTORY = "Data";
#endif

    public static string DATA_FILE_NAME = "GameData.data";

#if UNITY_WEBGL
    public static string GetLogFolderPath()
        => System.IO.Path.Combine(
            "idbfs",
            "BlockPaperScissorsANDupwq3wg0zw4czbq",
            Globals.DATA_DIRECTORY,
            "Logs");
#else
    public static string GetLogFolderPath()
        => System.IO.Path.Combine(
            Application.persistentDataPath,
            Globals.DATA_DIRECTORY,
            "Logs");
#endif
}
