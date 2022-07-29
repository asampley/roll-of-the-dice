using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataHandler : MonoBehaviour
{
    private void Start()
    {
        DeserializeGameData();
    }

    public static void LoadGameData()
    {
        Globals.UNIT_DATA = Resources.LoadAll<UnitData>(Globals.DICE_CLASS_SO) as UnitData[];

        string gameUid = CoreDataHandler.Instance.LevelID;

        // Load game scene data
        GameData.levelId = gameUid;
        GameData.Load();
    }

    public static void SaveGameData()
    {
        GameData.levelId = CoreDataHandler.Instance.LevelID;
        GameData.Save(SerializeGameData());
    }

    public static GameData SerializeGameData()
    {
        GameData data = new GameData();
        List<GameUnitData> dice = new List<GameUnitData>();
        foreach (Unit die in Unit.DICE_LIST)
        {
            if (!die.Transform) continue;

            List<GameFaceData> faceData = new List<GameFaceData>();
            foreach (Face face in die.Faces)
            {
                GameFaceData f = new GameFaceData()
                {
                    diceState = face.state,
                    position = face.position,
                };
                faceData.Add(f);
            }
                

            Dictionary<int, Vector3> dicVectors = new Dictionary<int, Vector3>();
            for (int n = 0; n < die.Faces.Length; n++)
                dicVectors.Add(n, die.Faces[n].position);
  
            GameUnitData d = new GameUnitData()
            {
                isEnemy = die.IsEnemy,
                position = die.GetPosition(),
                faces = faceData.ToArray(),
                orientation = die.Orientation,
            };

            dice.Add(d);
        }

        data.dice = dice.ToArray();
        data.camPosition = Camera.main.transform.position;

        return data;
    }

    public static void DeserializeGameData()
    {
        GameData data = GameData.Instance;
        if (data == null) return;

        foreach (GameUnitData die in data.dice)
        {
            UnitData unitData = Globals.UNIT_DATA.Where((UnitData x) => x.unitClass == die.diceClass).First();
            Unit u = new Unit(unitData, die.isEnemy, die.orientation);

            u.SetPosition(die.position);
            for (int n = 0; n < die.faces.Length; n++)
            {
                u.Faces[n].state = die.faces[n].diceState;
                u.Faces[n].position = die.faces[n].position;
            }
            GameManager.Instance.ImportUnit(u);
        }

        Camera.main.transform.position = data.camPosition;
    }
}
