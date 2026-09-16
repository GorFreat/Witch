using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private HealthUI _healthUI;

    private void Awake()
    {
        _healthUI.SetMaxHeart((int)_maxHealth);
    }

    public override void TakeDamage(float damage, Vector2 knockBackDir, float knockBackForce)
    {
        base.TakeDamage(damage, knockBackDir, knockBackForce);
        _healthUI.UpdateHeart((int)_currentHealth);
    }
}
