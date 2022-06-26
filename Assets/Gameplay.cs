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
        map = GetComponent<Map>();
        map.Init();

        foreach (var playerSetting in NewGameSettings.Instance.Players)
            players.Add(new Player(playerSetting));
        Player.AssignStartingCities(players, map);

        map.ViewEntireMap();
    }


    void Update()
    {

    }
}
