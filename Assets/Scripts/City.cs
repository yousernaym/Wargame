using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City
{
    public Vector2Int Pos { get; private set; }
    public int ProdTime { get; private set; }
    public Player Owner { get; set; }

    public City(Player owner, int x, int y)
    {
        this.Owner = owner;
        Pos = new Vector2Int(x, y);
    }

    UnitType production;
    UnitType Production
    {
        get => production;
        set
        {
            production = value;
            ProdTime = (int)(UnitInfo.Types[value].ProdTime * Owner.ProdFactor);
        }
    }
}
