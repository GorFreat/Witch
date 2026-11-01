using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using System.Collections;

public class UpgradeCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Elements")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _upgradeDescription;
    [SerializeField] private Button _selectButton;

    [Header("Hover Effect")]
    [SerializeField] private GameObject _outlineBorder;
    [SerializeField] private float _hoverScale = 1.08f;
    [SerializeField] private float _scaleSpeed = 15f;

    private ActiveSkillDataSO _skillData;
    private Action<ActiveSkillDataSO> _onSelectCallback;
    private Coroutine _scaleCoroutine;
    private Vector3 _originalScale = Vector3.one;

    private void Awake()
    {
        _originalScale = transform.localScale;                                                                                                                                                                  
    }

    private void OnDisable()
    {
        if(_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        transform.localScale = _originalScale;
        if(_outlineBorder != null) _outlineBorder.SetActive(false);
    }

    public void Setup(ActiveSkillDataSO skillData, Action<ActiveSkillDataSO> onSelect, PlayerSkillManager skillManager)
    {
        _skillData = skillData;
        _onSelectCallback = onSelect;

        _selectButton.onClick.RemoveAllListeners();
        _selectButton.onClick.AddListener(() => _onSelectCallback?.Invoke(skillData));

        _iconImage.sprite = skillData.skillIcon;
        _nameText.text = skillData.skillName;

        bool isNew = !skillManager.HasSkill(skillData);
        int currentLevel = skillManager.GetSkillLevel(skillData);
        int nextLevel = currentLevel + 1;

        _levelText.text = isNew ? "New skill!" : $"Level {currentLevel} -> {nextLevel}";
        _upgradeDescription.text = skillData.GetLevelData(nextLevel).upgradeDescription;

        transform.localScale = _originalScale;
        if (_outlineBorder != null) _outlineBorder.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_outlineBorder != null) _outlineBorder.SetActive(true);
        StartScaleAnimation(_originalScale * _hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_outlineBorder != null) _outlineBorder.SetActive(false);
        StartScaleAnimation(_originalScale);
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
        if(_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(AnimateScale(targetScale));
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        while(Vector3.Distance(transform.localScale, targetScale) < 0.002f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, _scaleSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        transform.localScale = targetScale;
    }

}
