using UnityEngine;

public class DemonBatController : EnemyCore
{
    [field: SerializeField] public Collider2D hitBox { get; private set; }

    #region DEMON BAT ANIMATION CLIPS
    [field:SerializeField] public AnimationClip idleClip {  get; set; }
    [field: SerializeField] public AnimationClip chaseClip { get; set; }
    [field: SerializeField] public AnimationClip attackClip { get; set; }
    [field: SerializeField] public AnimationClip hurtClip { get; set; }
    [field: SerializeField] public AnimationClip dieClip { get; set; }
    #endregion


    protected override void Awake()
    {
        base.Awake();
        idleState = new EnemyIdleState(_animator, idleClip, this);
        chaseState = new EnemyChaseState(_animator, chaseClip, this);
        attackState = new EnemyCloseAttack(_animator, attackClip, hitBox, this);
        hurtState = new HurtState(_animator, hurtClip, health, _rb,
        () => {
                _machine.ChangeState(idleState);
            });
        dieState = new DieState(_animator, dieClip, health, _rb);

    }
    public void Start()
    {
        hitBox.enabled = false;
        _machine.Initialize(chaseState);
    }

    public void Update()
    {
        _machine.currentState.LogicUpdate();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _machine.currentState.PhysicalUpdate();
    }
}
