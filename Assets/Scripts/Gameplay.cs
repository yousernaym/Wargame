using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

public enum CameraAction { ZoomIn, ZoomOut, ZoomPreset1, ZoomPreset2, ZoomPreset3, ViewEntireMap, PanLeft, PanRight, PanUp, PanDown, CenterOnActiveTile }

public class Gameplay : MonoBehaviour
{
    // If starting the game from a savefile, set this to true before game scene is loaded, then set it to false at end of Start()
    // (I can't figure out a proper way to transition between scenes in Unity)
    public static string SavedGamePath;
    Map GlobalMap
    {
        get => serializableData.GlobalMap;
        set => serializableData.GlobalMap = value;
    }
    
    Hmap hmap;
    List<Player> Players => serializableData.Players;
    int CurrentTurn
    {
        get => serializableData.CurrentTurn;
        set => serializableData.CurrentTurn = value;
    }
    
    Player CurrentPlayer => Players[CurrentPlayerIndex];
    int CurrentPlayerIndex
    {
        get => serializableData.CurrentPlayerIndex;
        set => serializableData.CurrentPlayerIndex = value;
    }
    
    [SerializeField] ProdDialog prodDialog;
    Vector2 panReferencePos;

    [Serializable]
    class SerializableData : ISerializable
    {
        public List<Player> Players = new List<Player>();
        public int CurrentTurn;
        public int CurrentPlayerIndex;
        public Map GlobalMap;

        public SerializableData()
        {

        }

        public SerializableData(SerializationInfo info, StreamingContext ctxt)
        {
            foreach (SerializationEntry entry in info)
            {
                if (entry.Name == "Players")
                    Players = (List<Player>)entry.Value;
                else if (entry.Name == "CurrentTurn")
                    CurrentTurn = (int)entry.Value;
                else if (entry.Name == "CurrentPlayerIndex")
                    CurrentPlayerIndex = (int)entry.Value;
                else if (entry.Name == "GlobalMap")
                    GlobalMap = (Map)entry.Value;
            }
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Players", Players);
            info.AddValue("CurrentTurn", CurrentTurn);
            info.AddValue("CurrentPlayerIndex", CurrentPlayerIndex);
            info.AddValue("GlobalMap", GlobalMap);
        }
    }

    SerializableData serializableData;

    void Start()
    {
        //var gameObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var globalTilemap = (GameObject)gameObjects.First(obj => obj.name == "GlobalMap");
        var globalMapGO = GameObject.Find("Grid/GlobalMap");
        var globalMapRenderer = globalMapGO.GetComponent<MapRenderer>();
        
        if (SavedGamePath != null)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(SerializableData), Cloning.KnownTypes);
            FileStream stream = new FileStream(SavedGamePath, FileMode.Open);
            serializableData = (SerializableData)dcs.ReadObject(stream);
            GlobalMap.Renderer = globalMapRenderer;
            GlobalMap.Renderer.UpdateTiles(GlobalMap);
            foreach (var player in Players)
                player.ProdDialog = prodDialog;
        }
        else
        {
            serializableData = new SerializableData();
            GlobalMap = new Map(globalMapRenderer, null);
            foreach (var playerSetting in NewGameSettings.Instance.PlayerSettings)
                Players.Add(new Player(playerSetting, GlobalMap, prodDialog));
        }

        hmap = gameObject.GetComponent<Hmap>();
        hmap.Init(GlobalMap);
        GlobalMap.Generate(hmap);

        if (SavedGamePath == null)
            Player.AssignStartingCities(Players, GlobalMap);

        ShowMap(CurrentPlayer);
        CurrentPlayer.Map.Renderer.SetZoomPreset(1);
    }

    void Update()
    {
        if (CurrentPlayer.ProcessCurrentState() == PlayerState.EndTurn)
        {
            CurrentPlayer.State = PlayerState.StartTurn;
            if (++CurrentPlayerIndex >= Players.Count)
            {
                CurrentPlayerIndex = 0;
                CurrentTurn++;
            }
            ShowMap(CurrentPlayer);
        }
        CheckCameraInput();
    }

    void ShowMap(Player player)
    {
        if (CurrentPlayer.AiLevel == 0)
        {
            foreach (var otherPlayer in Players)
                otherPlayer.Map.Renderer.Visible = false;
            player.Map.Renderer.Visible = true;
        }
    }

    void CheckCameraInput()
    {
        if (CurrentPlayer.AiLevel > 0)
            return;
        var mapRenderer = CurrentPlayer.Map.Renderer;
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
