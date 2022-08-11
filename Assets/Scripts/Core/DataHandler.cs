using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;


public class DataHandler : MonoBehaviour
{
    public event Action<Win> WinEvent;

    private void Start()
    {
        GameManager.Instance.WinEvent += (_) => ClearData();
    }

    public static async UniTask LoadGameData()
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

        Globals.ORIENTATION_TO_EULERS = new();

        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0000, new Vector3(-20.705f, 49.107f, 67.792f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0001, new Vector3(-20.705f, -49.107f, 112.208f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0002, new Vector3(-200.705f, 49.107f, 292.208f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0003, new Vector3(-200.705f, -49.107f, 247.792f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0100, new Vector3(159.295f, 49.107f, 112.208f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0101, new Vector3(159.295f, -49.107f, 67.792f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0102, new Vector3(-20.705f, 49.107f, -112.208f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0103, new Vector3(-20.705f, -49.107f, -67.792f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0200, new Vector3(-20.705f, 49.107f, -22.208f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0201, new Vector3(-200.705f, -49.107f, 157.792f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0202, new Vector3(20.705f, -130.893f, 22.208f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0203, new Vector3(-20.705f, -49.107f, 22.208f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0300, new Vector3(159.295f, 49.107f, 22.208f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0301, new Vector3(-20.705f, 49.107f, 157.792f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0302, new Vector3(-200.705f, -49.107f, 337.792f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0303, new Vector3(-200.705f, -49.107f, 157.792f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0400, new Vector3(-120f, 0f, 135f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0401, new Vector3(-120f, 0f, 45f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0402, new Vector3(-120f, 0f, 315f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0403, new Vector3(-120f, 0f, 225f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0500, new Vector3(60f, 0f, 135f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0501, new Vector3(60f, 0f, 45f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0502, new Vector3(60f, 0f, 315f));
        Globals.ORIENTATION_TO_EULERS.Add(DiceOrientation.C0503, new Vector3(60f, 0f, 225f));

        Globals.EULERS_TO_ORIENTATION = new();

        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-20.705f, 49.107f, 67.792f), DiceOrientation.C0000);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(339.30f, 49.11f, 67.79f), DiceOrientation.C0000);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-20.705f, -49.107f, 112.208f), DiceOrientation.C0001);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(339.30f, 310.89f, 112.21f), DiceOrientation.C0001);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-200.705f, 49.107f, 292.208f), DiceOrientation.C0002);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-200.705f, -49.107f, 247.792f), DiceOrientation.C0003);

        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(159.295f, 49.107f, 112.208f), DiceOrientation.C0100);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(159.295f, -49.107f, 67.792f), DiceOrientation.C0101);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(20.70f, 130.89f, 247.79f), DiceOrientation.C0101);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-20.705f, 49.107f, -112.208f), DiceOrientation.C0102);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(339.30f, 49.11f, 247.79f), DiceOrientation.C0102);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-20.705f, -49.107f, -67.792f), DiceOrientation.C0103);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(339.30f, 310.89f, 292.21f), DiceOrientation.C0103);

        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-20.705f, 49.107f, -22.208f), DiceOrientation.C0200);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(339.30f, 49.11f, 337.79f), DiceOrientation.C0200);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-200.705f, -49.107f, 157.792f), DiceOrientation.C0201);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(20.705f, -130.893f, 22.208f), DiceOrientation.C0202);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(20.705f, 130.893f, 337.79f), DiceOrientation.C0202);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-20.705f, -49.107f, 22.208f), DiceOrientation.C0203);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(339.29f, 310.89f, 22.21f), DiceOrientation.C0203);

        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(159.295f, 49.107f, 22.208f), DiceOrientation.C0300);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-20.705f, 49.107f, 157.792f), DiceOrientation.C0301);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(339.3f, 49.11f, 157.79f), DiceOrientation.C0301);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-200.705f, -49.107f, 337.792f), DiceOrientation.C0302);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(159.295f, -49.107f, -22.208f), DiceOrientation.C0302);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(20.70f, 130.89f, 157.79f), DiceOrientation.C0302);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(339.295f, -49.107f, -157.792f), DiceOrientation.C0303);

        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-120f, 0f, 135f), DiceOrientation.C0400);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-120f, 0f, 45f), DiceOrientation.C0401);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(300f, 180f, 135f), DiceOrientation.C0401);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-120f, 0f, 315f), DiceOrientation.C0402);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(20.70f, 229.11f, 112.21f), DiceOrientation.C0402);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(-120f, 0f, 225f), DiceOrientation.C0403);

        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(60f, 0f, 135f), DiceOrientation.C0500);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(60f, 0f, 45f), DiceOrientation.C0501);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(60f, 0f, 315f), DiceOrientation.C0502);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(60f, 0f, -45f), DiceOrientation.C0502);
        Globals.EULERS_TO_ORIENTATION.Add(new Vector3(60f, 0f, 225f), DiceOrientation.C0503);

        Globals.QUATERNION_TO_ORIENTATION = new();

        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.3651079f, -0.2507781f, 0.560671f, 0.6996103f), DiceOrientation.C0000);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.0923367f, -0.4304636f, -0.4370117f, -0.7843442f), DiceOrientation.C0000);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.4304627f, -0.0922953f, 0.7843471f, 0.4370161f), DiceOrientation.C0001);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.701057f, 0.5609835f, 0.2481507f, 0.3636452f), DiceOrientation.C0002);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.5609835f, 0.701057f, -0.3636452f, -0.2481507f), DiceOrientation.C0003);

        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.5609835f, -0.701057f, 0.3636452f, -0.2481507f), DiceOrientation.C0100);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.701057f, -0.5609835f, -0.2481507f, 0.3636452f), DiceOrientation.C0101);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.4304437f, 0.0923295f, -0.784343f, 0.4370349f), DiceOrientation.C0102);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.2317429f, -0.3449218f, -0.5072575f, 0.7549927f), DiceOrientation.C0103);

        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.2391205f, 0.3696458f, -0.2456001f, 0.8636342f), DiceOrientation.C0200);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.0990469f, 0.8923974f, -0.081667f, -0.432605f), DiceOrientation.C0201);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.0990469f, -0.8923974f, -0.081667f, 0.432605f), DiceOrientation.C0202);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.0990625f, -0.8923957f, -0.0816594f, -0.4326064f), DiceOrientation.C0202);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.2391205f, -0.3696458f, 0.2456001f, 0.8636342f), DiceOrientation.C0203);

        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.8923974f, -0.0990469f, 0.432605f, 0.081667f), DiceOrientation.C0300);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.3696458f, 0.2391205f, 0.8636342f, 0.2456001f), DiceOrientation.C0301);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.3696458f, -0.2391205f, -0.8636342f, -0.2456001f), DiceOrientation.C0301);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.8923974f, 0.0990469f, -0.432605f, 0.081667f), DiceOrientation.C0302);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.8923895f, 0.0990751f, 0.4326247f, -0.0816141f), DiceOrientation.C0302);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.3696458f, 0.2391205f, 0.8636342f, -0.2456001f), DiceOrientation.C0303);

        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.3314136f, 0.8001031f, 0.4619398f, 0.1913417f), DiceOrientation.C0400);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.8001031f, 0.3314136f, 0.1913417f, 0.4619398f), DiceOrientation.C0401);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.8001031f, -0.3314136f, 0.1913417f, -0.4619398f), DiceOrientation.C0401);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.8001031f, 0.3314136f, 0.1913417f, -0.4619398f), DiceOrientation.C0402);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.7010714f, 0.560958f, -0.248202f, -0.3636217f), DiceOrientation.C0402);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.3314136f, 0.8001031f, 0.4619398f, -0.1913417f), DiceOrientation.C0403);

        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.1913417f, -0.4619398f, 0.8001031f, 0.3314136f), DiceOrientation.C0500);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.4619398f, -0.1913417f, 0.3314136f, 0.8001031f), DiceOrientation.C0501);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.4619398f, -0.1913417f, 0.3314136f, -0.8001031f), DiceOrientation.C0502);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(0.4619398f, 0.1913417f, -0.3314136f, 0.8001031f), DiceOrientation.C0502);
        Globals.QUATERNION_TO_ORIENTATION.Add(new Quaternion(-0.1913417f, -0.4619398f, 0.8001031f, -0.3314136f), DiceOrientation.C0503);


        // Load game scene data
        GameLevelData.levelId = CoreDataHandler.Instance.LevelID;
        GameLevelData.Load();

        // Setup debugging if in 

        await UniTask.Yield();
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

            GameUnitData d = new()
            {
                isEnemy = die.IsEnemy,
                position = die.GetPosition(),
                faces = die.Faces,
                orientation = die.Orientation,
                movesRemaining = die.MovesRemainging,
                path = movement.ToArray(),
                uid = die.Uid,
            };
            dice.Add(d);
        }

        List<string> movedPiecesByUid = new();
        foreach (UnitManager manager in GameManager.Instance.MovedPieces)
            movedPiecesByUid.Add(manager.Unit.Uid);

        data.dice = dice.ToArray();
        data.camPosition = Camera.main.transform.position;
        data.camDistance = Camera.main.orthographicSize;
        data.currentPhase = GameManager.Instance.phaseManager.CurrentPhase.Value;
        data.currentRound = GameManager.Instance.CurrentRound;
        data.movedPieces = movedPiecesByUid.ToArray();
        data.playerPiecesMoved = GameManager.Instance.PlayerPiecesMoved;

        data.alliedOrientations = new DiceOrientation[GameManager.Instance.AlliedSpawnPositions.Count];
        data.enemyOrientations = new DiceOrientation[GameManager.Instance.EnemySpawnPositions.Count];
        for (int i = 0; i < GameManager.Instance.AlliedSpawnPositions.Count; i++)
            data.alliedOrientations[i] = GameManager.Instance.AlliedSpawnPositions.ElementAt(i).Value;
        for (int i = 0; i < GameManager.Instance.EnemySpawnPositions.Count; i++)
            data.enemyOrientations[i] = GameManager.Instance.EnemySpawnPositions.ElementAt(i).Value;

        return data;
    }

    public static async UniTask DeserializeGameData()
    {
        GameLevelData data = GameLevelData.Instance;
        if (data == null) return;

        foreach (GameUnitData die in data.dice)
        {
            UnitData unitData = Globals.UNIT_DATA.Where((UnitData x) => x.unitClass == die.diceClass).First();
            List<Vector2Int> movement = new();
            for (int i = 0; i < die.path.Length; i++)
                movement.Add(die.path[i].path);

            Unit u = new(unitData, die.isEnemy, die.orientation, die.position, die.movesRemaining, true, movement);
            u.SetPosition(die.position);
            u.Faces = die.faces;
            u.Uid = die.uid;
            foreach (string movedUnit in data.movedPieces)
            {
                if (movedUnit == u.Uid)
                    GameManager.Instance.MovedPieces.Add(u.UnitManager);
            }

            GameManager.Instance.ImportUnit(u);
        }

        Camera.main.transform.position = data.camPosition;
        Camera.main.orthographicSize = data.camDistance;
        GameManager.Instance.CurrentRound = data.currentRound;


        await UniTask.Yield();
    }

    public static void ClearData()
    {
        if (File.Exists(GameLevelData.GetFilePath()))
            File.Delete(GameLevelData.GetFilePath());
    }
}
