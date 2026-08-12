using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;

public class SoundMixermanager : MonoBehaviour
{   
    // ➕ SINGLETON PATTERN
    public static SoundMixermanager Instance { get; private set; }
    
    [SerializeField] private AudioMixer mixer;
    
    [Header("Volume Sliders (Optional - assign in Inspector)")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    
    void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SoundMixermanager] Duplicate instance found - destroying duplicate");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[SoundMixermanager] Instance created and will persist across scenes");
    }
    
    void Start()
    {
        ConnectSliders();
    }
    
    void ConnectSliders()
    {
        // Connect music slider if assigned
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(setMusicVolume);
            Debug.Log($"[SoundMixermanager] Music slider connected, initial value: {musicSlider.value}");
        }
        
        // Connect SFX slider if assigned
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(setSFXVolume);
            Debug.Log($"[SoundMixermanager] SFX slider connected, initial value: {sfxSlider.value}");
        }
    }
    
    public void RegisterSliders(Slider music, Slider sfx)
    {
        // Allow dynamic registration from other scripts
        musicSlider = music;
        sfxSlider = sfx;
        ConnectSliders();
    }
    public void SetMasterVolume(float volume)
    {
        if (mixer == null)
        {
            Debug.LogError("[SoundMixermanager] AudioMixer reference is null!");
            return;
        }
        // Slider sends -80 to 0 (decibels directly)
        mixer.SetFloat("Master", volume);
    }
    public void setMusicVolume(float volume)
    {
        if (mixer == null)
        {
            Debug.LogError("[SoundMixermanager] AudioMixer reference is null!");
            return;
        }
        // Slider sends -80 to 0 (decibels directly)
        mixer.SetFloat("Music", volume);
        Debug.Log($"[SoundMixermanager] Music volume set to {volume} dB");
    }
    
    public void setSFXVolume(float volume)
    {
        if (mixer == null)
        {
            Debug.LogError("[SoundMixermanager] AudioMixer reference is null!");
            return;
        }
        // Slider sends -80 to 0 (decibels directly)
        mixer.SetFloat("SFX", volume);
        Debug.Log($"[SoundMixermanager] SFX volume set to {volume} dB");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
