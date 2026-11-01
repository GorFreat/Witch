using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SateliteSkill : MonoBehaviour
{
    [SerializeField] private TransformAnchor _playerAnchor;
    
    private float _damage;
    private float _damageInterval;
    private float _knockbackForce;

    // For storing the time that enemy got hit by the Satelite
    private readonly Dictionary<IDamageable, float> _hitHistory = new Dictionary<IDamageable, float>();

    // For cleaning up data of _hitHistory
    private readonly List<IDamageable> _cleanupBuffer = new List<IDamageable>();
    private float _lastCleanupTime;

    private void Awake()
    {
        if(_playerAnchor == null)
        {
            Debug.LogError($"[SateliteOrb] _playerAnchor is Null", this);
        }
    }
    public void Setup(float damage, float damageInterval, float knockbackForce)
    {
        _damage = damage;
        _damageInterval = damageInterval;
        _knockbackForce = knockbackForce;
    }

    void Update()
    {
        if(Time.time >= _lastCleanupTime + 3)
        {
            _lastCleanupTime = Time.time;
            CleanupDeadTargets();
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IDamageable>(out var target))
        {
            if(!_hitHistory.TryGetValue(target, out float lastHitTime) || Time.time >= lastHitTime + _damageInterval)
            {
                Vector2 knockbackDir;
                if(_playerAnchor != null && _playerAnchor.HasValue)
                {
                    knockbackDir = (collision.transform.position - _playerAnchor.Value.transform.position).normalized;
                }
                else
                {
                    knockbackDir = (collision.transform.position - transform.position).normalized;
                }
                target.TakeDamage(_damage, knockbackDir, _knockbackForce);

                _hitHistory[target] = Time.time;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IDamageable>(out var target))
        {
            _hitHistory.Remove(target);
        }
    }

    private void CleanupDeadTargets()
    {
        _cleanupBuffer.Clear();

        foreach(var pair in _hitHistory)
        {
            if(pair.Key == null)
            {
                _cleanupBuffer.Add(pair.Key);
                continue;
            }
            
            if(pair.Key is Component comp && comp.gameObject.activeInHierarchy)
            {
                _cleanupBuffer.Add(pair.Key);
            }
        }

        foreach(var deadTarget in _cleanupBuffer)
        {
            _hitHistory.Remove(deadTarget);
        }
    }
}
