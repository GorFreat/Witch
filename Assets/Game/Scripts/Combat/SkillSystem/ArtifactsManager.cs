using System.Collections.Generic;
using UnityEngine;
using Witch.Core.Stats;

public class ArtifactsManager : MonoBehaviour
{
    [Header("Slot Settings")]
    [Tooltip("Maximum artifacts can be equipped at the same time")]
    [SerializeField] private int _maxArtifactSlots = 6;

    //List of all the artifacts
    private readonly List<ArtifactDataSO> _equippedArtifacts = new List<ArtifactDataSO>();

    // Public read only list of artifact for UI interaction
    public IReadOnlyList<ArtifactDataSO> EquippedArtifacts => _equippedArtifacts;

    public int MaxArtifactSlot => _maxArtifactSlots;
    public int CurrentArtifactCount => _equippedArtifacts.Count;
    public bool IsFullArtifactSlots => _equippedArtifacts.Count >= _maxArtifactSlots;

    private PlayerStats _playerStats;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        if( _playerStats == null)
        {
            Debug.Log($"[ArtifactManager] cannot find PlayerStats on GameObject {gameObject.name}!");
        }
    }

    /// <summary>
    /// Add a new Artifact into the unequipped slot
    /// </summary>
    public bool EquipArtifact(ArtifactDataSO artifactData)
    {
        if (artifactData == null || _playerStats == null) return false;

        //Check for the dupplication of the equipped items
        if (_equippedArtifacts.Contains(artifactData))
        {
            Debug.Log($"[ArtifactManager] dupplicate artifact, artifact {artifactData.artifactName} is already equipped!");
            return false;
        }

        //Check for empty slot
        if (IsFullArtifactSlots)
        {
            Debug.Log($"[ArtifactManager] artifact slots is fulled, you need to get rid of other equipped artifact for {artifactData.artifactName}!");
            return false;
        }

        _equippedArtifacts.Add(artifactData);
        ApplyStatBonus(artifactData);

        Debug.Log($"[ArtifactManager] artifact {artifactData.artifactName} is equipped!");
        return true;
    }

    /// <summary>
    /// Remove an Artifact in the inventory -> auto remove artifact effect
    /// </summary>
    public bool UnequipArtifact(ArtifactDataSO artifactData)
    {
        if (artifactData != null || _equippedArtifacts.Contains(artifactData)) return false;

        _equippedArtifacts.Remove(artifactData);
        RemoveStatBonus(artifactData);

        Debug.Log($"[ArtifactManager] artifact {artifactData.artifactName} is completely remove from inventory (artifact slot remain: " +
                    $"{_equippedArtifacts.Count} / {_maxArtifactSlots})!");
        return true;
    }

    /// <summary>
    /// Replace a new artifact immediately
    /// </summary>
    public bool ReplaceArtifact(ArtifactDataSO oldArtifactData, ArtifactDataSO newArtifactData)
    {
        if(oldArtifactData == null || newArtifactData == null) return false;
        if (UnequipArtifact(oldArtifactData))
        {
            return EquipArtifact(newArtifactData);
        }
        return false;
    }

    //-----------------------------------------ADD AND REMOVE ARTIFACT MECHANISM---------------------------------------
    private void ApplyStatBonus(ArtifactDataSO artifactData) 
    {
        CharacterStat TargetStat = GetTargetStat(artifactData.targetStat);
        if (TargetStat == null) return;

        //artifactData is the source
        StatModifier mod = new StatModifier(artifactData.modifierValue, artifactData.modType, artifactData);
        TargetStat.AddModifier(mod);

        // Separately handling MaxHealth
        if(artifactData.targetStat == StatType.MaxHealth)
        {
            Health health = GetComponent<Health>();
            if(health != null) health.SetMaxHealth(_playerStats.MaxHealth.Value);
        }
    }

    private void RemoveStatBonus(ArtifactDataSO artifactData)
    {
        CharacterStat TargetStat = GetTargetStat(artifactData.targetStat);
        if (TargetStat == null) return;

        //Remove artifact's stat modifier
        TargetStat.RemoveModifierFromSource(artifactData);

        // Separately handling MaxHealth
        if (artifactData.targetStat == StatType.MaxHealth)
        {
            Health health = GetComponent<Health>();
            if (health != null) health.SetMaxHealth(_playerStats.MaxHealth.Value);
        }
    }

    private CharacterStat GetTargetStat(StatType statType)
    {
        switch (statType)
        {
            case StatType.Might: return _playerStats.Might;
            case StatType.CooldownReduction: return _playerStats.CooldownReduction;
            case StatType.Area: return _playerStats.Area;
            case StatType.ProjectileSpeed: return _playerStats.ProjectileSpeed;
            case StatType.Duration: return _playerStats.Duration;
            case StatType.AdditionalProjectiles: return _playerStats.AdditionalProjectiles;
            case StatType.MaxHealth: return _playerStats.MaxHealth;
            case StatType.MoveSpeed: return _playerStats.MoveSpeed;
            case StatType.Armor: return _playerStats.Armor;
            case StatType.HealthRegen: return _playerStats.HealthRegen;
            case StatType.MagnetRange: return _playerStats.MagnetRange;
            case StatType.Luck: return _playerStats.Luck;
            case StatType.Growth: return _playerStats.Growth;
            default:
                Debug.LogWarning($"[ArtifactManager] No mapping has been defined for {statType}");
                return null;
        }
    }
}
