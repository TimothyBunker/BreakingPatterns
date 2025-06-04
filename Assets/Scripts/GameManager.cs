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
    
    [Header("New Systems")]
    private StatModifier statModifier;
    private FeedbackSystem feedbackSystem;
    private UIManager uiManager;
    private DialogueUIIntegration uiIntegration;


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
        Relationships = 50; // Start with some relationships to make it meaningful
        Suspicion = 0;

        // Store initial values to compare for sound triggers on first ApplyChoice
        lastProfit = Profit;
        lastRelationships = Relationships;
        lastSuspicion = Suspicion;
        
        // Initialize new systems
        if (statModifier == null)
            statModifier = GetComponent<StatModifier>() ?? gameObject.AddComponent<StatModifier>();
        if (feedbackSystem == null)
            feedbackSystem = GetComponent<FeedbackSystem>() ?? gameObject.AddComponent<FeedbackSystem>();
        
        // Create AudioManager if missing
        if (AudioManager.Instance == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
            Debug.Log("GameManager: Created AudioManager");
        }
            
        // Disable UI debugger for now to prevent errors
        // #if UNITY_EDITOR
        // var debugger = FindFirstObjectByType<UILayerDebugger>();
        // if (debugger == null)
        // {
        //     new GameObject("UILayerDebugger").AddComponent<UILayerDebugger>();
        // }
        // #endif
        
        // Add simple UI fix as the main UI system
        StartCoroutine(AddSimpleUIFix());

        UpdateUI(); // Update UI with initial values
    }
    
    System.Collections.IEnumerator AddSimpleUIFix()
    {
        yield return new WaitForSeconds(0.1f);
        
        // Always add simple UI fix as the main UI system
        var simpleUIFix = FindFirstObjectByType<SimpleUIFix>();
        if (simpleUIFix == null)
        {
            Debug.Log("GameManager: Creating SimpleUIFix");
            new GameObject("SimpleUIFix").AddComponent<SimpleUIFix>();
        }
    }


    // Legacy method for backwards compatibility
    public void ApplyChoice(int p, int r, int s)
    {
        var option = new DialogueOption();
        option.profit = p;
        option.relationships = r;
        option.suspicion = s;
        ApplyChoiceWithRNG(option);
    }
    
    // New method that uses the dynamic stat system
    public void ApplyChoiceWithRNG(DialogueOption option)
    {
        // Get dynamic stat changes
        var result = statModifier.ApplyStatChanges(option);
        
        // Apply the actual changes
        Profit = Mathf.Clamp(Profit + result.profitChange, 0, 999);
        Relationships = Mathf.Clamp(Relationships + result.relationshipChange, 0, 100);
        Suspicion = Mathf.Clamp(Suspicion + result.suspicionChange, 0, maxSuspicion);
        
        // Show feedback
        if (feedbackSystem != null)
        {
            Vector3 feedbackPos = Input.mousePosition;
            feedbackSystem.ShowStatChangeFeedback(result, feedbackPos);
        }

        // Play sounds based on changes (enhanced for critical results)
        if (AudioManager.Instance != null)
        {
            if (result.criticalType == CriticalType.Success)
            {
                AudioManager.Instance.PlayPositiveStatSound();
            }
            else if (result.criticalType == CriticalType.Failure)
            {
                AudioManager.Instance.PlayNegativeStatSound();
            }
            else
            {
                // Normal sounds
                if (result.profitChange > 0 || result.relationshipChange > 0)
                {
                    if (Profit > lastProfit || Relationships > lastRelationships)
                        AudioManager.Instance.PlayPositiveStatSound();
                }
                if (result.profitChange < 0 || result.relationshipChange < 0 || result.suspicionChange > 0)
                {
                    if (Suspicion > lastSuspicion || Profit < lastProfit || Relationships < lastRelationships)
                        AudioManager.Instance.PlayNegativeStatSound();
                }
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
        if (profitText != null) profitText.text = $"Profit: ${Profit}";
        else Debug.LogWarning("profitText is not assigned in GameManager.");

        if (relText != null) 
        {
            string relStatus = GetRelationshipStatus();
            relText.text = $"Relations: {Relationships} ({relStatus})";
            
            // Color code relationships
            if (Relationships >= 75)
                relText.color = Color.green;
            else if (Relationships >= 50)
                relText.color = Color.white;
            else if (Relationships >= 25)
                relText.color = Color.yellow;
            else
                relText.color = Color.red;
        }
        else Debug.LogWarning("relText is not assigned in GameManager.");

        if (suspText != null)
        {
            string sTxt = $"Suspicion: {Suspicion}/{maxSuspicion}";
            
            // Add warning if relationships are affecting suspicion
            if (Relationships > 75)
                sTxt += " <size=14><color=#4CAF50>(Protected)</color></size>";
            
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
    
    // Relationship-based mechanics
    public bool HasHighRelationships()
    {
        return Relationships >= 75;
    }
    
    public bool HasLowRelationships()
    {
        return Relationships <= 25;
    }
    
    public float GetRelationshipModifier()
    {
        // Returns a modifier from 0.5 to 1.5 based on relationships
        return 0.5f + (Relationships / 100f);
    }
    
    public string GetRelationshipStatus()
    {
        if (Relationships >= 80) return "Trusted Partner";
        if (Relationships >= 60) return "Reliable Associate";
        if (Relationships >= 40) return "Business Partner";
        if (Relationships >= 20) return "Uneasy Alliance";
        return "Hostile";
    }
}
