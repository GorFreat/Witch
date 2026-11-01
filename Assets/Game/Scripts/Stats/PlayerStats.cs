using UnityEngine;
using Witch.Core.Stats;

public class PlayerStats : MonoBehaviour
{
    [field:SerializeField] public CharacterDataSO characterData { get; private set; }
    [Header("Offensive Stats")]
    [Tooltip("Total damage (Might) - Default 1.0 = 100%")]
    public CharacterStat Might { get; private set; }

    [Tooltip("Cooldown - Ex: -0.1f means decrease 10% CD")]
    public CharacterStat CooldownReduction { get; private set; }

    [Tooltip("Impact area - Default 1.0 = 100%")]
    public CharacterStat Area { get; private set; }

    [Tooltip("Projectile Speed")]
    public CharacterStat ProjectileSpeed { get; private set; }

    [Tooltip("Skills Duration")]
    public CharacterStat Duration { get; private set; }

    [Tooltip("Additional projectiles (Amount) - Default +0")]
    public CharacterStat AdditionalProjectiles { get; private set; }

    [Header("Survival & Movement Stats")]
    public CharacterStat MaxHealth { get; private set; }
    public CharacterStat MoveSpeed { get; private set; }
    public CharacterStat Armor { get; private set; }
    public CharacterStat HealthRegen { get; private set; }

    [Header("Utility Stats")]
    public CharacterStat MagnetRange { get; private set; }
    public CharacterStat Luck { get; private set; }
    public CharacterStat Growth { get; private set; } // Increase exp gain


    private void Awake()
    {
        Might = new CharacterStat(characterData.baseMight);
        CooldownReduction = new CharacterStat(characterData.baseCooldownReduction);
        Area = new CharacterStat(characterData.baseArea);
        ProjectileSpeed = new CharacterStat(characterData.baseProjectileSpeed);
        Duration = new CharacterStat(characterData.baseDuration);
        AdditionalProjectiles = new CharacterStat(characterData.baseAdditionalProjectiles);
        MaxHealth = new CharacterStat(characterData.baseMaxHealth);
        MoveSpeed = new CharacterStat(characterData.baseMoveSpeed);
        Armor = new CharacterStat(characterData.baseArmor);
        HealthRegen = new CharacterStat(characterData.baseHealthRegen);
        MagnetRange = new CharacterStat(characterData.baseMagnetRange);
        Luck = new CharacterStat(characterData.baseLuck);
        Growth = new CharacterStat(characterData.baseGrowth);
    }
}
