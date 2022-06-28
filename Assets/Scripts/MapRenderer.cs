using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRenderer : MonoBehaviour
{
    public static MapRenderer Instance { get; private set; }
    float width;
    float height;
    Tilemap tilemap;
    Tile grassTile;
    Tile waterTile;
    Tile neutralCityTile;
    Camera Camera => Camera.main;
    Vector3 CamPos
    {
        get => Camera.transform.position;
        set => Camera.transform.position = value;
    }
    

    void Awake()
    {
        Instance = this;
        grassTile = Resources.Load<Tile>("Tiling/grassTile");
        waterTile = Resources.Load<Tile>("Tiling/waterTile");
        neutralCityTile = Resources.Load<Tile>("Tiling/cityTile");
        tilemap = gameObject.GetComponent<Tilemap>();
        width = NewGameSettings.Instance.NewMapSettings.Width.value;
        height = NewGameSettings.Instance.NewMapSettings.Height.value;
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

    public void UpdateTilemap(Map map)
    {
        tilemap.ClearAllTiles();
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                Tile tile = null;
                MapTile mapTile = map[x, y];
                if (mapTile.TileType == TileType.Land)
                    tile = grassTile;
                else if (mapTile.TileType == TileType.Water)
                    tile = waterTile;
                else if (mapTile.TileType == TileType.City)
                {
                    var owner = mapTile.City.Owner;
                    tile = owner == null ? neutralCityTile : owner.CityTile;
                }
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    public void MoveCameraToTile(Vector2Int pos, bool center = false)
    {
        CamPos = new Vector3(pos.x, pos.y, CamPos.z);
    }
}
