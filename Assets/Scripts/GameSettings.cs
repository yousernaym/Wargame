using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameSettings : MonoBehaviour
{
    public Camera RtCamera;
    public Material HmapMaterial;
    public Tile GrassTile;
    public Tile WaterTile;
    public Tile CityTile;
    public int MapWidth;
    public int MapHeight;
    public float WaterLevel = 0.7f;
    public Renderer screenGrabRenderer;

    const int HmapResolutionFactor = 20;
    int hmapWidth => MapWidth * HmapResolutionFactor;
    int hmapHeight => MapHeight * HmapResolutionFactor;

    Tilemap map;
    Texture2D hmapTexture;
    RenderTexture hmapRenderTexture;
    bool isPerformingScreenGrab = false;
    int renderCount = 0;
    string HmapLayerName = "Hmap";
    int HmapLayerNumber => LayerMask.NameToLayer(HmapLayerName);

    //float mapAspect => (float)MapHeight / MapWidth;

    void Start()
    {
        CreateQuad();
        hmapTexture = new Texture2D(hmapWidth, hmapHeight, TextureFormat.RFloat, true);
        hmapRenderTexture = new RenderTexture(hmapTexture.width, hmapTexture.height, 32, RenderTextureFormat.RFloat);
        RtCamera.targetTexture = hmapRenderTexture;
        Debug.Log(HmapLayerNumber);
        RtCamera.cullingMask = 1 << HmapLayerNumber;
        screenGrabRenderer.material.mainTexture = hmapTexture;
        var tileMapObject = GameObject.Find("Tilemap");
        map = tileMapObject.GetComponent<Tilemap>();
        Camera.onPostRender += OnPostRenderCallback;
        CenterMap();
        ViewEntireMap();
        GenerateMap();
    }

    private void OnPostRenderCallback(Camera cam)
    {
        if (isPerformingScreenGrab)
        {
            if (cam == RtCamera)
            {
                hmapTexture.ReadPixels(new Rect(0, 0, hmapWidth, hmapHeight), 0, 0);
                hmapTexture.Apply();
                UpdateTiles();
                isPerformingScreenGrab = false;
            }
        }
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
            GenerateMap();
    }

    private void GenerateMap()
    {
        isPerformingScreenGrab = true;
    }

    public void UpdateTiles()
    {
        map.ClearAllTiles();
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                Tile tile;
                if (hmapTexture.GetPixel(x * HmapResolutionFactor, y * HmapResolutionFactor).r > WaterLevel)
                    tile = GrassTile;
                else
                    tile = WaterTile;
                map.SetTile(new Vector3Int(x, y, 0), tile);
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
        map.transform.position = new Vector3(-MapWidth / 2.0f, -MapHeight / 2.0f, 0);
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
        renderer.material = HmapMaterial;
    }
}
