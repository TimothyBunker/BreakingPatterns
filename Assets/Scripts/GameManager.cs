using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Required for LoseGame if it loads a scene

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text profitText;
    public TMP_Text relText;
    public TMP_Text suspText;

    [Header("Game Stats")]
    public int Profit { get; private set; }
    public int Relationships { get; private set; }
    public int Suspicion { get; private set; }
    public int maxSuspicion = 100;

    private int lastProfit, lastRelationships, lastSuspicion;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeStats();
    }

    void InitializeStats()
    {
        // Initialize with starting values or load from save
        Profit = 0;
        Relationships = 0; // Or a starting value
        Suspicion = 0;

        // Store initial values to compare for sound triggers on first ApplyChoice
        lastProfit = Profit;
        lastRelationships = Relationships;
        lastSuspicion = Suspicion;

        UpdateUI(); // Update UI with initial values
    }


    public void ApplyChoice(int p, int r, int s)
    {
        Profit += p;
        Relationships += r;
        Suspicion = Mathf.Clamp(Suspicion + s, 0, maxSuspicion);

        // Play sounds based on changes
        // Check if AudioManager exists to prevent errors if it's not in the scene
        if (AudioManager.Instance != null)
        {
            if (p > 0 || r > 0) // Any positive gain
            {
                if (Profit > lastProfit || Relationships > lastRelationships)
                    AudioManager.Instance.PlayPositiveStatSound();
            }
            if (p < 0 || r < 0 || s > 0) // Any negative impact or suspicion increase
            {
                // Check specifically if suspicion increased or other stats decreased
                if (Suspicion > lastSuspicion || Profit < lastProfit || Relationships < lastRelationships)
                    AudioManager.Instance.PlayNegativeStatSound();
            }
        }

        // Update last known values
        lastProfit = Profit;
        lastRelationships = Relationships;
        lastSuspicion = Suspicion;

        UpdateUI();

        if (Suspicion >= maxSuspicion)
        {
            LoseGame();
        }
    }

    void UpdateUI()
    {
        if (profitText != null) profitText.text = $"Profit: {Profit}";
        else Debug.LogWarning("profitText is not assigned in GameManager.");

        if (relText != null) relText.text = $"Relations: {Relationships}";
        else Debug.LogWarning("relText is not assigned in GameManager.");

        if (suspText != null)
        {
            string sTxt = $"Suspicion: {Suspicion}/{maxSuspicion}";
            suspText.text = sTxt;

            bool danger = Suspicion >= maxSuspicion * 0.8f && Suspicion < maxSuspicion;
            bool busted = Suspicion >= maxSuspicion;

            if (busted)
            {
                suspText.color = Color.magenta; // Or some other "busted" color
            }
            else if (danger)
            {
                suspText.color = Color.red;
            }
            else
            {
                suspText.color = Color.white;
            }
        }
        else Debug.LogWarning("suspText is not assigned in GameManager.");
    }


    void LoseGame()
    {
        Debug.Log("Busted! Suspicion maxed out.");
        // Here you might want to play a specific "game over" sound
        // AudioManager.Instance?.PlaySFX(AudioManager.Instance.gameOverSound); // If you add a gameOverSound
        SceneManager.LoadScene("EndScene"); // Ensure "EndScene" is in Build Settings
    }

    public void ResetGame()
    {
        InitializeStats(); // Re-initialize stats to their starting values
        // If you have other game state to reset (like dialogue progress), do it here.
        // Potentially, DialogueManager might need a Reset method too if it holds state
        // that needs clearing for a new game (e.g., if nodes list could change).
    }
}
