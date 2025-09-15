using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicPopup : MonoBehaviour
{
    [Header("Refs")]
    public AudioSource audioSource;
    public TMP_Text titleText;
    public Button playStopButton;
    public Button nextButton;
    public Image playStopImage;     // Image component của nút
    public Sprite playSprite;       // Sprite hiển thị khi đang STOP
    public Sprite stopSprite;       // Sprite hiển thị khi đang PLAY

    [Header("Playlist")]
    public List<AudioClip> playlist = new List<AudioClip>();

    private int currentIndex = 0;

    void Start()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        // Khi mở scene chạy luôn bài đầu
        if (playlist.Count > 0)
        {
            currentIndex = 0;
            PlayTrack(currentIndex);
        }

        playStopButton.onClick.AddListener(TogglePlayStop);
        nextButton.onClick.AddListener(NextTrack);
    }

    void TogglePlayStop()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            UpdatePlayStopSprite(false);
        }
        else
        {
            audioSource.Play();
            UpdatePlayStopSprite(true);
        }
    }

    void NextTrack()
    {
        if (playlist.Count == 0) return;
        currentIndex = (currentIndex + 1) % playlist.Count;
        PlayTrack(currentIndex);
    }

    void PlayTrack(int index)
    {
        audioSource.clip = playlist[index];
        audioSource.time = 0f;
        audioSource.Play();

        // Update UI
        if (titleText) titleText.text = audioSource.clip.name;
        UpdatePlayStopSprite(true);
    }

    void UpdatePlayStopSprite(bool isPlaying)
    {
        if (!playStopImage) return;
        playStopImage.sprite = isPlaying ? stopSprite : playSprite;
    }
}
