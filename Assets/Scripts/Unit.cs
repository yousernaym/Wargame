using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType { Army, Fighter, Transport, Destroyer, Submarine, Cruiser, Battleship, Carrier }

public class Unit
{
    static Dictionary<UnitType, GameObject> UnitPrefabs;
    Vector2Int pos;
    public Vector2Int Pos 
    {
        get => pos;
        private set
        {
            pos = value;
            gameObject.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
        }
    }
    public UnitInfo UnitInfo { get; private set; }
    public int Damage { get; private set; }
    public UnitType UnitType { get; private set; }
    public int CurrentTurn { get; private set; }

    GameObject gameObject;

    public Unit(UnitType unitType, Vector2Int pos, Player owner)
    {
        if (UnitPrefabs == null)
        {
            UnitPrefabs = new Dictionary<UnitType, GameObject>();
            foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
                UnitPrefabs[type] = Resources.Load<GameObject>("Units/" + Enum.GetName(typeof(UnitType), type));
        }
        UnitType = unitType;
        UnitInfo = UnitInfo.Types[unitType];
        gameObject = GameObject.Instantiate(UnitPrefabs[unitType], owner.GameObject.transform);
        Pos = pos;
    }

    public virtual bool Move()
    {
        Vector2Int newPos = Pos;
        if (Input.GetKeyDown(KeyCode.Keypad1))
            newPos+= new Vector2Int(-1, -1);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            newPos.y += -1;
        Pos = newPos;
        CurrentTurn++;
        return true;
    }
}

public class Army : Unit
{
    public Army(Vector2Int pos, Player owner) : base(UnitType.Army, pos, owner)
    {
    }
}

public class Fighter : Unit
{
    public Fighter(Vector2Int pos, Player owner) : base(UnitType.Fighter, pos, owner)
    {
    }
}

public class Ship : Unit
{
    protected bool canAttackArmies;
    public Ship(UnitType unitType, Vector2Int pos, Player owner, bool canAttackArmies) : base(unitType, pos, owner)
    {
        this.canAttackArmies = canAttackArmies;
    }
}

public class Transport : Ship
{
    public Transport(Vector2Int pos, Player owner) : base(UnitType.Transport, pos, owner, false)
    {
    }
}

public class Destroyer : Ship
{
    public Destroyer(Vector2Int pos, Player owner) : base(UnitType.Destroyer, pos, owner, false)
    {
    }
}

public class Submarine : Ship
{
    public Submarine(Vector2Int pos, Player owner) : base(UnitType.Submarine, pos, owner, false)
    {
    }
}

public class Cruiser : Ship
{
    public Cruiser(Vector2Int pos, Player owner) : base(UnitType.Cruiser, pos, owner, true)
    {
    }
}

public class Battleship : Ship
{
    public Battleship(Vector2Int pos, Player owner) : base(UnitType.Battleship, pos, owner, true)
    {
    }
}



public class Carrier : Ship
{
    public Carrier(Vector2Int pos, Player owner) : base(UnitType.Carrier, pos, owner, false)
    {
    }
}
