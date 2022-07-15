using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }


    private Vector3 startPos = new Vector3(0, 0, 1);
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
        GameObject die = Instantiate(diePrefab, startPos, Quaternion.identity);
        DieManager dieManager = die.GetComponent<DieManager>();
        var placedOnTile = MapManager.Instance.GetTileAtPos(startPos);

        if (placedOnTile.HasValue)
        {
            GameObject overlayTile = placedOnTile.Value.collider.gameObject;
            OverlayTile overlayTileManager = overlayTile.GetComponent<OverlayTile>();

            overlayTileManager.occupyingDie = dieManager;
        }
        else
        {
            Debug.Log("Spawning die failed");
        }
    }


}
