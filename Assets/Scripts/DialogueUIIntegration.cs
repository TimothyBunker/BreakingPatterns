using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogueUIIntegration : MonoBehaviour
{
    private DialogueManager dialogueManager;
    private DialogueUI modernUI;
    private UIManager uiManager;
    
    [Header("Override Settings")]
    [SerializeField] private bool overrideExistingUI = true;
    [SerializeField] private bool hideOriginalElements = true;
    
    void Start()
    {
        // Find components
        dialogueManager = GetComponent<DialogueManager>() ?? FindFirstObjectByType<DialogueManager>();
        modernUI = FindFirstObjectByType<DialogueUI>();
        uiManager = FindFirstObjectByType<UIManager>();
        
        if (modernUI == null)
        {
            // Create modern UI if it doesn't exist
            GameObject uiObj = new GameObject("ModernDialogueUI");
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
                uiObj.transform.SetParent(canvas.transform, false);
            modernUI = uiObj.AddComponent<DialogueUI>();
        }
        
        if (overrideExistingUI && dialogueManager != null)
        {
            OverrideDialogueManager();
        }
        
        // Force initial update
        StartCoroutine(ForceInitialUpdate());
    }
    
    System.Collections.IEnumerator ForceInitialUpdate()
    {
        yield return new WaitForSeconds(0.1f);
        
        // Try to get initial state from DialogueManager
        if (dialogueManager != null)
        {
            UpdateVisualsFromDialogueManager();
        }
    }
    
    void OverrideDialogueManager()
    {
        // Hide original UI elements
        if (hideOriginalElements)
        {
            if (dialogueManager.bodyLabel != null)
                dialogueManager.bodyLabel.gameObject.SetActive(false);
            if (dialogueManager.bgImage != null)
                dialogueManager.bgImage.gameObject.SetActive(false);
            if (dialogueManager.charLeftImage != null)
                dialogueManager.charLeftImage.gameObject.SetActive(false);
            if (dialogueManager.charRightImage != null)
                dialogueManager.charRightImage.gameObject.SetActive(false);
        }
        
        // Hook into DialogueManager's update cycle
        StartCoroutine(InterceptDialogueUpdates());
    }
    
    System.Collections.IEnumerator InterceptDialogueUpdates()
    {
        while (true)
        {
            yield return null; // Wait one frame
            
            if (dialogueManager != null && dialogueManager.bodyLabel != null)
            {
                // Intercept the text that would be displayed
                string fullText = dialogueManager.bodyLabel.text;
                if (!string.IsNullOrEmpty(fullText))
                {
                    ParseAndDisplayModern(fullText);
                }
            }
        }
    }
    
    void ParseAndDisplayModern(string fullText)
    {
        Debug.Log($"DialogueUIIntegration: Parsing text with length {fullText.Length}");
        
        // Parse the dialogue text
        string[] lines = fullText.Split('\n');
        bool isEvent = false;
        string bodyText = "";
        List<DialogueOption> options = new List<DialogueOption>();
        int selectedIndex = -1;
        string speakerName = "";
        
        int bodyEndIndex = -1;
        
        // Find where options start
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            
            // Check for event marker
            if (line.Contains("[EVENT]"))
            {
                isEvent = true;
                continue;
            }
            
            // Check if this is an option line
            if (line.Length > 3 && (line[0] == '>' || line.StartsWith("  ")) && 
                (line[2] == '1' || line[2] == '2' || line[2] == '3' || line[3] == '1' || line[3] == '2' || line[3] == '3'))
            {
                bodyEndIndex = i;
                break;
            }
        }
        
        // Extract body text
        if (bodyEndIndex > 0)
        {
            for (int i = 0; i < bodyEndIndex; i++)
            {
                if (!lines[i].Contains("[EVENT]"))
                {
                    bodyText += lines[i] + "\n";
                }
            }
        }
        else
        {
            bodyText = fullText; // No options found
        }
        
        // Extract speaker name from body text
        if (bodyText.Contains(":"))
        {
            int colonIndex = bodyText.IndexOf(':');
            int newlineBeforeColon = bodyText.LastIndexOf('\n', colonIndex);
            if (newlineBeforeColon == -1) newlineBeforeColon = 0;
            
            string potentialSpeaker = bodyText.Substring(newlineBeforeColon, colonIndex - newlineBeforeColon).Trim();
            if (potentialSpeaker.Length < 20 && !potentialSpeaker.Contains(" "))
            {
                speakerName = potentialSpeaker;
            }
        }
        
        // Parse options
        for (int i = bodyEndIndex; i >= 0 && i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            // Check if this line is selected
            bool isSelected = line.TrimStart().StartsWith(">");
            if (isSelected) selectedIndex = options.Count;
            
            // Extract option text
            string optionLine = line.Trim();
            if (optionLine.StartsWith(">")) optionLine = optionLine.Substring(1).TrimStart();
            
            // Find the option number
            int dotIndex = optionLine.IndexOf('.');
            if (dotIndex > 0 && dotIndex < 3)
            {
                string optionText = optionLine.Substring(dotIndex + 1).Trim();
                
                // Remove stat display for clean text
                int bracketIndex = optionText.LastIndexOf('[');
                if (bracketIndex > 0)
                {
                    optionText = optionText.Substring(0, bracketIndex).Trim();
                }
                
                // Create dialogue option (we'll use dummy stats for now)
                var option = new DialogueOption
                {
                    text = optionText,
                    profit = 0,
                    relationships = 0,
                    suspicion = 0,
                    nextNode = -1
                };
                
                // Try to extract stats from the line
                ExtractStatsFromLine(line, option);
                
                options.Add(option);
            }
        }
        
        // Update modern UI
        if (modernUI != null)
        {
            modernUI.DisplayDialogue(speakerName, bodyText.Trim(), options, selectedIndex);
            
            // Update background and characters
            UpdateVisualsFromDialogueManager();
        }
    }
    
    void ExtractStatsFromLine(string line, DialogueOption option)
    {
        // Look for stat display pattern [+X|-X|+X]
        int lastBracket = line.LastIndexOf('[');
        if (lastBracket > 0 && line.EndsWith("]"))
        {
            string statString = line.Substring(lastBracket + 1, line.Length - lastBracket - 2);
            string[] stats = statString.Split('|');
            
            if (stats.Length >= 3)
            {
                // Parse profit
                option.profit = ParseStatValue(stats[0]);
                // Parse relationships
                option.relationships = ParseStatValue(stats[1]);
                // Parse suspicion
                option.suspicion = ParseStatValue(stats[2]);
            }
        }
    }
    
    int ParseStatValue(string statStr)
    {
        // Remove color tags
        string cleaned = System.Text.RegularExpressions.Regex.Replace(statStr, "<.*?>", "");
        cleaned = cleaned.Trim();
        
        // Handle ranges (e.g., "+10~+20")
        if (cleaned.Contains("~"))
        {
            string[] range = cleaned.Split('~');
            if (range.Length == 2)
            {
                int min = ParseSingleValue(range[0]);
                int max = ParseSingleValue(range[1]);
                return (min + max) / 2; // Return average
            }
        }
        
        return ParseSingleValue(cleaned);
    }
    
    int ParseSingleValue(string val)
    {
        val = val.Replace("+", "").Trim();
        if (int.TryParse(val, out int result))
        {
            return result;
        }
        return 0;
    }
    
    void UpdateVisualsFromDialogueManager()
    {
        // Update background
        if (dialogueManager.bgImage != null && dialogueManager.bgImage.sprite != null)
        {
            modernUI.SetBackground(dialogueManager.bgImage.sprite);
        }
        
        // Update characters
        if (dialogueManager.charLeftImage != null && dialogueManager.charLeftImage.sprite != null)
        {
            modernUI.SetCharacter(dialogueManager.charLeftImage.sprite, true);
        }
        else
        {
            modernUI.SetCharacter(null, true); // Hide left character
        }
        
        if (dialogueManager.charRightImage != null && dialogueManager.charRightImage.sprite != null)
        {
            modernUI.SetCharacter(dialogueManager.charRightImage.sprite, false);
        }
        else
        {
            modernUI.SetCharacter(null, false); // Hide right character
        }
    }
}