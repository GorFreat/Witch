using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum GameState
{
    GamePlay,
    Paused,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Duration Settings")]
    [Tooltip("Total duration of the game in seconds.")]
    [SerializeField] private float _gameDuration = 300f; // Total game duration in seconds

    [Tooltip("Enable or disable the time limits(if false game is going to be in endless mode")]
    [SerializeField] private bool _hasTimeLimited = true; // Flag to enable/disable time limits

    [Header("Current Game Status")]
    [SerializeField] private GameState _currentGameState = GameState.GamePlay; // Current game state
    [SerializeField] private float _elapsedTime; // Elapsed time in seconds

    public GameState CurrentGameState => _currentGameState;
    public float ElapsedTime => _elapsedTime;
    public float GameDuration => _gameDuration;
    public float RemainingTime => _hasTimeLimited ? Mathf.Max(0, _gameDuration - _elapsedTime) : float.PositiveInfinity;

    // Events for UI and other systems to subscribe to
    public event Action<GameState> OnGameStateChanged;
    public event Action<float> OnTimerTick; // Shoot out each frame along with elapsed time
    public event Action OnVictory; // Event for victory condition
    public event Action OnGameOver; // Event for game over condition

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.timeScale = 1f; // Ensure the game starts unpaused
    }

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDied -= HandlePlayerDied;
    }

    private void Start()
    {
        _elapsedTime = 0f;
        _currentGameState = GameState.GamePlay;
        OnGameStateChanged?.Invoke(_currentGameState);
    }

    private void Update()
    {
        // Only update the timer if the game is in play state
        if(_currentGameState != GameState.GamePlay)
            return;

        _elapsedTime += Time.deltaTime;
        OnTimerTick?.Invoke(_elapsedTime);

        // Check for victory condition if time limits are enabled
        if(_hasTimeLimited && _elapsedTime >= _gameDuration)
        {
            TriggerVictory();
        }
    }

    private void HandlePlayerDied()
    {
        if(_currentGameState == GameState.GamePlay)
        {
            TriggerGameOver();
        }
    }
    #region GAME STATE CONTROLS
    public void SetGameState(GameState newState)
    {
        _currentGameState = newState;
        OnGameStateChanged?.Invoke(_currentGameState);
    }
    public void TriggerVictory()
    {
        // Prevent triggering victory if the game is already over or won
        if (CurrentGameState == GameState.Victory || CurrentGameState == GameState.GameOver)return;

        SetGameState(GameState.Victory);
        Time.timeScale = 0f; // Pause the game
        OnVictory?.Invoke();
        Debug.Log("[GameManager] Victory! You are survived!");
    }
    public void TriggerGameOver()
    {
        // Prevent triggering game over if the game is already over or won
        if (CurrentGameState == GameState.Victory || CurrentGameState == GameState.GameOver)return;

        SetGameState(GameState.GameOver);
        Time.timeScale = 0f; // Pause the game
        OnGameOver?.Invoke();
        Debug.Log("[GameManager] Game Over! You are dead!");
    }

    public void PauseGame()
    {
        if (_currentGameState == GameState.GamePlay)
        {
            SetGameState(GameState.Paused);
            Time.timeScale = 0f; // Pause the game
            Debug.Log("[GameManager] Game Paused!");
        }
    }

    public void ResumeGame()
    {
        if (_currentGameState == GameState.Paused)
        {
            SetGameState(GameState.GamePlay);
            Time.timeScale = 1f; // Resume the game
            Debug.Log("[GameManager] Game Resumed!");
        }
    }
    #endregion

    #region SCENE & RESTART CONTROLS
    public void RestartGame()
    {
        Time.timeScale = 1f; // Ensure the game is unpaused before restarting
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu(string mainMenuSceneName = "MainMenu")
    {
        Time.timeScale = 1f; // Ensure the game is unpaused before loading main menu
        SceneManager.LoadScene(mainMenuSceneName); // Replace with your main menu scene name
    }
    #endregion

    #region HELPER FORMAT METHODS
    // Helper method to format elapsed time in minutes and seconds
    public string GetFormattedElapsedTime()
    {
        int minutes = Mathf.FloorToInt(_elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    // Helper method to format remaining time in minutes and seconds
    public string GetFormattedRemainingTime()
    {
        if (!_hasTimeLimited)
           return GetFormattedElapsedTime();
        float remainingTime = RemainingTime;
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    #endregion
}
