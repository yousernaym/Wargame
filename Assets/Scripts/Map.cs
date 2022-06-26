using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Land, Water, City }
public class Map : MonoBehaviour
{
    [SerializeField] Camera rtCamera;
    Material hmapMaterial;
    Tile grassTile;
    Tile waterTile;
    Tile cityTile;
    public int Width { get; set; }
    public int Height { get; set; }
    public float Aspect => (float)Height / Width;
    float waterLevel;
    public int Seed { get; private set; }
    public List<City> Cities { get; private set; } = new List<City>();

    [SerializeField] Renderer screenGrabRenderer; //Shows height map for debugging purposes

    const int HmapResolutionFactor = 20;
    int HmapWidth => Width * HmapResolutionFactor;
    int HmapHeight => Height * HmapResolutionFactor;

    TileType?[,] tiles;
    TileType? this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            return tiles[x, y];
        }
        set
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;
            Tile tile = null;
            if (value == TileType.City)
            {
                if (tiles[x, y] != TileType.City)
                    Cities.Add(new City(null, x, y));
                tile = cityTile;
            }
            else
            {
                if (tiles[x, y] == TileType.City)
                {
                    var existingCity = Cities.Find((city) => city.Pos.x == x && city.Pos.y == y);
                    Cities.Remove(existingCity);
                }
                if (value == TileType.Land)
                    tile = grassTile;
                else
                    tile = waterTile;
            }
         
            tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            tiles[x, y] = value;
        }
    }

    Tilemap tilemap;
    Texture2D hmapTexture;
    string HmapLayerName = "Hmap";
    System.Random random;

    int HmapLayerNumber => LayerMask.NameToLayer(HmapLayerName);

    public void Init()
    {
        Width = NewGameSettings.Instance.NewMapSettings.Width.value;
        Height = NewGameSettings.Instance.NewMapSettings.Height.value;
        tiles = new TileType?[Width, Height];
        waterLevel = NewGameSettings.Instance.NewMapSettings.WaterLevel;
        LoadResources();
        hmapTexture = new Texture2D(HmapWidth, HmapHeight, TextureFormat.RFloat, true);
        hmapTexture.name = "HmapTexture";
        CreateQuad();
        rtCamera.targetTexture = new RenderTexture(HmapWidth, HmapHeight, 32, RenderTextureFormat.RFloat); ;
        var tileMapObject = GameObject.Find("Tilemap");
        tilemap = tileMapObject.GetComponent<Tilemap>();
        GenerateMap();
    }

    private void Update()
    {
    }

    private void LoadResources()
    {
        grassTile = Resources.Load<Tile>("Tiling/grassTile");
        waterTile = Resources.Load<Tile>("Tiling/waterTile");
        cityTile = Resources.Load<Tile>("Tiling/cityTile"); 
        hmapMaterial = Resources.Load<Material>("Tiling/HmapMaterial");
        hmapMaterial.SetFloat("_WaveAmplitude", NewGameSettings.Instance.NewMapSettings.NoiseAmplitude);
        hmapMaterial.SetFloat("_WaveFrequency", NewGameSettings.Instance.NewMapSettings.NoiseFrequency);
        hmapMaterial.SetFloat("_WaveFbmGain", NewGameSettings.Instance.NewMapSettings.FbmGain);

        // <Seed> is a value between 0 and 1024^3.
        // It is used to create a 3d vector with random values between 0 and 1023 which is used to offset the procedural noise texture
        // The map has a size of 1, so offsetting by 1 gives a completely new map, meaning we get 1024^3 unique maps
        int? seed = NewGameSettings.Instance.NewMapSettings.Seed;
        if (seed == null)
            seed = (int)new System.Random().Next(0, 1024*1024*1024);
        Seed = (int)seed;
        var seedVector = new Vector4((float)(seed & 1023), (float)((seed << 10) & 1023), (float)((seed << 20) & 1023));
        hmapMaterial.SetVector("_Offset", seedVector);
        hmapMaterial.SetFloat("_WaveFbmGain", NewGameSettings.Instance.NewMapSettings.FbmGain);
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
                this[x, y] = TileType.City;
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
                if (this[i, j] == tileTypeToLookFor)
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
                //Debug.Log(pixel);
                if (pixel > waterLevel)
                    tileType = TileType.Land;
                else
                    tileType = TileType.Water;
                this[x, y] = tileType;
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

    private void CreateQuad()
    {
        GameObject quad = new GameObject("HmapQuad");
        quad.layer = HmapLayerNumber;
        MeshFilter filter = quad.AddComponent<MeshFilter>();
        MeshRenderer renderer = quad.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3 (-1, -1, 1),
            new Vector3 (1, -1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (-1, 1, 1)
         };

        mesh.triangles = new int[] {
            0, 3, 2,
            0, 2, 1
        };
        filter.mesh = mesh;
        renderer.material = hmapMaterial;

        //var go = new GameObject();
        //var screenGrabRenderer = go.AddComponent<MeshRenderer>();
        screenGrabRenderer.material.mainTexture = hmapTexture;
        //filter = go.AddComponent<MeshFilter>();
        //filter.mesh = mesh;
        //go.transform.SetPositionAndRotation(new Vector3(-30, 10, -20), Quaternion.identity);
        //go.transform.localScale = new Vector3(10, 10, 1);
        //screenGrabRenderer.enabled = false;
    }
}
