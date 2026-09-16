using UnityEngine;

public interface IMovable
{
    public Rigidbody2D _body { get; }
    public Animator _animator { get; }
    public float _moveSpeed { get; }
    public Vector2 GetMovementDirection();

}
