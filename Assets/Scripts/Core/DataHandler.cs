using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public class DataHandler : MonoBehaviour
{
    public event Action<Win> WinEvent;

    private void Start()
    {
        DeserializeGameData();
        GameManager.Instance.WinEvent += (_) => ClearData();
    }

    public static void LoadGameData()
    {
        Globals.UNIT_DATA = Resources.LoadAll<UnitData>(Globals.DICE_CLASS_SO) as UnitData[];

        Globals.DICE_MATERIALS = new();
        Globals.GHOST_MATERIALS = new();

        foreach (UnitData u in Globals.UNIT_DATA) {
            Material allyDice = new(u.diceMaterial);
            Material enemyDice = new(u.diceMaterial);
            Material allyGhost = new(u.ghostMaterial);
            Material enemyGhost = new(u.ghostMaterial);

            allyDice.name = u.unitClass + "AllyMaterial";
            enemyDice.name = u.unitClass + "EnemyMaterial";
            allyGhost.name = u.unitClass + "AllyMaterial";
            enemyGhost.name = u.unitClass + "EnemyMaterial";

            allyDice.color = u.allyColor;
            enemyDice.color = u.enemyColor;
            allyGhost.color = new Color32(u.allyColor.r, u.allyColor.g, u.allyColor.b, 170);
            enemyGhost.color = new Color32(u.enemyColor.r, u.enemyColor.g, u.enemyColor.b, 170);

            Globals.DICE_MATERIALS.Add((u.unitClass, false), allyDice);
            Globals.DICE_MATERIALS.Add((u.unitClass, true), enemyDice);
            Globals.GHOST_MATERIALS.Add((u.unitClass, false), allyGhost);
            Globals.GHOST_MATERIALS.Add((u.unitClass, true), enemyGhost);
        }

        string levelId = CoreDataHandler.Instance.LevelID;

        // Load game scene data
        GameLevelData.levelId = levelId;
        GameLevelData.Load();
    }

    public static void SaveGameData()
    {
        GameLevelData.levelId = CoreDataHandler.Instance.LevelID;
        GameLevelData.Save(SerializeGameData());
    }

    public static GameLevelData SerializeGameData()
    {
        GameLevelData data = new();
        List<GameUnitData> dice = new();
        foreach (Unit die in Unit.DICE_LIST)
        {
            if (!die.Transform) continue;

            List<GamePathData> movement = new();
            for (int i = 0; i < die.Path.Count; i++)
            {
                GamePathData m = new()
                {
                    path = die.Path[i]
                };
                movement.Add(m);
            }



            GameUnitData d = new GameUnitData()
            {
                isEnemy = die.IsEnemy,
                position = die.GetPosition(),
                faces = die.Faces,
                orientation = die.orientation,
                movesRemaining = die.MovesRemainging,
                path = movement.ToArray(),
            };
            dice.Add(d);
        }

        data.dice = dice.ToArray();
        data.camPosition = Camera.main.transform.position;
        data.camDistance = Camera.main.orthographicSize;
        data.currentPhase = GameManager.Instance.phaseManager.CurrentPhase.Value;
        data.currentRound = GameManager.Instance.currentRound;

        data.alliedOrientations = new DiceOrientationData[GameManager.Instance.AlliedSpawnPositions.Count];
        data.enemyOrientations = new DiceOrientationData[GameManager.Instance.EnemySpawnPositions.Count];
        for (int i = 0; i < GameManager.Instance.AlliedSpawnPositions.Count; i++)
            data.alliedOrientations[i] = GameManager.Instance.AlliedSpawnPositions.ElementAt(i).Value;
        for (int i = 0; i < GameManager.Instance.EnemySpawnPositions.Count; i++)
            data.enemyOrientations[i] = GameManager.Instance.EnemySpawnPositions.ElementAt(i).Value;

        return data;
    }

    public static void DeserializeGameData()
    {
        GameLevelData data = GameLevelData.Instance;
        if (data == null) return;

        foreach (GameUnitData die in data.dice)
        {
            UnitData unitData = Globals.UNIT_DATA.Where((UnitData x) => x.unitClass == die.diceClass).First();
            List<Vector2Int> movement = new();
            for (int i = 0; i < die.path.Length; i++)
                movement.Add(die.path[i].path);

            Unit u = new Unit(unitData, die.isEnemy, die.orientation, die.position, die.movesRemaining, true, movement);
            u.SetPosition(die.position);
            u.Faces = die.faces;
            GameManager.Instance.ImportUnit(u);
        }

        Camera.main.transform.position = data.camPosition;
        Camera.main.orthographicSize = data.camDistance;
    }

    public static void ClearData()
    {
        if (File.Exists(GameLevelData.GetFilePath()))
            File.Delete(GameLevelData.GetFilePath());
    }
}
