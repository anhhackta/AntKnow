using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;
    public Slider musicSlider;
    public Image musicIcon;
    public Sprite musicIconLow;   // <=30%
    public Sprite musicIconHigh;  // >30%
    public Slider sfxSlider;
    public Toggle fullscreenToggle;
    public Dropdown resolutionDropdown;

    [Header("Audio (khuyến nghị dùng Mixer)")]
    public AudioMixer mixer;            // expose "MusicVol", "SFXVol"
    public string musicParam = "MusicVol";
    public string sfxParam = "SFXVol";
    [Header("Fallback nếu không có Mixer")]
    public List<AudioSource> musicSources = new();
    public List<AudioSource> sfxSources = new();

    // 4 độ phân giải cố định theo yêu cầu
    readonly Vector2Int[] fixedRes = new Vector2Int[]
    {
        new Vector2Int(1920,1080),
        new Vector2Int(1600, 900),
        new Vector2Int(1366, 768),
        new Vector2Int(1280, 720),
    };

    // PlayerPrefs keys
    const string K_Music = "SET_MUSIC";
    const string K_SFX   = "SET_SFX";
    const string K_FS    = "SET_FS";
    const string K_ResIx = "SET_RES_INDEX";

    void Awake()
    {
        BuildResOptions();
        WireEvents();
    }

    void OnEnable()
    {
        // Auto-populate AudioSources from AudioManager and PopupMusic
        AutoPopulateAudioSources();

        LoadPrefs(out float mv, out float sv, out bool fs, out int resIx);

        // Sync with AudioManager if exists
        if (AudioManager.Instance != null)
        {
            mv = AudioManager.Instance.GetMusicVolume();
            sv = AudioManager.Instance.GetSFXVolume();
        }

        // Nếu đang Windowed mà res không tồn tại, chọn gần nhất
        resIx = Mathf.Clamp(resIx, 0, fixedRes.Length - 1);
        ApplyAll(mv, sv, fs, resIx, applyToSystem:true, save:false);
        RefreshUI(mv, sv, fs, resIx);
    }

    /// <summary>
    /// Tự động tìm và gắn AudioSources từ AudioManager và PopupMusic
    /// </summary>
    void AutoPopulateAudioSources()
    {
        // Clear existing lists
        musicSources.Clear();
        sfxSources.Clear();

        // Find AudioManager
        if (AudioManager.Instance != null)
        {
            // Get AudioSources from AudioManager using public methods
            var musicSrc = AudioManager.Instance.GetMusicSource();
            var sfxSrc = AudioManager.Instance.GetSFXSource();

            if (musicSrc != null)
            {
                musicSources.Add(musicSrc);
                Debug.Log("SettingsPanel: Added AudioManager music source");
            }

            if (sfxSrc != null)
            {
                sfxSources.Add(sfxSrc);
                Debug.Log("SettingsPanel: Added AudioManager SFX source");
            }
        }

        // Find PopupMusic (LoginScene)
        var popupMusic = FindObjectOfType<MusicPopup>();
        if (popupMusic != null && popupMusic.audioSource != null)
        {
            musicSources.Add(popupMusic.audioSource);
            Debug.Log("SettingsPanel: Added PopupMusic source");
        }

        Debug.Log($"SettingsPanel: Total music sources = {musicSources.Count}, sfx sources = {sfxSources.Count}");
    }

    void WireEvents()
    {
        musicSlider.onValueChanged.AddListener(v =>
        {
            ApplyMusic(v);
            SaveFloat(K_Music, v);
            UpdateMusicIcon(v);

            // Update AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(v);
            }
        });
        sfxSlider.onValueChanged.AddListener(v =>
        {
            ApplySFX(v);
            SaveFloat(K_SFX, v);

            // Update AudioManager
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(v);
            }
        });
        fullscreenToggle.onValueChanged.AddListener(isOn =>
        {
            var ix = resolutionDropdown.value;
            ApplyDisplay(isOn, ix);
            PlayerPrefs.SetInt(K_FS, isOn ? 1 : 0);
            PlayerPrefs.Save();
        });
        resolutionDropdown.onValueChanged.AddListener(ix =>
        {
            var fs = fullscreenToggle.isOn;
            ApplyDisplay(fs, ix);
            PlayerPrefs.SetInt(K_ResIx, ix);
            PlayerPrefs.Save();
        });
    }

    void BuildResOptions()
    {
        resolutionDropdown.ClearOptions();
        var opts = new List<string>();
        foreach (var r in fixedRes) opts.Add($"{r.x} x {r.y}");
        resolutionDropdown.AddOptions(opts);
    }

    void RefreshUI(float mv, float sv, bool fs, int resIx)
    {
        musicSlider.SetValueWithoutNotify(mv);
        sfxSlider.SetValueWithoutNotify(sv);
        fullscreenToggle.SetIsOnWithoutNotify(fs);
        resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(resIx, 0, fixedRes.Length - 1));
        UpdateMusicIcon(mv);
    }

    void LoadPrefs(out float mv, out float sv, out bool fs, out int resIx)
    {
        mv = PlayerPrefs.GetFloat(K_Music, 1f);
        sv = PlayerPrefs.GetFloat(K_SFX, 1f);
        fs = PlayerPrefs.GetInt(K_FS, 1) == 1;
        resIx = PlayerPrefs.GetInt(K_ResIx, 0);
    }

    // ——— Apply ———
    void ApplyAll(float mv, float sv, bool fs, int resIx, bool applyToSystem, bool save)
    {
        ApplyMusic(mv);
        ApplySFX(sv);
        if (applyToSystem) ApplyDisplay(fs, resIx);
        if (save)
        {
            SaveFloat(K_Music, mv);
            SaveFloat(K_SFX, sv);
            PlayerPrefs.SetInt(K_FS, fs ? 1 : 0);
            PlayerPrefs.SetInt(K_ResIx, Mathf.Clamp(resIx, 0, fixedRes.Length - 1));
            PlayerPrefs.Save();
        }
    }

    void ApplyMusic(float v)
    {
        if (mixer) mixer.SetFloat(musicParam, Linear01ToDb(v));
        foreach (var a in musicSources) if (a) a.volume = v;
    }

    void ApplySFX(float v)
    {
        if (mixer) mixer.SetFloat(sfxParam, Linear01ToDb(v));
        foreach (var a in sfxSources) if (a) a.volume = v;
    }

    void ApplyDisplay(bool fullscreen, int resIndex)
    {
        resIndex = Mathf.Clamp(resIndex, 0, fixedRes.Length - 1);
        var r = fixedRes[resIndex];
        var mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(r.x, r.y, mode);
    }

    // ——— Helpers ———
    float Linear01ToDb(float v) => v <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(v)) * 20f;

    void UpdateMusicIcon(float v)
    {
        if (!musicIcon) return;
        musicIcon.sprite = (v <= 0.3f) ? musicIconLow : musicIconHigh;
    }

    void SaveFloat(string k, float v)
    {
        PlayerPrefs.SetFloat(k, Mathf.Clamp01(v));
        PlayerPrefs.Save();
    }

    // Button Close
    public void ClosePanel()
    {
        if (panelRoot) panelRoot.SetActive(false);
    }
}
