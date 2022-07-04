using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hmap : MonoBehaviour
{
    [SerializeField] Camera rtCamera;
    [SerializeField] Renderer screenGrabRenderer; //Shows height map for debugging purposes
    Map map;
    Material material;
    const int resolutionFactor = 10;
    int Width => map.Width * resolutionFactor;
    int Height => map.Height * resolutionFactor;
    Texture2D texture;
    float waterLevel;
    string layerName = "Hmap";
    int layerNumber => LayerMask.NameToLayer(layerName);

    public void Init(Map map)
    {
        LoadResources();
        this.map = map;
        texture = new Texture2D(Width, Height, TextureFormat.RFloat, true);
        texture.name = "HmapTexture";
        CreateQuad();
        rtCamera.targetTexture = new RenderTexture(Width, Height, 32, RenderTextureFormat.RFloat); ;
        waterLevel = NewGameSettings.Instance.NewMapSettings.WaterLevel;
    }

    private void LoadResources()
    {
        material = Resources.Load<Material>("Tiling/HmapMaterial");
        material = ScriptableObject.Instantiate(material); //Instantiate to prevent editing of the asset file and annoying git changes
        material.SetFloat("_WaveAmplitude", NewGameSettings.Instance.NewMapSettings.NoiseAmplitude);
        material.SetFloat("_WaveFrequency", NewGameSettings.Instance.NewMapSettings.NoiseFrequency);
        material.SetFloat("_WaveFbmGain", NewGameSettings.Instance.NewMapSettings.FbmGain);
        material.SetFloat("_WaveFbmGain", NewGameSettings.Instance.NewMapSettings.FbmGain);

        // <seed> is a value between 0 and 1024^3.
        // It is used to create a 3d vector with random values between 0 and 1023 which is used to offset the procedural noise texture
        // The map has a size of 1, so offsetting by 1 gives a completely new map, meaning we get 1024^3 unique maps
        int seed = NewGameSettings.Instance.NewMapSettings.GetSeed();
        var seedVector = new Vector4((float)(seed & 1023), (float)((seed << 10) & 1023), (float)((seed << 20) & 1023));
        material.SetVector("_Offset", seedVector);
    }

    void CreateQuad()
    {
        GameObject quad = new GameObject("HmapQuad");
        quad.layer = layerNumber;
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
        renderer.material = material;
        screenGrabRenderer.material.mainTexture = texture;
    }

    internal TileType GetTileType(int x, int y)
    {
        TileType tileType;
        var pixel = texture.GetPixel(x * resolutionFactor, y * resolutionFactor).r;
        if (pixel > waterLevel)
            tileType = TileType.Land;
        else
            tileType = TileType.Water;
        return tileType;
    }

    public void Generate()
    {
        rtCamera.Render();
        RenderTexture.active = rtCamera.targetTexture;
        texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        RenderTexture.active = null;
        texture.Apply();
    }
}
