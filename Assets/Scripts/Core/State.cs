using UnityEngine;

public class State
{
    protected Animator _animator;
    protected AnimationClip _animClip;
    public float startTime;
    public float StateTime => Time.time - startTime;
    public State(Animator animator, AnimationClip clip)
    {
        this._animator = animator;
        this._animClip = clip;
    }
    
    public virtual void EnterState()
    {
        startTime = Time.time;
        _animator.Play(_animClip.name);
    }

    public virtual void LogicUpdate()
    {

    }
    public virtual void PhysicalUpdate()
    {

    }
    public virtual void ExitState() { }
}
