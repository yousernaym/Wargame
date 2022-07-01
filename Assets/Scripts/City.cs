using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class City
{
    public Vector2Int Pos { get; private set; }
    public int ProdTime { get; set; }
    public Player Owner { get; set; }

    public City(Player owner, int x, int y)
    {
        this.Owner = owner;
        Pos = new Vector2Int(x, y);
    }

    UnitType production;
    public UnitType Production
    {
        get => production;
        set
        {
            production = value;
            ProdTime = (int)(UnitInfo.Types[value].ProdTime / Owner.ProdEfficiency);
        }
    }

    public bool IsProdDone()
    {
        if (ProdTime < 0)
            throw new ArgumentOutOfRangeException(nameof(ProdTime));
        return ProdTime == 0;
    }

    public Unit CreateUnit()
    {
        switch (Production)
        {
            case UnitType.Army:
                return new Army(Pos, Owner);
            case UnitType.Fighter:
                return new Fighter(Pos, Owner);
            case UnitType.Transport:
                return new Transport(Pos, Owner);
            case UnitType.Destroyer:
                return new Destroyer(Pos, Owner);
            case UnitType.Submarine:
                return new Submarine(Pos, Owner);
            case UnitType.Cruiser:
                return new Cruiser(Pos, Owner);
            case UnitType.Battleship:
                return new Battleship(Pos, Owner);
            case UnitType.Carrier:
                return new Carrier(Pos, Owner);
            default: throw new NotImplementedException();
        }
    }
}