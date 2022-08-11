using System;
using UnityEngine;

public class DebugCommand : DebugCommandBase
{
    private readonly Action _action;

    public DebugCommand(string id, string description, string format, Action action) : base(id, description, format)
    {
        _action = action;
    }

    public void Invoke()
    {
        _action.Invoke();
    }
}


public class DebugCommand<T> : DebugCommandBase
{
    private readonly Action<T> _action;

    public DebugCommand(string id, string description, string format, Action<T> action) : base(id, description, format)
    {
        _action = action;
    }

    public void Invoke(T value)
    {
        _action.Invoke(value);
    }
}


public class DebugCommand<T1, T2> : DebugCommandBase
{
    private readonly Action<T1, T2> _action;

    public DebugCommand(string id, string description, string format, Action<T1, T2> action) : base(id, description, format)
    {
        _action = action;
    }

    public void Invoke(T1 v1, T2 v2)
    {
        _action.Invoke(v1, v2);
    }
}

public class DebugCommand<T1, T2, T3, T4> : DebugCommandBase
{
    private readonly Action<T1, T2, T3, T4> _action;

    public DebugCommand(string id, string description, string format, Action<T1, T2, T3, T4> action) : base(id, description, format)
    {
        _action = action;
    }

    public void Invoke(T1 v1, T2 v2, T3 v3, T4 v4)
    {
        _action.Invoke(v1, v2, v3, v4);
    }
}
