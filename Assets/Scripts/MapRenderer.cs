using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRenderer : MonoBehaviour
{
    float width;
    float height;
    Tilemap baseTilemap;
    Tilemap unitTilemap;
    //Tile whiteTile;
    Tile grassTile;
    Tile waterTile;
    Tile neutralCityTile;
    Tile unexploredTile;
    Tile[] playerCityTiles;
    Dictionary<UnitType, Tile[]> playerUnitTiles;

    Camera Camera => Camera.main;
    RectTransform canvasRt;
    float[] zoomPresets = new float[] { 10, 20, 30 };
    float zoomLevel = 60;
    public float ZoomLevel
    {
        get => zoomLevel;
        set
        {
            Camera.fieldOfView = value * 1;
            zoomLevel = value;
        }
    }
    
    public Vector3 CamPos
    {
        get => Camera.transform.position;
        set
        {
            if (gameObject.activeInHierarchy)
                Camera.transform.position = value;
        }
    }

    bool visible;
    public bool Visible
    {
        get => visible;
        set
        {
            visible = value;
            baseTilemap.GetComponent<TilemapRenderer>().enabled = value;
            unitTilemap.GetComponent<TilemapRenderer>().enabled = value;
        }
    }

    void Awake()
    {
        var whiteTile = Resources.Load<Tile>("Tiling/whiteTile");
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
        playerUnitTiles = new Dictionary<UnitType, Tile[]>();
        var whiteUnits = new Dictionary<UnitType, Tile>();
        foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
        {
            whiteUnits[type] = Resources.Load<Tile>("Tiling/Units/" + Enum.GetName(typeof(UnitType), type) + "Tile");
            playerUnitTiles[type] = new Tile[PlayerSettings.MaxPlayers];
            for (int i = 0; i < PlayerSettings.MaxPlayers; i++)
            {
                playerUnitTiles[type][i] = null;
                if (whiteUnits[type] == null)
                    continue;
                playerUnitTiles[type][i] = ScriptableObject.Instantiate(whiteUnits[type]);
                playerUnitTiles[type][i].color = PlayerSettings.GetPlayerColor(i);
            }
        }

        var btTransform = gameObject.transform.Find("BaseTilemap");
        baseTilemap = btTransform.GetComponent<Tilemap>();
        baseTilemap.ClearAllTiles();
        unitTilemap = gameObject.transform.Find("UnitTilemap").GetComponent<Tilemap>();
        unitTilemap.ClearAllTiles();
        width = NewGameSettings.Instance.NewMapSettings.Width.value;
        height = NewGameSettings.Instance.NewMapSettings.Height.value;
        canvasRt = GameObject.Find("Canvas").GetComponent<RectTransform>();
        ViewEntireMap();
        Visible = false;
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

    public void Pan(Vector2 screenSpaceOffset)
    {
        var tileStartScreenSpace = Camera.WorldToScreenPoint(new Vector3(0, 0, 0)).x;
        var tileEndScreenSpace = Camera.WorldToScreenPoint(new Vector3(1, 0, 0)).x;
        var tileSizeScreenSpace = tileEndScreenSpace - tileStartScreenSpace;
        CamPos -= new Vector3(screenSpaceOffset.x, screenSpaceOffset.y, 0) / tileSizeScreenSpace;
    }

    public Vector2 TileToCanvasPos(Vector2Int pos)
    {
        Vector3 screenPos = TileToScreenPos(pos);
        var canvasPos = new Vector2(screenPos.x * canvasRt.rect.width / Screen.width, screenPos.y * canvasRt.rect.height / Screen.height);
        return canvasPos;
    }

    private Vector3 TileToScreenPos(Vector2Int pos)
    {
        var worldPos = baseTilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
        var screenPos = Camera.WorldToScreenPoint(worldPos);
        return screenPos;
    }

    public void UpdateTiles(Map map)
    {
        baseTilemap.ClearAllTiles();
        unitTilemap.ClearAllTiles();
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                UpdateTile(x, y, map);
            }
        }
    }

    public void UpdateTile(int x, int y, Map map)
    {
        Tile baseTile = null;
        MapTile mapTile = map[x, y];
        if (mapTile.TileType == TileType.Land)
            baseTile = grassTile;
        else if (mapTile.TileType == TileType.Water)
            baseTile = waterTile;
        else if (mapTile.TileType == TileType.Unexplored)
            baseTile = unexploredTile;
        else if (mapTile.TileType == TileType.City)
        {
            var owner = mapTile.City.Owner;
            baseTile = owner == null ? neutralCityTile : playerCityTiles[owner.PlayerIndex];
        }

        Tile unitTile = null;
        if (mapTile.Unit != null)
            unitTile = playerUnitTiles[mapTile.Unit.Type][mapTile.Unit.Owner.PlayerIndex];

        unitTilemap.SetTile(new Vector3Int(x, y, 0), unitTile);
        baseTilemap.SetTile(new Vector3Int(x, y, 0), baseTile);

    }

    public void MoveCameraToTile(Vector2Int pos)
    {
        if (!IsTileInView(pos))
            CenterCameraOnTile(pos);
    }

    public void CenterCameraOnTile(Vector2Int pos)
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
}
