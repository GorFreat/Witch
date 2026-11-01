using System.Collections;
using UnityEngine;

public class PlayerInvicible : MonoBehaviour
{
    [SerializeField] private float _invicibleDuration = 2.0f;
    private float _flickerDuration = 0.15f;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _playerHurtBox;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerHurtBox = GetComponent<Collider2D>();
    }

    public void Invicible()
    {
        StartCoroutine(InvicibleRoutine());
        Debug.Log("Start invicible");
    }

    private IEnumerator InvicibleRoutine()
    {
        float timeCountDown = 0f;
        _playerHurtBox.enabled = false;
        while (timeCountDown < _invicibleDuration)
        {
            _spriteRenderer.enabled = !_spriteRenderer.enabled;
            yield return new WaitForSeconds(_flickerDuration);
            timeCountDown += _flickerDuration;
        }
        _playerHurtBox.enabled = true;
    }
}
