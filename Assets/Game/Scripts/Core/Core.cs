using UnityEngine;

public class Core : MonoBehaviour
{
    public Animator _animator {  get; private set; }

    public Rigidbody2D _rb { get; private set; }

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

}
