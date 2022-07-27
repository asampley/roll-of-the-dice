using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreDataHandler : MonoBehaviour
{
    public static CoreDataHandler Instance;
    private LevelData _levelData;
    public LevelData LevelData
    {
        get { return _levelData; }
    }
    private string _gameUID;


    public string Scene => _levelData != null ? _levelData.levelName : null;
    public string GameUID => _gameUID;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void SetLevelData(LevelData d)
    {
        _levelData = d;
    }

    public void SetGameUID(LevelData d)
    {
        _gameUID = $"{d.levelName}__{System.Guid.NewGuid().ToString()}";
    }

    public void SetGameUID(string uid)
    {
        _gameUID = uid;
    }
}
