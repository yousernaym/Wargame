using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType { Army, Fighter, Transport, Destroyer, Submarine, Cruiser, Battleship, Carrier }
public enum UnitAction { Left, Right, Up, Down, LeftUp, RightUp, LeftDown, RightDown, Skip, Wait}

public class Unit
{
    static Dictionary<UnitType, GameObject> UnitPrefabs;
    public Unit Container { get; private set; }
    Vector2Int pos;
    public Vector2Int Pos 
    {
        get => pos;
        private set
        {
            if (pos != null)
                Owner.Map[pos.x, pos.y].Unit = null;
            Owner.Map[value.x, value.y].Unit = this;
            pos = value;
            gameObject.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
            Owner.ExploreMap(value);
        }
    }
    public UnitInfo UnitInfo { get; private set; }
    public int Hp { get; private set; }
    public int RemainingMoves { get; private set; }
    public UnitType Type { get; private set; }
    public int CurrentTurn { get; private set; }
    public Player Owner { get; private set; }

    GameObject gameObject;

    public Unit(UnitType unitType, Vector2Int pos, Player owner)
    {
        if (UnitPrefabs == null)
        {
            UnitPrefabs = new Dictionary<UnitType, GameObject>();
            foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
                UnitPrefabs[type] = Resources.Load<GameObject>("Units/" + Enum.GetName(typeof(UnitType), type));
        }
        Type = unitType;
        UnitInfo = UnitInfo.Types[unitType];
        Hp = UnitInfo.MaxHp;
        RemainingMoves = UnitInfo.MovesPerTurn;
        gameObject = GameObject.Instantiate(UnitPrefabs[unitType], owner.GameObject.transform);
        Owner = owner;
        Pos = pos;
        CurrentTurn = owner.CurrentTurn;
        owner.Map.SetUnit(this);
    }

    protected virtual bool CanMove(Vector2Int pos)
    {
        return false;
    }

    public bool ExecuteAction(UnitAction action)
    {
        Vector2Int newPos = Pos;
        if (action == UnitAction.Left)
            newPos.x -= 1;
        else if (action == UnitAction.Right)
            newPos.x += 1;
        else if (action == UnitAction.Up)
            newPos.y += 1;
        else if (action == UnitAction.Down)
            newPos.y -= 1;
        else if (action == UnitAction.LeftUp)
            newPos += new Vector2Int(-1, 1);
        else if (action == UnitAction.RightUp)
            newPos += new Vector2Int(1, 1);
        else if (action == UnitAction.LeftDown)
            newPos += new Vector2Int(-1, -1);
        else if (action == UnitAction.RightUp)
            newPos += new Vector2Int(1, 1);
        else if (action == UnitAction.Skip)
        {
            EndTurn();
            return true;
        }
        else if (action == UnitAction.Wait)
            return true;
        
        if (newPos != pos)
            return Move(newPos);
       
        return false;
    }

    void EndTurn()
    {
        CurrentTurn++;
        RemainingMoves = UnitInfo.MovesPerTurn;
    }

    bool Sleep()
    {
        //Todo: return false if there are enemy units nearby
        return true;
    }

    bool Move(Vector2Int newPos)
    {
        if (!CanMove(newPos))
            return false;
        Pos = newPos;
        if (--RemainingMoves <= 0)
            EndTurn();
        return true;
    }

    public void Destroy()
    {
        GameObject.Destroy(gameObject);
    }
}

public class Army : Unit
{
    public Army(Vector2Int pos, Player owner) : base(UnitType.Army, pos, owner)
    {
    }

    protected override bool CanMove(Vector2Int pos)
    {
        return true; //Todo: check if army can move to this tile
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
