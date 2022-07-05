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
    static Dictionary<UnitType, GameObject> UnitPrefabs;
    DateTime lastBlinkTime;
    
    List<Unit> passengers;
    Unit container;
    
    Vector2Int? pos;
    public Vector2Int Pos 
    {
        get => (Vector2Int)pos;
        private set
        {
            if (owner != null)
            {
                if (pos != null)
                    GetTile(Pos).Unit = null;
                pos = value;
                Owner.AddUnit(this);
            }
            else
                pos = value;
        }
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
            if (value)
                StartBlink(Owner.Map[Pos.x, Pos.y].City == null);
            else
                StopBlink();
            isActive = value;
        }
    }

    public bool IsWaiting { get; set; }

    public Unit(UnitType unitType, Vector2Int pos, Player owner)
    {
        if (UnitPrefabs == null)
        {
            //UnitPrefabs = new Dictionary<UnitType, GameObject>();
            //foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
                //UnitPrefabs[type] = Resources.Load<GameObject>("Units/" + Enum.GetName(typeof(UnitType), type));
        }
        Type = unitType;
        
        //gameObject = GameObject.Instantiate(UnitPrefabs[unitType], owner.GameObject.transform);
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
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("Pos", Pos);
        info.AddValue("Type", Type);
        info.AddValue("CurrentTurn", CurrentTurn);
        info.AddValue("IsActive", IsActive);
    }

    protected bool CanMove(MapTile tile)
    {
        return tile.TileType == TileType.City
            && tile.City.Owner == owner
            || UnitInfo.MoveTargets.Any(tileType => tileType == tile.TileType)
            && tile.Unit.owner != owner;
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
        owner.Map[Pos.x, Pos.y].Unit = this;
        owner.Map.Renderer.UpdateTile(Pos.x, Pos.y, owner.Map);
    }

    public void StartBlink(bool initialState)
    {
        owner.Map[Pos.x, Pos.y].Unit = null;
        owner.Map.Renderer.UpdateTile(Pos.x, Pos.y, owner.Map);
        lastBlinkTime = DateTime.Now;
    }

    public void UpdateBlink()
    {
        if (isActive)
        {
            var timeElapsed = DateTime.Now - lastBlinkTime;
            if (timeElapsed.TotalSeconds > BlinkIntervalSeconds)
            {
                owner.Map[Pos.x, Pos.y].Unit = owner.Map[Pos.x, Pos.y].Unit == null ? this : null;
                owner.Map.Renderer.UpdateTile(Pos.x, Pos.y, owner.Map);
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

        if (--RemainingMoves <= 0 || GetTile(Pos).TileType == TileType.City)
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
    }

    public MapTile GetTile(Vector2Int pos)
    {
        return Owner.GlobalMap[pos.x, pos.y];
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
    }

    public Carrier(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
    }
}

