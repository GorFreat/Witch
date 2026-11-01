using System.Collections.Generic;
using UnityEngine;

public class SateliteSkillController : ActiveSkillController
{
    [SerializeField] private GameObject _orbPrefab;
    [SerializeField] private float _baseRadius = 2.5f;

    private readonly List<GameObject> _activeOrbs = new List<GameObject>();
    private float _currentAngle = 0f;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        UpdateOrbs();
    }

    protected override void Update()
    {
        base.Update();

        if (_activeOrbs.Count == 0) return;

        float currentOrbitSpeed = GetCalculatedSpeed();
        _currentAngle += currentOrbitSpeed * Time.deltaTime;
        if (_currentAngle >= 360f) _currentAngle -= 360;

        float currentRadius = _baseRadius * GetCalculatedArea();

        float angleStep = 360f / _activeOrbs.Count;

        for (int i = 0; i < _activeOrbs.Count; i++)
        {
            if (!_activeOrbs[i].activeSelf) continue;

            //Calculating the rotate angle and convert it into coordinate
            float orbAngle = _currentAngle + (i * angleStep);
            float rad = orbAngle * Mathf.Deg2Rad;

            float x = Mathf.Cos(rad) * currentRadius;
            float y = Mathf.Sin(rad) * currentRadius;

            _activeOrbs[i].transform.position = transform.position + new Vector3(x, y, 0f);
        }
    }

    public void UpdateOrbs()
    {
        int targetCount = GetCalculatedAmount();
        float damage = GetCalculatedDamage();
        float knockback = SkillData.GetLevelData(CurrentLevel).baseKnockbackForce;

        float damageInterval = GetCalculatedCooldown();

        while(_activeOrbs.Count < targetCount)
        {
            GameObject newOrb = Instantiate(_orbPrefab, transform.position, Quaternion.identity);
            _activeOrbs.Add(newOrb);
        }

        for (int i = 0; i < targetCount; i++)
        {
            bool shouldBeActive = i < targetCount;
            _activeOrbs[i].SetActive(shouldBeActive);

            if (shouldBeActive)
            {
               SateliteSkill _sateliteOrb = _activeOrbs[i].GetComponent<SateliteSkill>();
                if (_sateliteOrb != null)
                {
                    _sateliteOrb.Setup(damage, damageInterval, knockback);
                }
            }
        }
    }

    public override void Upgrade()
    {
        base.Upgrade();
        UpdateOrbs();
    }

    protected override void ExecuteSkill()
    {
    }

}
