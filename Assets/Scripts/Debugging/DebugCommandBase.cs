using System;
using System.Collections.Generic;

public class DebugCommandBase
{
    public static Dictionary<string, DebugCommandBase> DebugCommands;

    private string _id;
    private string _description;
    private string _format;

    public DebugCommandBase(string id, string description, string format)
    {
        _id = id;
        _description = description;
        _format = format;

        if (DebugCommands == null)
            DebugCommands = new Dictionary<string, DebugCommandBase>();
        string mainKeyword = format.Split(' ')[0];
        DebugCommands[mainKeyword] = this;
    }

    public string Id => _id;
    public string Description => _description;
    public string Format => _format;

}