using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class City
{
    public Vector2Int Pos { get; private set; }
    public int ProdTime { get; set; }
    public Player Owner { get; set; }
    [SerializeField] GameObject prodDialog;

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

    public void ShowProdDialog()
    {
        prodDialog.SetActive(true);
    }

    public bool IsProdDone()
    {
        if (ProdTime < 0)
            throw new ArgumentOutOfRangeException(nameof(ProdTime));
        return ProdTime == 0;
    }
}
