using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("General Info")]
    public string enemyName = "Enemy";

    [Header("Exp drop")]
    public int expAmount = 10;

    [Header("Base Stats")]
    [Tooltip("Max Health")]
    public float maxHealth = 50f;

    [Tooltip("Move Speed")]
    public float moveSpeed = 5f;

    [Tooltip("Damage Deal")]
    public float damage = 1f;

    [Header("Combat Setting")]
    [Tooltip("Attack Range")]
    public float attackRange = 1f;

    [Tooltip("Attack Cooldown")]
    public float attackCooldown = 1f;

    [Tooltip("Knockback Force")]
    public float knockBackforce = 5f;

    [Header("Enemy animations")]
    public AnimationClip idleClip;
    public AnimationClip moveClip;
    public AnimationClip attackClip;
    public AnimationClip hurtClip;
    public AnimationClip dieClip;
}
