using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip uiNavigateSound;
    public AudioClip uiSelectSound;
    public AudioClip positiveStatSound;
    public AudioClip negativeStatSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-create audio sources
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.7f;
        }

        // Auto-load audio clips
        LoadAudioClips();
        
        Debug.Log("AudioManager: Initialized successfully");
    }

    void LoadAudioClips()
    {
        if (backgroundMusic == null)
            backgroundMusic = Resources.Load<AudioClip>("Audio/background-music");
        if (uiNavigateSound == null)
            uiNavigateSound = Resources.Load<AudioClip>("Audio/click-sound-help-other");
        if (uiSelectSound == null)
            uiSelectSound = Resources.Load<AudioClip>("Audio/default-choice");
        if (positiveStatSound == null)
            positiveStatSound = Resources.Load<AudioClip>("Audio/bonus-point");
        if (negativeStatSound == null)
            negativeStatSound = Resources.Load<AudioClip>("Audio/bad-or-error-choice");
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
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

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