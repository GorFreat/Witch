using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SoundType
{
    SELECT,
    CLICK
}

public enum  BGMType
{
    MainMenuBGM,
    GamePlayBGM
}
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance => _instance;
    [SerializeField]private AudioSource _sfxSource;
    [SerializeField] private AudioSource _bgmSource;

    [SerializeField] private SoundList[] _soundList;
    [SerializeField] private AudioClip[] _bgmList;

    [Header("Scene BGM Configurations")]
    [SerializeField] private SceneBGMConfig[] _sceneBGMConfigs;

    private Coroutine _bgmFadeCoroutine;

    private void Awake()
    {
        if (!Application.isPlaying) return;
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        UpdateBGMForScene(SceneManager.GetActiveScene().buildIndex);
    }
    public static void PlaySound(SoundType soundType, float volume = 1)
    {
        if (_instance == null || _instance._sfxSource == null) return;
        if (_instance._soundList == null) return;

        int index = (int)soundType;
        if(index < 0 || index >= _instance._soundList.Length) return;

        AudioClip[] clips = _instance._soundList[(int)soundType].Sounds;

        if(clips == null || clips.Length == 0) return;

        AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        if(clip != null)
        {
            _instance._sfxSource.PlayOneShot(clip, volume);
        }
    }

    public static void PlayBGM(BGMType bgmType, float volume = 1f, float fadeDuration = 1f)
    {
        if(_instance == null || _instance._bgmSource == null) return;

        int index = (int)bgmType;
        if (_instance._bgmList == null || index < 0 || index >= _instance._bgmList.Length) return;

        AudioClip clip = _instance._bgmList[index];
        if(clip == null) return;

        if(_instance._bgmSource.clip == clip && _instance._bgmSource.isPlaying)
        {
            return;
        }

        if (_instance._bgmFadeCoroutine != null)
        {
            _instance.StopCoroutine(_instance._bgmFadeCoroutine);
        }
        _instance._bgmFadeCoroutine = _instance.StartCoroutine(
        _instance.FadeBGMCoroutine(clip, volume, fadeDuration)
);
    }

    private IEnumerator FadeBGMCoroutine(AudioClip newClip, float targetVolume, float fadeDuration)
    {
        AudioSource source = _instance._bgmSource;
        if (source == null) yield break;
        float halfDuration = fadeDuration / 2f;

        if(source.isPlaying && source.clip != null)
        {
            float startVolume = source.volume;
            float timer = 0f;
            while (timer < halfDuration)
            {
                timer += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, timer / halfDuration);
                yield return null;
            }

            source.volume = 0f;
            source.Stop();
        }
        source.clip = newClip;
        source.volume = 0f;
        source.loop = true;
        source.Play();

        float inTimer = 0f;
        while(inTimer < halfDuration)
        {
            inTimer += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, inTimer / halfDuration);
            yield return null;
        }
        source.volume = targetVolume;
        _bgmFadeCoroutine = null;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateBGMForScene(scene.buildIndex);
    }

    private void UpdateBGMForScene(int sceneIndex)
    {
        if (_sceneBGMConfigs == null) return;
        foreach(var config in _sceneBGMConfigs)
        {
            if(config.scene == (SceneIndex)sceneIndex)
            {
                PlayBGM(config.bgm);
                return;
            }
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref _soundList, names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            _soundList[i]._name = names[i];
        }
    }
#endif
}
[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds => _sounds;
    [HideInInspector] public string _name;
    [SerializeField] private AudioClip[] _sounds;
}

[Serializable]
public struct SceneBGMConfig
{
    public SceneIndex scene;
    public BGMType bgm;
}
