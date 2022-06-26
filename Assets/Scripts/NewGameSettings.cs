using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGameSettings
{
    public NewMapSettings NewMapSettings = new NewMapSettings();
    public List<Player> Players = new List<Player>();

    static NewGameSettings instance;
    public static NewGameSettings Instance => instance ?? (instance = new NewGameSettings());

    NewGameSettings()
    {
        Players.Add(new Player());
        for (int i = 0; i < 2; i++)
            Players.Add(new Player(1));
    }
}

public class NewMapSettings
{
    public int Width = 80;
    public int Height = 60;
    public float Scale = 1;
    public float NoiseFrequency => 3.8f * Scale;
    public float Smoothness = 1;
    public float FbmGain => 0.32f / Smoothness;
    public float LandMass = 1;
    public float WaterLevel => 0.57f / LandMass;
    public int? Seed = null;
}
