using UnityEngine;

public class ProjectileSkill : MonoBehaviour
{
    protected DamageDealer _damageDealer;
    protected Vector2 _direction;
    protected float _speed;
    protected float _damage;
    protected float _kbForce;
    protected float _lifeTime;
    protected float _lifeTimer;

    // Callback to return projectile to pool without knowing the exact pool
    protected System.Action<ProjectileSkill> _returnToPoolAction;
    public void Initialize( Vector2 direction, float speed, float damage, float kbForce, float areaScale,
                            float duration, System.Action<ProjectileSkill> returnToPool)
    {
        _direction = direction;
        _speed = speed;
        _damage = damage;
        _kbForce = kbForce;
        _lifeTime = duration;
        _lifeTimer = 0f;
        _returnToPoolAction = returnToPool;

        transform.localScale = Vector3.one * areaScale;

        // Flip the projectile to the right direction
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (_damageDealer != null)
        {
            _damageDealer.SetDamageStat(_damage, _kbForce);
        }

        gameObject.SetActive(true);

    }
    protected virtual void Awake()
    {
        _damageDealer = GetComponent<DamageDealer>();
    }

    protected virtual void Update()
    {
        //Moving
        transform.Translate(_direction * (_speed * Time.deltaTime), Space.World);

        //Counting lifetime
        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= _lifeTime)
        {
            Despawn();
        }
    }

    private void OnEnable()
    {
        if(_damageDealer != null)
            _damageDealer.OnDamageDealt += HandleDamageDealt;
    }

    private void OnDisable()
    {
        if( _damageDealer != null )
            _damageDealer.OnDamageDealt -= HandleDamageDealt;
    }

    private void HandleDamageDealt(IDamageable target)
    {
        Despawn();
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
        _returnToPoolAction?.Invoke(this);
    }
}
