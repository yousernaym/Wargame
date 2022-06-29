using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProdDialog : Dialog
{
    [SerializeField] GameObject unitRowTemplate;
    [SerializeField] ListBox listBox;
    void Start()
    {
        foreach (UnitType unitType in Enum.GetValues(typeof(UnitType)))
        {
            var unitRow = Instantiate(unitRowTemplate, listBox.transform);
            string unitNameString = Enum.GetName(typeof(UnitType), unitType);
            unitRow.name = unitNameString + " row";
            var unitName = unitRow.transform.Find("UnitName");
            unitName.GetComponent<TMPro.TextMeshProUGUI>().text = unitNameString;
            var prodTime = unitRow.transform.Find("ProdTime");
            prodTime.GetComponent<TMPro.TextMeshProUGUI>().text = UnitInfo.Types[unitType].ProdTime.ToString();

            listBox.AddItem(unitRow);
        }

        var listBoxRt = listBox.GetComponent<RectTransform>();
        var listBoxPosY = listBoxRt.anchoredPosition.y;
        var windowRt = transform.GetComponent<RectTransform>();
        windowRt.sizeDelta = new Vector2(windowRt.sizeDelta.x, -listBoxPosY * 2 + listBox.Height);
    }
}
