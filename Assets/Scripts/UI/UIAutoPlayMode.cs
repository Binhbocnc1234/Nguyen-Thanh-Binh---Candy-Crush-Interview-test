using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAutoPlayMode : MonoBehaviour
{
    public Button autoPlayButton;
    public TMP_Text buttonText;
    private GameManager gameManager;

    private GameManager.eAutoPlay[] states = { GameManager.eAutoPlay.WIN, GameManager.eAutoPlay.LOSE, GameManager.eAutoPlay.NONE };
    private int currentStateIndex = 2;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (autoPlayButton != null)
        {
            autoPlayButton.onClick.AddListener(ToggleAutoPlayMode);
        }

        UpdateButtonText();
    }

    public void ToggleAutoPlayMode()
    {
        // Cycle through WIN → LOSE → NONE
        currentStateIndex = (currentStateIndex + 1) % states.Length;
        gameManager.autoPlayMode = states[currentStateIndex];

        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = $"Auto Play: {states[currentStateIndex]}";
        }
    }
}
