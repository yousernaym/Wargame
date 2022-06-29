using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PlayerState { StartGame, StartTurn, Move, EndTurn }

public class Player : PlayerSettings
{
    ProdDialog prodDialog;
    bool selectingProd;
    //bool SelectingProd
    //{
    //    get => selectingProd;
    //    set
    //    {
    //        if (value == selectingProd)
    //            throw new ArgumentException();
    //        selectingProd = value;
    //        if (value)
    //            return;
    //        if (state == PlayerState.StartTurn)
    //            currentCityIndex++;
    //        else if (state == PlayerState.StartGame)
    //            state = PlayerState.EndTurn;
    //    }
    //}

    public Tile CityTile { get; private set; }

    List<City> cities = new List<City>();
    List<Unit> units = new List<Unit>();
    Unit currentUnit;
    Map globalMap;
    Map playerMap = new Map();
    int currentTurn;
    int currentCityIndex;
    City CurrentCity => cities[currentCityIndex];

    PlayerState state = PlayerState.StartGame;
    PlayerState State
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
                    break;
                case PlayerState.Move:
                    break;
                case PlayerState.EndTurn:
                    currentCityIndex = 0;
                    foreach (var city in cities)
                        city.ProdTime--;
                    currentTurn++;
                    break;
            }
        }
    }

    public Player(PlayerSettings settings, Map globalMap, ProdDialog prodDialog) : base(settings)
    {
        this.globalMap = globalMap;
        this.prodDialog = prodDialog;
        CityTile = Resources.Load<Tile>($"Tiling/player{PlayerNumber + 1}CityTile");
        CityTile.color = Color;
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
        if (currentCityIndex >= cities.Count)
            return state = PlayerState.EndTurn;
        switch (state)
        {
            case PlayerState.StartGame:
                state = PlayerState.StartTurn;
                SelectProd(CurrentCity);
                break;
            case PlayerState.StartTurn:
                if (CurrentCity.IsProdDone())
                    SelectProd(CurrentCity);
                break;
            case PlayerState.Move:
                break;
            case PlayerState.EndTurn:
                break;
        }
        return state;
    }

    void SelectProd(City city)
    {
        selectingProd = true;
        MapRenderer.Instance.MoveCameraToTile(city.Pos);
        ShowProdDialog(city);
    }

    public void ShowProdDialog(City city)
    {
        prodDialog.SetPos(new Vector2(1600, -700));
        prodDialog.OnHide = OnProdDialogClose;
        prodDialog.Show();
    }

    void OnProdDialogClose()
    {
        if (state == PlayerState.StartGame || state == PlayerState.StartTurn)
        {
            if (++currentCityIndex >= cities.Count)
                state = PlayerState.EndTurn;
        }
    }
}