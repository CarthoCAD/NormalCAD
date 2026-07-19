using System;

namespace NormalCAD.Controller.Commands
{
    [Flags]
    public enum CommandFlags
    {
        None = 0,
        UsePickSet = 1,
        NoUndoMarker = 2,
        Transparent = 4,
        NoMultiple = 8,
        Session = 16
    }
}
