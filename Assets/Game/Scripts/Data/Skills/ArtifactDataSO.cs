using System.Collections.Generic;
using UnityEngine;
using Witch.Core.Stats;

public enum StatType
{
    Might,
    CooldownReduction,
    Area,
    ProjectileSpeed,
    Duration,
    AdditionalProjectiles,
    MaxHealth,
    MoveSpeed,
    Armor,
    HealthRegen,
    MagnetRange,
    Luck,
    Growth
}

[CreateAssetMenu(fileName = "ArtifactDataSO", menuName = "Scriptable Objects/ArtifactDataSO")]
public class ArtifactDataSO : ScriptableObject
{
    [Header("General Info")]
    public string artifactName = "Name";
    public Sprite artifactIcon;

    [TextArea(2, 3)]
    public string description;

    [Header("Stat Effect")]
    [Tooltip("The stat this item affects in PlayerStats ")]
    public StatType targetStat;

    [Tooltip("Increment method: Flat(add directly), Percent Add (cumulative percentage)")]
    public StatModType modType;

    [Tooltip("Increment value (Ex: 0.15 = +10%, 1 = +1 projectile)")]
    public float modifierValue;
}
