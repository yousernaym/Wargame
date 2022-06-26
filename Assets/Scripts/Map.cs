using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    enum TileType { Land, Water, City }
    [SerializeField] Camera rtCamera;
    Material hmapMaterial;
    Tile grassTile;
    Tile waterTile;
    Tile cityTile;
    public int MapWidth;
    public int MapHeight;
    float Aspect => (float)MapHeight / MapWidth;
    float waterLevel;
    public int Seed { get; private set; }
    public int CityCount { get; private set; }

    [SerializeField] Renderer screenGrabRenderer; //Shows height map for debugging purposes

    const int HmapResolutionFactor = 20;
    int HmapWidth => MapWidth * HmapResolutionFactor;
    int HmapHeight => MapHeight * HmapResolutionFactor;

    TileType?[,] tiles;
    TileType? this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight)
                return null;
            return tiles[x, y];
        }
        set
        {
            if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight)
                return;
            Tile tile = null;
            if (value == TileType.City)
            {
                if (tiles[x, y] != TileType.City)
                    CityCount++;
                tile = cityTile;
            }
            else
            {
                if (tiles[x, y] == TileType.City)
                    CityCount--;
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
    bool isPerformingScreenGrab = false;
    string HmapLayerName = "Hmap";
    System.Random random;

    int HmapLayerNumber => LayerMask.NameToLayer(HmapLayerName);

    public void Start()
    {
        MapWidth = NewGameSettings.Instance.NewMapSettings.Width.value;
        MapHeight = NewGameSettings.Instance.NewMapSettings.Height.value;
        tiles = new TileType?[MapWidth, MapHeight];
        waterLevel = NewGameSettings.Instance.NewMapSettings.WaterLevel;
        LoadResources();
        hmapTexture = new Texture2D(HmapWidth, HmapHeight, TextureFormat.RFloat, true);
        hmapTexture.name = "HmapTexture";
        CreateQuad();
        CreateCamera();
        
        //var tileMapObject = GameObject.Find("Tilemap");
        //tilemap = tileMapObject.GetComponent<Tilemap>();
        tilemap = gameObject.GetComponent<Tilemap>();
        Camera.onPostRender += OnPostRenderCallback;
        ViewEntireMap();
        GenerateMap();
    }

    private void Update()
    {
        GenerateMap();
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

    private void CreateCamera()
    {
        //var rtCameraObject = new GameObject();
        //rtCamera = rtCameraObject.AddComponent<Camera>();
        //rtCamera.depth = -1;
        
        rtCamera.targetTexture = new RenderTexture(HmapWidth, HmapHeight, 32, RenderTextureFormat.RFloat); ;
        
        //rtCamera.cullingMask = 1 << HmapLayerNumber;
        //rtCamera.orthographic = true;
        //rtCamera.orthographicSize = Aspect;
        //rtCameraObject.SetActive(true);
    }

    private void OnPostRenderCallback(Camera cam)
    {
        if (isPerformingScreenGrab)
        {
            if (cam == rtCamera)
            {
                hmapTexture.ReadPixels(new Rect(0, 0, HmapWidth, HmapHeight), 0, 0);
                hmapTexture.Apply();
                UpdateTilesFromHmap();
                GenerateCities();
                isPerformingScreenGrab = false;
            }
        }
    }

    private void GenerateCities()
    {
        while (CityCount < NewGameSettings.Instance.NewMapSettings.CityCount.value)
        {
            int x = random.Next(MapWidth);
            int y = random.Next(MapHeight);
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
        isPerformingScreenGrab = true;
    }

    public void UpdateTilesFromHmap()
    {
        tilemap.ClearAllTiles();
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
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
        float zh = (float)((MapHeight + 2) / 2.0f / Math.Tan(Camera.main.fieldOfView / 2 * Math.PI / 180));
        float horizontalFov = Camera.VerticalToHorizontalFieldOfView(Camera.main.fieldOfView, Camera.main.aspect);
        float zw = (float)((MapWidth + 2) / 2.0f / Math.Tan(horizontalFov / 2 * Math.PI / 180));
        float z = -Math.Max(zh, zw);

        Camera.main.transform.SetPositionAndRotation(new Vector3(0, 0, z), Quaternion.identity);
    }

    private void CenterMap()
    {
        tilemap.transform.position = new Vector3(-MapWidth / 2.0f, -MapHeight / 2.0f, 0);
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
