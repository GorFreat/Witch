using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panel Root")]
    [Tooltip("The root GameObject of the pause panel containing all pause menu UI elements.")]
    [SerializeField] private GameObject _pausePanel;

    [Header("Buttons")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;

    private void Awake()
    {
        if(_pausePanel != null)
            _pausePanel.SetActive(false); // Ensure the pause panel is hidden at the start
    }

    private void OnEnable()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(Resume);

        if (_retryButton != null)
            _retryButton.onClick.AddListener(OnRetryClicked);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }
    private void OnDisable()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(Resume);

        if (_retryButton != null)
            _retryButton.onClick.RemoveListener(OnRetryClicked);

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
    private void TogglePause()
    {
        if (GameManager.Instance == null) return;
        // If Game state is not GamePlay, toogle pause is not allowed
        if (GameManager.Instance.CurrentGameState == GameState.GameOver || GameManager.Instance.CurrentGameState == GameState.Victory)
        {
            return;
        }

        if (GameManager.Instance.CurrentGameState == GameState.GamePlay)
        {
            Pause();
        }
        else if (GameManager.Instance.CurrentGameState == GameState.Paused)
        {
            Resume();
        }
    }

    private void Pause()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        if(_pausePanel != null)
        {
            _pausePanel.SetActive(true);
        }
    }

    private void Resume()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        if(_pausePanel != null)
        {
            _pausePanel.SetActive(false);
        }
    }

    private void OnRetryClicked()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnMainMenuClicked()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
    }
}
