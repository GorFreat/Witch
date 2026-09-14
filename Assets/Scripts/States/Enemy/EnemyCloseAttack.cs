using UnityEngine;

public class EnemyCloseAttack : State
{
    private Collider2D _hitBox;
    private EnemyCore _enemy;
    public EnemyCloseAttack(Animator animator, AnimationClip clip, Collider2D _hitBox, EnemyCore enemy) : base(animator, clip)
    {
        this._hitBox = _hitBox;
        this._enemy = enemy;
    }

    public override void EnterState()
    {
        base.EnterState();
        _enemy._rb.linearVelocity = Vector2.zero;
        _hitBox.enabled = true;
        _enemy.TriggerAttackCoolDown();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (StateTime >= _animClip.length)
        {
            if(Vector2.Distance((Vector2)_enemy.transform.position, (Vector2)_enemy.targetTransform.position) <= _enemy.attackRange)
            {
                _enemy._machine.ChangeState(_enemy.idleState);
                return;
            }
            else if (Vector2.Distance(_enemy.transform.position, _enemy.targetTransform.position) > _enemy.attackRange)
            {
                _enemy._machine.ChangeState(_enemy.chaseState);
                return;
            }
        }
    }


    public override void ExitState()
    {
        base.ExitState();
        _hitBox.enabled = false;
    }
}
