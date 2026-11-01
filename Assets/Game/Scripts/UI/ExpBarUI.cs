using UnityEngine;
using UnityEngine.UI;

public class ExpBarUI : MonoBehaviour
{
   [SerializeField] private Slider _expSlider;
   [SerializeField] private PlayerLevel _playerLevel;

    void Update()
    {
        float expToNextLevel = _playerLevel.ExpToNextLevel;
        _expSlider.value = _playerLevel.CurrentExp / expToNextLevel;
    }
}
