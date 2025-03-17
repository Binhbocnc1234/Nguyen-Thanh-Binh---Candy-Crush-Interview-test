using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Backpack : Singleton<Backpack>
{
    public bool ShouldAddToBackpack(Cell addedCell)
    {
        if (addedCell.Item == null)
        {
            Debug.LogError("Item is null!");
        }
        else if (IsFull())
        {
            return false;
        }
        foreach (Cell c in cells)
        {
            if (c.Item == null)
            {
                continue;
            }
            if (c.Item.IsSameType(addedCell.Item))
            {
                return true;
            }
        }
        return false;
    }
}