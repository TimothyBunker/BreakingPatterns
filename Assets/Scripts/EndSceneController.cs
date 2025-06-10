using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndSceneController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text endingTitleText;
    public TMP_Text endingDescriptionText;
    public TMP_Text statsText;
    public TMP_Text epilogueText;
    public Image backgroundImage;
    public Image characterImage;
    
    [Header("Buttons")]
    public Button restartButton;
    public Button quitButton;
    
    [Header("Resources")]
    public string backgroundsPath = "Sprites/Backgrounds/";
    public string charactersPath = "Sprites/Characters/";
    
    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found! Loading from EndScene directly?");
            ShowDefaultEnding();
            return;
        }
        
        StartCoroutine(ShowEndingSequence());
        
        // Setup button listeners
        if (restartButton) restartButton.onClick.AddListener(RestartGame);
        if (quitButton) quitButton.onClick.AddListener(QuitGame);
    }
    
    IEnumerator ShowEndingSequence()
    {
        // Fade in or initial setup
        yield return new WaitForSeconds(0.5f);
        
        // Determine and show ending based on stats
        ShowEnding();
    }
    
    void ShowEnding()
    {
        var gm = GameManager.Instance;
        int profit = gm.Profit;
        int relationships = gm.Relationships;
        int suspicion = gm.Suspicion;
        
        // Determine ending type
        EndingType ending = DetermineEnding(profit, relationships, suspicion);
        
        // Display appropriate ending
        switch (ending)
        {
            case EndingType.Kingpin:
                ShowKingpinEnding(profit, relationships, suspicion);
                break;
            case EndingType.FamilyMan:
                ShowFamilyManEnding(profit, relationships, suspicion);
                break;
            case EndingType.Fugitive:
                ShowFugitiveEnding(profit, relationships, suspicion);
                break;
            case EndingType.Captured:
                ShowCapturedEnding(profit, relationships, suspicion);
                break;
            case EndingType.Betrayed:
                ShowBetrayedEnding(profit, relationships, suspicion);
                break;
            case EndingType.Survivor:
                ShowSurvivorEnding(profit, relationships, suspicion);
                break;
            default:
                ShowDefaultEnding();
                break;
        }
        
        // Show final stats
        if (statsText)
        {
            statsText.text = $"Final Stats:\nProfit: ${profit}k\nRelationships: {GetRelationshipStatus(relationships)}\nSuspicion: {suspicion}%";
            PositionStatsRight();
        }
    }
    
    EndingType DetermineEnding(int profit, int relationships, int suspicion)
    {
        // High suspicion always leads to capture
        if (suspicion >= 100)
            return EndingType.Captured;
        
        // High profit + good relationships = Kingpin
        if (profit >= 70 && relationships >= 60)
            return EndingType.Kingpin;
        
        // Low profit but high relationships = Family Man
        if (profit < 30 && relationships >= 70)
            return EndingType.FamilyMan;
        
        // High profit but low relationships = Fugitive
        if (profit >= 60 && relationships < 30)
            return EndingType.Fugitive;
        
        // Low relationships overall = Betrayed
        if (relationships < 20)
            return EndingType.Betrayed;
        
        // Default survivor ending
        return EndingType.Survivor;
    }
    
    void ShowKingpinEnding(int profit, int relationships, int suspicion)
    {
        SetBackground("bb_guslospollos");
        SetCharacter("Gus_front");
        PositionCharacterLeft();
        
        if (endingTitleText) endingTitleText.text = "THE KINGPIN";
        if (endingDescriptionText) endingDescriptionText.text = "You built an empire and kept your allies close.";
        
        if (epilogueText)
        {
            epilogueText.text = $"With ${profit}k in profit and the loyalty of your partners, you've become the undisputed king of the Southwest drug trade. " +
                               $"Your {GetRelationshipStatus(relationships)} relationships ensured your operation ran smoothly. " +
                               $"The DEA suspects nothing, and your empire continues to grow.\n\n" +
                               $"You are Heisenberg. You are the one who knocks.";
        }
    }
    
    void ShowFamilyManEnding(int profit, int relationships, int suspicion)
    {
        SetBackground("bb_walthouse");
        SetCharacter("WalterWhite_front");
        PositionCharacterLeft();
        
        if (endingTitleText) endingTitleText.text = "THE FAMILY MAN";
        if (endingDescriptionText) endingDescriptionText.text = "You chose family over fortune.";
        
        if (epilogueText)
        {
            epilogueText.text = $"With only ${profit}k saved, money remains tight. But your family stands by you. " +
                               $"Your {GetRelationshipStatus(relationships)} relationships mean everything. " +
                               $"You may not be rich, but you're not alone in your final days.\n\n" +
                               $"Sometimes, that's worth more than all the money in the world.";
        }
    }
    
    void ShowFugitiveEnding(int profit, int relationships, int suspicion)
    {
        SetBackground("bb_desertnight");
        SetCharacter("");
        
        if (endingTitleText) endingTitleText.text = "THE FUGITIVE";
        if (endingDescriptionText) endingDescriptionText.text = "Rich but alone, you vanished into the night.";
        
        if (epilogueText)
        {
            epilogueText.text = $"With ${profit}k in cash hidden away, you have the money but no one to share it with. " +
                               $"Your {GetRelationshipStatus(relationships)} relationships meant burning every bridge. " +
                               $"Now you live under assumed names, always looking over your shoulder.\n\n" +
                               $"You won the game, but lost everything else.";
        }
    }
    
    void ShowCapturedEnding(int profit, int relationships, int suspicion)
    {
        SetBackground("bb_townstreet");
        SetCharacter("Hank_front");
        PositionCharacterLeft();
        
        if (endingTitleText) endingTitleText.text = "CAPTURED";
        if (endingDescriptionText) endingDescriptionText.text = "The DEA finally caught up with Heisenberg.";
        
        if (epilogueText)
        {
            epilogueText.text = $"With {suspicion}% suspicion, it was only a matter of time. " +
                               $"Your ${profit}k fortune is seized. Your {GetRelationshipStatus(relationships)} relationships couldn't save you. " +
                               $"As Hank reads you your rights, you realize this is how it ends.\n\n" +
                               $"The great Heisenberg, in handcuffs. Game over.";
        }
    }
    
    void ShowBetrayedEnding(int profit, int relationships, int suspicion)
    {
        SetBackground("bb_parkinglotb");
        SetCharacter("Jesse_front");
        PositionCharacterLeft();
        
        if (endingTitleText) endingTitleText.text = "BETRAYED";
        if (endingDescriptionText) endingDescriptionText.text = "Your partners turned on you.";
        
        if (epilogueText)
        {
            epilogueText.text = $"With relationships at rock bottom ({relationships}%), it was inevitable. " +
                               $"Jesse sold you out to the DEA. Mike refused to help. Even Saul abandoned you. " +
                               $"Your ${profit}k means nothing when everyone wants you dead.\n\n" +
                               $"You should have treated people better, Mr. White.";
        }
    }
    
    void ShowSurvivorEnding(int profit, int relationships, int suspicion)
    {
        SetBackground("bb_waltnjesselair");
        SetCharacter("WalterWhite_front");
        PositionCharacterLeft();
        
        if (endingTitleText) endingTitleText.text = "THE SURVIVOR";
        if (endingDescriptionText) endingDescriptionText.text = "You made it through, barely.";
        
        if (epilogueText)
        {
            epilogueText.text = $"With ${profit}k saved and {GetRelationshipStatus(relationships)} relationships, you survived the game. " +
                               $"Not rich, not powerful, but alive. The {suspicion}% suspicion level kept you on edge. " +
                               $"You're no kingpin, but you're no prisoner either.\n\n" +
                               $"Sometimes, surviving is victory enough.";
        }
    }
    
    void ShowDefaultEnding()
    {
        if (endingTitleText) endingTitleText.text = "THE END";
        if (endingDescriptionText) endingDescriptionText.text = "Your journey has concluded.";
        if (epilogueText) epilogueText.text = "Every choice led you here.";
        if (statsText) statsText.text = "";
    }
    
    string GetRelationshipStatus(int relationships)
    {
        if (relationships >= 80) return "Trusted Partner";
        if (relationships >= 60) return "Reliable Associate";
        if (relationships >= 40) return "Business Partner";
        if (relationships >= 20) return "Uneasy Alliance";
        return "Hostile";
    }
    
    void SetBackground(string bgName)
    {
        if (backgroundImage && !string.IsNullOrEmpty(bgName))
        {
            Sprite bg = Resources.Load<Sprite>(backgroundsPath + bgName);
            if (bg) backgroundImage.sprite = bg;
        }
    }
    
    void SetCharacter(string charName)
    {
        if (characterImage)
        {
            if (string.IsNullOrEmpty(charName))
            {
                characterImage.enabled = false;
            }
            else
            {
                Sprite character = Resources.Load<Sprite>(charactersPath + charName);
                if (character)
                {
                    characterImage.sprite = character;
                    characterImage.enabled = true;
                }
            }
        }
    }
    
    public void RestartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResetGame();
        
        SceneManager.LoadScene(0);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
    
    void PositionCharacterLeft()
    {
        if (characterImage && characterImage.rectTransform)
        {
            characterImage.rectTransform.anchoredPosition = new Vector2(-600, 0);
        }
    }
    
    void PositionStatsRight()
    {
        if (statsText && statsText.rectTransform)
        {
            statsText.rectTransform.anchoredPosition = new Vector2(700, 0);
        }
    }
    
    enum EndingType
    {
        Kingpin,    // High profit + good relationships
        FamilyMan,  // Low profit + high relationships  
        Fugitive,   // High profit + poor relationships
        Captured,   // High suspicion (100+)
        Betrayed,   // Very low relationships
        Survivor    // Default/balanced
    }
}