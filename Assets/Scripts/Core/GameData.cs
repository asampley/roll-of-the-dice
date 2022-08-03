using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;


[System.Serializable]
public class GameLevelData : BinarySerializable
{
    public static string levelId;
    private static GameLevelData _instance;
    public static GameLevelData Instance => _instance;

    public GameUnitData[] dice;
    public Vector3 camPosition;
    public float camDistance;
    public int currentRound;
    public Phase currentPhase;
    public string[] movedPieces;
    public int playerPiecesMoved;
    public DiceOrientationData[] alliedOrientations;
    public DiceOrientationData[] enemyOrientations;

    public static string GetFolderPath()
        => System.IO.Path.Combine(
            Application.persistentDataPath,
            Globals.DATA_DIRECTORY,
            "Games",
            levelId);


    public static string GetFilePath()
        => System.IO.Path.Combine(GetFolderPath(), Globals.DATA_FILE_NAME);



    public GameLevelData() { }

    protected GameLevelData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }

    public static GameLevelData Load()
    {
        _instance = (GameLevelData)BinarySerializable.Load(GetFilePath());
        return _instance;
    }

    public static void Save(GameLevelData instance)
    {
        BinarySerializable.Save(GetFilePath(), instance);
    }
}

[System.Serializable]
public class GameUnitData : BinarySerializable
{
    public DiceClass diceClass;
    public bool isEnemy;
    public string uid;
    public Vector2Int position;
    public DiceState[] faces;
    public Vector3 orientation;
    public int movesRemaining;
    public GamePathData[] path;

    public GameUnitData() { }

    protected GameUnitData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }
}



[System.Serializable]
public class DiceOrientationData : BinarySerializable
{
    public int xRolls;
    public int yRolls;
    public int zRolls;

    public DiceOrientationData() { }

    protected DiceOrientationData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }
}


[System.Serializable]
public class GamePathData : BinarySerializable
{
    public Vector2Int path;

    public GamePathData() { }

    protected GamePathData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }
}
