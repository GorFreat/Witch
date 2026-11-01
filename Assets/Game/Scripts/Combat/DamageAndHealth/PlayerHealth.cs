using System;
using UnityEngine;

public class PlayerHealth : Health
{
    public static event Action<float, float> OnPlayerHealthChanged;
    public static event Action OnPlayerDied;

    private void OnEnable()
    {
        OnHealthChanged += ForwardHealthChanged;
        OnDie.AddListener(ForwardPlayerDied);
    }

    private void OnDisable()
    {
        OnHealthChanged -= ForwardHealthChanged;
        OnDie.RemoveListener(ForwardPlayerDied);
    }

    private void ForwardHealthChanged(float current, float max)
    {
        OnPlayerHealthChanged?.Invoke(current, max);
    }

    private void ForwardPlayerDied()
    {
        OnPlayerDied?.Invoke();
    }

}
