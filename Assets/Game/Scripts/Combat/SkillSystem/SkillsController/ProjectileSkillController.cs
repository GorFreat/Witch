using System.Collections.Generic;
using UnityEngine;

public class ProjectileSkillController : ActiveSkillController
{
    [Header("Target Setting")]
    [SerializeField] private float _detectionRadius = 15f;

    //Pool store projectile for this skill
    private Queue<ProjectileSkill> _pool = new Queue<ProjectileSkill>();

    private readonly List<EnemyCore> _nearbyEnemiesBuffer = new List<EnemyCore>();
    private readonly List<Transform> _targetBuffer = new List<Transform>();

    protected override void ExecuteSkill()
    {
        int projectileCount = GetCalculatedAmount();
        List<Transform> targets = FindClosestEnemies(projectileCount);

        //Shoot projectile toward enemies (face direction if there are no enemies in range)
        for (int i = 0; i < projectileCount; i++)
        {
            Vector2 fireDir;
            if (i < targets.Count && targets[i] != null)
            {
                fireDir = (targets[i].position - transform.position).normalized;
            }
            else
            {
                //If there is no enemy, shoot at random direction
                fireDir = Random.insideUnitCircle.normalized;
            }
            SpawnProjectile(fireDir);
        }
    }

    private void SpawnProjectile(Vector2 direction)
    {
        ProjectileSkill bullet = GetFromPool();
        //Projectile spawn distance relative to player
        float projectileSpawnDistance = 0.5f;
        bullet.transform.position = transform.position + (Vector3)(new Vector2(projectileSpawnDistance, projectileSpawnDistance) 
                                                                    * new Vector2(direction.x, direction.y));
        var levelData = SkillData.GetLevelData(CurrentLevel);
        bullet.Initialize(
            direction,
            GetCalculatedSpeed(),
            GetCalculatedDamage(),
            levelData.baseKnockbackForce,
            GetCalculatedArea(),
            levelData.baseDuration,
            ReturnToPool
        );
    }

    //TARGETING LOGIC
    private List<Transform> FindClosestEnemies(int count)
    {
        _targetBuffer.Clear();

        if (SpatialHashGrid.Instance == null) return _targetBuffer;

        SpatialHashGrid.Instance.QueryRadius(transform.position, _detectionRadius, _nearbyEnemiesBuffer);

        if (_nearbyEnemiesBuffer.Count == 0) return _targetBuffer;

        Vector2 myPos = transform.position;

        _nearbyEnemiesBuffer.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            float distA = ((Vector2)(a.transform.position) - myPos).sqrMagnitude;
            float distB = ((Vector2)(b.transform.position) - myPos).sqrMagnitude;
            return distA.CompareTo(distB);
        });

        int targetAmount = Mathf.Min(_nearbyEnemiesBuffer.Count, count);
        for (int i = 0; i < targetAmount; i++)
        {
            _targetBuffer.Add(_nearbyEnemiesBuffer[i].transform);
        }
        return _targetBuffer;
    }


    private ProjectileSkill GetFromPool()
    {
        if (_pool.Count > 0)
        {
            return _pool.Dequeue();
        }

        //If out of bullet, create new one
        GameObject obj = Instantiate(SkillData.skillPrefab, transform.position, Quaternion.identity);
        return obj.GetComponent<ProjectileSkill>();
    }
    private void ReturnToPool(ProjectileSkill bullet)
    {
        _pool.Enqueue(bullet);
    }
}
