using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreDataHandler : MonoBehaviour
{
    public static CoreDataHandler instance;
    private MapData _mapData;
    private string _gameUID;


    public string Scene => _mapData != null ? _mapData.levelName : null;
    public string GameUID => _gameUID;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void SetMapData(MapData d)
    {
        _mapData = d;
    }

    public void SetGameUID(MapData d)
    {
        _gameUID = $"{d.levelName}__{System.Guid.NewGuid().ToString()}";
    }

    public void SetGameUID(string uid)
    {
        _gameUID = uid;
    }
}
