using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Land, Water, City, Unexplored }

[Serializable]
public class Map : ISerializable
{
    public Map ReferenceMap { get; private set; }
    public Player Owner { get; set; }
    public MapRenderer Renderer { get; set; }
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

    public Map(MapRenderer renderer, Player owner, Map referenceMap)
    {
        this.ReferenceMap = referenceMap;
        Renderer = renderer;
        Width = NewGameSettings.Instance.NewMapSettings.Width.value;
        Height = NewGameSettings.Instance.NewMapSettings.Height.value;
        tiles = new MapTile[Width, Height];
        Seed = NewGameSettings.Instance.NewMapSettings.GetSeed();
        this.Owner = owner;
    }

    public Map(SerializationInfo info, StreamingContext ctxt)
    {
        foreach (SerializationEntry entry in info)
        {
            if (entry.Name == "Width")
                Width = (int)entry.Value;
            else if (entry.Name == "Height")
                Height = (int)entry.Value;
            else if (entry.Name == "Seed")
                Seed = (int)entry.Value;
            else if (entry.Name == "Tiles")
                tiles = (MapTile[,])entry.Value;
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("Width", Width);
        info.AddValue("Height", Height);
        info.AddValue("Seed", Seed);
        info.AddValue("Tiles", tiles);
    }

    MapTile CreateMapTile(int x, int y, TileType tileType)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return null;
        var oldTile = tiles[x, y];
        if (oldTile != null && oldTile.TileType == tileType)
            return oldTile;

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
        Renderer.UpdateTile(x, y, this);
        return newTile;
    }

    public void SetUnit(Unit unit)
    {
        tiles[unit.Pos.x, unit.Pos.y].Unit = unit;
        Renderer.UpdateTile(unit.Pos.x, unit.Pos.y, this);
    }

    public void Explore(Vector2Int pos)
    {
        for (int y = pos.y - 1; y <= pos.y + 1; y++)
        {
            for (int x = pos.x - 1; x <= pos.x + 1; x++)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    continue;

                var referenceTile = ReferenceMap[x, y];

                if (x == pos.x && y == pos.y)
                    tiles[x, y] = ReferenceMap[x, y];
                else
                {
                    //Don't update tile of another friendly unit/city (it is already updated or will be)
                    if (referenceTile.Unit != null && referenceTile.Unit.Owner != Owner
                        || referenceTile.City != null && referenceTile.City.Owner != Owner)
                        continue;
                    // Enemy units/cities can move/change without our knowledge so we should make a copy that will not change
                    tiles[x, y] = ReferenceMap[x, y].Clone();
                }
                Renderer.UpdateTile(x, y, this);
            }
        }
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
                CreateMapTile(x, y, TileType.City);
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
        //Renderer.UpdateTiles(this);
    }

    public void InitToUnexplored()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                CreateMapTile(x, y, TileType.Unexplored);
    }

    public void UpdateTilesFromHmap(Hmap hmap)
    {
        tiles.Initialize();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                TileType tileType = hmap.GetTileType(x, y);
                CreateMapTile(x, y, tileType);
            }
        }
    }

    public void SetCity(City city)
    {
        var mapTile = tiles[city.Pos.x, city.Pos.y];
        if (mapTile.TileType != TileType.City)
            throw new ArgumentException("Can't set city on non-city tile");
        mapTile.City = city;
        Renderer.UpdateTile(city.Pos.x, city.Pos.y, this);
    }

    

    public static int Distance(Vector2Int pos1, Vector2Int pos2)
    {
        var diff = pos1 - pos2;
        return Math.Max(diff.x, diff.y);
    }

    public void RemoveUnit(Unit unit)
    {
        tiles[unit.Pos.x, unit.Pos.y].Unit = null;
        Renderer.UpdateTile(unit.Pos.x, unit.Pos.y, this);
    }
}

[Serializable]
public class MapTile : ISerializable
{
    public TileType TileType;
    public Unit Unit;
    public City City;

    public MapTile()
    {

    }

    public MapTile(SerializationInfo info, StreamingContext ctxt)
    {
        foreach (SerializationEntry entry in info)
        {
            if (entry.Name == "TileType")
                TileType = (TileType)entry.Value;
            else if (entry.Name == "Unit")
                Unit = (Unit)entry.Value;
            else if (entry.Name == "City")
                City = (City)entry.Value;
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("TileType", TileType);
        info.AddValue("Unit", Unit);
        info.AddValue("City", City);
    }

    public MapTile Clone()
    {
        var clone = Cloning.Clone<MapTile>(this);
        if (Unit != null)
            clone.Unit.Owner = Unit.Owner;
        else if (City != null)
            clone.City.Owner = City.Owner;
        return clone;
    }
}
