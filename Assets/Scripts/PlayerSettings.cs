using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings
{
    public static int PlayerCount { get; private set; }
    static Color[] playerColors;
    public const int MaxPlayers = 4;
    public Color Color
    {
        get => GetPlayerColor(PlayerIndex);
        set => playerColors[PlayerIndex] = value;
    }
    public int PlayerIndex { get; private set; }
    public string Name;
    public int AiLevel = 0; //0 = Human
    public float CombatFactor = 0.5f;
    public float ProdFactor = 0.5f;

    public PlayerSettings(int aILevel)
    {
        AiLevel = aILevel;
        PlayerIndex = PlayerCount++;
        Name = $"Player {PlayerIndex}";
        //if (PlayerNumber == 0)
        //    Color = Color.red;
        //else if (PlayerNumber == 1)
        //    Color = Color.rgb;
        //else if (PlayerNumber == 2)
        //    Color = Color.yellow;
        //else if (PlayerNumber == 3)
        //    Color = Color.cyan;
    }

    protected PlayerSettings(PlayerSettings copy)
    {
        AiLevel = copy.AiLevel;
        Name = copy.Name;
        PlayerIndex = copy.PlayerIndex;
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

    public static Color GetPlayerColor(int playerIndex)
    {
        if (playerColors == null)
        {
            playerColors = new Color[MaxPlayers];
            //for (int i = 0; i < MaxPlayers; i++)
            //playerColors[i] = Color.HSVToRGB((float)i / MaxPlayers, 0.5f, 1);
            playerColors[0] = Color.yellow;
            playerColors[1] = Color.red;
            playerColors[2] = new Color(1, 0.5f, 0);
            playerColors[2] = new Color(1, 0, 1);

        }
        return playerColors[playerIndex];
    }
}
