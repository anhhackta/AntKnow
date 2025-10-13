using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script để gọi AudioManager từ UI buttons
/// Attach vào GameObject có Button component
/// </summary>
public class AudioHelper : MonoBehaviour
{
    [Header("Auto Play on Button Click")]
    [SerializeField] private bool playClickSoundOnButton = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        
        if (button != null && playClickSoundOnButton)
        {
            button.onClick.AddListener(PlayButtonClickSound);
        }
    }

    // ===== PUBLIC METHODS (Call from Unity Events) =====

    public void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    public void PlayNotificationSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNotification();
        }
    }

    public void PlayStartSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayStart();
        }
    }

    public void PlayBounceSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBounce();
        }
    }

    public void PlayProfitSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayProfit();
        }
    }

    public void PlayLossSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLoss();
        }
    }

    private void OnDestroy()
    {
        if (button != null && playClickSoundOnButton)
        {
            button.onClick.RemoveListener(PlayButtonClickSound);
        }
    }
}

