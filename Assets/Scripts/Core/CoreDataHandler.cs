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
    private string _levelID;


    public string Scene => _levelData != null ? _levelData.levelName : null;
    public string LevelID => _levelID;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void SetLevelData(LevelData d)
    {
        _levelData = d;
    }

    public void SetLevelID(LevelData d)
    {
        _levelID = $"{d.levelName}";
    }

    public void SetLevelID(string uid)
    {
        _levelID = uid;
    }
}
