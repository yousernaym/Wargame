using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRenderer : MonoBehaviour
{
    float width;
    float height;
    Tilemap tilemap;
    Tile whiteTile;
    Tile grassTile;
    Tile waterTile;
    Tile neutralCityTile;
    Tile unexploredTile;
    Tile[] playerCityTiles;
    Camera Camera => Camera.main;
    RectTransform canvasRt;
    float[] zoomPresets = new float[] { 10, 20, 30 };
    public float ZoomLevel
    {
        get => -CamPos.z;
        set
        {
            var pos = CamPos;
            pos.z = -value;
            CamPos = pos;
        }
    }
    
    public Vector3 CamPos
    {
        get => Camera.transform.position;
        set => Camera.transform.position = value;
    }
    
    void Awake()
    {
        whiteTile = Resources.Load<Tile>("Tiling/whiteTile");
        grassTile = ScriptableObject.Instantiate(whiteTile);
        grassTile.color = Color.green;
        waterTile = ScriptableObject.Instantiate(whiteTile);
        waterTile.color = Color.blue;
        neutralCityTile = ScriptableObject.Instantiate(whiteTile);
        neutralCityTile.color = Color.white;
        unexploredTile = ScriptableObject.Instantiate(whiteTile);
        unexploredTile.color = Color.black;
        playerCityTiles = new Tile[PlayerSettings.MaxPlayers];
        for (int i = 0; i < PlayerSettings.MaxPlayers; i++)
        {
            playerCityTiles[i] = ScriptableObject.Instantiate(whiteTile); ;
            playerCityTiles[i].color = PlayerSettings.GetPlayerColor(i); ;
        }
                
        tilemap = gameObject.GetComponent<Tilemap>();
        tilemap.ClearAllTiles();
        width = NewGameSettings.Instance.NewMapSettings.Width.value;
        height = NewGameSettings.Instance.NewMapSettings.Height.value;
        canvasRt = GameObject.Find("Canvas").GetComponent<RectTransform>();
    }

    void Update()
    {
        var camPos = Camera.transform.position;
        if (Input.GetKeyDown(KeyCode.Alpha1))
            camPos.z = -zoomPresets[0];
        if (Input.GetKeyDown(KeyCode.Alpha2))
            camPos.z = -zoomPresets[1];
        if (Input.GetKeyDown(KeyCode.Alpha3))
            camPos.z = -zoomPresets[2];
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ViewEntireMap();
            return;
        }
        
        Camera.transform.position = camPos;
    }

    public void CenterCamera()
    {
        CamPos = new Vector3(width / 2, height / 2, CamPos.z);
    }

    public void ViewEntireMap()
    {
        float zh = (float)((height + 2) / 2.0f / Math.Tan(Camera.fieldOfView / 2 * Math.PI / 180));
        float horizontalFov = Camera.VerticalToHorizontalFieldOfView(Camera.fieldOfView, Camera.aspect);
        float zw = (float)((width + 2) / 2.0f / Math.Tan(horizontalFov / 2 * Math.PI / 180));
        float z = -Math.Max(zh, zw);

        Camera.transform.SetPositionAndRotation(new Vector3(width / 2, height / 2, z), Quaternion.identity);
    }

    public Vector2 TileToCanvasPos(Vector2Int pos)
    {
        Vector3 screenPos = TileToScreenPos(pos);
        var canvasPos = new Vector2(screenPos.x * canvasRt.rect.width / Screen.width, screenPos.y * canvasRt.rect.height / Screen.height);
        return canvasPos;
    }

    private Vector3 TileToScreenPos(Vector2Int pos)
    {
        var worldPos = tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
        var screenPos = Camera.WorldToScreenPoint(worldPos);
        return screenPos;
    }

    //public void UpdateTiles(Map map)
    //{
    //    tilemap.ClearAllTiles();
    //    for (int y = 0; y < map.Height; y++)
    //    {
    //        for (int x = 0; x < map.Width; x++)
    //        {
    //            UpdateTile(x, y, map);
    //        }
    //    }
    //}

    public void UpdateTile(int x, int y, Map map)
    {
        Tile tile = null;
        MapTile mapTile = map[x, y];
        if (mapTile.TileType == TileType.Land)
            tile = grassTile;
        else if (mapTile.TileType == TileType.Water)
            tile = waterTile;
        else if (mapTile.TileType == TileType.Unexplored)
            tile = unexploredTile;
        else if (mapTile.TileType == TileType.City)
        {
            var owner = mapTile.City.Owner;
            tile = owner == null ? neutralCityTile : playerCityTiles[owner.PlayerIndex];
        }
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    public void MoveCameraToTile(Vector2Int pos, bool center = false)
    {
        CamPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, CamPos.z);
    }

    public bool IsTileInView(Vector2Int pos)
    {
        var screenPos = TileToScreenPos(pos);
        return screenPos.x < Screen.width - 0.5f && screenPos.x > 0.5f
            && screenPos.y < Screen.height - 0.5f && screenPos.y > 0.5f;
    }

    public void SetZoomPreset(int preset)
    {
        ZoomLevel = zoomPresets[preset];
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
