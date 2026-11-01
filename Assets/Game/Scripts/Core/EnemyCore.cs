using UnityEngine;

public class EnemyCore : Core, IMovable
{
    // Original enemy prefab for classifying enemies to know which pool to put this enemy in
    public GameObject OriginalPrefab { get; set; }
    public System.Action<EnemyCore> OnDespawn;

    [SerializeField] private bool isSpriteFacingRight = true;
    //Enemy datas
    [field: SerializeField] public EnemyData enemyData { get; private set; }
    public float baseMoveSpeed => enemyData != null ? enemyData.moveSpeed : 5f;
    public float currentMoveSpeed;
    public float attackRange => enemyData != null ? enemyData.attackRange : 1f;
    [field: SerializeField] public TransformAnchor targetAnchor {  get; private set; }
    public Transform targetTransform { get; private set; }

    public StateMachine _machine { get; private set; }
    //Every enemy have these states
    #region ENEMY's GENERAL STATES
    public State idleState;
    public State chaseState;
    public State attackState;
    public State hurtState;
    public State dieState;
    #endregion


    //Health
    public Health health { get; private set; }

    public Rigidbody2D _body => _rb;

    public float _moveSpeed => currentMoveSpeed;

    public bool canAttack => Time.time >= _nextAttackTime;
    public float attackCooldown => enemyData != null ? enemyData.attackCooldown : 2f;
    private float _nextAttackTime;

    //Hurt box
    protected Collider2D _hurtBox;

    public virtual Vector2 GetMovementDirection()
    {
        if(targetTransform == null) return Vector2.zero;
        return ((Vector2)targetTransform.position - (Vector2)transform.position).normalized;
    }
    protected override void Awake()
    {
        base.Awake();
        _machine = new StateMachine();
        health = GetComponent<Health>();

        if (health != null && enemyData != null)
        {
            health.SetMaxHealth(enemyData.maxHealth);
        }
        _hurtBox = GetComponent<Collider2D>();
    }
    protected virtual void Start()
    {
        if (targetAnchor != null && targetAnchor.Value)
        {
            targetTransform = targetAnchor.Value;
        }
        else if (targetAnchor != null)
        {
            targetAnchor.OnTransformSet += GetTargetTransform;
        }

        //Set damage and knockback force in enemy data for Damage Dealer
        DamageDealer damageDealer = GetComponentInChildren<DamageDealer>();
        if(damageDealer != null && enemyData != null)
        {
            damageDealer.SetDamageStat(enemyData.damage, enemyData.knockBackforce);
        }
    }


    private void OnDestroy()
    {
        if(targetAnchor != null)
        {
            targetAnchor.OnTransformSet -= GetTargetTransform;
        }
    }

    protected void GetTargetTransform(Transform target)
    {
        targetTransform = target;
    }
    
    public virtual void FixedUpdate()
    {
        FlipSprite();

        if(SpatialHashGrid.Instance != null)
        {
            SpatialHashGrid.Instance.UpdatePosition(this);
        }
    }
    public void TriggerAttackCoolDown()
    {
        _nextAttackTime = Time.time + attackCooldown;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    public void FlipSprite()
    {
        float dirX = GetMovementDirection().x;
        if (Mathf.Abs(dirX) > 0.01f)
        {
            Vector3 localScale = transform.localScale;
            bool faceRight = dirX > 0;
            if(!isSpriteFacingRight) faceRight = !faceRight;
            localScale.x = faceRight ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
    }

    public void OnEnable()
    {
        health.OnDie.AddListener(Dying);
        health.OnHurt.AddListener(Hurting);
    }
    private void OnDisable()
    {
        health.OnDie.RemoveListener(Dying);
        health.OnHurt.RemoveListener(Hurting);

        if (SpatialHashGrid.Instance != null)
        {
            SpatialHashGrid.Instance.Unregistered(this);
        }

        //Inform to spawner that this enemy is returned to pool
        OnDespawn?.Invoke(this);
    }

    private void Hurting()
    {
        _machine.ChangeState(hurtState);
        return;
    } 
    private void Dying()
    {
        _machine.ChangeState(dieState);
        return;
    }

    public virtual void ResetEnemy(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        float randomMultipler = Random.Range(0.8f, 1.0f);
        currentMoveSpeed = baseMoveSpeed * randomMultipler;

        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        _nextAttackTime = 0;
        gameObject.SetActive(true);
        if(SpatialHashGrid.Instance != null)
        {
            SpatialHashGrid.Instance.Register(this);
        }
        if (enemyData != null && health != null)
        {
            health.SetMaxHealth(enemyData.maxHealth);
        }
        if (_machine != null && chaseState != null)
        {
            _machine.Initialize(chaseState);
        }
        if (_hurtBox != null)
        {
            _hurtBox.enabled = true;
        }
    }


}
