using System.Runtime.Serialization;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameFaceData : BinarySerializable
{
    public DiceState state;
    public Vector3 position;

    public GameFaceData() { }

    protected GameFaceData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }
}

[System.Serializable]
public class GameUnitData : BinarySerializable
{
    public DiceClass diceClass;
    public bool isEnemy;
    public Vector2Int position;
    public GameFaceData[] faces;
    public Vector3 orientation;
    public int movesRemaining;

    public GameUnitData() { }

    protected GameUnitData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }
}

[System.Serializable]
public class GameLevelData : BinarySerializable
{
    public static string levelId;
    private static GameLevelData _instance;
    public static GameLevelData Instance => _instance;

    
    public static string DATA_FILE_NAME = "GameData.data";

    public Vector3 camPosition;
    public GameUnitData[] dice;
    public bool hasData;

    public static string GetFolderPath()
        => System.IO.Path.Combine(
            Application.persistentDataPath,
            DATA_DIRECTORY,
            "Games",
            levelId);


    public static string GetFilePath()
        => System.IO.Path.Combine(GetFolderPath(), DATA_FILE_NAME);



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
