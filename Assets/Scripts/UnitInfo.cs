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
    public TileType[] CanMoveOn;

    UnitInfo(UnitType unitType)
    {
        switch (unitType)
        {
            case UnitType.Army:
                ProdTime = 6;
                MovesPerTurn = 1;
                MaxHp = 1;
                CanMoveOn = new TileType[] { TileType.Land };
                break;
            case UnitType.Fighter:
                ProdTime = 12;
                MovesPerTurn = 5;
                MaxHp = 1;
                CanMoveOn = new TileType[] { TileType.Land, TileType.Water };
                break;
            case UnitType.Transport:
                ProdTime = 36;
                MovesPerTurn = 2;
                MaxHp = 3;
                CanMoveOn = new TileType[] { TileType.Water };
                break;
            case UnitType.Destroyer:
                ProdTime = 24;
                MovesPerTurn = 3;
                MaxHp = 3;
                CanMoveOn = new TileType[] { TileType.Water };
                break;
            case UnitType.Submarine:
                ProdTime = 24;
                MovesPerTurn = 2;
                MaxHp = 2;
                CanMoveOn = new TileType[] { TileType.Water };
                break;
            case UnitType.Cruiser:
                ProdTime = 48;
                MovesPerTurn = 2;
                MaxHp = 8;
                CanMoveOn = new TileType[] { TileType.Water };
                break;
            case UnitType.Battleship:
                ProdTime = 60;
                MovesPerTurn = 2;
                MaxHp = 12;
                CanMoveOn = new TileType[] { TileType.Water };
                break;
            case UnitType.Carrier:
                ProdTime = 50;
                MovesPerTurn = 2;
                MaxHp = 8;
                CanMoveOn = new TileType[] { TileType.Water };
                break;
        }
    }
}