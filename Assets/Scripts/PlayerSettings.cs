using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettings
{
    public string Name;
    public int AiLevel = 0; //0 = Human
    public float CombatFactor = 0.5f;
    public float ProdFactor = 0.5f;
    public PlayerSettings(int aILevel = 0)
    {
        AiLevel = aILevel;
    }
}
