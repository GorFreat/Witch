using UnityEngine;

public abstract class ActiveSkillController : MonoBehaviour
{
    [field:SerializeField] public ActiveSkillDataSO SkillData { get; private set; }

    public int CurrentLevel { get; protected set; } = 1;
    public bool isMaxLevel => CurrentLevel >= SkillData.MaxLevel;

    protected PlayerStats _playerStats;
    protected float _coolDownTimer = 0f;

    protected virtual void Awake()
    {
        _playerStats = GetComponentInParent<PlayerStats>();
        if (_playerStats == null)
        {
            Debug.LogError($"[ActiveSkillController] Cannot find PlayerStats from parent of {gameObject.name}!");
        }
    }

    protected virtual void Update()
    {
        _coolDownTimer += Time.deltaTime;
        float calculatedCooldown = GetCalculatedCooldown();
        if(_coolDownTimer >= calculatedCooldown)
        {
            _coolDownTimer = 0;
            ExecuteSkill();
        }
    }

    public virtual float GetCalculatedDamage()
    {
        float baseDamage = SkillData.GetLevelData(CurrentLevel).baseDamage;
        float might = _playerStats != null ? _playerStats.Might.Value : 1f;
        return baseDamage * might;
    }

    public virtual float GetCalculatedCooldown()
    {
        float baseCooldown = SkillData.GetLevelData(CurrentLevel).baseCooldown;
        float cdReduction = _playerStats != null ? _playerStats.CooldownReduction.Value : 0f;
        //Fix limited cooldown to be maximum 80%
        cdReduction = Mathf.Clamp(cdReduction, -0.8f, 0.8f);
        return Mathf.Max(0.1f, baseCooldown * (1 - cdReduction));
    }
    //Calculated amount of skill shot or projectile
    public virtual int GetCalculatedAmount()
    {
        int baseAmount = SkillData.GetLevelData(CurrentLevel).baseAmount;
        int extra = _playerStats != null ? (int)_playerStats.AdditionalProjectiles.Value : 0;
        return Mathf.Max(1, baseAmount + extra);
    }

    public virtual float GetCalculatedArea()
    {
        float baseArea = SkillData.GetLevelData(CurrentLevel).baseArea;
        float playerArea = _playerStats != null ? _playerStats.Area.Value : 1f;
        return baseArea * playerArea;
    }

    public virtual float GetCalculatedSpeed()
    {
        float baseSpeed = SkillData.GetLevelData(CurrentLevel).baseSpeed;
        float playerSpeedMod = _playerStats != null ? _playerStats.ProjectileSpeed.Value : 1f;
        return baseSpeed * playerSpeedMod;
    }

    public virtual void Upgrade()
    {
        if (!isMaxLevel)
        {
            CurrentLevel++;
            Debug.Log($"[Skill] {SkillData.skillName} upgraded to level {CurrentLevel}!");
        }
    }

    public void InitializeSkillData(ActiveSkillDataSO data)
    {
        SkillData = data;
    }

    protected abstract void ExecuteSkill();
}
