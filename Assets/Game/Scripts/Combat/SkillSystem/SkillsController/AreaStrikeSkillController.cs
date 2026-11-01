using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaStrikeSkillController : ActiveSkillController
{
    [Header("Target Setting")]
    [SerializeField] private float _detectionRadius = 15f;

    //Pool for storing strikes
    private Queue<AreaStrikeSkill> _pool = new Queue<AreaStrikeSkill>();

    private readonly List<EnemyCore> _nearbyEnemiesBuffer = new List<EnemyCore>();
    private readonly List<Transform> _targetBuffer = new List<Transform>();


    protected override void ExecuteSkill()
    {
        int strikeCount = GetCalculatedAmount();
        List<Transform> targets = FindClosestEnemies(strikeCount);

        StartCoroutine(CastStrikesRoutine(targets, strikeCount));
    }

    private IEnumerator CastStrikesRoutine(List<Transform> targets, int strikeCount)
    {
        for (int i = 0; i < strikeCount; i++)
        {
            if(targets.Count > 0)
            {
                Transform target = targets[i % targets.Count];
                if (target != null && target.gameObject.activeInHierarchy)
                {
                    SpawnStrike(target);
                }
            }
            yield return new WaitForSeconds(0.06f);
        }
    }

    private void SpawnStrike(Transform target)
    {
        AreaStrikeSkill strike = GetFromPool();

        var LevelData = SkillData.GetLevelData(CurrentLevel);
        strike.transform.position = target.position;
        strike.Initialize(
            GetCalculatedDamage(),
            LevelData.baseKnockbackForce,
            ReturnToPool
            );
        Debug.Log("Damage:" + GetCalculatedDamage());
    }

    //TARGETING LOGIC
    private List<Transform> FindClosestEnemies(int count)
    {
        _targetBuffer.Clear();

        if (SpatialHashGrid.Instance == null) return _targetBuffer;

        SpatialHashGrid.Instance.QueryRadius(transform.position, _detectionRadius, _nearbyEnemiesBuffer);

        if(_nearbyEnemiesBuffer.Count == 0) return _targetBuffer;

        Vector2 myPos = transform.position;

        _nearbyEnemiesBuffer.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            float distA = ((Vector2)(a.transform.position) - myPos).sqrMagnitude;
            float distB = ((Vector2)(b.transform.position) - myPos).sqrMagnitude;
            return distA.CompareTo(distB);
        });

        int targetAmount = Mathf.Min(_nearbyEnemiesBuffer.Count, count);
        for(int i = 0; i < targetAmount; i++)
        {
            _targetBuffer.Add(_nearbyEnemiesBuffer[i].transform);
        }
        return _targetBuffer;
    }

    private AreaStrikeSkill GetFromPool()
    {
        if (_pool.Count > 0)
        {
            return _pool.Dequeue();
        }

        //If out of bullet, create new one
        GameObject obj = Instantiate(SkillData.skillPrefab, transform.position, Quaternion.identity);
        return obj.GetComponent<AreaStrikeSkill>();
    }
    private void ReturnToPool(AreaStrikeSkill strike)
    {
        _pool.Enqueue(strike);
    }
}
