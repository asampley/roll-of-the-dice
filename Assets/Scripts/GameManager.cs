using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Turn {
    Player,
    Enemy,
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public GameObject diePrefab;
    public GameObject enemyPrefab;

    public HashSet<EnemyAI> EnemiesWaiting = new HashSet<EnemyAI>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private Turn _turn;
    public Turn CurrentTurn {
        get { return _turn; }
        set { _turn = value; TurnChange?.Invoke(_turn); }
    }
    public event Action<Turn> TurnChange;

    private void Start()
    {
        CurrentTurn = Turn.Player;

        SpawnDie(new Vector2Int(0, 0), false);
        SpawnDie(new Vector2Int(2, 2));
        SpawnDie(new Vector2Int(3, 3));

        TurnChange += t => Debug.Log("Turn: " + t);
    }


    public void SpawnDie(Vector2Int startPos, bool isEnemy = true)
    {
        GameObject prefab = isEnemy ? enemyPrefab : diePrefab;

        Vector3 pos = MapManager.Instance.GetTileWorldSpace(startPos);
        GameObject die = Instantiate(prefab, pos, Quaternion.identity);
        DieManager dieManager = die.GetComponent<DieManager>();
        var placedOnTile = MapManager.Instance.GetTileAtPos(startPos);

        dieManager.Initialize(isEnemy);

        if (placedOnTile != null)
        {
            GameObject overlayTile = placedOnTile.gameObject;
            OverlayTile overlayTileManager = overlayTile.GetComponent<OverlayTile>();

            overlayTileManager.MoveDiceToTile(dieManager);
        }

        dieManager.isEnemy = isEnemy;
    }
}
