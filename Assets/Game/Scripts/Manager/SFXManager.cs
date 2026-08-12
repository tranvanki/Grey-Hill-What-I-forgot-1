using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class SFXManager : MonoBehaviour
{   
    public static SFXManager Instance;
    
    public enum SFXType
    {
        OpenPanel, ClickButton, ClickX,
        ClickDoor, PickupItem, DialogueBlip, MonsterAttack, PlayerDie, PuzzleClick,
        PowerDown, MonsterSpawn
    }
    public enum MusicType { MainMenu, GameBGM, Cutscene }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   [System.Serializable]
    public struct SFXEntry { public SFXType type; public AudioClip clip; }
    [System.Serializable]
    public struct MusicEntry { public MusicType type; public AudioClip clip; }

    [Header("Clip Bank")]
    public SFXEntry[] sfxBank;
    public MusicEntry[] musicBank;

    [Header("Sources")]
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;
    public AudioSource musicSource;      // loop = true, gán mixerGroup = musicMixerGroup
    [SerializeField] private int poolSize = 6;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private AudioSource[] _sfxPool;
    private int _poolIndex;
    private Dictionary<SFXType, AudioClip> _sfxLookup;
    private Dictionary<MusicType, AudioClip> _musicLookup;
    private Coroutine _fadeRoutine;
    private MusicType? _currentMusic;

    void Awake()
    {
        if (Instance == null) {
        Instance = this; 
        DontDestroyOnLoad(gameObject);}
        else { Destroy(gameObject); return; }

        _sfxLookup = new Dictionary<SFXType, AudioClip>();
        foreach (var e in sfxBank) _sfxLookup[e.type] = e.clip;

        _musicLookup = new Dictionary<MusicType, AudioClip>();
        foreach (var e in musicBank) _musicLookup[e.type] = e.clip;

        _sfxPool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxMixerGroup;
            src.playOnAwake = false;
            _sfxPool[i] = src;
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

     void Start()
    {
        if (Instance != this) return; // chặn instance trùng chưa kịp bị Destroy chạy vào logic dùng dictionary null
        EvaluateMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return; // tương tự, phòng trường hợp OnEnable đã kịp subscribe trên bản duplicate
        EvaluateMusicForScene(scene.name);
    }
    private void EvaluateMusicForScene(string sceneName)
    {
        // Stop music in cutscene scenes
        if (sceneName.Contains("Cutscene") || sceneName.Contains("cutscene"))
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeOutMusic(0.5f));
            _currentMusic = null;
            Debug.Log("[SFXManager] Cutscene detected - stopping background music");
            return;
        }
        
        bool isMainMenu = sceneName == mainMenuSceneName;
        MusicType target = isMainMenu ? MusicType.MainMenu : MusicType.GameBGM;

        if (_currentMusic == target) return; // đang đúng bài rồi thì không crossfade lại

        PlayMusic(target);
    }
    // ── SFX ─────────────────────────────
    public void PlaySFX(SFXType type)
    {
        if (!_sfxLookup.TryGetValue(type, out var clip) || clip == null) return;
        var src = _sfxPool[_poolIndex];
        _poolIndex = (_poolIndex + 1) % _sfxPool.Length;
        src.PlayOneShot(clip);
    }

    // ── Music ────────────────────────────
    public void PlayMusic(MusicType type, float fadeTime = 1f)
    {
        if (!_musicLookup.TryGetValue(type, out var clip) || clip == null) return;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _currentMusic = type;
        _fadeRoutine = StartCoroutine(CrossfadeMusic(clip, fadeTime));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float t)
    {
        float startVol = musicSource.volume;
        for (float i = 0; i < t; i += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0, i / t);
            yield return null;
        }
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();
        for (float i = 0; i < t; i += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, startVol, i / t);
            yield return null;
        }
        musicSource.volume = startVol;
    }
    
    private IEnumerator FadeOutMusic(float t)
    {
        float startVol = musicSource.volume;
        for (float i = 0; i < t; i += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0, i / t);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = startVol;
    }
}
