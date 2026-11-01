using UnityEngine;

public class AreaStrikeSkill : MonoBehaviour
{
    [SerializeField] private AnimationClip _lightningStrikeCLip;
    protected DamageDealer _damageDealer;
    protected float _damage;
    protected float _kbForce;
    protected float _lifeTime;
    protected float _lifeTimer;

    // Callback to return projectile to pool without knowing the exact pool
    protected System.Action<AreaStrikeSkill> _returnToPoolAction;

    public void Initialize(float damage, float kbForce, System.Action<AreaStrikeSkill> returnToPool)
    {
        _damage = damage;
        _kbForce = kbForce;
        _returnToPoolAction = returnToPool;

        _lifeTimer = 0f;

        if (_damageDealer != null)
        {
            _damageDealer.SetDamageStat(_damage, _kbForce);
            Debug.Log("Damage, kbforce:" + _damage + ", " + _kbForce);
        }
        gameObject.SetActive(true);
    }

    protected void Awake()
    {
        _damageDealer = GetComponent<DamageDealer>();
        _lifeTime = _lightningStrikeCLip.length;
    }

    private void Update()
    {
        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= _lifeTime)
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
        _returnToPoolAction?.Invoke(this);
    }
}
