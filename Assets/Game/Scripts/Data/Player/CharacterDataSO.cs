using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    [Header("Info")]
    public string characterName = "Witch";
    public Sprite characterIcon;

    [Header("Starter Equipment")]
    [Tooltip("Starter skill when the game start")]
    public ActiveSkillDataSO starterSkill;

    [Header("Base Stats")]
    [Tooltip("Total damage (Might) - Default 1.0 = 100%")]
    public float baseMight = 1.0f;
    [Tooltip("Cooldown - Ex: -0.1f means decrease 10% CD")]
    public float baseCooldownReduction = 0.0f;
    [Tooltip("Impact area - Default 1.0 = 100%")]
    public float baseArea = 1.0f;
    [Tooltip("Projectile Speed")]
    public float baseProjectileSpeed = 1.0f;
    [Tooltip("Skills Duration")]
    public float baseDuration = 1.0f;
    [Tooltip("Additional projectiles (Amount) - Default +0")]
    public float baseAdditionalProjectiles = 0f;

    public int baseMaxHealth = 3;
    public float baseMoveSpeed = 5.0f;
    public int baseArmor = 0;
    public int baseHealthRegen = 0;

    [Header("Utility Stats")]
    public float baseMagnetRange = 2f;
    public float baseLuck = 1f;
    public float baseGrowth = 1f;

}
