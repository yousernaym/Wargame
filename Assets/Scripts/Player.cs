using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public int AiLevel = 0; //0 = Human
    public float CombatFactor = 0.5f;
    public float ProductionFactor = 0.5f;
    public Player(int aILevel = 0)
    {
        AiLevel = aILevel;
    }
}
