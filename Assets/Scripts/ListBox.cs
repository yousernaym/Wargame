using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListBox<T> : MonoBehaviour
{
    [SerializeField] GameObject activeItemHighlightObject;
    [SerializeField] GameObject selectedItemHighlightObject;
    HighlightedItem<T> activeItemHighlight;
    HighlightedItem<T> selectedItemHighlight;

    List<ListBoxItem<T>> items = new List<ListBoxItem<T>>();
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

    public T ActiveItem
    {
        get => items[ActiveItemIndex].Value;
        set => ActiveItemIndex = items.FindIndex(item => item.Value.Equals(value));
    }

    public T SelectedItem
    {
        get => items[SelectedItemIndex].Value;
        set => SelectedItemIndex = items.FindIndex(item => item.Value.Equals(value));
    }    
    
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
        activeItemHighlight = new HighlightedItem<T>(activeItemHighlightObject);
        selectedItemHighlight = new HighlightedItem<T>(selectedItemHighlightObject);
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

    public void AddItem(GameObject displayItem, T value)
    {
        var lbItem = new ListBoxItem<T>(displayItem, value);
        lbItem.Pos = new Vector2(0, -Height);
        Height += lbItem.Height;
        if (lbItem.Width > Width)
            Width = lbItem.Width;
        items.Add(lbItem);
    }
}

class ListBoxItem<T>
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
    public T Value { get; set; }
    public float Width => rectTransform.rect.width;
    public float Height => rectTransform.rect.height;
    public Vector2 Pos
    {
        get => rectTransform.anchoredPosition;
        set => rectTransform.anchoredPosition = value;
    }

    public ListBoxItem(GameObject displayItem, T value)
    {
        DisplayItem = displayItem;
        Value = value;
    }
}

class HighlightedItem<T>
{
    RectTransform rectTransform;
    public HighlightedItem(GameObject gameObject)
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    public void SetItem(ListBoxItem<T> item)
    {
        rectTransform.anchoredPosition = item.Pos;
    }

}
