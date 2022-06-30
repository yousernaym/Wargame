using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
    Map map;
    Hmap hmap;
    List<Player> players = new List<Player>();
    int currentTurn;
    Player currentPlayer => players[currentPlayerIndex];
    int currentPlayerIndex;
    [SerializeField] ProdDialog prodDialog;

    void Start()
    {
        map = new Map();
        hmap = gameObject.GetComponent<Hmap>();
        hmap.Init(map);
        foreach (var playerSetting in NewGameSettings.Instance.PlayerSettings)
            players.Add(new Player(playerSetting, map, prodDialog));
        map.Generate(hmap);
        Player.AssignStartingCities(players, map);
        map.Show();
        MapRenderer.Instance.SetZoomPreset(1);
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
