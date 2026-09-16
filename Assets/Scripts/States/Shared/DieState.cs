using UnityEngine;

public class DieState : State
{
    private Health _health;
    private Rigidbody2D _body;
    public DieState(Animator animator, AnimationClip clip, Health health, Rigidbody2D body) : base(animator, clip)
    {
        this._health = health;
        this._body = body;
    }

    public override void EnterState()
    {
        base.EnterState();
        _body.linearVelocity = Vector3.zero;
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
