using UnityEngine;
using System.Collections.Generic; // If you want to manage a list of sounds by name

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource; // For background music
    public AudioSource sfxSource;   // For sound effects

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip uiNavigateSound;     // e.g., click-sound-help-other.mp3 for up/down arrow
    public AudioClip uiSelectSound;       // e.g., default-choice.mp3 for making a choice
    public AudioClip positiveStatSound;   // e.g., bonus-point.mp3
    public AudioClip negativeStatSound;   // e.g., bad-or-error-choice.mp3
    // Add more clips as needed

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep AudioManager across scenes

        // Auto-create audio sources if not assigned
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f;
            Debug.Log("AudioManager: Created MusicSource");
        }
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.7f;
            Debug.Log("AudioManager: Created SFXSource");
        }
        
        // Try to auto-load audio clips from Resources
        if (backgroundMusic == null)
        {
            backgroundMusic = Resources.Load<AudioClip>("Audio/background-music");
        }
        if (uiNavigateSound == null)
        {
            uiNavigateSound = Resources.Load<AudioClip>("Audio/click-sound-help-other");
        }
        if (uiSelectSound == null)
        {
            uiSelectSound = Resources.Load<AudioClip>("Audio/default-choice");
        }
        if (positiveStatSound == null)
        {
            positiveStatSound = Resources.Load<AudioClip>("Audio/bonus-point");
        }
        if (negativeStatSound == null)
        {
            negativeStatSound = Resources.Load<AudioClip>("Audio/bad-or-error-choice");
        }
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioManager: Cannot play background music. Source or Clip missing.");
        }
    }

    // Play a one-shot sound effect
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"AudioManager: Cannot play SFX. Source missing or clip is null.");
        }
    }

    // Specific sound event methods (examples)
    public void PlayUINavigationSound()
    {
        PlaySFX(uiNavigateSound);
    }

    public void PlayUISelectSound()
    {
        PlaySFX(uiSelectSound);
    }

    public void PlayPositiveStatSound()
    {
        PlaySFX(positiveStatSound);
    }

    public void PlayNegativeStatSound()
    {
        PlaySFX(negativeStatSound);
    }
}
