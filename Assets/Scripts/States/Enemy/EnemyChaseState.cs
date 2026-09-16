using UnityEngine;

public class EnemyChaseState : State
{
    private EnemyCore _enemy;
    public EnemyChaseState(Animator animator, AnimationClip clip, EnemyCore enemy) : base(animator, clip)
    {
        this._enemy = enemy;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        float distance = Vector2.Distance(_enemy.transform.position, _enemy.targetTransform.position);
        if(distance <= _enemy.attackRange)
        {
            if (_enemy.canAttack)
            {
                _enemy._machine.ChangeState(_enemy.attackState);
                return;
            }
            else
            {
                _enemy._machine.ChangeState(_enemy.idleState);
            }
        }
    }

    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        Chase();
    }

    private void Chase()
    {
        _enemy._rb.linearVelocity = _enemy.GetMovementDirection() * _enemy.moveSpeed;
    }

}
