using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : PlayerSettings
{
    public Tile CityTile { get; private set; }

    List<City> cities = new List<City>();
    Map globalMap;
    Map playerMap = new Map();

    public Player(PlayerSettings settings, Map globalMap) : base(settings)
    {
        this.globalMap = globalMap;
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
}
