using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("PLayer Reference")]
    [Tooltip("Read player position via Scriptable Anchor")]
    [SerializeField] private TransformAnchor _playerAnchor;

    [Header("Wave Settings")]
    [Tooltip("List of wave in chronological order")]
    [SerializeField] private List<WaveData> _waves = new List<WaveData>();

    [Header("Spawn Setting")]
    [Tooltip("Spawn Distance (should be out of camera range)")]
    [SerializeField] private float _spawnRadius = 12f;

    [Header("Global Cap")]
    [Tooltip("Maximum number of enemies that can appear at the same time")]
    [SerializeField] private int _maxEnemyAlive = 30;

    //Multi-pool: each enemy has their own pool
    private Dictionary<GameObject, Queue<EnemyCore>> _enemyPools = new Dictionary<GameObject, Queue<EnemyCore>>();
    private int _currentActiveCount = 0;
    
    private int _currentWaveIndex = 0;
    private float _waveTimer = 0f;
    private float _spawnTimer = 0f;

    private void Update()
    {
        //If player anchor is null of player anchor's value is null then return
        if(_playerAnchor == null || !_playerAnchor.HasValue) return;

        if (_waves == null || _waves.Count == 0) return;
        
        WaveData currentWave = _waves[_currentWaveIndex];

        //Manage wave timer for the current wave
        _waveTimer += Time.deltaTime;
        if(_waveTimer >= currentWave.waveDuration)
        {
            AdvanceToNextWave();
            return;
        }

        //Manage the frequency of enemies spawning
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= currentWave.spawnInterval)
        {
            _spawnTimer = 0f;
            TrySpawnEnemy(currentWave);
        }
    }

    private void AdvanceToNextWave()
    {
        if (_currentWaveIndex + 1 < _waves.Count)
        {
            _currentWaveIndex++;
            _waveTimer = 0f;
            Debug.Log($"Move to wave: {_waves[_currentWaveIndex].waveName}");
        }
        else
        {
            _waveTimer = 0f;
            _spawnTimer = 0f;
        }
    }

    private void TrySpawnEnemy(WaveData currentWave)
    {
        //Count still alive enemies on screen, if there are enough enemies on screen, stop spawning enemies
        if (_currentActiveCount >= _maxEnemyAlive) return;

        //Get random enemies in the wave config
        GameObject prefabToSpawn = currentWave.GetRandomEnemyPrefab();
        if (prefabToSpawn == null) return;

        //Caculate random position on a circle surrounding player
        Vector2 randomCircle = Random.insideUnitCircle.normalized * _spawnRadius;
        Vector3 spawnPos = _playerAnchor.Value.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

        //Get the exact enemies from multi-pool
        EnemyCore enemyToSpawn = GetEnemyFromPool(prefabToSpawn);
        if (enemyToSpawn != null)
        {
            _currentActiveCount++;
            enemyToSpawn.ResetEnemy(spawnPos);
        }
    }

    private EnemyCore GetEnemyFromPool(GameObject enemyPrefab)
    {
        //If there are no pool for this prefab, create a new pool
        if (!_enemyPools.ContainsKey(enemyPrefab))
        {
            _enemyPools[enemyPrefab] = new Queue<EnemyCore>();
        }

        Queue<EnemyCore> specificPool = _enemyPools[enemyPrefab];

        if(specificPool.Count > 0)
        {
            return specificPool.Dequeue();
        }

        //If out of enemies in specific pool, Instantiate a new one
        GameObject newEnemyObject = Instantiate(enemyPrefab, transform);
        EnemyCore newEnemy = newEnemyObject.GetComponent<EnemyCore>();
        
        newEnemy.OriginalPrefab = enemyPrefab;
        newEnemy.OnDespawn += HandleEnemyDespawn;

        return newEnemy;
    }

    private void HandleEnemyDespawn(EnemyCore enemy)
    {
        _currentActiveCount = Mathf.Max(0, _currentActiveCount - 1);

        if(_enemyPools.TryGetValue(enemy.OriginalPrefab, out var pool))
        {
            pool.Enqueue(enemy);
        }
    }
}
