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
    int selectedItem;
    public int SelectedItem
    {
        get => selectedItem;
        set
        {
            selectedItem = ClampItemIndex(value);
            selectedItemHighlight.SetItem(items[selectedItem]);
        }
    }
    int activeItem;
    public int ActiveItem
    {
        get => activeItem;
        set
        {
            activeItem = ClampItemIndex(value);
            activeItemHighlight.SetItem(items[activeItem]);
        }
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
        activeItemHighlight = new HighlightedItem(activeItemHighlightObject);
        selectedItemHighlight = new HighlightedItem(selectedItemHighlightObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            ActiveItem--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            ActiveItem++;
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            SelectedItem = ActiveItem;
    }

    public void AddItem(GameObject item)
    {
        var lbItem = new ListBoxItem(item);
        lbItem.Pos = new Vector2(0, -Height);
        Height += lbItem.Height;
        if (lbItem.Width > Width)
            Width = lbItem.Width;
        items.Add(lbItem);
    }
}

class ListBoxItem
{
    GameObject gameObject;
    RectTransform rectTransform;
    public float Width => rectTransform.rect.width;
    public float Height => rectTransform.rect.height;
    public Vector2 Pos
    {
        get => rectTransform.anchoredPosition;
        set => rectTransform.anchoredPosition = value;
    }

    public ListBoxItem(GameObject gameObject)
    {
        this.gameObject = gameObject;
        rectTransform = gameObject.GetComponent<RectTransform>();
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
