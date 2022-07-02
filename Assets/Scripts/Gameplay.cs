using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CameraAction { ZoomIn, ZoomOut, ZoomPreset1, ZoomPreset2, ZoomPreset3, ViewEntireMap, PanLeft, PanRight, PanUp, PanDown, CenterOnActiveTile }

public class Gameplay : MonoBehaviour
{
    Map globalMap;
    Hmap hmap;
    List<Player> players = new List<Player>();
    int currentTurn;
    Player currentPlayer => players[currentPlayerIndex];
    int currentPlayerIndex;
    [SerializeField] ProdDialog prodDialog;
    Vector2 panReferencePos;

    void Start()
    {
        //var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var globalTilemap = (GameObject)gameObjects.First(obj => obj.name == "GlobalMap");
        var globaMapGO = GameObject.Find("Grid/GlobalMap");
        var globalMapRenderer = globaMapGO.GetComponent<MapRenderer>();
        globalMap = new Map(globalMapRenderer, null);
        hmap = gameObject.GetComponent<Hmap>();
        hmap.Init(globalMap);
        foreach (var playerSetting in NewGameSettings.Instance.PlayerSettings)
            players.Add(new Player(playerSetting, globalMap, prodDialog));
        globalMap.Generate(hmap);
        Player.AssignStartingCities(players, globalMap);
        ShowMap(currentPlayer);
        currentPlayer.Map.Renderer.SetZoomPreset(1);
    }

    void Update()
    {
        if (currentPlayer.ProcessCurrentState() == PlayerState.EndTurn)
        {
            currentPlayer.State = PlayerState.StartTurn;
            if (++currentPlayerIndex >= players.Count)
            {
                currentPlayerIndex = 0;
                currentTurn++;
            }
            ShowMap(currentPlayer);
        }
        CheckCameraInput();
    }

    void ShowMap(Player player)
    {
        if (currentPlayer.AiLevel == 0 && !currentPlayer.IsRemote)
        {
            foreach (var otherPlayer in players)
                otherPlayer.Map.Renderer.Hide();
            player.Map.Renderer.Show();
        }
    }

    void CheckCameraInput()
    {
        if (currentPlayer.AiLevel > 0)
            return;
        var mapRenderer = currentPlayer.Map.Renderer;
        foreach (var cameraAction in InputMappings.CameraActions)
        {
            if (Input.GetKeyDown(cameraAction.Key))
            {
                if (cameraAction.Value == CameraAction.ZoomPreset1)
                    mapRenderer.SetZoomPreset(0);
                else if (cameraAction.Value == CameraAction.ZoomPreset2)
                    mapRenderer.SetZoomPreset(1);
                else if (cameraAction.Value == CameraAction.ZoomPreset3)
                    mapRenderer.SetZoomPreset(2);
                else if (cameraAction.Value == CameraAction.ViewEntireMap)
                    mapRenderer.ViewEntireMap();
                return;
            }
        }

        mapRenderer.ZoomLevel -= Input.mouseScrollDelta.y;
        if (Input.GetMouseButtonDown(1))
            panReferencePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        if (Input.GetMouseButton(1))
        {
            var mouseDelta = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(panReferencePos.x, panReferencePos.y);
            mapRenderer.Pan(mouseDelta);
            panReferencePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
    }
}
