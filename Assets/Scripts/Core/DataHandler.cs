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
        GameLevelData data = new GameLevelData();
        List<GameUnitData> dice = new List<GameUnitData>();
        foreach (Unit die in Unit.DICE_LIST)
        {
            if (!die.Transform) continue;

            List<GameFaceData> faceData = new List<GameFaceData>();
            foreach (Face face in die.Faces)
            {
                GameFaceData f = new GameFaceData()
                {
                    state = face.state,
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
                orientation = die.orientation,
            };
            dice.Add(d);
        }

        data.dice = dice.ToArray();
        data.camPosition = Camera.main.transform.position;

        return data;
    }

    public static void DeserializeGameData()
    {
        GameLevelData data = GameLevelData.Instance;
        if (data == null) return;

        foreach (GameUnitData die in data.dice)
        {
            UnitData unitData = Globals.UNIT_DATA.Where((UnitData x) => x.unitClass == die.diceClass).First();
            Unit u = new Unit(unitData, die.isEnemy, die.orientation, die.position);
            u.SetPosition(die.position);
            for (int n = 0; n < die.faces.Length; n++)
            {
                u.Faces[n].state = die.faces[n].state;
                u.Faces[n].position = die.faces[n].position;
            }
                       
            GameManager.Instance.ImportUnit(u);
        }

        Debug.Log(data.camPosition);
        Camera.main.transform.position = data.camPosition;
        data.hasData = true;
    }
}
