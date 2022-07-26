using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class GameUnitData : BinarySerializable
{
    public DiceClass diceClass;
    public DiceOrientation orientation;
    public bool isEnemy;
    public Vector2Int position;
    public Face[] faces;

    public GameUnitData() { }

    protected GameUnitData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }
}

[System.Serializable]
public class GameData : BinarySerializable
{
    public Vector3 camPosition;
    public GameUnitData[] dice;

    public static string gameUid;
    public static string DATA_FILE_NAME = "GameData.data";

    private static string _GetFilePath()
        => System.IO.Path.Combine(
            Application.persistentDataPath,
            DATA_DIRECTORY,
            "Games",
            gameUid,
            DATA_FILE_NAME);

    public static void Save(GameData instance)
    {
        BinarySerializable.Save(_GetFilePath(), instance);
    }

    public GameData() { }

    protected GameData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }

    private static GameData _instance;
    public static GameData Instance => _instance;

    public static GameData Load()
    {
        _instance = (GameData)BinarySerializable.Load(_GetFilePath());
        return _instance;
    }
}
