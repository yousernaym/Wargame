using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListBox : MonoBehaviour
{
    [SerializeField] GameObject activeItemHighlightObject;
    [SerializeField] GameObject selectedItemHighlightObject;
    HighlightedItem activeItemHighlight;
    HighlightedItem selectedItemHighlight;

    List<ListBoxItem> items = new List<ListBoxItem>();
    public float Width { get; private set; }
    public float Height { get; private set; }
    int selectedItemIndex;
    public int SelectedItemIndex
    {
        get => selectedItemIndex;
        set
        {
            selectedItemIndex = ClampItemIndex(value);
            selectedItemHighlight.SetItem(items[selectedItemIndex]);
        }
    }
    int activeItemIndex;
    public int ActiveItemIndex
    {
        get => activeItemIndex;
        set
        {
            activeItemIndex = ClampItemIndex(value);
            activeItemHighlight.SetItem(items[activeItemIndex]);
        }
    }

    public object ActiveItem => items[ActiveItemIndex];
    public object SelectedItem => items[SelectedItemIndex].Value;
    
    int ClampItemIndex(int index)
    {
        if (index < 0)
            index = 0;
        if (index >= items.Count)
            index = items.Count - 1;
        return index;
    }

    void Start()
    {
        activeItemHighlight = new HighlightedItem(activeItemHighlightObject);
        selectedItemHighlight = new HighlightedItem(selectedItemHighlightObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            ActiveItemIndex--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            ActiveItemIndex++;
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            SelectedItemIndex = ActiveItemIndex;
    }

    public void AddItem(GameObject displayItem, object value)
    {
        var lbItem = new ListBoxItem(displayItem, value);
        lbItem.Pos = new Vector2(0, -Height);
        Height += lbItem.Height;
        if (lbItem.Width > Width)
            Width = lbItem.Width;
        items.Add(lbItem);
    }
}

class ListBoxItem
{
    RectTransform rectTransform;
    public GameObject displayItem;
    public GameObject DisplayItem
    {
        get => displayItem;
        set
        {
            displayItem = value;
            rectTransform = displayItem.GetComponent<RectTransform>();
        }
    }
    public object Value { get; set; }
    public float Width => rectTransform.rect.width;
    public float Height => rectTransform.rect.height;
    public Vector2 Pos
    {
        get => rectTransform.anchoredPosition;
        set => rectTransform.anchoredPosition = value;
    }

    public ListBoxItem(GameObject displayItem, object value)
    {
        DisplayItem = displayItem;
        Value = value;
    }
}

class HighlightedItem
{
    RectTransform rectTransform;
    public HighlightedItem(GameObject gameObject)
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    public void SetItem(ListBoxItem item)
    {
        rectTransform.anchoredPosition = item.Pos;
    }

}
