using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public partial class BoardController : Singleton<BoardController>
{
    public event Action OnMoveEvent = delegate { };
    public bool IsBusy { get; private set; }
    public List<Transform> boardContainer;
    private Board upperBoard, lowerBoard;
    private Backpack backpack;
    private GameManager m_gameManager;
    private bool m_isDragging;
    private Camera m_cam;
    private Collider2D m_hitCollider;
    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch = new List<Cell>();
    private float m_timeAfterFill;
    private bool m_hintIsShown;
    private bool m_gameOver;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;
        m_gameSettings = gameSettings;
        m_gameManager.StateChangedAction += OnGameStateChange;
        m_cam = Camera.main;
        upperBoard = new Board(boardContainer[0], gameSettings.BoardSizeX, gameSettings.BoardSizeY, Vector2.zero, 0, Board.eLayer.Highest);
        lowerBoard = new Board(boardContainer[1], gameSettings.BoardSizeX + 1, gameSettings.BoardSizeY + 1, Vector2.zero, -1, Board.eLayer.Lowest);
        
        backpack = Backpack.Instance;
        backpack.Reset();
        if (m_gameManager.autoPlayMode != GameManager.eAutoPlay.NONE)
        {
            Debug.Log("Enter Auto play mode");
            isAutoPlaying = true;
            StartCoroutine(AutoPlayRoutine());
        }
        FillBoardWithItem();
        ToggleInteractable();
    }

    private void FillBoardWithItem()
    {
        upperBoard.FillWithRandomItem();
        lowerBoard.FillWithRandomItem();
    }
    private void ToggleInteractable()
    {
        foreach (Cell c in lowerBoard.cellList)
        {
            c.ToggleInteractable(false);

        }
        foreach (Cell c in upperBoard.cellList)
        {
            c.ToggleInteractable(true);
            Vector3 cellPos = c.transform.position;
            List<Vector3> lst = new List<Vector3>() { new Vector3(0.5f, 0.5f), new Vector3(0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f), new Vector3(-0.5f, -0.5f) };
            foreach (Vector3 offset in lst)
            {
                Cell lowerCell = lowerBoard.GetCellAtPosition(cellPos +offset);
                lowerCell.AddOverlapped();
                c.OnItemNull += lowerCell.MinusOverlapped;
            }
        }
        foreach (Cell c in lowerBoard.cellList)
        {
            Debug.Log(c.GetString());

        }
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.LOSE:
                m_gameOver = true;
                StopHints();
                break;
        }
    }

    public void Update()
    {
        if (GameManager.Instance.State == GameManager.eStateGame.GAME_STARTED && !IsBusy)
        {
            //Non-autoplay mode
            if (!isAutoPlaying && Input.GetMouseButtonDown(0))
            {
                ProcessPlayerClick();
            }
            // Check win condition
            if (upperBoard.IsEmpty() && lowerBoard.IsEmpty())
            {
                m_gameManager.Win();
            }
        }
        
    }
    private List<Cell> GetMatches(Cell cell)
    {
        return new List<Cell>();
    }
    private void CollapseMatches(List<Cell> matches, Cell cellEnd)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }

        if (matches.Count > m_gameSettings.MatchesMin)
        {
            upperBoard.ConvertNormalToBonus(matches, cellEnd);
        }
    }


    private IEnumerator RefillBoardCoroutine()
    {
        upperBoard.ExplodeAllItems();

        yield return new WaitForSeconds(0.2f);

        upperBoard.FillWithRandomItem();

        yield return new WaitForSeconds(0.2f);
    }



    private void SetSortingLayer(Cell cell1, Cell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    internal void Clear()
    {
        upperBoard.Clear();
        lowerBoard.Clear();
    }

    private void ShowHint()
    {
        m_hintIsShown = true;
        foreach (var cell in m_potentialMatch)
        {
            cell.AnimateItemForHint();
        }
    }

    private void StopHints()
    {
        m_hintIsShown = false;
        foreach (var cell in m_potentialMatch)
        {
            cell.StopHintAnimation();
        }

        m_potentialMatch.Clear();
    }
}
