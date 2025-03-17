using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the overall state and flow of the game. 
/// Handles game states, level loading, and game over conditions.
/// 
/// <para>Some important methods:</para>
/// - <c>SetState(eStateGame state)</c>: Sets the game state and pauses or plays all DOTween animations based on the state.
/// <br/>
/// - <c>LoadLevel(eLevelMode mode)</c>: Loads a new level based on the specified mode (TIMER or MOVES), sets up the board controller, and attaches the game over event.
/// <br/>
/// - <c>GameOver()</c>: Starts a coroutine to wait for the board controller to finish its tasks and then sets the game state to GAME_OVER.
/// <br/>
/// - <c>ClearLevel()</c>: Clears the current level by destroying the board controller and setting it to null.
/// <br/>
/// - <c>WaitBoardController()</c>: Coroutine that waits for the board controller to finish its tasks, then sets the game state to GAME_OVER and cleans up the level condition.
/// 
/// <para>Dependencies:</para>
/// <list type="bullet">
///     <item>BoardController</item>
///     <item>UIMainManager</item>
///     <item>LevelCondition</item>
///     <item>GameSettings</item>
///     <item>DOTween</item>
/// </list>
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        NORMAL
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        LOSE,
        WIN,
    }
    public enum eAutoPlay
    {
        LOSE,
        WIN,
        NONE
    }
    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }

    public eAutoPlay autoPlayMode = eAutoPlay.NONE;
    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    protected override void Awake(){
        base.Awake();
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {

    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if (State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        State = eStateGame.GAME_STARTED;
        m_boardController = BoardController.Instance;
        m_boardController.StartGame(this, m_gameSettings);
        Backpack.Instance.OnBackPackFull += GameOver;
        if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), this);
            m_levelCondition.ConditionCompleteEvent += GameOver;
        }
    }
    public void Win()
    {
        StartCoroutine(WaitBoardController(eStateGame.WIN));
    }
    public void GameOver()
    {
        StartCoroutine(WaitBoardController(eStateGame.LOSE));
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
        }
    }

    private IEnumerator WaitBoardController(eStateGame state)
    {
        State = state;
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        
        if (m_levelCondition != null)
        {
            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }
}
