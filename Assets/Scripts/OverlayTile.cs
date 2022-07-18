using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OverlayTile : MonoBehaviour
{
    public Vector3Int gridLocation;
    public DieManager occupyingDie;

    public TileData data;
    private TextMeshProUGUI gridLocationText;

    private void OnEnable()
    {
        EventManager.AddListener("DebugMap", _onDebugMap);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("DebugMap", _onDebugMap);
    }

    public bool IsBlocked {
        get { return data.blocking || occupyingDie != null; }
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

    private void _onDebugMap()
    {
        gridLocationText.enabled = !gridLocationText.enabled;
    }
}
