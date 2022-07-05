using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInfo
{
    static Dictionary<UnitType, UnitInfo> types;
    static public Dictionary<UnitType, UnitInfo> Types
    {
        get
        {
            if (types != null)
                return types;
            types = new Dictionary<UnitType, UnitInfo>();
            foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
                types.Add(unitType, new UnitInfo(unitType));
            return types;
        }
    }

    public int ProdTime { get; private set; }
    public int MovesPerTurn { get; private set; }
    public int MaxHp { get; private set; }
    public UnitType[] AttackTargets { get; set; }
    public TileType[] MoveTargets { get; set; }

    UnitInfo(UnitType unitType)
    {
        switch (unitType)
        {
            case UnitType.Army:
                ProdTime = 3;
                MovesPerTurn = 1;
                MaxHp = 1;
                break;
            case UnitType.Fighter:
                ProdTime = 6;
                MovesPerTurn = 5;
                MaxHp = 1;
                break;
            case UnitType.Transport:
                ProdTime = 18;
                MovesPerTurn = 2;
                MaxHp = 3;
                break;
            case UnitType.Destroyer:
                ProdTime = 12;
                MovesPerTurn = 3;
                MaxHp = 3;
                break;
            case UnitType.Submarine:
                ProdTime = 12;
                MovesPerTurn = 2;
                MaxHp = 2;
                break;
            case UnitType.Cruiser:
                ProdTime = 24;
                MovesPerTurn = 2;
                MaxHp = 8;
                break;
            case UnitType.Battleship:
                ProdTime = 30;
                MovesPerTurn = 2;
                MaxHp = 12;
                break;
            case UnitType.Carrier:
                ProdTime = 25;
                MovesPerTurn = 2;
                MaxHp = 8;
                break;
        }
    }
}