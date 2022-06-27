using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Land, Water, City }
public class Map
{
   Tile grassTile;
    Tile waterTile;
    Tile cityTile;
    public int Width { get; set; }
    public int Height { get; set; }
    public float Aspect => (float)Height / Width;
    float waterLevel;
    public int Seed { get; private set; }
    public List<City> Cities { get; private set; } = new List<City>();

   

    MapTile[,] tiles;
    MapTile this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            return tiles[x, y];
        }
    }
    void SetTile(int x, int y, TileType tileType)
    {
        var oldTile = tiles[x, y];
        if (x < 0 || x >= Width || y < 0 || y >= Height || oldTile != null && oldTile.TileType == tileType)
            return;
        MapTile newTile = new MapTile();
        newTile.TileType = tileType;
        Tile tile = null;
        if (tileType == TileType.City)
        {
            newTile.City = new City(null, x, y);
            Cities.Add(newTile.City);
            tile = cityTile;
        }
        else
        {
            if (oldTile != null && oldTile.TileType == TileType.City)
                Cities.Remove(tiles[x, y].City);
            if (tileType == TileType.Land)
                tile = grassTile;
            else
                tile = waterTile;
        }
         
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        tiles[x, y] = newTile;
    }

    Tilemap tilemap;
  
    System.Random random;

   

    public Map()
    {
        Width = NewGameSettings.Instance.NewMapSettings.Width.value;
        Height = NewGameSettings.Instance.NewMapSettings.Height.value;
        tiles = new MapTile[Width, Height];
        waterLevel = NewGameSettings.Instance.NewMapSettings.WaterLevel;
        LoadResources();
        var tileMapObject = GameObject.Find("Tilemap");
        tilemap = tileMapObject.GetComponent<Tilemap>();
    }

    void LoadResources()
    {
        grassTile = Resources.Load<Tile>("Tiling/grassTile");
        waterTile = Resources.Load<Tile>("Tiling/waterTile");
        cityTile = Resources.Load<Tile>("Tiling/cityTile"); 
        Seed = NewGameSettings.Instance.NewMapSettings.GetSeed();
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
                SetTile(x, y, TileType.City);
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

    public void GenerateMap()
    {
        CenterMap();
        random = new System.Random(Seed);
        rtCamera.Render();
        RenderTexture.active = rtCamera.targetTexture;
        hmapTexture.ReadPixels(new Rect(0, 0, HmapWidth, HmapHeight), 0, 0);
        RenderTexture.active = null;
        hmapTexture.Apply();
        UpdateTilesFromHmap();
        GenerateCities();
    }

    public void UpdateTilesFromHmap()
    {
        tilemap.ClearAllTiles();
        tiles.Initialize();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                TileType tileType;
                float pixel = hmapTexture.GetPixel(x * HmapResolutionFactor, y * HmapResolutionFactor).r;
                if (pixel > waterLevel)
                    tileType = TileType.Land;
                else
                    tileType = TileType.Water;
                SetTile(x, y, tileType);
            }
        }
    }

    public void ViewEntireMap()
    {
        float zh = (float)((Height + 2) / 2.0f / Math.Tan(Camera.main.fieldOfView / 2 * Math.PI / 180));
        float horizontalFov = Camera.VerticalToHorizontalFieldOfView(Camera.main.fieldOfView, Camera.main.aspect);
        float zw = (float)((Width + 2) / 2.0f / Math.Tan(horizontalFov / 2 * Math.PI / 180));
        float z = -Math.Max(zh, zw);

        Camera.main.transform.SetPositionAndRotation(new Vector3(0, 0, z), Quaternion.identity);
    }

    private void CenterMap()
    {
        tilemap.transform.position = new Vector3(-Width / 2.0f, -Height / 2.0f, 0);
    }

    public void SetCity(City city)
    {
        var mapTile = this[city.Pos.x, city.Pos.y];
        if (mapTile.TileType != TileType.City)
            throw new ArgumentException("Can't set city on non-city tile");
        mapTile.City = city;
        tilemap.SetTile(new Vector3Int(city.Pos.x, city.Pos.y, 0), city.Owner.CityTile);
    }
}

class MapTile
{
    public TileType TileType;
    public Unit Unit;
    public City City;
}
