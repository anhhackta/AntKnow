using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Global Settings Manager - Singleton xuyên suốt game
/// Quản lý Settings Panel và đồng bộ volume cho tất cả scenes
/// Sử dụng SettingsPanel hiện có (đã có UI đẹp)
/// </summary>
public class GlobalSettingsManager : MonoBehaviour
{
    public static GlobalSettingsManager Instance { get; private set; }

    [Header("Settings Panel Component")]
    [SerializeField] private SettingsPanel settingsPanel;

    [Header("Settings Button")]
    [SerializeField] private Button buttonOpenSettings;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSettings()
    {
        // Setup button listener
        if (buttonOpenSettings != null)
        {
            buttonOpenSettings.onClick.AddListener(OpenSettings);
        }

        // Sync volume with PopupMusic on start
        SyncPopupMusicVolume();

        Debug.Log("GlobalSettingsManager initialized");
    }

    // ===== SETTINGS PANEL CONTROL =====

    public void OpenSettings()
    {
        if (settingsPanel != null && settingsPanel.panelRoot != null)
        {
            settingsPanel.panelRoot.SetActive(true);
        }

        // Play click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Get current music volume from PlayerPrefs
    /// </summary>
    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("SET_MUSIC", 1f);
    }

    /// <summary>
    /// Get current SFX volume from PlayerPrefs
    /// </summary>
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SET_SFX", 1f);
    }

    // ===== SYNC POPUP MUSIC =====

    private void SyncPopupMusicVolume()
    {
        // Find PopupMusic in scene (LoginScene)
        var popupMusic = FindObjectOfType<MusicPopup>();
        if (popupMusic != null && popupMusic.audioSource != null)
        {
            // Get volume from SettingsPanel PlayerPrefs
            float musicVolume = GetMusicVolume();
            popupMusic.audioSource.volume = musicVolume;
            Debug.Log($"Synced PopupMusic volume: {musicVolume * 100}%");
        }
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
        // Sync volume with PopupMusic when loading LoginScene
        if (scene.name == "LoginScene")
        {
            SyncPopupMusicVolume();
        }

        Debug.Log($"GlobalSettingsManager: Scene loaded - {scene.name}");
    }
}

