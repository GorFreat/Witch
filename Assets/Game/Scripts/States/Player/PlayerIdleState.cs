using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIdleState : State
{
    private IMovable entity;
    public PlayerIdleState(Animator animator, AnimationClip clip, IMovable entity) : base(animator, clip)
    {
        this.entity = entity;
    }

    public override void EnterState()
    {
        base.EnterState();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (entity is PlayerController)
        {
            PlayerController player = (PlayerController)entity;
            if (Mathf.Abs(player.GetMovementDirection().x) > 0.1f || Mathf.Abs(player.GetMovementDirection().y) > 0.1f)
            {
                player._machine.ChangeState(player.moveState);
            }
        }
    }

    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        entity._body.linearVelocity = Vector2.zero;
    }
}
