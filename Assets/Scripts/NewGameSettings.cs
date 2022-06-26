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
    public RangeSetting Width = new RangeSetting(80);
    public RangeSetting Height = new RangeSetting(60);
    public RangeSetting CityCount = new RangeSetting(55, 75);
    public float NoiseAmplitude => 0.5f;

    public RangeSetting IslandFrequency = new RangeSetting(40, 60);
    public float NoiseFrequency => InterpolateSetting(0.5f, 3.8f, 9, IslandFrequency.Value);

    public RangeSetting Smoothness = new RangeSetting(40, 60);
    public float FbmGain => InterpolateSetting(0.9f, 0.5f, 0f, Smoothness.Value);

    public RangeSetting LandMass = new RangeSetting(40, 60);
    public float WaterLevel => InterpolateSetting(0.7f, 0.57f, 0.5f, LandMass.Value);

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

public class RangeSetting
{
    public int value;
    public int Value
    {
        get => value;
        set => this.value = low = high = value;
    }

    int low;
    public int Low
    {
        get => low;
        set
        {
            low = value;
            RandomizeValue();
        }
    }
    int high;
    public int High
    {
        get => high;
        set
        {
            high = value;
            RandomizeValue();
        }
    }

    public RangeSetting(int value)
    {
        Value = Low = High = value;
    }

    public RangeSetting(int low, int high)
    {
        Low = low;
        High = high;
        RandomizeValue();
    }

    public void RandomizeValue()
    {
        Value = UnityEngine.Random.Range(Low, High + 1);
    }
}
