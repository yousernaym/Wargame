using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProdDialog : Dialog
{
    [SerializeField] GameObject unitRowTemplate;
    [SerializeField] ListBox_UnitType listBox;
    public UnitType SelectedUnitType
    {
        get => (UnitType)listBox.SelectedItem;
        set => listBox.SelectedItem = value;
    }
    public UnitType ActiveUnitType
    {
        get => (UnitType)listBox.ActiveItem;
        set => listBox.ActiveItem = value;
    }

    City city;

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

            listBox.AddItem(unitRow, unitType);
        }

        var listBoxRt = listBox.GetComponent<RectTransform>();
        var listBoxPosY = listBoxRt.anchoredPosition.y;
        var windowRt = transform.GetComponent<RectTransform>();
        windowRt.sizeDelta = new Vector2(windowRt.sizeDelta.x, -listBoxPosY * 2 + listBox.Height);
    }

    public void Show(City city)
    {
        this.city = city;
        var cityCanvasPos = city.Owner.Map.Renderer.TileToCanvasPos(city.Pos);
        Vector2 dialogPos = new Vector2(ParentSize.x * 3 / 4 - Size.x / 2, ParentSize.y / 2 + Size.y / 2);
        if (cityCanvasPos.x > ParentSize.x / 2)
            dialogPos.x -= ParentSize.x / 2;
        SelectedUnitType = ActiveUnitType = city.Production;
        base.Show(dialogPos);
    }

    override public void Hide()
    {
        OnHide?.Invoke();
        if (ResultOk)
            SelectedUnitType = ActiveUnitType;
        city.Production = SelectedUnitType;
        city.Owner.OnProdDialogClose();
        base.Hide();
    }
}
