using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)] // Execute before other scripts
public class SimpleUIFix : MonoBehaviour
{
    [Header("References")]
    private DialogueManager dialogueManager;
    private Canvas mainCanvas;
    
    [Header("UI Elements")]
    private Image backgroundImage;
    private Image leftCharacter;
    private Image rightCharacter;
    private TextMeshProUGUI dialogueText;
    private RectTransform optionsPanel;
    private List<Button> optionButtons = new List<Button>();
    
    [Header("Current State")]
    private int currentSelection = 0;
    private List<DialogueOption> currentOptions;
    
    void Awake()
    {
        // Execute early to set up DialogueManager before it initializes
        SetupDialogueManagerReferences();
    }
    
    void Start()
    {
        // Find DialogueManager
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogError("SimpleUIFix: No DialogueManager found!");
            return;
        }
        
        // Create our UI
        CreateUI();
        
        // Take over DialogueManager
        TakeOverDialogueManager();
        
        // Wait a frame then show dialogue
        StartCoroutine(ShowDialogueAfterFrame());
    }
    
    System.Collections.IEnumerator ShowDialogueAfterFrame()
    {
        yield return null; // Wait one frame
        ShowCurrentDialogue();
    }
    
    void SetupDialogueManagerReferences()
    {
        // Find ALL DialogueManagers and set up their references
        DialogueManager[] allDialogueManagers = FindObjectsByType<DialogueManager>(FindObjectsSortMode.None);
        foreach (var dm in allDialogueManagers)
        {
            SetupSingleDialogueManager(dm);
        }
        
        // Set the first one as our main reference
        if (allDialogueManagers.Length > 0)
        {
            dialogueManager = allDialogueManagers[0];
        }
        
        Debug.Log($"SimpleUIFix: Set up {allDialogueManagers.Length} DialogueManager(s)");
    }
    
    void SetupSingleDialogueManager(DialogueManager dm)
    {
        // Create a hidden canvas for this DialogueManager's UI references
        GameObject dummyCanvas = new GameObject($"DummyCanvas_{dm.name}");
        Canvas canvas = dummyCanvas.AddComponent<Canvas>();
        canvas.enabled = false; // Hide it
        
        // Create dummy UI elements for this specific DialogueManager
        if (dm.bodyLabel == null)
        {
            GameObject textObj = new GameObject("DummyText");
            textObj.transform.SetParent(dummyCanvas.transform, false);
            dm.bodyLabel = textObj.AddComponent<TextMeshProUGUI>();
        }
        
        if (dm.bgImage == null)
        {
            GameObject bgObj = new GameObject("DummyBG");
            bgObj.transform.SetParent(dummyCanvas.transform, false);
            dm.bgImage = bgObj.AddComponent<Image>();
        }
        
        if (dm.charLeftImage == null)
        {
            GameObject charObj = new GameObject("DummyCharLeft");
            charObj.transform.SetParent(dummyCanvas.transform, false);
            dm.charLeftImage = charObj.AddComponent<Image>();
        }
        
        if (dm.charRightImage == null)
        {
            GameObject charObj = new GameObject("DummyCharRight");
            charObj.transform.SetParent(dummyCanvas.transform, false);
            dm.charRightImage = charObj.AddComponent<Image>();
        }
        
        Debug.Log($"SimpleUIFix: Created dummy UI for DialogueManager '{dm.name}'");
    }
    
    void CreateUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("FixedCanvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Ensure EventSystem exists
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
        
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        backgroundImage = bgObj.AddComponent<Image>();
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Characters
        CreateCharacter(ref leftCharacter, "LeftCharacter", new Vector2(-0.1f, 0f), new Vector2(0.3f, 0.85f));
        CreateCharacter(ref rightCharacter, "RightCharacter", new Vector2(0.7f, 0f), new Vector2(1.1f, 0.85f));
        
        // Dialogue text
        GameObject dialogueObj = new GameObject("DialogueText");
        dialogueObj.transform.SetParent(canvasObj.transform, false);
        dialogueText = dialogueObj.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 20;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform dialogueRect = dialogueObj.GetComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0.2f, 0.6f);
        dialogueRect.anchorMax = new Vector2(0.8f, 0.9f);
        dialogueRect.offsetMin = new Vector2(20, 20);
        dialogueRect.offsetMax = new Vector2(-20, -20);
        
        // Options panel
        GameObject optionsPanelObj = new GameObject("OptionsPanel");
        optionsPanelObj.transform.SetParent(canvasObj.transform, false);
        optionsPanel = optionsPanelObj.AddComponent<RectTransform>();
        optionsPanel.anchorMin = new Vector2(0.2f, 0.1f);
        optionsPanel.anchorMax = new Vector2(0.8f, 0.6f);
        
        VerticalLayoutGroup layout = optionsPanelObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        
        Debug.Log("SimpleUIFix: UI created successfully");
    }
    
    void CreateCharacter(ref Image character, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject charObj = new GameObject(name);
        charObj.transform.SetParent(mainCanvas.transform, false);
        character = charObj.AddComponent<Image>();
        character.preserveAspect = true;
        
        RectTransform charRect = charObj.GetComponent<RectTransform>();
        charRect.anchorMin = anchorMin;
        charRect.anchorMax = anchorMax;
        charRect.sizeDelta = Vector2.zero;
    }
    
    void TakeOverDialogueManager()
    {
        // Disable DialogueManager's Update to prevent interference
        if (dialogueManager != null)
        {
            dialogueManager.enabled = false;
        }
        
        Debug.Log("SimpleUIFix: Took over DialogueManager");
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        if (currentOptions == null || currentOptions.Count == 0) return;
        
        // Navigation
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection = (currentSelection - 1 + currentOptions.Count) % currentOptions.Count;
            UpdateSelection();
            PlaySound("ui-navigate");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection = (currentSelection + 1) % currentOptions.Count;
            UpdateSelection();
            PlaySound("ui-navigate");
        }
        
        // Selection
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SelectOption(currentSelection);
        }
        
        // Number keys
        for (int i = 0; i < Mathf.Min(currentOptions.Count, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectOption(i);
            }
        }
    }
    
    void SelectOption(int index)
    {
        if (index < 0 || index >= currentOptions.Count) return;
        
        PlaySound("ui-select");
        
        // Apply the choice using GameManager's RNG system
        var option = currentOptions[index];
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyChoiceWithRNG(option);
        }
        
        // Navigate to next node
        if (option.nextNode >= 0)
        {
            var nodeIdxField = dialogueManager.GetType().GetField("nodeIdx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nodesField = dialogueManager.GetType().GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (nodeIdxField != null && nodesField != null)
            {
                var nodes = nodesField.GetValue(dialogueManager) as List<DialogueNode>;
                if (nodes != null && option.nextNode < nodes.Count)
                {
                    nodeIdxField.SetValue(dialogueManager, option.nextNode);
                    ShowCurrentDialogue();
                }
                else
                {
                    // End of story
                    UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");
                }
            }
        }
        else
        {
            // End of story
            UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");
        }
    }
    
    void ShowCurrentDialogue()
    {
        // Get current node using reflection
        var nodeIdxField = dialogueManager.GetType().GetField("nodeIdx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nodesField = dialogueManager.GetType().GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (nodeIdxField == null || nodesField == null) return;
        
        int nodeIdx = (int)nodeIdxField.GetValue(dialogueManager);
        var nodes = nodesField.GetValue(dialogueManager) as List<DialogueNode>;
        
        if (nodes == null || nodeIdx < 0 || nodeIdx >= nodes.Count) return;
        
        var currentNode = nodes[nodeIdx];
        
        // Update display
        dialogueText.text = currentNode.body;
        
        // Update background
        if (!string.IsNullOrEmpty(currentNode.background))
        {
            Sprite bgSprite = Resources.Load<Sprite>("Backgrounds/" + currentNode.background);
            if (bgSprite != null)
                backgroundImage.sprite = bgSprite;
        }
            
        // Update characters
        if (!string.IsNullOrEmpty(currentNode.charLeft))
        {
            Sprite charSprite = Resources.Load<Sprite>("Characters/" + currentNode.charLeft);
            if (charSprite != null)
            {
                leftCharacter.sprite = charSprite;
                leftCharacter.color = Color.white;
            }
            else
            {
                leftCharacter.color = Color.clear;
            }
        }
        else
        {
            leftCharacter.color = Color.clear;
        }
        
        // Right character (DialogueNode doesn't have charRight, so keep it hidden)
        rightCharacter.color = Color.clear;
        
        // Filter options based on relationship requirements (like DialogueManager does)
        var filteredOptions = new List<DialogueOption>();
        foreach (var opt in currentNode.options)
        {
            if (opt == null) continue;
            
            // Check relationship requirements
            bool meetsReqs = true;
            if (GameManager.Instance != null)
            {
                meetsReqs = GameManager.Instance.Relationships >= opt.minRelationship &&
                           GameManager.Instance.Relationships <= opt.maxRelationship;
            }
                           
            // Add visible options or hidden options that meet requirements
            if (!opt.isHidden || meetsReqs)
            {
                filteredOptions.Add(opt);
            }
        }
        
        // Ensure we have at least one option
        if (filteredOptions.Count == 0)
        {
            filteredOptions.Add(new DialogueOption 
            { 
                text = "Continue...", 
                nextNode = -1 
            });
        }
        
        currentOptions = filteredOptions;
        currentSelection = 0;
        CreateOptionButtons();
    }
    
    void CreateOptionButtons()
    {
        // Clear existing buttons
        foreach (var button in optionButtons)
        {
            if (button != null) Destroy(button.gameObject);
        }
        optionButtons.Clear();
        
        if (currentOptions == null) return;
        
        // Create new buttons
        for (int i = 0; i < currentOptions.Count; i++)
        {
            var option = currentOptions[i];
            GameObject buttonObj = new GameObject($"Option_{i}");
            buttonObj.transform.SetParent(optionsPanel, false);
            
            Button button = buttonObj.AddComponent<Button>();
            Image bg = buttonObj.AddComponent<Image>();
            
            // Layout
            LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
            layout.preferredHeight = 50;
            
            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{i + 1}. {option.text}";
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 5);
            textRect.offsetMax = new Vector2(-15, -5);
            
            // Click handler
            int index = i;
            button.onClick.AddListener(() => SelectOption(index));
            
            optionButtons.Add(button);
        }
        
        UpdateSelection();
    }
    
    void UpdateSelection()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                Image bg = optionButtons[i].GetComponent<Image>();
                TextMeshProUGUI text = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                
                if (i == currentSelection)
                {
                    bg.color = new Color(1f, 0.8f, 0f, 0.8f); // Golden
                    text.color = Color.black;
                }
                else
                {
                    bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark
                    text.color = Color.white;
                }
            }
        }
    }
    
    void PlaySound(string soundType)
    {
        if (AudioManager.Instance != null)
        {
            if (soundType == "ui-navigate")
                AudioManager.Instance.PlayUINavigationSound();
            else if (soundType == "ui-select")
                AudioManager.Instance.PlayUISelectSound();
        }
    }
}