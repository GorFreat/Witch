using UnityEngine;

public class DemonBatController : EnemyCore
{
    [field: SerializeField] public Collider2D hitBox { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        idleState = new EnemyIdleState(_animator, enemyData.idleClip, this);
        chaseState = new EnemyChaseState(_animator, enemyData.moveClip, this);
        attackState = new EnemyCloseAttack(_animator, enemyData.attackClip, hitBox, this);
        hurtState = new HurtState(_animator, enemyData.hurtClip, health, _rb,
        () => {
                _machine.ChangeState(idleState);
            });
        dieState = new DieState(_animator, enemyData.dieClip, health, _rb, _hurtBox, enemyData);
    }
    protected override void Start()
    {
        base.Start();
        hitBox.enabled = false;
        _machine.Initialize(chaseState);
    }
    protected virtual void Update()
    {
        _machine.currentState.LogicUpdate();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _machine.currentState.PhysicalUpdate();
    }

    public override void ResetEnemy(Vector3 spawnPosition)
    {
        base.ResetEnemy(spawnPosition);
        hitBox.enabled = false;
    }
}
