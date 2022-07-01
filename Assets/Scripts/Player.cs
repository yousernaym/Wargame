using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PlayerState { StartGame, StartTurn, Move, EndTurn }

public class Player : PlayerSettings
{
    ProdDialog prodDialog;
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
    Map globalMap;
    public Map Map { get; private set; }
    public GameObject GameObject { get; private set; }
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
                    SyncWithGlobalMap();
                    break;
                case PlayerState.Move:
                    CurrentUnit = NextUnit();
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

    public void ExploreMap(Vector2Int pos)
    {
        Map.Explore(pos, globalMap);
    }

    private void SyncWithGlobalMap()
    {
        foreach (var unit in units)
            ExploreMap(unit.Pos);
        foreach (var city in cities)
            ExploreMap(city.Pos);
    }

    public Player(PlayerSettings settings, Map globalMap, ProdDialog prodDialog) : base(settings)
    {
        this.globalMap = globalMap;
        this.prodDialog = prodDialog;
        var globalMapPrefab = (GameObject)Resources.Load("Tiling/GlobalMap");
        GameObject = GameObject.Instantiate(globalMapPrefab, globalMap.Renderer.gameObject.transform.parent);
        GameObject.name = Name;
        var mapRenderer = GameObject.GetComponent<MapRenderer>();
        Map = new Map(mapRenderer, this);
        Map.InitToUnexplored();
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
        globalMap.SetCity(city);
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
                    units.Add(CurrentCity.CreateUnit());
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
                    CurrentUnit = NextUnit();
                break;
            case PlayerState.EndTurn:
                break;
        }
        return state;
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
                    CurrentUnit = NextUnit();
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
    Unit NextUnit()
    {
        float minDistance = float.MaxValue;
        Unit nextUnit = null;
        if (units.Count == 0)
            return null;
        foreach (var unit in units)
        {
            if (unit.CurrentTurn < CurrentTurn)
                throw new Exception("Unit out of sync with current turn number");
            if (unit != CurrentUnit && unit.CurrentTurn == CurrentTurn)
            {
                var camPos = Map.Renderer.CamPos;
                int distance = Map.Distance(unit.Pos, new Vector2Int((int)camPos.x, (int)camPos.y));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nextUnit = unit;
                }
            }
        }

        //All other units have already moved this turn and this unit still has not used its turn (it probably used the wait action)
        if (nextUnit == null && CurrentUnit != null && CurrentUnit.CurrentTurn == CurrentTurn) 
            nextUnit = CurrentUnit;

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
        city.Production = UnitType.Army;
    }

    public void ShowProdDialog(City city)
    {
        prodDialog.Show(city);
    }

    public void OnProdDialogClose()
    {
        selectingProd = false;
        if (State == PlayerState.StartTurn)
        {
            
        }
    }
}