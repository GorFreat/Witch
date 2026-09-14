using UnityEngine;

public class EnemyCore : Core, IMovable
{
    [SerializeField] private bool isSpriteFacingRight = true;
    [field: SerializeField] public float moveSpeed { get; private set; } = 5f;
    [field: SerializeField] public Transform targetTransform {  get; private set; }
    [field: SerializeField] public float attackRange { get; private set; }

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

    public float _moveSpeed => moveSpeed;

    public bool canAttack => Time.time >= _nextAttackTime;

    public float attackCooldown { get; private set; } = 1.5f;
    private float _nextAttackTime; 

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
    }
    public virtual void FixedUpdate()
    {
        FlipSprite();
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
}
