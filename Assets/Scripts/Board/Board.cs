using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the game board for a Candy Crush-like game. 
/// Manages the creation, manipulation, and interaction of cells and items on the board.
/// 
/// <para>Some important methods: </para>
/// <para>- <c>Swap(Cell c1, Cell c2)</c>: DISABLED, swap Cell refs and Item position</para>
/// <para>- <c>Shuffle()</c>: DISABLED, Swaps the positions of items with each other, without changing the number of items.</para>
/// <para>- <c>FillWithRandomItem()</c>: Fills the board with <c>NormalItem</c> objects, ensuring no immediate matches by checking neighboring cells.</para>
/// <para>- <c>FillGapsWithNewItems()</c>: Fills empty cells with new <c>NormalItem</c> objects.</para>
/// <para>- <c>ExplodeAllItems()</c>: Triggers the explosion of all items on the board.</para>
/// <para>- <c>GetHorizontalMatches(Cell cell)</c>: Finds and returns a list of horizontally matching cells starting from the given cell.</para>
/// <para>- <c>GetVerticalMatches(Cell cell)</c>: Finds and returns a list of vertically matching cells starting from the given cell.</para>
/// <para>- <c>ConvertNormalToBonus(List&lt;Cell&gt; matches, Cell cellToConvert)</c>: Converts a normal item to a bonus item based on the match direction.</para>
/// <para>- <c>GetMatchDirection(List&lt;Cell&gt; matches)</c>: Determines the match direction (horizontal, vertical, or all) based on the given matches.</para>
/// <para>- <c>FindFirstMatch()</c>: Finds and returns the first match on the board.</para>
/// <para>- <c>CheckBonusIfCompatible(List&lt;Cell&gt; matches)</c>: Checks if the matches are compatible with bonus items and returns the compatible matches.</para>
/// <para>- <c>GetPotentialMatches()</c>: Finds and returns potential matches on the board.</para>
/// <para>- <c>ShiftDownItems()</c>: Shifts items down to fill empty cells below them.</para>
/// <para>- <c>Clear()</c>: Clears the board by destroying all cell objects.</para>
/// <para>Dependencies:</para>
/// <list type="bullet">
///     <item>Cell</item>
///     <item>NormalItem</item>
///     <item>BonusItem</item>
///     <item>GameSettings</item>
///     <item>Utils</item>
/// </list>
/// </summary>
public class Board
{
    public enum eMatchDirection
    {
        NONE,
        HORIZONTAL,
        VERTICAL,
        ALL
    }
    public enum eLayer
    {
        Lowest,
        Highest,
        Middle
    }

    private int boardSizeX;
    private int boardSizeY;
    private int layer;
    private eLayer layerType;
    public Vector3 offset { get; private set; }
    private Cell[,] m_cells;
    public List<Cell> cellList { get; private set; }
    private Transform m_root;
    private Backpack backpack;

    public Board(Transform parent, int sizeX, int sizeY, Vector2 offset, int layer, eLayer layerType)
    {
        m_root = parent;
        this.boardSizeX = sizeX;
        this.boardSizeY = sizeY;
        this.offset = offset;
        this.layer = layer;
        this.layerType = layerType;
        m_cells = new Cell[boardSizeX, boardSizeY];
        // SortingGroup sortingGroup = m_root.gameObject.AddComponent<SortingGroup>();
        // sortingGroup.sortingOrder = layer;
        this.backpack = Backpack.Instance;
        SetUpCell();
        cellList = GetCellList();
    }

