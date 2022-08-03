using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


public enum LogType
{
    GAME_SETUP,
    UNIT_SPAWN,
    SERIALIZATION,
    PHASES,
    AI,
}

public class Logging : MonoBehaviour
{
    private void Awake()
    {
        if (!Directory.Exists(Globals.GetLogFolderPath()))
            Directory.CreateDirectory(Globals.GetLogFolderPath());
        File.Create(Path.Combine(Globals.GetLogFolderPath(), "GameSetup.txt"));
        File.Create(Path.Combine(Globals.GetLogFolderPath(), "UnitSpawn.txt"));
        File.Create(Path.Combine(Globals.GetLogFolderPath(), "Serialization.txt"));
        File.Create(Path.Combine(Globals.GetLogFolderPath(), "Phases.txt"));
        File.Create(Path.Combine(Globals.GetLogFolderPath(), "AI.txt"));
        File.Create(Path.Combine(Globals.GetLogFolderPath(), "Unknown.txt"));
    }

    public static void LogNotification(string input, LogType logType)
    {
        string filePath;
        bool shouldPrint;

        switch (logType)
        {
            case LogType.GAME_SETUP:
                filePath = Path.Combine(Globals.GetLogFolderPath(), "GameSetup.txt");
                shouldPrint = Globals.DEBUG_GAME_SETUP;
                break;
            case LogType.AI:
                filePath = Path.Combine(Globals.GetLogFolderPath(), "AI.txt");
                shouldPrint = Globals.DEBUG_AI;
                break;
            case LogType.PHASES:
                filePath = Path.Combine(Globals.GetLogFolderPath(), "Phases.txt");
                shouldPrint = Globals.DEBUG_PHASES;
                break;
            case LogType.SERIALIZATION:
                filePath = Path.Combine(Globals.GetLogFolderPath(), "Serialization.txt");
                shouldPrint = Globals.DEBUG_SERIALIZATION;
                break;
            case LogType.UNIT_SPAWN:
                filePath = Path.Combine(Globals.GetLogFolderPath(), "UnitSpawn.txt");
                shouldPrint = Globals.DEBUG_UNIT_SPAWN;
                break;
            default:
                filePath = Path.Combine(Globals.GetLogFolderPath(), "Unknown.txt");
                shouldPrint = true;
                break;
        }

        if (shouldPrint)
            Debug.Log(input);
        File.AppendAllText(filePath, "\n" + DateTime.Now.ToLongTimeString() + " : " + input);
    }
}
