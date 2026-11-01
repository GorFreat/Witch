using System;
using UnityEngine;


[Serializable]
public struct SkillLevelData
{
    [Tooltip("Base damage at this level")]
    public float baseDamage;

    [Tooltip("Base cooldown at this level(seconds)")]
    public float baseCooldown;

    [Tooltip("Basic skill shot amount at this level(number of projectiles, number of thunder strikes,...)")]
    public int baseAmount;

    [Tooltip("Basic skill speed(projectile speed, satelite speed,....)")]
    public float baseSpeed;

    [Tooltip("Size/ Area of skills or effect(1.0 = 100%)")]
    public float baseArea;

    [Tooltip("Base duration")]
    public float baseDuration;

    [Tooltip("Base knockback force")]
    public float baseKnockbackForce;

    [TextArea(2,4)]
    [Tooltip("Udpate description on selection card when leveling up")]
    public string upgradeDescription;

}
