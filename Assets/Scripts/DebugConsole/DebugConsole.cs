using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

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

    public static event Action DebugMap;
    public static event Action DebugNames;

    private void Awake()
    {
        _time = 0f;

        new DebugCommand("?", "Lists all available debug commands.", "?", () =>
        {
            _displayType = DisplayType.Help;
        });

        new DebugCommand("debug_map", "Toggles grid location text on/off.", "debug_map", () =>
        {
            DebugMap?.Invoke();
        });
        new DebugCommand("debug_names", "Toggles unit names on/off.", "debug_names", () =>
        {
            DebugNames?.Invoke();
        });
        new DebugCommand("kill", "Kills the selected unit.", "kill", () =>
        {
            Globals.SELECTED_UNIT.Kill();
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
                newInput = Regex.Replace(newInput, @"[^a-zA-Z0-9 _?]", "");
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
                    int i;
                    if (int.TryParse(inputParts[1], out i))
                        dcInt.Invoke(i);
                    else
                    {
                        Debug.LogError($"'{command.Id}' requires an int parameter!");
                        return;
                    }
                }
                else if (command is DebugCommand<float> dcFloat)
                {
                    float f;
                    if (float.TryParse(inputParts[1], out f))
                        dcFloat.Invoke(f);
                    else
                    {
                        Debug.LogError($"'{command.Id}' requires a float parameter!");
                        return;
                    }
                }
                else if (command is DebugCommand<string, int> dcStringInt)
                {
                    int i;
                    if (int.TryParse(inputParts[2], out i))
                        dcStringInt.Invoke(inputParts[1], i);
                    else
                    {
                        Debug.LogError($"'{command.Id}' requires a string and an int parameter!");
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
