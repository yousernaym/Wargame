using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
    Map map;
    List<Player> players = new List<Player>();
    int currentTurn;

    void Start()
    {
        map = new Map();

        foreach (var playerSetting in NewGameSettings.Instance.Players)
            players.Add(new Player(playerSetting, map));
        Player.AssignStartingCities(players, map);

        map.ViewEntireMap();
    }


    void Update()
    {

    }
}
