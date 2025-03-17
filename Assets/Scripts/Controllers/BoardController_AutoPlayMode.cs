
using System.Collections;
using UnityEngine;

public partial class BoardController : Singleton<BoardController>{
    private bool isAutoPlaying = false;
    public float autoPlayDelay = 0.75f;
    private void ProcessPlayerClick()
    {
        // Debug.Log("Player clicked");
        var hits = Physics2D.RaycastAll(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null && cell.Item != null && cell.isInteractable)
                {
                    backpack.AddToBackpack(cell);
                    return;
                }
            }
        }
        
    }

    private IEnumerator AutoPlayRoutine()
    {
        while (m_gameManager.State == GameManager.eStateGame.GAME_STARTED)
        {
            yield return new WaitUntil(() => !IsBusy);
            yield return new WaitForSeconds(autoPlayDelay); // Delay each action

            if (m_gameManager.autoPlayMode == GameManager.eAutoPlay.WIN)
            {
                ClickValidItem();
            }
            else if (m_gameManager.autoPlayMode == GameManager.eAutoPlay.LOSE)
            {
                ClickInvalidItem();
            }
        }
        isAutoPlaying = false;
    }

    private void ClickValidItem()
    {
        Debug.Log("Click valid item");
        foreach (Cell cell in upperBoard.cellList)
        {
            if (cell != null && cell.Item != null && (backpack.IsEmpty() || backpack.ShouldAddToBackpack(cell)))
            {
                backpack.AddToBackpack(cell);
                return;
            }
        }
        foreach (Cell cell in upperBoard.cellList)
        {
            if (cell != null && cell.Item != null)
            {
                backpack.AddToBackpack(cell);
                return;
            }
        }
        
    }

    private void ClickInvalidItem()
    {
        Debug.Log("Click invalid item");
        foreach (Cell cell in upperBoard.cellList)
        {
            if (cell != null && cell.Item != null && (backpack.IsEmpty() || !backpack.ShouldAddToBackpack(cell)))
            {
                backpack.AddToBackpack(cell);
                return;
            }
        }
        foreach (Cell cell in upperBoard.cellList)
        {
            if (cell != null && cell.Item != null)
            {
                backpack.AddToBackpack(cell);
                return;
            }
        }
    }
}
