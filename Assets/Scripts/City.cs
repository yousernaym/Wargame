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
            ProdTime = (int)(UnitInfo.Types[value].ProdTime * Owner.ProdFactor);
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
                return new Army(Pos);
            case UnitType.Fighter:
                return new Fighter(Pos);
            case UnitType.Transport:
                return new Transport(Pos);
            case UnitType.Destroyer:
                return new Destroyer(Pos);
            case UnitType.Submarine:
                return new Submarine(Pos);
            case UnitType.Cruiser:
                return new Cruiser(Pos);
            case UnitType.Battleship:
                return new Battleship(Pos);
            case UnitType.Carrier:
                return new Carrier(Pos);
            default: throw new NotImplementedException();
        }
    }
}