using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float knockBackForce;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<IDamageable>() != null)
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

            Vector2 knockBackDir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
            damageable.TakeDamage(damage, knockBackDir, knockBackForce);
        }
    }
}
