using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public GameObject diePrefab;

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

    private void Start()
    {
        SpawnDie(new Vector2Int(0, 0), false);
        SpawnDie(new Vector2Int(2, 2));
    }


    public void SpawnDie(Vector2Int startPos, bool isEnemy = true)
    {
        Vector3 pos = MapManager.Instance.GetTileWorldSpace(startPos);
        GameObject die = Instantiate(diePrefab, pos, Quaternion.identity);
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
