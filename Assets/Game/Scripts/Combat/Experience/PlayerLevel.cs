using System;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [field: SerializeField] public int CurrentLevel { get; private set; } = 1;
    [field:SerializeField] public float CurrentExp { get; private set; } = 0f;
    [field: SerializeField] public float ExpToNextLevel { get; private set; } = 50f;

    //Event send to update UI
    public event Action<float, float> OnExpChange; // <CurrentExp, ExpToNextLevel>
    public event Action<int> OnLevelUp; // <new Level>
    private void Start()
    {
        OnExpChange?.Invoke(CurrentExp, ExpToNextLevel);
    }

    public void AddExp(float amount)
    {
        CurrentExp += amount;

        while (CurrentExp >= ExpToNextLevel)
        {
            CurrentExp -= ExpToNextLevel;
            LevelUp();
        }
        OnExpChange?.Invoke(CurrentExp, ExpToNextLevel);
    }

    private void LevelUp()
    {
        CurrentLevel++;

        ExpToNextLevel = Mathf.Round(ExpToNextLevel * 1.2f + 5f);
        Debug.Log($"[LevelUp] Player reached level {CurrentLevel}! Exp need for next level: {ExpToNextLevel}");

        // Invoke level up event to open choosing skill UI
        OnLevelUp?.Invoke(CurrentLevel);
    }
}
