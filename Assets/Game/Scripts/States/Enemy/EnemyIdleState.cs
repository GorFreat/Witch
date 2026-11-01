using UnityEngine;

public class EnemyIdleState : State
{
    private EnemyCore _enemy;
    private Vector2 _velocity;
    public EnemyIdleState(Animator animator, AnimationClip clip, EnemyCore enemy) : base(animator, clip)
    {
        this._enemy = enemy;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        float distance = Vector2.Distance(_enemy.transform.position, _enemy.targetTransform.position);
        if (_enemy.canAttack && distance <= _enemy.attackRange)
        {
            _enemy._machine.ChangeState(_enemy.attackState);
            return;
        }

        if (distance > _enemy.attackRange)
        {
            _enemy._machine.ChangeState(_enemy.chaseState);
            return;
        }
    }
    public override void EnterState()
    {
        base.EnterState();
        _velocity = _enemy._rb.linearVelocity;
    }
    public override void PhysicalUpdate()
    {
        base.EnterState();
        _enemy._rb.linearVelocity = Vector2.zero;
    }
    public override void ExitState()
    {
        base.ExitState();
        _enemy._rb.linearVelocity = _velocity;
    }
}
