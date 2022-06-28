using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Land, Water, City }
public class Map
{
    public int Width { get; set; }
    public int Height { get; set; }
    public float Aspect => (float)Height / Width;
    public int Seed { get; private set; }
    public List<City> Cities { get; private set; } = new List<City>();
    System.Random random;

    MapTile[,] tiles;
    public MapTile this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            return tiles[x, y];
        }
    }

    public Map()
    {
        Width = NewGameSettings.Instance.NewMapSettings.Width.value;
        Height = NewGameSettings.Instance.NewMapSettings.Height.value;
        tiles = new MapTile[Width, Height];
        Seed = NewGameSettings.Instance.NewMapSettings.GetSeed();
        var tileMapObject = GameObject.Find("Tilemap");
    }

    void SetTileType(int x, int y, TileType tileType)
    {
        var oldTile = tiles[x, y];
        if (x < 0 || x >= Width || y < 0 || y >= Height || oldTile != null && oldTile.TileType == tileType)
            return;
        MapTile newTile = new MapTile();
        newTile.TileType = tileType;
        if (tileType == TileType.City)
        {
            newTile.City = new City(null, x, y);
            Cities.Add(newTile.City);
        }
        else
        {
            if (oldTile != null && oldTile.TileType == TileType.City)
                Cities.Remove(tiles[x, y].City);
        }

        tiles[x, y] = newTile;
    }

    void GenerateCities()
    {
        while (Cities.Count < NewGameSettings.Instance.NewMapSettings.CityCount.value)
        {
            int x = random.Next(Width);
            int y = random.Next(Height);
            float randomValue = (float)random.NextDouble();
            
            if (HasAdjacentTileTypes(x, y, TileType.Water, TileType.Land)           //If tile is on coast, place city
                || HasAdjacentTileTypes(x, y, TileType.Land) && randomValue > 0.75   //If tile has no adjacent water, place city some of the time
                || randomValue > 0.9f)                                             //If tile has no adjacent land, place city even more rarely
                SetTileType(x, y, TileType.City);
        }
    }

    bool HasAdjacentTileTypes(int x, int y, params TileType[] tileTypesToLookFor)
    {
        foreach (var tileType in tileTypesToLookFor)
        {
            if (!HasAdjacentTileType(x, y, tileType))
                return false;
        }
        return true;
    }

    bool HasAdjacentTileType(int x, int y, TileType tileTypeToLookFor)
    {
        for (int j = y - 1; j < y + 1; j++)
        {
            for (int i = x - 1; i < x + 1; i++)
            {
                if (i == x && j == y)
                    continue;
                var mapTile = this[i, j];
                if (mapTile != null && mapTile.TileType == tileTypeToLookFor)
                    return true;
            }
        }
        return false;
    }

    public void Generate(Hmap hmap)
    {
        random = new System.Random(Seed);
        hmap.Generate();
        UpdateTilesFromHmap(hmap);
        GenerateCities();
    }

    public void UpdateTilesFromHmap(Hmap hmap)
    {
        tiles.Initialize();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                TileType tileType = hmap.GetTileType(x, y);
                SetTileType(x, y, tileType);
            }
        }
    }

    public void SetCity(City city)
    {
        var mapTile = this[city.Pos.x, city.Pos.y];
        if (mapTile.TileType != TileType.City)
            throw new ArgumentException("Can't set city on non-city tile");
        mapTile.City = city;
    }

    public void Show()
    {
        MapRenderer.Instance.UpdateTilemap(this);
    }
}

public class MapTile
{
    public TileType TileType;
    public Unit Unit;
    public City City;
}
