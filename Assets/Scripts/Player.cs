using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PlayerState { StartGame, StartTurn, Move, EndTurn }

[Serializable]
public class Player : PlayerSettings
{
    public ProdDialog ProdDialog;
    bool selectingProd;
    List<City> cities = new List<City>();
    List<Unit> units = new List<Unit>();
    Unit currentUnit;
    public Unit CurrentUnit
    {
        get => currentUnit;
        private set
        {
            if (value == currentUnit)
                return;
            if (value != null)
            {
                Map.Renderer.MoveCameraToTile(value.Pos);
                value.IsActive = true;

            }
            if (currentUnit != null)
                currentUnit.IsActive = false;
            currentUnit = value;
        }
    }
    public Map GlobalMap => Map.ReferenceMap;
    public Map Map { get; private set; }
    public int CurrentTurn { get; private set; }
    int currentCityIndex;
    City CurrentCity => cities[currentCityIndex];

    PlayerState state = PlayerState.StartGame;
    public PlayerState State
    {
        get => state;
        set
        {
            if (value == state)
                throw new ArgumentException("Setting player state to same value.");
            state = value;
            switch (value)
            {
                case PlayerState.StartTurn:
                    currentCityIndex = 0;
                    //SyncWithGlobalMap();
                    break;
                case PlayerState.Move:
                    CurrentUnit = GetNextUnit();
                    break;
                case PlayerState.EndTurn:
                    currentCityIndex = 0;
                    foreach (var city in cities)
                        city.ProdTime--;
                    CurrentTurn++;
                    break;
            }
        }
    }

    public void AddUnit(Unit unit)
    {
        units.Add(unit);
        GlobalMap.SetUnit(unit);
        Map.Explore(unit.Pos);
    }

    private void SyncWithGlobalMap()
    {
        foreach (var unit in units)
            Map.Explore(unit.Pos);
        foreach (var city in cities)
            Map.Explore(city.Pos);
    }

    public Player(PlayerSettings settings, Map globalMap, ProdDialog prodDialog) : base(settings)
    {
        this.ProdDialog = prodDialog;
        var globalMapPrefab = (GameObject)Resources.Load("Tiling/GlobalMap");
        var tilemapsParent = GameObject.Instantiate(globalMapPrefab, globalMap.Renderer.gameObject.transform.parent);
        tilemapsParent.name = Name;
        var mapRenderer = tilemapsParent.GetComponent<MapRenderer>();
        Map = new Map(mapRenderer, this, globalMap);
        Map.InitToUnexplored();
    }

    public Player(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
        foreach (SerializationEntry entry in info)
        {
            if (entry.Name == "Cities")
                cities = (List<City>)entry.Value;
            else if (entry.Name == "Units")
                units = (List<Unit>)entry.Value;
            else if (entry.Name == "CurrentTurn")
                CurrentTurn = (int)entry.Value;
            else if (entry.Name == "Map")
                Map = (Map)entry.Value;
        }
        state = PlayerState.Move;
        Map.Owner = this;
        foreach (var city in cities)
            city.Owner = this;
        foreach (var unit in units)
        {
            unit.Owner = this;
            unit.Owner.AddUnit(unit);
            if (unit.IsActive)
            {
                CurrentUnit = unit;
                unit.StartBlink(true);
            }
        }

    }

