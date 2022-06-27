using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings
{
    public static int PlayerCount = 3;
    public string Name;
    public int PlayerNumber { get; private set; }
    public int AiLevel = 0; //0 = Human
    public float CombatFactor = 0.5f;
    public float ProdFactor = 0.5f;
    public PlayerSettings(int aILevel)
    {
        AiLevel = aILevel;
        PlayerNumber = PlayerCount++;
    }

    protected PlayerSettings(PlayerSettings copy)
    {
        AiLevel = copy.AiLevel;
        PlayerNumber = copy.PlayerNumber;
        CombatFactor = copy.CombatFactor;
        ProdFactor = copy.ProdFactor;
    }
}
