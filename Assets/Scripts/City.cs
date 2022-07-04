using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class City : ISerializable
{
    public Vector2Int Pos { get; private set; }
    public int ProdTime { get; set; }
    public Player Owner { get; set; }

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

    public City(Player owner, int x, int y)
    {
        this.Owner = owner;
        Pos = new Vector2Int(x, y);
    }

    public City(SerializationInfo info, StreamingContext ctxt)
    {
        foreach (SerializationEntry entry in info)
        {
            if (entry.Name == "Pos")
                Pos = (Vector2Int)entry.Value;
            else if (entry.Name == "ProdTime")
                ProdTime = (int)entry.Value;
            else if (entry.Name == "Production")
                production = (UnitType)entry.Value;
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("Pos", Pos);
        info.AddValue("ProdTime", ProdTime);
        info.AddValue("Production", Production);
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