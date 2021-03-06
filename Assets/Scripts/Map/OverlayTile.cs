using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OverlayTile : MonoBehaviour
{
    public Vector3Int gridLocation;
    public UnitManager occupyingDie;

    public TileData data;
    private TextMeshProUGUI gridLocationText;

    private void OnEnable()
    {
        DebugConsole.DebugMap += _onDebugMap;
    }

    private void OnDisable()
    {
        DebugConsole.DebugMap -= _onDebugMap;
    }

    public bool IsBlocked {
        get { return data.TileType == TileType.Blocking || occupyingDie != null; }
    }

    private void Awake()
    {
        occupyingDie = null;
        gridLocationText = GetComponentInChildren<TextMeshProUGUI>();
        gridLocationText.enabled = false;
    }

    private void Start()
    {
        gridLocationText.text = gridLocation.ToString();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HideTile();
    }

    public void ShowTile()
    {
        gameObject.GetComponent<Renderer>().enabled = true;
    }

    public void HideTile()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
    }

    public void MoveDiceToTile(UnitManager die)
    {
        occupyingDie = die;
        die.parentTile = this;
    }

    public void RemoveDiceFromTile()
    {
        occupyingDie = null;
    }

    private void _onDebugMap()
    {
        gridLocationText.enabled = !gridLocationText.enabled;
    }
}
