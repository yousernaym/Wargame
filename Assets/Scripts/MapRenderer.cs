using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapRenderer : MonoBehaviour
{
    Tilemap tilemap;
    Tile grassTile;
    Tile waterTile;
    Tile neutralCityTile;

    void Awake()
    {
        grassTile = Resources.Load<Tile>("Tiling/grassTile");
        waterTile = Resources.Load<Tile>("Tiling/waterTile");
        neutralCityTile = Resources.Load<Tile>("Tiling/cityTile");
        tilemap = gameObject.GetComponent<Tilemap>();
    }

    public void CenterMap(int width, int height)
    {
        tilemap.transform.position = new Vector3(-width / 2.0f, -height / 2.0f, 0);
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
   
}
