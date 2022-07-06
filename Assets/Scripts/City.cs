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

    public List<Unit> Units { get; private set; } = new List<Unit>();
    public Unit ActiveUnit => Units.Find(unit => unit.IsActive);

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
        Unit newUnit = null;
        switch (Production)
        {
            case UnitType.Army:
                newUnit = new Army(Pos, Owner);
                break;
            case UnitType.Fighter:
                newUnit = new Fighter(Pos, Owner);
                break;
            case UnitType.Transport:
                newUnit = new Transport(Pos, Owner);
                break;
            case UnitType.Destroyer:
                newUnit = new Destroyer(Pos, Owner);
                break;
            case UnitType.Submarine:
                newUnit = new Submarine(Pos, Owner);
                break;
            case UnitType.Cruiser:
                newUnit = new Cruiser(Pos, Owner);
                break;
            case UnitType.Battleship:
                newUnit = new Battleship(Pos, Owner);
                break;
            case UnitType.Carrier:
                newUnit = new Carrier(Pos, Owner);
                break;
            default: throw new NotImplementedException();
        }
        Units.Add(newUnit);
        return newUnit;
    }
}