using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class PlayerSettings : ISerializable
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
    public float CombatEfficientcy = 0.5f;
    public float ProdEfficiency = 0.5f;

    public PlayerSettings(int aILevel)
    {
        AiLevel = aILevel;
        PlayerIndex = PlayerCount++;
        Name = $"Player {PlayerIndex}";
    }

    protected PlayerSettings(PlayerSettings copy)
    {
        AiLevel = copy.AiLevel;
        Name = copy.Name;
        PlayerIndex = copy.PlayerIndex;
        CombatEfficientcy = copy.CombatEfficientcy;
        ProdEfficiency = copy.ProdEfficiency;
        Color = copy.Color;
    }

    public PlayerSettings(SerializationInfo info, StreamingContext ctxt)
    {
        foreach (SerializationEntry entry in info)
        {
            if (entry.Name == "PlayerIndex")
                PlayerIndex = (int)entry.Value;
            else if (entry.Name == "Name")
                Name = (string)entry.Value;
            else if (entry.Name == "AiLevel")
                AiLevel = (int)entry.Value;
            else if (entry.Name == "CombatEfficientcy")
                CombatEfficientcy = (float)entry.Value;
            else if (entry.Name == "ProdEfficiency")
                ProdEfficiency = (float)entry.Value;
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("PlayerIndex", PlayerIndex);
        info.AddValue("Name", Name);
        info.AddValue("AiLevel", AiLevel);
        info.AddValue("CombatEfficientcy", CombatEfficientcy);
        info.AddValue("ProdEfficiency", ProdEfficiency);
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