    private void SetUpCell()
    {
        // if offset is Vector3.zero, board will lie in center of camera
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f + 1f, 0f);

        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y);

                m_cells[x, y] = cell;
            }
        }

        // Set neighbours
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (y + 1 < boardSizeY) m_cells[x, y].NeighbourUp = m_cells[x, y + 1];
                if (x + 1 < boardSizeX) m_cells[x, y].NeighbourRight = m_cells[x + 1, y];
                if (y > 0) m_cells[x, y].NeighbourBottom = m_cells[x, y - 1];
                if (x > 0) m_cells[x, y].NeighbourLeft = m_cells[x - 1, y];
            }
        }
    }

    internal void FillWithRandomItem()
    {
        List<NormalItem.eNormalType> typeList = Enum.GetValues(typeof(NormalItem.eNormalType)).Cast<NormalItem.eNormalType>().ToList();
        int totalCells = boardSizeX * boardSizeY; //It's guaranteed that totalCells is divisible by 3
        int typesCount = Enum.GetValues(typeof(NormalItem.eNormalType)).Length;
        int triples = totalCells / 3; // Each type must appear in multiples of 3
        List<NormalItem.eNormalType> itemPool = new List<NormalItem.eNormalType>();

        for (int i = 0; i < triples; i++)
        {
            NormalItem.eNormalType type = typeList[UnityEngine.Random.Range(0, typeList.Count)];
            for (int j = 0; j < 3; j++)
            {
                itemPool.Add(type);
            }
        }

        // Shuffle the item pool to randomize placement
        itemPool = itemPool.OrderBy(x => UnityEngine.Random.value).ToList();

        int index = 0;

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                NormalItem item = new NormalItem();
                item.SetType(itemPool[index++]); // Assign shuffled type
                item.SetView();
                item.SetViewRoot(m_root);
                cell.Assign(item);
                cell.ApplyItemPosition(false);
            }
        }
    }

    public bool IsEmpty()
    {
        bool isBoardEmpty = true;
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (m_cells[x, y].Item != null)
                {
                    isBoardEmpty = false;
                    break;
                }
            }
        }
        return isBoardEmpty;
    }

    internal void Shuffle()
    {
        List<Item> list = new List<Item>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                list.Add(m_cells[x, y].Item);
                m_cells[x, y].Free();
            }
        }
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                int rnd = UnityEngine.Random.Range(0, list.Count);
                m_cells[x, y].Assign(list[rnd]);
                m_cells[x, y].ApplyItemMoveToPosition();
                list.RemoveAt(rnd);
            }
        }
    }

    internal void FillGapsWithNewItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (!cell.IsEmpty) continue;
                NormalItem item = new NormalItem();
                item.SetType(Utils.GetRandomNormalType());
                item.SetView();
                item.SetViewRoot(m_root);
                cell.Assign(item);
                cell.ApplyItemPosition(true);
            }
        }
    }

    internal void ExplodeAllItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.ExplodeItem();
            }
        }
    }

    public void ConvertNormalToBonus(List<Cell> matches, Cell cellToConvert)
    {
        // Implementation for converting normal item to bonus item
    }

    public eMatchDirection GetMatchDirection(List<Cell> matches)
    {
        // Implementation for determining match direction
        return eMatchDirection.NONE;
    }

    public List<Cell> CheckBonusIfCompatible(List<Cell> matches)
    {
        // Implementation for checking if matches are compatible with bonus items
        return new List<Cell>();
    }

    public List<Cell> GetPotentialMatches()
    {
        // Implementation for finding potential matches
        return new List<Cell>();
    }

    public void ShiftDownItems()
    {
        // Implementation for shifting items down to fill empty cells
    }

    public void Clear()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.Clear();
                GameObject.Destroy(cell.gameObject);
                m_cells[x, y] = null;
            }
        }
    }
    public Cell GetCellAtPosition(Vector3 pos)
    {
        foreach (Cell c in cellList)
        {
            if (c.transform.position == pos)
            {
                return c;
            }
        }
        Debug.LogError("Not return a valid cell");
        return null;
    }
    private List<Cell> GetCellList()
    {
        List<Cell> cells = new List<Cell>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                cells.Add(m_cells[x, y]);
            }
        }
        return cells;
    }
    
}
