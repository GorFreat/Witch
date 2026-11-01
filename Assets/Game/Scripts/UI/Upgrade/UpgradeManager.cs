using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Header("Databases")]
    [SerializeField] private List<ActiveSkillDataSO> _allSkills = new List<ActiveSkillDataSO>();

    [Header("References")]
    [SerializeField] private PlayerSkillManager _skillManager;
    [SerializeField] private PlayerLevel _playerLevel;
    [SerializeField] private GameObject _upgradePanelUI;
    [SerializeField] private Transform _cardsContainer;
    [SerializeField] private UpgradeCardUI _cardUIPrefab;

    private readonly List<UpgradeCardUI> _spawnedCards = new List<UpgradeCardUI>();
    private void OnEnable()
    {
        if (_playerLevel != null) _playerLevel.OnLevelUp += HandleLevelUp;
    }
    private void OnDisable()
    {
        if (_playerLevel != null) _playerLevel.OnLevelUp -= HandleLevelUp;
    }

    private void HandleLevelUp(int newLevel)
    {
        // Pause game
        Time.timeScale = 0f;

        List<ActiveSkillDataSO> chosenSkill = RollUpgrade(3);

        if(chosenSkill.Count == 0)
        {
            // If all the skills are max
            Time.timeScale = 1;
            return;
        }

        // Display upgrade UI
        _upgradePanelUI.SetActive(true);
        DisplayCards(chosenSkill);
    }

    /// <summary>
    /// Algorithm for filtering valid skills and randomly selecting non-duplicate ones
    /// </summary>
    private List<ActiveSkillDataSO> RollUpgrade (int count)
    {
        List<ActiveSkillDataSO> availablePool = new List<ActiveSkillDataSO>();

        foreach(var skill in _allSkills)
        {
            bool isEquipped = _skillManager.HasSkill(skill);
            bool isMax = _skillManager.IsSkillMaxLevel(skill);

            if (isEquipped && !isMax)
            {
                availablePool.Add(skill);
            }
            else if(!isEquipped && !_skillManager.IsFullActiveSlots)
            {
                availablePool.Add(skill);
            }
        }
        List<ActiveSkillDataSO> pickedSkills = new List<ActiveSkillDataSO>();
        int rollsNeeded = Mathf.Min(count, availablePool.Count);

        for (int i = 0; i < rollsNeeded; i++)
        {
            int randomIndex = Random.Range(0, availablePool.Count);
            pickedSkills.Add(availablePool[randomIndex]);
            availablePool.RemoveAt(randomIndex); // Remove the skill out of available pool after picking
        }
        return pickedSkills;
    }

    private void DisplayCards(List<ActiveSkillDataSO> skills)
    {
        while(_spawnedCards.Count < skills.Count)
        {
            UpgradeCardUI newCard = Instantiate(_cardUIPrefab, _cardsContainer);
            _spawnedCards.Add(newCard);
        }

        // Only change the data inside the card but don't destroy the whole thing
        for (int i = 0; i < skills.Count; i++)
        {
            _spawnedCards[i].gameObject.SetActive(true);
            _spawnedCards[i].Setup(skills[i], OnSelectedSkill, _skillManager);
        }

        // Disable excess card when the skills that can be upgraded less than 3
        for(int i = skills.Count; i < _spawnedCards.Count; i++)
        {
            _spawnedCards[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called when player click onto an upgrade card
    /// </summary>
    private void OnSelectedSkill(ActiveSkillDataSO skill)
    {
        _skillManager.AddOrUpgradeSkill(skill);

        _upgradePanelUI.SetActive(false);
        Time.timeScale = 1;
    }
}
