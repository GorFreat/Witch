using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [field: SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected float _currentHealth;

    public UnityEvent OnHurt;
    public UnityEvent OnDie;

    public Vector2 knockBackVelocity { get; private set; }

    protected bool isDead = false;

    void Start()
    {
        _currentHealth = _maxHealth;
    }

    public virtual void TakeDamage(float damage, Vector2 knockBackDir, float knockBackForce)
    {
        if (isDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        knockBackVelocity = knockBackDir.normalized * knockBackForce;

        OnHurt?.Invoke();
        Debug.Log("Taking damage: " + damage);
        if(_currentHealth <= 0)
        {
            Die();
        }
    }

    protected void Die()
    {
        if(isDead) return;
        isDead = true;
        OnDie?.Invoke();
    }


    public float GetMaxHealth() => _maxHealth;
    public float GetCurrentHealth() => _currentHealth;
}
