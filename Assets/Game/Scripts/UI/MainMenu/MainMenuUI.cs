using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _quitButton;

    public void OnPlayClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene((int)SceneIndex.GamePlay);
    }
    public void OnButtonClick()
    {
        SoundManager.PlaySound(SoundType.CLICK, 0.5f);
    }
    public void OnButtonSelect()
    {
        SoundManager.PlaySound(SoundType.SELECT, 0.3f);
    }
    private void Start()
    {
        SoundManager.PlayBGM(BGMType.MainMenuBGM, 0.5f);
    }
}

