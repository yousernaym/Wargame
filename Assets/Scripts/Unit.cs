using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;

public enum UnitType { Army, Fighter, Transport, Destroyer, Submarine, Cruiser, Battleship, Carrier }
public enum UnitAction { Left, Right, Up, Down, LeftUp, RightUp, LeftDown, RightDown, Skip, Wait}

[Serializable]
public class Unit : ISerializable
{
    const float BlinkIntervalSeconds = 0.5f;
    protected int maxPassengers;
    DateTime lastBlinkTime;
    protected UnitType? canCarryType = null;
    protected List<Unit> passengers = new List<Unit>();
    Unit container;
    
    Vector2Int? pos;
    public Vector2Int Pos 
    {
        get => (Vector2Int)pos;
        private set
        {
            if (pos == value)
                return;
            if (owner != null)
            {
                if (pos != null)
                {
                    var oldTile = GetTile(Pos);
                    if (IsCarriedBy(oldTile.Unit))
                        oldTile.Unit.RemovePassenger(this);
                    else if (IsInCity(oldTile.City))
                    {
                        oldTile.City.Units.Remove(this);
                        AddPassengersFromCity(oldTile.City);
                    }
                    else
                        oldTile.Unit = null;
                    StopBlink();
                }
                pos = value;
                var tile = GetTile(Pos);
                if (CanBeCarriedBy(tile.Unit))
                    tile.Unit.AddPassenger(this);
                else if (tile.City == null)
                    tile.Unit = this;
                else
                {
                    tile.City.Units.Add(this);
                    for (int i = 0; i < passengers.Count; i++)
                    {
                        RemovePassenger(passengers[i]);
                        tile.City.Units.Add(passengers[i]);
                    }
                }
                foreach (var passenger in passengers)
                    passenger.pos = value;
                owner.Map.Explore(Pos);
                if (owner.AiLevel == 0)
                    owner.Map.Renderer.MoveCameraToTile(Pos);
            }
            else
                pos = value;
        }
    }

    private void AddPassengersFromCity(City city)
    {
        foreach (var unit in city.Units)
        {
            if (IsFull)
                return;
            if (canCarryType == unit.type)
                AddPassenger(unit);
        }
    }

    public bool IsInCity(City city)
    {
        return city != null && city.Units.Contains(this);
    }

    void AddPassenger(Unit unit)
    {
        passengers.Add(unit);
        unit.container = this;
    }

    public UnitInfo UnitInfo { get; private set; }
    int hp;
    public int Hp
    {
        get => hp;
        private set
        {
            hp = Math.Min(UnitInfo.MaxHp, value);
            if (hp <= 0)
                Kill();
        }
    }

    public int RemainingMoves { get; private set; }
    UnitType type;
    public UnitType Type 
    {
        get => type;
        private set
        {
            type = value;
            UnitInfo = UnitInfo.Types[type];
            Hp = UnitInfo.MaxHp;
            RemainingMoves = UnitInfo.MovesPerTurn;
        }
    }


    public int CurrentTurn { get; private set; }
    Player owner;
    public Player Owner 
    {
        get => owner;
        set
        {
            owner = value;
        }
    }

    bool isActive;
    public bool IsActive 
    {
        get => isActive;
        set
        {
            if (IsActive == value)
                return;
            isActive = value;
            if (value)
                StartBlink(Owner.Map[Pos.x, Pos.y].City != null || container != null);
                //StartBlink(Owner.Map[Pos.x, Pos.y].City == null);
            else
                StopBlink();
        }
    }

    public bool IsWaiting { get; set; }
    public Unit ActivePassenger => passengers.FirstOrDefault(passenger => passenger.isActive);
    bool IsFull => passengers.Count > maxPassengers;

    public Unit(UnitType unitType, Vector2Int pos, Player owner)
    {
        Type = unitType;
        Pos = pos;
        Owner = owner;
        Owner.AddUnit(this);
        CurrentTurn = owner.CurrentTurn;
    }

