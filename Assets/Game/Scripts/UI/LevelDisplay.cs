using TMPro;
using UnityEngine;

public class LevelDisplay : MonoBehaviour
{
    [SerializeField] private PlayerLevel _playerLevel;
    [SerializeField] private TextMeshProUGUI _levelText;
    void Update()
    {
        _levelText.text = "Level " + _playerLevel.CurrentLevel.ToString();
    }
}
