using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class DataHandler : MonoBehaviour
{
    private void Start()
    {
        DeserializeGameData();
    }

    public static void LoadGameData()
    {
        string gameUid = CoreDataHandler.instance.GameUID;

        // Load game scene data
        GameData.gameUid = gameUid;
        GameData.Load();
    }

    public static void SaveGameData()
    {
        GameData.gameUid = CoreDataHandler.instance.GameUID;
        GameData.Save(SerializeGameData());
    }

    public static GameData SerializeGameData()
    {
        GameData data = new GameData();
        List<GameUnitData> dice = new List<GameUnitData>();
        foreach (UnitManager die in UnitManager.DICE_LIST)
        {
            if (!die.transform) continue;

            GameUnitData d = new GameUnitData()
            {
                isEnemy = die.IsEnemy,
                position = die.transform.position,
            };

            dice.Add(d);
        }

        data.dice = dice.ToArray();
        data.camPosition = Camera.main.transform.position;

        return data;
    }

    public static void DeserializeGameData()
    {
        GameData data = GameData.Instance;
        if (data == null) return;


        foreach (GameUnitData die in data.dice)
        {
            UnitManager d;

        }

        Camera.main.transform.position = data.camPosition;
        EventManager.TriggerEvent("UpdateResourceTexts");
    }

    public static List<(string, System.DateTime)> GetGamesList()
    {
        string rootPath = Path.Combine(Application.persistentDataPath, BinarySerializable.DATA_DIRECTORY, "Games");
        string[] gameDirs = Directory.GetDirectories(rootPath);

        IEnumerable<string> validGameDirs = gameDirs.Where((string d) => File.Exists(Path.Combine(d, GameData.DATA_FILE_NAME)));

        List<(string, System.DateTime)> games = new List<(string, System.DateTime)>();

        foreach (string dir in validGameDirs)
        {
            games.Add((
                dir,
                File.GetLastWriteTime(Path.Combine(dir, GameData.DATA_FILE_NAME))
                ));
        }

        return games.OrderByDescending(((string, System.DateTime) x) => x.Item2).ToList();
    }
}
