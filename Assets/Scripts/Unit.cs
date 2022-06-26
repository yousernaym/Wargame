using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType { Army, Fighter, Transport, Destroyer, Submarine, Cruiser, Battleship, Carrier }

public class Unit
{
    Vector2Int pos;
    public UnitInfo UnitInfo { get; private set; }
    public int Damage { get; private set; }
    public UnitType UnitType { get; private set; }
    protected Unit(UnitType unitType)
    {
        UnitType = unitType;
        UnitInfo = UnitInfo.Types[unitType];
    }
}

public class Army : Unit
{
    public Army() : base(UnitType.Army)
    {
    }
}

public class Fighter : Unit
{
    public Fighter() : base(UnitType.Fighter)
    {
    }
}

public class Ship : Unit
{
    protected bool canAttackArmies;
    public Ship(UnitType unitType, bool canAttackArmies) : base(unitType)
    {
        this.canAttackArmies = canAttackArmies;
    }
}

public class Transport : Ship
{
    public Transport() : base(UnitType.Transport, false)
    {
    }
}

public class Destroyer : Ship
{
    public Destroyer() : base(UnitType.Destroyer, false)
    {
    }
}

public class Submarine : Ship
{
    public Submarine() : base(UnitType.Submarine, false)
    {
    }
}

public class Cruiser : Ship
{
    public Cruiser() : base(UnitType.Cruiser, true)
    {
    }
}

public class Battleship : Ship
{
    public Battleship() : base(UnitType.Battleship, true)
    {
    }
}



public class Carrier : Ship
{
    public Carrier() : base(UnitType.Carrier, false)
    {
    }
}
