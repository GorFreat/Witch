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
        if (_enemy.targetTransform == null) return;
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
                return;
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
        Vector2 moveDir = _enemy.GetMovementDirection();
        if(SpatialHashGrid.Instance != null)
        {
            Vector2 separation = SpatialHashGrid.Instance.GetSeparationForce(_enemy);
            moveDir = (moveDir + separation * 0.5f).normalized;
        }
        _enemy._rb.linearVelocity = moveDir * _enemy._moveSpeed;
    }

}
