using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputMappings
{
    public static Dictionary<KeyCode, UnitAction> UnitActions = new Dictionary<KeyCode, UnitAction>()
    {
        { KeyCode.Keypad4, UnitAction.Left },
        { KeyCode.Keypad6, UnitAction.Right },
        { KeyCode.Keypad8, UnitAction.Up },
        { KeyCode.Keypad2, UnitAction.Down },
        { KeyCode.Keypad7, UnitAction.LeftUp},
        { KeyCode.Keypad1, UnitAction.LeftDown},
        { KeyCode.Keypad9, UnitAction.RightUp},
        { KeyCode.Keypad3, UnitAction.RightDown},
        { KeyCode.Space, UnitAction.Skip},
        { KeyCode.W, UnitAction.Wait}
    };
    //Dictionary<KeyCode, Order> Orders;
}
