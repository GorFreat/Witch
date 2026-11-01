using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkillDataSO", menuName = "Scriptable Objects/ActiveSkillDataSO")]
public class ActiveSkillDataSO : ScriptableObject
{
    [Header("General Info")]
    public string skillName;
    public Sprite skillIcon;

    [Header("Prefabs")]
    [Tooltip("Prefab of the skill or projectile")]
    public GameObject skillPrefab;

    [Header("Controller Template")]
    [Tooltip("Prefab Controller for all the skills")]
    public ActiveSkillController skillControllerPrefab;

    [Header("Levels Progression (1 -> 8)")]
    public List<SkillLevelData> levels = new List<SkillLevelData>();

    public int MaxLevel => levels.Count;

    public SkillLevelData GetLevelData(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        return levels[index];
    }

}
