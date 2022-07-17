using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayTile : MonoBehaviour
{
    public Vector3Int gridLocation;
    public DieManager occupyingDie;

    public TileData data;

    public bool IsBlocked {
        get { return data.blocking || occupyingDie != null; }
    }

    private void Awake()
    {
        occupyingDie = null;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HideTile();
    }

    public void ShowTile()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void HideTile()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void MoveDiceToTile(DieManager die)
    {
        occupyingDie = die;
        die.parentTile = this;
    }

    public void RemoveDiceFromTile()
    {
        occupyingDie = null;
    }
}
