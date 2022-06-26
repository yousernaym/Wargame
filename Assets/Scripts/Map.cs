using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
    [SerializeField] Camera rtCamera;
    Material hmapMaterial;
    Tile grassTile;
    Tile waterTile;
    Tile cityTile;
    public static int MapWidth = 80;
    public static int MapHeight = 60;
    static float Aspect => (float)MapHeight / MapWidth;
    static float WaterLevel = 0.56f;

    [SerializeField] Renderer screenGrabRenderer; //Shows height map for debugging purposes

    const int HmapResolutionFactor = 20;
    int hmapWidth => MapWidth * HmapResolutionFactor;
    int hmapHeight => MapHeight * HmapResolutionFactor;

    Tilemap tilemap;
    Texture2D hmapTexture;
    bool isPerformingScreenGrab = false;
    string HmapLayerName = "Hmap";
    int HmapLayerNumber => LayerMask.NameToLayer(HmapLayerName);

    public void Start()
    {
        MapWidth = NewGameSettings.Instance.NewMapSettings.Width;
        MapHeight = NewGameSettings.Instance.NewMapSettings.Height;
        WaterLevel = NewGameSettings.Instance.NewMapSettings.WaterLevel;
        LoadResources();
        hmapTexture = new Texture2D(hmapWidth, hmapHeight, TextureFormat.RFloat, true);
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
        hmapMaterial.SetFloat("_WaveFrequency", NewGameSettings.Instance.NewMapSettings.NoiseFrequency);
        hmapMaterial.SetFloat("_WaveFbmGain", NewGameSettings.Instance.NewMapSettings.FbmGain);

        // <seed> is a value between 0 and 1024^3.
        // It is used to create a 3d vector with random values between 0 and 1023 which is used to offset the procedural noise texture
        // The map has a size of 1, so offsetting by 1 gives a completely new map, meaning we get 1024^3 unique maps
        int? seed = NewGameSettings.Instance.NewMapSettings.Seed;
        if (seed == null)
            seed = (int)new System.Random().Next(0, 1024*1024*1024);
        var seedVector = new Vector4((float)(seed & 1023), (float)((seed << 10) & 1023), (float)((seed << 20) & 1023));
        hmapMaterial.SetVector("_Offset", seedVector);
        hmapMaterial.SetFloat("_WaveFbmGain", NewGameSettings.Instance.NewMapSettings.FbmGain);
    }

    private void CreateCamera()
    {
        //var rtCameraObject = new GameObject();
        //rtCamera = rtCameraObject.AddComponent<Camera>();
        //rtCamera.depth = -1;
        
        rtCamera.targetTexture = new RenderTexture(hmapWidth, hmapHeight, 32, RenderTextureFormat.RFloat); ;
        
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
                hmapTexture.ReadPixels(new Rect(0, 0, hmapWidth, hmapHeight), 0, 0);
                hmapTexture.Apply();
                UpdateTiles();
                isPerformingScreenGrab = false;
            }
        }
    }

    public void GenerateMap()
    {
        CenterMap();
        isPerformingScreenGrab = true;
    }

    public void UpdateTiles()
    {
        tilemap.ClearAllTiles();
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                Tile tile;
                float pixel = hmapTexture.GetPixel(x * HmapResolutionFactor, y * HmapResolutionFactor).r;
                //Debug.Log(pixel);
                if (pixel > WaterLevel)
                    tile = grassTile;
                else
                    tile = waterTile;
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
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
