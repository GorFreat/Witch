using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    [Tooltip("Type of enemy")]
    public GameObject enemyPrefab;

    [Tooltip("Spawning rate (Weight), the higher the rate, the more often this enemy spawn ")]
    [Range(1,100)]
    public int spawnWeight = 10;
}

[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    [Header("Wave Info")]
    [Tooltip("Wave name (for debugging or UI displaying)")]
    public string waveName = " Wave 1";

    [Header("Timing")]
    [Tooltip("The duration of this wave (measured in seconds)")]
    public float waveDuration = 60f;

    [Tooltip("Spawn cooldown (measured in seconds)")]
    public float spawnInterval = 1.5f;

    [Header("Enemy in this wave")]
    [Tooltip("List of enemy in this wave")]
    public List<EnemySpawnConfig> enemies = new List<EnemySpawnConfig>();

    //Algorithm for random calculating with weight (Weighted Random)
    public GameObject GetRandomEnemyPrefab()
    {
        if (enemies == null || enemies.Count == 0) return null;
        if(enemies.Count == 1) return enemies[0].enemyPrefab;

        //Calculate all the weight (Ex: 70 + 30 = 100)
        int totalWeight = 0;
        foreach (var config in enemies)
        {
            if(config.enemyPrefab != null)
            {
                totalWeight += config.spawnWeight;
            }
        }
        if(totalWeight <= 0) return null;

        //Randon a number in range 1 to totalWeight - 1
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        //Find out which enemy is the chosen one
        foreach (var config in enemies)
        {
            if (config.enemyPrefab == null) continue;

            currentWeightSum += config.spawnWeight;
            if(randomValue < currentWeightSum)
            {
                return config.enemyPrefab;
            }
        }
        return enemies[0].enemyPrefab;
    }

}
