using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
    Map globalMap;
    Hmap hmap;
    List<Player> players = new List<Player>();
    int currentTurn;
    Player currentPlayer => players[currentPlayerIndex];
    int currentPlayerIndex;
    [SerializeField] ProdDialog prodDialog;

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
}
