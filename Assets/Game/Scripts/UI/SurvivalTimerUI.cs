using UnityEngine;
using TMPro;

public class SurvivalTimerUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("The TextMeshProUGUI component that displays the survived time.")]
    [SerializeField] private TextMeshProUGUI _survivedTimeText;

    [Header("Display Mode")]
    [SerializeField] private bool _countUp = true; // If true, counts up; if false, counts down.

    private void Awake()
    {
       if(_survivedTimeText == null)
       {
           _survivedTimeText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerTick += UpdateTimerText;
        }
    }

    private void OnDisable()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerTick -= UpdateTimerText;
        }
    }

    private void Start()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerTick -= UpdateTimerText;
            GameManager.Instance.OnTimerTick += UpdateTimerText;

            UpdateTimerText(GameManager.Instance.ElapsedTime); // Initialize the timer text at start
        }
    }

    private void UpdateTimerText(float currentElapsedTime)
    {
        if(_survivedTimeText != null && GameManager.Instance != null)
        {
            _survivedTimeText.text = _countUp ? GameManager.Instance.GetFormattedElapsedTime() : GameManager.Instance.GetFormattedRemainingTime();
        }
    }
}
