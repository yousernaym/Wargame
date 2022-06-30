using System.Collections;
using System.Collections.Generic;
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
        var globalTilemap = GameObject.Find("Grid/GlobalTilemap");
        var globalMapRenderer = globalTilemap.GetComponent<MapRenderer>();
        globalMap = new Map(globalMapRenderer);
        hmap = gameObject.GetComponent<Hmap>();
        hmap.Init(globalMap);
        foreach (var playerSetting in NewGameSettings.Instance.PlayerSettings)
            players.Add(new Player(playerSetting, globalMap, prodDialog));
        globalMap.Generate(hmap);
        Player.AssignStartingCities(players, globalMap);
        globalMap.Show();
        globalMap.Renderer.SetZoomPreset(1);
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
        }
    }
}
