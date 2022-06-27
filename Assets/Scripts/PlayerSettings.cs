using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings
{
    public static int PlayerCount { get; private set; }
    public const int MaxPlayers = 4;
    public Color Color { get; private set; }
    public int PlayerNumber { get; private set; }
    public string Name;
    public int AiLevel = 0; //0 = Human
    public float CombatFactor = 0.5f;
    public float ProdFactor = 0.5f;

    public PlayerSettings(int aILevel)
    {
        AiLevel = aILevel;
        PlayerNumber = PlayerCount++;
        Name = $"Player {PlayerNumber}";
        //Color = Color.HSVToRGB((float)PlayerNumber / MaxPlayers, 1, 1);
        if (PlayerNumber == 0)
            Color = Color.red;
        else if (PlayerNumber == 1)
            Color = Color.black;
        else if (PlayerNumber == 2)
            Color = Color.yellow;
        else if (PlayerNumber == 3)
            Color = Color.cyan;
    }

    protected PlayerSettings(PlayerSettings copy)
    {
        AiLevel = copy.AiLevel;
        Name = copy.Name;
        PlayerNumber = copy.PlayerNumber;
        CombatFactor = copy.CombatFactor;
        ProdFactor = copy.ProdFactor;
        Color = copy.Color;
    }

    public static List<PlayerSettings> CreateDefault()
    {
        PlayerCount = 0;
        var players = new List<PlayerSettings>();
        players.Add(new PlayerSettings(0));
        for (int i = 0; i < 2; i++)
            players.Add(new PlayerSettings(1));
        return players;
    }
}