    new public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        base.GetObjectData(info, ctxt);
        info.AddValue("Cities", cities);
        info.AddValue("Units", units);
        info.AddValue("CurrentTurn", CurrentTurn);
        info.AddValue("Map", Map);
    }

    public static void AssignStartingCities(List<Player> players, Map map)
    {
        var random = new System.Random();
        var startingCities = new List<City>();
        foreach (var player in players)
            AssignStartingCity(player, players, startingCities, random, map);
    }

    static void AssignStartingCity(Player player, List<Player> players, List<City> startingCities, System.Random random, Map map)
    {
        City candidateCity = null;
        float candidateDistance = 0;
        foreach (var city in map.Cities)
        {
            if (city.Owner != null)
                continue;
            if (startingCities.Count == 0)
            {
                candidateCity = city;
                break;
            }
            float distance = GetDistanceToNearestStartingCity(city, player, startingCities, map);
            if (distance > candidateDistance)
            {
                candidateDistance = distance;
                candidateCity = city;
            }
        }
        startingCities.Add(candidateCity);
        player.AddCity(candidateCity, players);
    }

    static float GetDistanceToNearestStartingCity(City city, Player player, List<City> startingCities, Map map)
    {
        float minDistance = float.MaxValue;
        foreach(var city2 in startingCities)
        {
            float distance = Vector2Int.Distance(city.Pos, city2.Pos);
            if (distance < minDistance)
                minDistance = distance;
        }
        return minDistance;
    }

    void AddCity(City city, List<Player> players)
    {
        foreach (var player in players)
            player.RemoveCity(city);
        city.Owner = this;
        cities.Add(city);
        GlobalMap.SetCity(city);
        Map.Explore(city.Pos);
    }

    void RemoveCity(City city)
    {
        city.Owner = null;
        cities.Remove(city);
    }

    public PlayerState ProcessCurrentState()
    {
        if (selectingProd)
            return state;
        switch (state)
        {
            case PlayerState.StartGame:
                State = PlayerState.StartTurn;
                SelectProd(CurrentCity, true);
                break;
            case PlayerState.StartTurn:
                if (CurrentCity.IsProdDone())
                {
                    CurrentCity.CreateUnit();
                    SelectProd(CurrentCity, true);
                }
                if (++currentCityIndex >= cities.Count)
                    State = PlayerState.Move;
                break;
            case PlayerState.Move:
                if (units.Count == 0 || CurrentUnit == null)
                {
                    State = PlayerState.EndTurn;
                    break;
                }
                if (MoveUnit(CurrentUnit))
                    Map.Renderer.MoveCameraToTile(CurrentUnit.Pos);
                if (CurrentUnit.CurrentTurn > CurrentTurn)
                    CurrentUnit = GetNextUnit();
                break;
            case PlayerState.EndTurn:
                break;
        }
        return state;
    }

    public void RemoveUnit(Unit unit)
    {
        units.Remove(unit);
        GlobalMap.RemoveUnit(unit);
        Map.RemoveUnit(unit);
        //UnitStats[unit.Type].Lost++;
    }

    private bool MoveUnit(Unit currentUnit)
    {
        if (AiLevel == 0)
            return MoveUnit_Human(currentUnit);
        else
            return MoveUnit_AI(currentUnit);
    }

    bool MoveUnit_Human(Unit unit)
    {
        unit.UpdateBlink();
        foreach (var mapping in InputMappings.UnitActions)
        {
            if (Input.GetKeyDown(mapping.Key))
            {
                if (mapping.Value == UnitAction.Wait)
                {
                    currentUnit.IsWaiting = true;
                    CurrentUnit = GetNextUnit();
                    return false;
                }
                var moved = unit.ExecuteAction(mapping.Value);
                //Reset blink to active state when user moves
                if (moved)
                    unit.StartBlink(true);
                return moved;
            }
        }
        return false;
    }

    bool MoveUnit_AI(Unit currentUnit)
    {
        return currentUnit.ExecuteAction(UnitAction.Skip);
    }

   

    //Return unit closest to center of screen to minimize camera movement
    Unit GetNextUnit()
    {
        float minDistance = float.MaxValue;
        Unit nextUnit = null;
        if (units.Count == 0)
            return null;
        bool unitsAreWaiting = false;
        foreach (var unit in units)
        {
            if (unit.CurrentTurn < CurrentTurn)
                throw new Exception("Unit out of sync with current turn number");
            if (unit != CurrentUnit && unit.CurrentTurn == CurrentTurn)
            {
                if (unit.IsWaiting)
                {
                    unitsAreWaiting = true;
                    continue;
                }
             
                var camPos = Map.Renderer.CamPos;
                int distance = Map.Distance(unit.Pos, new Vector2Int((int)camPos.x, (int)camPos.y));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nextUnit = unit;
                }
            }
        }

        if (nextUnit == null)
        {
            if (unitsAreWaiting)
            {
                foreach (var unit in units)
                    unit.IsWaiting = false;
                nextUnit = GetNextUnit();
            }
            else if (CurrentUnit != null && CurrentUnit.CurrentTurn == CurrentTurn)
            {
                currentUnit.IsWaiting = false;
                nextUnit = currentUnit; //This is the last unit that hasn't moved. It probably tried to wait.
            }
        }

        return nextUnit;
    }

    void SelectProd(City city, bool goToCity)
    {
        if (AiLevel == 0)
            SelectProd_Human(city, goToCity);
        else
            SelectProd_AI(city);
    }

    void SelectProd_Human(City city, bool goToCity)
    {
        selectingProd = true;
        if (goToCity)
            Map.Renderer.MoveCameraToTile(city.Pos);
        ShowProdDialog(city);
    }

    void SelectProd_AI(City city)
    {
        city.Production = UnitType.Battleship;
    }

    public void ShowProdDialog(City city)
    {
        ProdDialog.Show(city);
    }

    public void OnProdDialogClose()
    {
        selectingProd = false;
        if (State == PlayerState.StartTurn)
        {
            
        }
    }
}