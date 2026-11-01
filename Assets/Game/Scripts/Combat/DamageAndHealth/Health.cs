using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [field: SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected float _currentHealth;

    public UnityEvent OnHurt;
    public UnityEvent OnDie;
    public event Action<float, float> OnHealthChanged;

    public Vector2 knockBackVelocity { get; private set; }

    protected bool isDead = false;

    void Start()
    {
        _currentHealth = _maxHealth;
        //Shoot out event when the game start for UI to update _maxHealth
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public virtual void TakeDamage(float damage, Vector2 knockBackDir, float knockBackForce)
    {
        if (isDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        knockBackVelocity = knockBackDir.normalized * knockBackForce;

        OnHurt?.Invoke();
        //Shoot out event to anouce that health is changed
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        Debug.Log("Taking damage: " + damage);
        if(_currentHealth <= 0)
        {
            Die();
        }
    }

    //Heal when player got portion or healing spell
    public virtual void Heal(float amounnt)
    {
        _currentHealth += amounnt;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        if (isDead) return;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    protected void Die()
    {
        if(isDead) return;
        isDead = true;
        OnDie?.Invoke();
    }

    public void SetMaxHealth(float maxHealth)
    {
        isDead = false;
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public float GetMaxHealth() => _maxHealth;
    public float GetCurrentHealth() => _currentHealth;
}