    public Unit(SerializationInfo info, StreamingContext ctxt)
    {
        foreach (SerializationEntry entry in info)
        {
            if (entry.Name == "Pos")
                pos = (Vector2Int)entry.Value;
            else if (entry.Name == "Type")
                Type = (UnitType)entry.Value;
            else if (entry.Name == "CurrentTurn")
                CurrentTurn = (int)entry.Value;
            else if (entry.Name == "IsActive")
                isActive = (bool)entry.Value;
            else if (entry.Name == "passengers")
            {
                passengers = (List<Unit>)entry.Value;
                foreach (var passenger in passengers)
                    passenger.container = this;
            }
            else if (entry.Name == "CanCarryType")
                canCarryType = (UnitType)entry.Value;
            else if (entry.Name == "MaxPassengers")
                maxPassengers = (int)entry.Value;
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("Pos", Pos);
        info.AddValue("Type", Type);
        info.AddValue("CurrentTurn", CurrentTurn);
        info.AddValue("IsActive", IsActive);
    }

    bool CanMove(MapTile tile)
    {
        return IsFriend(tile.City)
            || UnitInfo.MoveTargets.Any(tileType => tileType == tile.TileType)
            || CanBeCarriedBy(tile.Unit);
    }

    protected virtual bool CanAttack(MapTile tile)
    {
        return tile.Unit != null &&
            tile.Unit.owner != owner &&
            UnitInfo.AttackTargets.Any(type => type == tile.Unit.Type)
            ||
            tile.City != null &&
            tile.City.Owner != owner &&
            Type == UnitType.Army;
    }

    void StopBlink()
    {
        owner.Map.Renderer.SetUnitVisibility(Pos.x, Pos.y, owner.Map, true);
    }

    public void StartBlink(bool initialState)
    {
        owner.Map.Renderer.SetUnitVisibility(Pos.x, Pos.y, owner.Map, initialState);
        lastBlinkTime = DateTime.Now;
    }

    public void UpdateBlink()
    {
        if (isActive)
        {
            var timeElapsed = DateTime.Now - lastBlinkTime;
            if (timeElapsed.TotalSeconds > BlinkIntervalSeconds)
            {
                owner.Map.Renderer.ToggleUnitVisibility(Pos.x, Pos.y, owner.Map);
                lastBlinkTime = DateTime.Now;
            }
        }
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
        else if (action == UnitAction.RightDown)
            newPos += new Vector2Int(1, -1);
        else if (action == UnitAction.Skip)
        {
            EndTurn();
            return true;
        }
        
        if (newPos != pos)
            return Move(newPos);
       
        return false;
    }

    protected virtual void EndTurn()
    {
        CurrentTurn++;
        RemainingMoves = UnitInfo.MovesPerTurn;
        if (GetTile(Pos).TileType == TileType.City)
            Hp++;
    }

    bool Sleep()
    {
        //Todo: return false if there are enemy units nearby
        return true;
    }

    bool Move(Vector2Int newPos)
    {
        var tile = GetTile(newPos);
        bool attacking = false;
        if (tile.Unit != null && CanAttack(tile))
        {
            if (!(attacking = Attack(tile.Unit)))
                return false;
        }
        if (CanMove(tile))
            Pos = newPos;
        else if (!attacking)
            return false;

        if (--RemainingMoves <= 0 
            || tile.TileType == TileType.City
            || container != null)
            EndTurn();
        return true;
    }

    bool Attack(Unit target)
    {
        int totalHp = Hp + target.Hp;
        var outcome = UnityEngine.Random.Range(0, totalHp);
        Hp -= outcome;
        target.Hp -= totalHp - outcome - 1;
        if (target.Hp <= 0)
            return true;
        else
            return false;
    }

    protected void Kill()
    {
        owner.RemoveUnit(this);
        foreach (var passenger in passengers)
            owner.RemoveUnit(passenger);
    }

    public MapTile GetTile(Vector2Int pos)
    {
        return Owner.GlobalMap[pos.x, pos.y];
    }

    public bool CanBeCarriedBy(Unit unit)
    {
        return IsFriend(unit) && unit.canCarryType == Type;
    }

    public bool IsCarriedBy(Unit unit)
    {
        return container != null && container == unit;
    }

    public void RemovePassenger(Unit unit)
    {
        passengers.Remove(unit);
    }

    public bool IsFriend(Unit unit)
    {
        return unit != null && unit.owner == owner;
    }

    public bool IsFriend(City city)
    {
        return city != null && city.Owner == owner;
    }
}

[Serializable]
public class Army : Unit
{
    public Army(Vector2Int pos, Player owner) : base(UnitType.Army, pos, owner)
    {
        UnitInfo.AttackTargets = new UnitType[] { UnitType.Army, UnitType.Fighter, UnitType.Transport, UnitType.Destroyer, UnitType.Cruiser, UnitType.Battleship, UnitType.Carrier };
        UnitInfo.MoveTargets = new TileType[] { TileType.Land };
    }

