using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public partial class Backpack : Singleton<Backpack>
{
    private const int maxCapacity = 5;
    public GameObject cellPrefab;
    public List<Cell> cells = new List<Cell>();
    public event Action OnBackPackFull;
    public void Start()
    {
        Vector3 startPos = new Vector3(-2, -4, 0); // Adjust position for backpack
        for (int i = 0; i < maxCapacity; i++)
        {
            GameObject go = Instantiate(cellPrefab);
            go.transform.position = startPos + new Vector3(i, 0, 0);
            go.transform.SetParent(transform);

            Cell cell = go.GetComponent<Cell>();
            cell.belongtoBackpack = true;
            cell.Setup(i, -1);
            cells.Add(cell);
        }
    }
    public void Reset()
    {
        for (int i = 0; i < maxCapacity; i++)
        {
            Cell cell = cells[i].GetComponent<Cell>();
            cell.Clear();
        }
    }
    private void Update()
    {

    }
    public bool AddToBackpack(Cell cell)
    {
        if (IsFull())
        {
            Debug.Log("Backpack full!");
            OnBackPackFull?.Invoke();
            return false;
        }
        Item item = cell.Item;
        // Find first empty slot in backpack
        Cell emptyCell = cells.FirstOrDefault(c => c.Item == null);
        if (emptyCell == null)
        {
            Debug.Log("No empty cell to add item!");
            OnBackPackFull?.Invoke();
            return false;
        }
        item.SetViewRoot(transform);
        emptyCell.Assign(item);
        cell.Free(); // Remove from board
        item.View.DOMove(emptyCell.transform.position, 0.3f).OnComplete(() => {
            FindMatchesAndCollapse();
        });
        return true;
    }
    private void FindMatchesAndCollapse()
    {
        // Create a dictionary to count item types
        Dictionary<NormalItem.eNormalType, List<Cell>> itemGroups = new Dictionary<NormalItem.eNormalType, List<Cell>>();

        // Iterate through all occupied cells in the backpack
        foreach (var cell in cells)
        {
            if (cell.Item is NormalItem normalItem)
            {
                if (!itemGroups.ContainsKey(normalItem.ItemType))
                {
                    itemGroups[normalItem.ItemType] = new List<Cell>();
                }
                itemGroups[normalItem.ItemType].Add(cell);
            }
        }

        // Check for matches (3 or more of the same type)
        List<Cell> cellsToClear = new List<Cell>();
        foreach (var group in itemGroups.Values)
        {
            if (group.Count >= 3)
            {
                cellsToClear.AddRange(group.Take(3)); // Only remove the first 3 matching items
            }
        }
        // Remove matched items with DOTween
        foreach (var cell in cellsToClear)
        {
            cell.Item.View.DOScale(0, 0.3f).OnComplete(() =>
            {
                Destroy(cell.Item.View.gameObject);
                cell.Free();
            });
        }

        ShiftItemsLeft();
    }

    private void ShiftItemsLeft()
    {
        List<Item> remainingItems = cells.Where(c => c.Item != null)
                                                .Select(c => c.Item)
                                                .ToList();

        // Clear all cells
        foreach (var cell in cells)
        {
            cell.Free();
        }

        // Reassign items in order
        for (int i = 0; i < remainingItems.Count; i++)
        {
            cells[i].Assign(remainingItems[i]);
            remainingItems[i].SetViewPosition(cells[i].transform.position);
        }
    }

    public bool IsEmpty()
    {
        return cells.Count == 0;
    }
    public bool IsFull()
    {
        return cells.Count(c => c.Item != null) >= maxCapacity;
    }
}
