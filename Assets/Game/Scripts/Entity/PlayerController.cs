using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : Core, IMovable
{
    [field: SerializeField] public float _moveSpeed {get; private set; } = 8.0f;

    public PlayerInput playerInput {get; private set; }

    public Rigidbody2D _body => _rb;

    public StateMachine _machine { get; private set; }

    public PlayerHealth health { get; private set; }
    private PlayerInvicible _playerInvicible;

    //Player anchor
    [Header("Anchor")]
    [SerializeField] private TransformAnchor _playerAnchor;
    #region PLAYER ANIMATION CLIPS
    [field:SerializeField] public AnimationClip idleClip { get; private set; }
    [field:SerializeField] public AnimationClip moveClip { get; private set; }
    #endregion

    #region PLAYER STATES
    public State idleState { get; private set; }
    public State moveState { get; private set; }
    public State hurtState {  get; private set; }
    public State dieState {  get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        playerInput = GetComponent<PlayerInput>();
        health = GetComponent<PlayerHealth>();
        _playerInvicible = GetComponent<PlayerInvicible>();
        _machine = new StateMachine();
        idleState = new PlayerIdleState(_animator, idleClip, this);
        moveState = new PlayerMoveState(_animator, moveClip, this);
    }

    void Start()
    {
        _machine.Initialize(idleState);
    }

    void Update()
    {
        _machine.currentState.LogicUpdate();
    }

    void FixedUpdate()
    {
        FlipSprite();
        _machine.currentState.PhysicalUpdate();
    }

    public Vector2 GetMovementDirection()
    {
        return playerInput.moveInput;
    }

    private void OnEnable()
    {
        //Register player transform to the anchor
        if(_playerAnchor != null)
        {
            _playerAnchor.Set(transform);
        }

        health.OnHurt.AddListener(Hurt);
        health.OnDie.AddListener(Die);
    }

    private void OnDisable()
    {
        //Unregister player transform when player is disable or change scene
        if(_playerAnchor != null){
            _playerAnchor.Clear();
        }

        health.OnHurt.RemoveListener(Hurt);
        health.OnDie.RemoveListener(Die);
    }

    private void Hurt()
    {
        _playerInvicible.Invicible();
    }

    private void Die()
    {
        transform.gameObject.SetActive(false);
    }

    private void FlipSprite()
    {
        if(Mathf.Abs(playerInput.moveInput.x) > 0.1f)
        {
            Vector3 localScale = transform.localScale;
            localScale.x = Mathf.Abs(localScale.x) * playerInput.moveInput.x > 0 ? 1f : -1f;
            transform.localScale = localScale;
        }
    }
}