    public Army(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
    }
}

[Serializable]
public class Fighter : Unit
{
    const int MaxFuel = 20;
    const int FuelUsedPerTurn = 5;
    int fuel;

    public Fighter(Vector2Int pos, Player owner) : base(UnitType.Fighter, pos, owner)
    {
        UnitInfo.AttackTargets = new UnitType[] { UnitType.Army, UnitType.Fighter, UnitType.Transport, UnitType.Destroyer, UnitType.Submarine, UnitType.Cruiser, UnitType.Battleship, UnitType.Carrier };
        UnitInfo.MoveTargets = new TileType[] { TileType.Land, TileType.Water };
    }

    public Fighter(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
    }

    protected override void EndTurn()
    {
        fuel -= FuelUsedPerTurn;
        if (GetTile(Pos).TileType == TileType.City)
            fuel = MaxFuel;
        if (fuel == 0)
            Kill();
        else
            base.EndTurn();
    }

}

[Serializable]
public class Ship : Unit
{
    protected bool canAttackArmies;
    public Ship(UnitType unitType, Vector2Int pos, Player owner, bool canAttackArmies) : base(unitType, pos, owner)
    {
        this.canAttackArmies = canAttackArmies;
        UnitInfo.AttackTargets = new UnitType[] { UnitType.Army, UnitType.Fighter, UnitType.Transport, UnitType.Destroyer, UnitType.Submarine, UnitType.Cruiser, UnitType.Battleship, UnitType.Carrier };
        UnitInfo.MoveTargets = new TileType[] { TileType.Water };
    }

    public Ship(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
        foreach (SerializationEntry entry in info)
        {
            if (entry.Name == "CanAttackArmies")
                canAttackArmies = (bool)entry.Value;
        }
    }

    protected override bool CanAttack(MapTile tile)
    {
        return base.CanAttack(tile) ||
            tile.Unit.Type == UnitType.Army &&
            canAttackArmies &&
            tile.Unit.Owner != Owner;
    }
}

[Serializable]
public class Transport : Ship
{
    public Transport(Vector2Int pos, Player owner) : base(UnitType.Transport, pos, owner, false)
    {
        canCarryType = UnitType.Army;
        maxPassengers = 6;
    }

    public Transport(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
    }
}

[Serializable]
public class Destroyer : Ship
{
    public Destroyer(Vector2Int pos, Player owner) : base(UnitType.Destroyer, pos, owner, false)
    {
    }

    public Destroyer(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
    }
}

[Serializable]
public class Submarine : Ship
{
    public Submarine(Vector2Int pos, Player owner) : base(UnitType.Submarine, pos, owner, false)
    {
    }

    public Submarine(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
    }
}

[Serializable]
public class Cruiser : Ship
{
    public Cruiser(Vector2Int pos, Player owner) : base(UnitType.Cruiser, pos, owner, true)
    {
    }

    public Cruiser(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
    }
}

[Serializable]
public class Battleship : Ship
{
    public Battleship(Vector2Int pos, Player owner) : base(UnitType.Battleship, pos, owner, true)
    {
    }

    public Battleship(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
    }
}

[Serializable]
public class Carrier : Ship
{
    public Carrier(Vector2Int pos, Player owner) : base(UnitType.Carrier, pos, owner, false)
    {
        canCarryType = UnitType.Fighter;
    }

    public Carrier(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
        maxPassengers = 8;
    }
}

