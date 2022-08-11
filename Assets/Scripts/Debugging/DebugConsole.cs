using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public enum DebugMapType {
    Coord,
    PathSingle,
    PathKnight,
    Off,
}

public class DebugConsole : MonoBehaviour
{
    private bool _showConsole = false;
    private string _consoleInput;
    private float _time;
    [SerializeField]
    private float _exitTime = 0.5f;

    private enum DisplayType
    {
        None,
        Help,
        Autocomplete
    }

    private DisplayType _displayType = DisplayType.None;
    private static GUIStyle _logStyle;

    public static event Action<DebugMapType> DebugMap;
    public static event Action DebugNames;

    private void Awake()
    {
        _time = 0f;

        new DebugCommand("?", "Lists all available debug commands.", "?", () =>
        {
            _displayType = DisplayType.Help;
        });
        new DebugCommand<string>(
            "debug_map",
            "Toggles grid location text on/off.",
            "debug_map [Coord|PathSingle|PathKnight]",
            s => {
                DebugMap?.Invoke(Enum.Parse<DebugMapType>(s, true));
            }
        );
        new DebugCommand("debug_names", "Toggles unit names on/off.", "debug_names", () =>
        {
            DebugNames?.Invoke();
        });
        new DebugCommand("kill", "Kills the selected unit.", "kill", () =>
        {
            Globals.SELECTED_UNIT.Kill();
        });
        new DebugCommand("phase", "Print the current phase information.", "phase", () =>
        {
            Debug.Log(GameManager.Instance.phaseManager.StackString());
            Debug.Log(Utilities.EnumerableString(GameManager.Instance.phaseManager.CurrentPhaseResults()));
        });
        new DebugCommand("debug_game_setup", "Print game setup and cleanup debugging.", "debug_game_setup", () =>
        {
            Globals.DEBUG_GAME_SETUP = !Globals.DEBUG_GAME_SETUP;
        });
        new DebugCommand("debug_ai", "Print AI debugging.", "debug_ai", () =>
        {
            Globals.DEBUG_AI = !Globals.DEBUG_AI;
        });
        new DebugCommand("debug_phases", "Print phase debugging.", "debug_phases", () =>
        {
            Globals.DEBUG_PHASES = !Globals.DEBUG_PHASES;
        });
        new DebugCommand("debug_serialization", "Print serialization debugging.", "debug_serialization", () =>
        {
            Globals.DEBUG_SERIALIZATION = !Globals.DEBUG_SERIALIZATION;
        });
        new DebugCommand("debug_unit_spawn", "Print unit spawning debugging.", "debug_unit_spawn", () =>
        {
            Globals.DEBUG_UNIT_SPAWN = !Globals.DEBUG_UNIT_SPAWN;
        });
        new DebugCommand("win", "Win the game.", "win", () =>
        {
            GameManager.Instance.CheckWin(true);
        });
        new DebugCommand<float, float, float, float>("set_orientation_quaternion", "Sets the orientation to a quaternion.", "set_orientation_quaternion", (q1, q2, q3, q4) =>
        {
            Globals.SELECTED_UNIT.DieRotator.SetRotation(new Quaternion(q1, q2, q3, q4));
            Globals.SELECTED_UNIT.DieRotator.RotateNow();
            Debug.Log("Garfeel running");
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
            _OnShowDebugConsole();
    }

    private void OnGUI()
    {
        _time += Time.deltaTime;
        if (_logStyle == null)
        {
            _logStyle = new GUIStyle(GUI.skin.label);
            _logStyle.fontSize = 12;
        }

        if (_showConsole)
        {
            TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

            string newInput;
            GUI.SetNextControlName("ConsoleField");
            newInput = GUI.TextField(new Rect(0, 0, Screen.width, 24), _consoleInput);
            if (newInput != null)
                newInput = Regex.Replace(newInput, @"[^a-zA-Z0-9 _?.-]", "");
            GUI.FocusControl("ConsoleField");

            float y = 24;
            GUI.Box(new Rect(0, y, Screen.width, Screen.height - 24), "");
            if (_displayType == DisplayType.Help)
                _ShowHelp(y);
            else if (_displayType == DisplayType.Autocomplete)
                newInput = _ShowAutocomplete(y, newInput);

            if (_displayType != DisplayType.None && _consoleInput.Length != newInput.Length)
                _displayType = DisplayType.None;

            _consoleInput = newInput;

            Event e = Event.current;
            if (e.isKey)
            {
                if (e.keyCode == KeyCode.Return)
                {
                    _OnReturn();
                }
                else if (e.keyCode == KeyCode.Escape | (e.keyCode == KeyCode.BackQuote && _time > _exitTime))
                {
                    _showConsole = false;
#if UNITY_EDITOR
                    StartCoroutine("ConsoleDelay");
#endif
                }
                else if (e.keyCode == KeyCode.Tab)
                {
                    _displayType = DisplayType.Autocomplete;
                }
            }

            if (te != null)
            {
                //these two lines prevent a "select all" effect on the textfield which seems to be the default GUI.FocusControl behavior
                te.SelectNone();
                te.MoveTextEnd();
            }
        }
    }

    private void _OnShowDebugConsole()
    {
        _time = 0f;
        _showConsole = true;
    }

    private void _ShowHelp(float y)
    {
        foreach (DebugCommandBase command in DebugCommandBase.DebugCommands.Values)
        {
            GUI.Label(
                new Rect(2, y, Screen.width, 20),
                $"{command.Format} - {command.Description}"
            );
            y += 16;
        }
    }

    private string _ShowAutocomplete(float y, string newInput)
    {
        IEnumerable<string> autocompleteCommands =
            DebugCommandBase.DebugCommands.Keys
            .Where(k => k.StartsWith(newInput.ToLower()));
        int numMatchs = 0; string match = null;
        foreach (string k in autocompleteCommands)
        {
            DebugCommandBase c = DebugCommandBase.DebugCommands[k];
            GUI.Label(
                new Rect(2, y, Screen.width, 20),
                $"{c.Format} - {c.Description}",
                _logStyle
            );
            match = k;
            y += 16;
            numMatchs++;
        }
        if (numMatchs == 1 && match != null)
        {
            _displayType = DisplayType.None;
            GUI.FocusControl("null");
            return match;
        }
        return newInput;
    }

    private void _OnReturn()
    {
        _HandleConsoleInput();
        _consoleInput = "";
    }

    private void _HandleConsoleInput()
    {
        // parse input
        string[] inputParts = _consoleInput.Split(' ');
        string mainKeyword = inputParts[0];
        // check against available commands
        DebugCommandBase command;
        if (DebugCommandBase.DebugCommands.TryGetValue(mainKeyword.ToLower(), out command))
        {
            // try to invoke command if it exists
            if (command is DebugCommand dc)
                dc.Invoke();
            else
            {
                if (inputParts.Length < 2)
                {
                    Debug.LogError("Missing parameter!");
                    return;
                }

                if (command is DebugCommand<string> dcString)
                {
                    dcString.Invoke(inputParts[1]);
                }
                else if (command is DebugCommand<int> dcInt)
                {
                    if (int.TryParse(inputParts[1], out int i))
                        dcInt.Invoke(i);
                    else
                    {
                        Debug.LogError($"'{command.Id}' requires an int parameter!");
                        return;
                    }
                }
                else if (command is DebugCommand<float> dcFloat)
                {
                    if (float.TryParse(inputParts[1], out float f))
                        dcFloat.Invoke(f);
                    else
                    {
                        Debug.LogError($"'{command.Id}' requires a float parameter!");
                        return;
                    }
                }
                else if (command is DebugCommand<string, int> dcStringInt)
                {
                    if (int.TryParse(inputParts[2], out int i))
                        dcStringInt.Invoke(inputParts[1], i);
                    else
                    {
                        Debug.LogError($"'{command.Id}' requires a string and an int parameter!");
                        return;
                    }
                }
                else if (command is DebugCommand<float, float, float, float> dcFloatFloatFloatFloat)
                {
                    if (float.TryParse(inputParts[1], out float f1)
                        && float.TryParse(inputParts[2], out float f2)
                        && float.TryParse(inputParts[3], out float f3)
                        && float.TryParse(inputParts[4], out float f4))
                        dcFloatFloatFloatFloat.Invoke(f1, f2, f3, f4);
                    else
                    {
                        Debug.LogError($"'{command.Id}' requires four float parameters!");
                        return;
                    }
                }
            }
        }
    }

    IEnumerator ConsoleDelay()
    {
        yield return new WaitForSeconds(_exitTime);
    }
}
