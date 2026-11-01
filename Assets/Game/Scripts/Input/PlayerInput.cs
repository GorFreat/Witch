using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private InputSystem_Actions _action;
    public Vector2 moveInput { get; private set; }

    private void Awake()
    {
        _action = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _action.Player.Enable();
    }
    private void OnDisable()
    {
        _action.Player.Disable();
    }

    private void Update()
    {
        moveInput = _action.Player.Move.ReadValue<Vector2>();
    }
}
