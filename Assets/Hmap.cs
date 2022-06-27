using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hmap : MonoBehaviour
{
    [SerializeField] Camera rtCamera;
    [SerializeField] Renderer screenGrabRenderer; //Shows height map for debugging purposes
    Map map;
    Material hmapMaterial;
    const int HmapResolutionFactor = 10;
    int HmapWidth => map.Width * HmapResolutionFactor;
    int HmapHeight => map.Height * HmapResolutionFactor;
    Texture2D hmapTexture;
    string HmapLayerName = "Hmap";
    int HmapLayerNumber => LayerMask.NameToLayer(HmapLayerName);

    void Init(Map map)
    {
        this.map = map;
        hmapTexture = new Texture2D(HmapWidth, HmapHeight, TextureFormat.RFloat, true);
        hmapTexture.name = "HmapTexture";
        CreateQuad();
        rtCamera.targetTexture = new RenderTexture(HmapWidth, HmapHeight, 32, RenderTextureFormat.RFloat); ;
    }

    private void LoadResources()
    {
        hmapMaterial = Resources.Load<Material>("Tiling/HmapMaterial");
        hmapMaterial.SetFloat("_WaveAmplitude", NewGameSettings.Instance.NewMapSettings.NoiseAmplitude);
        hmapMaterial.SetFloat("_WaveFrequency", NewGameSettings.Instance.NewMapSettings.NoiseFrequency);
        hmapMaterial.SetFloat("_WaveFbmGain", NewGameSettings.Instance.NewMapSettings.FbmGain);
        hmapMaterial.SetFloat("_WaveFbmGain", NewGameSettings.Instance.NewMapSettings.FbmGain);

        // <seed> is a value between 0 and 1024^3.
        // It is used to create a 3d vector with random values between 0 and 1023 which is used to offset the procedural noise texture
        // The map has a size of 1, so offsetting by 1 gives a completely new map, meaning we get 1024^3 unique maps
        int seed = NewGameSettings.Instance.NewMapSettings.GetSeed();
        var seedVector = new Vector4((float)(seed & 1023), (float)((seed << 10) & 1023), (float)((seed << 20) & 1023));
        hmapMaterial.SetVector("_Offset", seedVector);
    }

    void CreateQuad()
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
        screenGrabRenderer.material.mainTexture = hmapTexture;
    }

}
