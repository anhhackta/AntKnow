using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton Audio Manager - Quản lý tất cả âm thanh trong game
/// Persist across scenes (DontDestroyOnLoad)
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;      // Background music
    [SerializeField] private AudioSource sfxSource;        // Sound effects

    [Header("Background Music")]
    [SerializeField] private AudioClip menuMusic;          // Menu scene music
    [SerializeField] private AudioClip gameMusic;          // Game scene music
    // Login scene uses PopupMusic (list music), không cần ở đây

    [Header("Sound Effects")]
    [SerializeField] private AudioClip btnClickSound;      // Button click
    [SerializeField] private AudioClip notificationSound;  // Notification panel
    [SerializeField] private AudioClip startSound;         // Start game button
    [SerializeField] private AudioClip bounceSound;        // Jump to tile (in game)
    [SerializeField] private AudioClip profitSound;        // Gain money (in game)
    [SerializeField] private AudioClip lossSound;          // Lose money (in game)

    [Header("Volume Settings")]
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;

    // PlayerPrefs keys
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        // Create AudioSources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // Load saved volumes
        LoadVolumes();

        Debug.Log("AudioManager initialized");
    }

    private void Start()
    {
        // Auto play menu music if in Menu scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MenuScene")
        {
            PlayMenuMusic();
        }
    }

    // ===== BACKGROUND MUSIC =====

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Music clip is null");
            return;
        }

        // Check if musicSource is valid
        if (musicSource == null)
        {
            Debug.LogError("AudioManager: musicSource is null! Recreating AudioSource...");
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            // Already playing this music
            return;
        }

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();

        Debug.Log($"AudioManager: Playing music - {clip.name}");
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    // ===== SOUND EFFECTS =====

    public void PlayButtonClick()
    {
        PlaySFX(btnClickSound);
    }

    public void PlayNotification()
    {
        PlaySFX(notificationSound);
    }

    public void PlayStart()
    {
        PlaySFX(startSound);
    }

    public void PlayBounce()
    {
        PlaySFX(bounceSound);
    }

    public void PlayProfit()
    {
        PlaySFX(profitSound);
    }

    public void PlayLoss()
    {
        PlaySFX(lossSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: SFX clip is null");
            return;
        }

        // Check if sfxSource is valid
        if (sfxSource == null)
        {
            Debug.LogError("AudioManager: sfxSource is null! Recreating AudioSource...");
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ===== VOLUME CONTROL =====

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        SaveVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveVolumes();
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    /// <summary>
    /// Get music AudioSource (for SettingsPanel to control volume)
    /// </summary>
    public AudioSource GetMusicSource()
    {
        return musicSource;
    }

    /// <summary>
    /// Get SFX AudioSource (for SettingsPanel to control volume)
    /// </summary>
    public AudioSource GetSFXSource()
    {
        return sfxSource;
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumes()
    {
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        musicSource.volume = musicVolume;
    }

    // ===== SCENE MANAGEMENT =====

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Auto switch music based on scene
        switch (scene.name)
        {
            case "MenuScene":
                PlayMenuMusic();
                break;

            case "GameScene":
                PlayGameMusic();
                break;

            case "LoginScene":
            case "LoadingScene":
            case "SelectCharacterScene":
                // These scenes have no music (LoginScene uses PopupMusic)
                StopMusic();
                break;

            default:
                Debug.Log($"AudioManager: Unknown scene - {scene.name}");
                break;
        }
    }
}
