using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private float _damage;
    [SerializeField] private float _knockBackForce;
    [SerializeField] private LayerMask _targetLayer;

    //Event shoot out when hitting the target
    public event System.Action<IDamageable> OnDamageDealt;

    public void SetDamageStat(float newDamage, float newKnockBackForce)
    {
        _damage = newDamage;
        _knockBackForce = newKnockBackForce;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((_targetLayer.value & (1 << collision.gameObject.layer)) == 0) return;
        if (collision.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector2 knockBackDir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
            damageable.TakeDamage(_damage, knockBackDir, _knockBackForce);

            //Invoke the event
            OnDamageDealt?.Invoke(damageable);
        }
    }
}
