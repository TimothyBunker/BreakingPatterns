using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TMP_Text profitText, relText, suspText;

    public int Profit { get; private set; }
    public int Relationships { get; private set; }
    public int Suspicion { get; private set; }
    public int maxSuspicion = 100;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ApplyChoice(int p, int r, int s)
    {
        Profit += p;
        Relationships += r;
        Suspicion = Mathf.Clamp(Suspicion + s, 0, maxSuspicion);
        UpdateUI();

        if (Suspicion >= maxSuspicion)
            LoseGame();
    }

    void UpdateUI()
    {
        profitText.text = $"Profit: {Profit}";
        relText.text = $"Relations: {Relationships}";
        string sTxt = $"Suspicion: {Suspicion}/{maxSuspicion}";
        suspText.text = sTxt;

        // flash red if ≥ 80 %
        bool danger = Suspicion >= maxSuspicion * 0.8f;
        suspText.color = danger ? Color.red : Color.white;
    }


    void LoseGame()
    {
        Debug.Log("Busted! Suspicion maxed out.");
        // TODO: show Game-Over panel or load “DeathScene”
    }

    public void ResetGame()
    {
        Profit = 0;
        Relationships = 0;
        Suspicion = 0;
        UpdateUI();
    }
}
