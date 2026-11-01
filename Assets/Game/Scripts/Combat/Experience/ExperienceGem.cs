using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    [SerializeField] private float _expAmount = 10f;
    [SerializeField] private float _moveSpeed = 12f;

    [SerializeField] private TransformAnchor _playerAnchor;
    private PlayerStats _playerStats;
    private PlayerLevel _playerLevel;
    private bool _isAttracted = false;


    //Callback return gem to pool
    private System.Action<ExperienceGem> _returnToPoolAction;

    public void Initialize(Vector3 spawnPosition, float expAmount, System.Action<ExperienceGem> returnToPool)
    {
        if (_playerAnchor == null || !_playerAnchor.HasValue)
        {
            Debug.LogError($"[ExperienceGem] _playerAnchor hasn't been assigned or player hasn't appreared!", this);
            return;
        }
        if (_playerStats == null)
        {
            _playerStats = _playerAnchor.Value.GetComponent<PlayerStats>();
            if (_playerStats == null)
            {
                Debug.LogError($"[ExperienceGem] Cannot find PlayerStats on Player ({_playerAnchor.Value.name})!", this);
            }
        }
        if (_playerLevel == null)
        {
            _playerLevel = _playerAnchor.Value.GetComponent<PlayerLevel>();
            if (_playerLevel == null)
            {
                Debug.LogError($"[ExperienceGem] Cannot find PlayerLevel on Player ({_playerAnchor.Value.name})!", this);
            }
        }
        _expAmount = expAmount;
        transform.position = spawnPosition;
        _returnToPoolAction = returnToPool;
        _isAttracted = false;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_playerAnchor == null || !_playerAnchor.HasValue || _playerStats == null || _playerLevel == null) return;

        Transform playerTransform = _playerAnchor.Value;
        float distance = Vector2.Distance(playerTransform.position, transform.position);

        //1. Check if the gem is inside the magnet range or not
        float magnetRange = _playerStats.MagnetRange.Value;
        if (!_isAttracted && distance <= magnetRange)
        {
            _isAttracted = true;
        }
        //2. If magnet is activated: fly toward player
        if (_isAttracted)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, _moveSpeed * Time.deltaTime);
        }
        if(distance < 0.3f)
        {
            Collect();
        }
    }

    private void Collect()
    {
        float growthMultipler = _playerStats.Growth.Value;
        float finalExp = _expAmount * growthMultipler;

        _playerLevel.AddExp(finalExp);
        Despawn();
    }

    private void Despawn()
    {
        gameObject.SetActive(false);
        _returnToPoolAction?.Invoke(this);
    }
}
