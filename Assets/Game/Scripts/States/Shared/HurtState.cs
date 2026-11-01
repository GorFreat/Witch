using UnityEngine;
using System;

public class HurtState : State
{
    private Action _onHurtComplete;
    private Health _health;
    private Rigidbody2D _body;
    public HurtState(Animator animator, AnimationClip clip, Health health, Rigidbody2D body, Action onHurtComplete) : base(animator, clip)
    {
        this._onHurtComplete = onHurtComplete;
        this._health = health;
        this._body = body;
    }

    public override void EnterState()
    {
        base.EnterState();
        Vector2 knockBack = _health.knockBackVelocity;
        _body.linearVelocity = knockBack;
        _body.mass = 1000;
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(StateTime >= _animClip.length)
        {
            _onHurtComplete?.Invoke();
        }
    }
    public override void ExitState()
    {
        base.ExitState();
    }
}
