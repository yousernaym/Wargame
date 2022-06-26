using System;
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
    public float NoiseAmplitude => 0.5f;
    public int IslandFrequency = 50;
    public float NoiseFrequency => InterpolateSetting(0.5f, 3.8f, 9, IslandFrequency);
    public int Smoothness = 50;
    public float FbmGain => InterpolateSetting(0.9f, 0.5f, 0f, Smoothness);
    public int LandMass = 50;
    public float WaterLevel => InterpolateSetting(0.7f, 0.57f, 0.5f, LandMass);
    public int? Seed = null;

    //Interpolate so that 0% = zero, 50% = fifty, 100% = hundred
    float InterpolateSetting(float zero, float fifty, float hundred, int percent)
    {
        if (percent < 50)
            return Lerp(zero, fifty, percent * 2);
        else
            return Lerp(fifty, hundred, percent * 2 - 100);
    }

    float Lerp(float v1, float v2, int percent)
    {
        float f = percent / 100f;
        var ret = (1 - f) * v1 + f * v2;
        return ret;
    }
}
