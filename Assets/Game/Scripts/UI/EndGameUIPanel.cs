using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameUIPanel : MonoBehaviour
{
    [Header("Panel Root")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Text UI Elements")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _levelReachedText;
    [SerializeField] private TextMeshProUGUI _survivedTimeText;

    [Header("Button UI Elements")]
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("Player's Endgame Information")]
    [SerializeField] private PlayerLevel _playerLevel;

    [Header("Custom Title & Color")]
    [SerializeField] private string _gameOverTitle = "GAME OVER";
    [SerializeField] private Color _gameOverColor = new Color(0.9f, 0.2f, 0.2f); // Red
    [SerializeField] private string _victoryTitle = "VICTORY!";
    [SerializeField] private Color _victoryColor = new Color(1.0f, 0.85f, 0.2f); // Golden

    private void Awake()
    {
    }

    private void OnEnable()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnVictory += ShowVictory;
        }

        if(_retryButton != null)
        {
            _retryButton.onClick.AddListener(OnRetryClicked);
        }
        if(_mainMenuButton != null)
        {
            _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnVictory -= ShowVictory;
        }

        if (_retryButton != null)
        {
            _retryButton.onClick.RemoveListener(OnRetryClicked);
        }
        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }
    }

    private void Start()
    {
        // To ensure that ShowGameOver and ShowVictory is subscribed if OnEnable was called before GameManager.Instance was initialized
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnVictory -= ShowVictory;

            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnVictory += ShowVictory;
            Debug.Log("EndGameUIPanel: Events have been Subscribed!");
        }
        _panelRoot.SetActive(false); // Ensure the panel is hidden at the start
    }

    private void SetUpPanel(string title, Color color) 
    {
        if (_panelRoot == null) return;

        if(_titleText != null)
        {
            _titleText.text = title;
            _titleText.color = color;
        }

        if (_survivedTimeText != null)
        {
            string survivedTime = GameManager.Instance.GetFormattedElapsedTime();
            _survivedTimeText.text = $"Survived Time: {survivedTime}";
        }
        if( _levelReachedText != null)
        {
            if (_playerLevel != null)
            {
                int levelReached = _playerLevel.CurrentLevel;
                _levelReachedText.text = $"Level Reached: {levelReached}";
            }
        }
        _panelRoot.SetActive(true);
    }

    private void ShowGameOver()
    {
        SetUpPanel(_gameOverTitle, _gameOverColor);
    }

    private void ShowVictory()
    {
        SetUpPanel(_victoryTitle, _victoryColor);
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

