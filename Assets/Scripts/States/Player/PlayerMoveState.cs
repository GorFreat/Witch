using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveState : State
{
    private PlayerController _player;
    public PlayerMoveState(Animator animator, AnimationClip clip, PlayerController player) : base(animator, clip)
    {
        this._player = player;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(_player != null)
        {
            if(Mathf.Abs(_player.GetMovementDirection().x) < 0.1f && Mathf.Abs(_player.GetMovementDirection().y) < 0.1f){
                _player._machine.ChangeState(_player.idleState);
            }
        }
    }

    public override void PhysicalUpdate()
    {
        _player._body.linearVelocity = _player.GetMovementDirection() * _player._moveSpeed;
    }
}
