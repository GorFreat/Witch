using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("Slot Settings")]
    [Tooltip("Maximum skill can be equiped")]
    [SerializeField] private int _maxActiveSlots = 6;

    [Header("Skill Holder")]
    [Tooltip("Transform store all the Controller of the subskill")]
    [SerializeField] private Transform _skillContainer;

    //Store all the available skill: Key = Data SO, Value = Skill Controller
    private readonly Dictionary<ActiveSkillDataSO, ActiveSkillController> _activeSkills = new Dictionary<ActiveSkillDataSO, ActiveSkillController>();

    //ReadOnly dictonary for UI setup
    public IReadOnlyDictionary<ActiveSkillDataSO, ActiveSkillController> ActiveSkills => _activeSkills;

    public int MaxActiveSlots => _maxActiveSlots;
    public int CurrentActiveCount => _activeSkills.Count;
    public bool IsFullActiveSlots => _activeSkills.Count >= _maxActiveSlots;

    private void Awake()
    {
        //If there is no Skill Container assigned, create a new GameObject for Skill Container
        if(_skillContainer == null)
        {
            GameObject container = new GameObject("Skills_container");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            _skillContainer = container.transform;
        }
    }
    void Start()
    {
        InitializeStarterSkill();
    }

    // Add starter skill
    private void InitializeStarterSkill()
    {
        PlayerStats playerStats = GetComponent<PlayerStats>();
        if (playerStats != null && playerStats.characterData != null && playerStats.characterData.starterSkill != null)
        {
            AddOrUpgradeSkill(playerStats.characterData.starterSkill);
        }
    }

    /// <summary>
    /// Main function, called when player choose a skill from level up screen
    /// </summary>
    public bool AddOrUpgradeSkill(ActiveSkillDataSO skillData)
    {
        if (skillData == null) return false;

        //Case 1: Already had this skill, then upgrade it
        if(_activeSkills.TryGetValue(skillData, out var existingController))
        {
            if (!existingController.isMaxLevel)
            {
                existingController.Upgrade();
                return true;
            }
            Debug.Log($"[SkillManager] skill {skillData.skillName} level is maxed!");
            return false;
        }

        //Case 2: Player doesn't have this skill, add new skill
        if (IsFullActiveSlots)
        {
            Debug.Log($"[SkillManager] Already full {_maxActiveSlots} slots of skill, can't add {skillData.skillName} skill!");
            return false;
        }

        //Determine skill controller that need for the skill base on It's Prefab or Configuration
        //Ex: Skills that shoot out projectiles is going to be with ProjectileSkillController
        ActiveSkillController controller = CreateControllerForSkill(skillData);
        
        if(controller != null)
        {
            _activeSkills.Add(skillData, controller);
            Debug.Log($"[SkillManager] new skill equipped: {skillData.skillName} (Slots: {_activeSkills.Count} / {_maxActiveSlots}");
            return true;
        }

        return false;
    }

    ///<summary>
    /// Helper create an appropriate skill controller
    /// </summary>
    protected virtual ActiveSkillController CreateControllerForSkill(ActiveSkillDataSO skillData)
    {
        if(skillData.skillControllerPrefab != null)
        {
            ActiveSkillController controller = Instantiate(skillData.skillControllerPrefab, _skillContainer);
            controller.name = $"Skill_{skillData.skillName}";
            controller.transform.localPosition = Vector3.zero;
            controller.InitializeSkillData(skillData);
            return controller;
        }
        return null;
    }

    //----------------------------------------UTILITY FUNCTION FOR UI LEVEL UP-----------------------------------------------

    public bool HasSkill(ActiveSkillDataSO skillData)
    {
        return _activeSkills.ContainsKey(skillData);
    }

    public bool IsSkillMaxLevel(ActiveSkillDataSO skillData)
    {
        if(_activeSkills.TryGetValue(skillData, out var controller))
        {
            return controller.isMaxLevel;
        }
        return false;
    }

    public int GetSkillLevel(ActiveSkillDataSO skillData)
    {
        if (_activeSkills.TryGetValue(skillData, out var controller))
        {
            return controller.CurrentLevel;
        }
        return 0;
    }
}
