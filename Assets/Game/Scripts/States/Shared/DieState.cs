using UnityEngine;

public class DieState : State
{
    private Health _health;
    private Rigidbody2D _body;
    private Collider2D _hurtBox;
    private EnemyData _enemyData;
    public DieState(Animator animator, AnimationClip clip, Health health, Rigidbody2D body, Collider2D hurtBox, EnemyData enemyData) : base(animator, clip)
    {
        this._health = health;
        this._body = body;
        this._hurtBox = hurtBox;
        this._enemyData = enemyData;
    }

    public override void EnterState()
    {
        base.EnterState();
        _body.linearVelocity = Vector3.zero;
        _hurtBox.enabled = false;
        if (ExperienceGemPool.Instance != null && _enemyData != null)
        {
            ExperienceGemPool.Instance.SpawnGem(_body.position, _enemyData.expAmount);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(StateTime > _animClip.length)
        {
            _health.gameObject.SetActive(false);
        }
    }
}
