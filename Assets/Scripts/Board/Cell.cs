using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Cells are elements of the Board. They refer to Items. Their appearance is a light gray square.
/// </summary>
public class Cell : MonoBehaviour
{
    public event Action OnItemNull;
    public bool belongtoBackpack = false;
    public int BoardX { get; private set; }

    public int BoardY { get; private set; }
    [HideInInspector] public int countOverlapped{ get; private set; }
    public Item Item { get; private set; }

    public Cell NeighbourUp { get; set; }

    public Cell NeighbourRight { get; set; }

    public Cell NeighbourBottom { get; set; }

    public Cell NeighbourLeft { get; set; }
    // [HideInInspector] public List<Cell> belowCell;
    public bool isInteractable = true;
    public bool IsEmpty => Item == null;
    public SpriteRenderer spriteRen;
    void Start() {
        spriteRen = GetComponent<SpriteRenderer>();
    }

    public void Setup(int cellX, int cellY)
    {
        this.BoardX = cellX;
        this.BoardY = cellY;
    }

    public bool IsNeighbour(Cell other)
    {
        return BoardX == other.BoardX && Mathf.Abs(BoardY - other.BoardY) == 1 ||
            BoardY == other.BoardY && Mathf.Abs(BoardX - other.BoardX) == 1;
    }

    /// <summary>
    /// Remove Item and the object Item still exists in memory
    /// </summary>
    public void Free()
    {
        OnItemNull?.Invoke();
        Item = null;
        if (belongtoBackpack == false)
        {
            spriteRen.enabled = false;
        }
    }

    public void Assign(Item item)
    {
        Item = item;
        Item.SetCell(this);
    }

    
    /// <summary>
    /// Remove Item and destroy its View-GameObject
    /// </summary>
    internal void Clear()
    {
        if (Item != null)
        {
            Item.Clear();
            Item = null;
        }
    }
    internal bool IsSameType(Cell other)
    {
        return Item != null && other.Item != null && Item.IsSameType(other.Item);
    }

    internal void ExplodeItem()
    {
        if (Item == null) return;

        Item.ExplodeView();
        Item = null;
    }
    public void ApplyItemPosition(bool withAppearAnimation)
    {
        Item.SetViewPosition(this.transform.position);

        if (withAppearAnimation)
        {
            Item.ShowAppearAnimation();
        }
    }
    

    internal void AnimateItemForHint()
    {
        Item.AnimateForHint();
    }

    internal void StopHintAnimation()
    {
        Item.StopAnimateForHint();
    }

    internal void ApplyItemMoveToPosition()
    {
        Item.AnimationMoveToPosition();
    }
    internal void ToggleInteractable(bool isInteractable)
    {
        this.isInteractable = isInteractable;
        Color targetColor = isInteractable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f); // Darkened effect
        spriteRen.DOColor(targetColor, 0.3f);
        SpriteRenderer itemRenderer = Item.View.GetComponent<SpriteRenderer>();
        itemRenderer.DOColor(targetColor, 0.3f);
    }
    public void AddOverlapped(int offset = 1) {
        countOverlapped += offset;
        if (countOverlapped == 0)
        {
            ToggleInteractable(true);
        }
    }
    public void MinusOverlapped()
    {
        AddOverlapped(-1);
    }
    internal string GetString()
    {
        return $"Cell {BoardX}, {BoardY}, CountOverlapped: {countOverlapped}";
    }
}
