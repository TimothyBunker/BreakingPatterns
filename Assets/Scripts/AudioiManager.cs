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

        // Basic setup validation
        if (musicSource == null)
        {
            Debug.LogError("AudioManager: Music Source is not assigned!");
        }
        if (sfxSource == null)
        {
            Debug.LogError("AudioManager: SFX Source is not assigned!");
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
