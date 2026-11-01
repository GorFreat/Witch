using System.Collections.Generic;
using UnityEngine;

public class ExperienceGemPool : MonoBehaviour
{
    //Singleton for enemies to call when they dead
    public static ExperienceGemPool Instance {  get; private set; }

    [Header("Gem Settings")]
    [SerializeField] private ExperienceGem _gemPrefab;
    [SerializeField] private int _initialPoolSize = 50;

    //Queue for gem pool
    private readonly Queue<ExperienceGem> _gemPool = new Queue<ExperienceGem>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        PreLoadPool();
    }
    private void PreLoadPool()
    {
        for(int i = 0; i < _initialPoolSize; i++) 
        {
            ExperienceGem gem = Instantiate(_gemPrefab, transform);
            gem.gameObject.SetActive(false);
            _gemPool.Enqueue(gem);
        }
    }

    ///<summary>
    /// Spawn exp orb function: Called when enemies dead
    /// </summary>
    public ExperienceGem SpawnGem(Vector3 spawnPos, float expAmount)
    {
        ExperienceGem gem;
        
        if(_gemPool.Count > 0)
        {
            gem = _gemPool.Dequeue(); //Reuse gems that are stored in the pool
        }
        else
        {
            //If to much gems on the map and the pool is out of gems, create a new one
            gem = Instantiate(_gemPrefab, transform);
        }
        gem.Initialize(spawnPos, expAmount, ReturnGemToPool);
        return gem;
    }

    ///<summary>
    /// Store back into pool when the gem is picked up
    /// </summary>
    private void ReturnGemToPool(ExperienceGem gem)
    {
        _gemPool.Enqueue(gem); // Store the gem back into the pool for next time reuse
    }
}
