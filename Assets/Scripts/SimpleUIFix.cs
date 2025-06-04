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
        Debug.Log("SimpleUIFix: Awake() starting...");
        // Execute early to set up DialogueManager before it initializes
        SetupDialogueManagerReferences();
        Debug.Log("SimpleUIFix: Awake() complete");
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
        
        // Check if EarlySetup canvas exists, if not create our own
        var earlySetupCanvas = GameObject.Find("EarlySetupCanvas");
        if (earlySetupCanvas != null)
        {
            mainCanvas = earlySetupCanvas.GetComponent<Canvas>();
            Debug.Log("SimpleUIFix: Using EarlySetup canvas");
            SetupUIReferencesFromEarlySetup();
        }
        else
        {
            CreateUI();
        }
        
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
        mainCanvas.sortingOrder = 50; // Lower than EarlySetup canvas
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Ensure EventSystem exists
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
        
        // Background (game background only, not UI background)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        backgroundImage = bgObj.AddComponent<Image>();
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgObj.transform.SetAsFirstSibling(); // Put background behind everything
        
        // Characters
        CreateCharacter(ref leftCharacter, "LeftCharacter", new Vector2(-0.1f, 0f), new Vector2(0.3f, 0.85f));
        CreateCharacter(ref rightCharacter, "RightCharacter", new Vector2(0.7f, 0f), new Vector2(1.1f, 0.85f));
        
        // Dialogue text (positioned to not overlap with stats)
        GameObject dialogueObj = new GameObject("DialogueText");
        dialogueObj.transform.SetParent(canvasObj.transform, false);
        dialogueText = dialogueObj.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 20;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform dialogueRect = dialogueObj.GetComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0.2f, 0.45f);
        dialogueRect.anchorMax = new Vector2(0.8f, 0.65f);
        dialogueRect.offsetMin = new Vector2(15, 15);
        dialogueRect.offsetMax = new Vector2(-15, -15);
        
        // Options panel (positioned below dialogue)
        GameObject optionsPanelObj = new GameObject("OptionsPanel");
        optionsPanelObj.transform.SetParent(canvasObj.transform, false);
        optionsPanel = optionsPanelObj.AddComponent<RectTransform>();
        optionsPanel.anchorMin = new Vector2(0.2f, 0.25f);
        optionsPanel.anchorMax = new Vector2(0.8f, 0.43f);
        
        VerticalLayoutGroup layout = optionsPanelObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.padding = new RectOffset(15, 15, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        
        Debug.Log("SimpleUIFix: UI created successfully");
    }
    
    void SetupUIReferencesFromEarlySetup()
    {
        // Find EarlySetup's dialogue panel and use it
        var dialoguePanel = GameObject.Find("DialoguePanel");
        if (dialoguePanel != null)
        {
            var textContainer = dialoguePanel.transform.Find("TextContainer");
            if (textContainer != null)
            {
                // Create dialogue text in the text container
                GameObject dialogueObj = new GameObject("DialogueText");
                dialogueObj.transform.SetParent(textContainer, false);
                dialogueText = dialogueObj.AddComponent<TextMeshProUGUI>();
                dialogueText.fontSize = 20;
                dialogueText.color = Color.white;
                dialogueText.alignment = TextAlignmentOptions.TopLeft;
                
                RectTransform dialogueRect = dialogueObj.GetComponent<RectTransform>();
                dialogueRect.anchorMin = Vector2.zero;
                dialogueRect.anchorMax = Vector2.one;
                dialogueRect.sizeDelta = Vector2.zero;
            }
        }
        
        // Create options panel below the dialogue panel
        GameObject optionsPanelObj = new GameObject("OptionsPanel");
        optionsPanelObj.transform.SetParent(mainCanvas.transform, false);
        optionsPanel = optionsPanelObj.AddComponent<RectTransform>();
        optionsPanel.anchorMin = new Vector2(0.1f, 0.25f);
        optionsPanel.anchorMax = new Vector2(0.9f, 0.43f);
        
        VerticalLayoutGroup layout = optionsPanelObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.padding = new RectOffset(15, 15, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        
        // Create background image for game backgrounds
        GameObject bgObj = new GameObject("GameBackground");
        bgObj.transform.SetParent(mainCanvas.transform, false);
        backgroundImage = bgObj.AddComponent<Image>();
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgObj.transform.SetAsFirstSibling(); // Put background behind everything
        
        // Character images will be handled by the background sprites
        // We'll just create dummy ones for compatibility
        CreateCharacter(ref leftCharacter, "LeftCharacter", new Vector2(-0.1f, 0f), new Vector2(0.3f, 0.85f));
        CreateCharacter(ref rightCharacter, "RightCharacter", new Vector2(0.7f, 0f), new Vector2(1.1f, 0.85f));
        
        Debug.Log("SimpleUIFix: Set up UI references from EarlySetup");
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
        if (dialogueManager == null)
        {
            Debug.LogError("SimpleUIFix: DialogueManager is null!");
            return;
        }
        
        // Get current node using reflection
        var nodeIdxField = dialogueManager.GetType().GetField("nodeIdx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var nodesField = dialogueManager.GetType().GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (nodeIdxField == null || nodesField == null)
        {
            Debug.LogError("SimpleUIFix: Could not find nodeIdx or nodes fields in DialogueManager");
            return;
        }
        
        int nodeIdx = (int)nodeIdxField.GetValue(dialogueManager);
        var nodes = nodesField.GetValue(dialogueManager) as List<DialogueNode>;
        
        if (nodes == null || nodeIdx < 0 || nodeIdx >= nodes.Count)
        {
            Debug.LogError($"SimpleUIFix: Invalid node access. nodeIdx={nodeIdx}, nodes.Count={nodes?.Count ?? 0}");
            return;
        }
        
        var currentNode = nodes[nodeIdx];
        
        if (currentNode == null)
        {
            Debug.LogError($"SimpleUIFix: Current node at index {nodeIdx} is null");
            return;
        }
        
        // Update display
        if (dialogueText != null)
            dialogueText.text = currentNode.body;
        else
            Debug.LogError("SimpleUIFix: dialogueText is null!");
        
        // Update background
        if (!string.IsNullOrEmpty(currentNode.background) && backgroundImage != null)
        {
            // Try both paths since backgrounds exist in both locations
            Sprite bgSprite = Resources.Load<Sprite>("Sprites/Backgrounds/" + currentNode.background);
            if (bgSprite == null)
            {
                bgSprite = Resources.Load<Sprite>("Backgrounds/" + currentNode.background);
            }
            
            if (bgSprite != null)
                backgroundImage.sprite = bgSprite;
            else
                Debug.LogWarning($"SimpleUIFix: Background sprite not found in either Sprites/Backgrounds/ or Backgrounds/: {currentNode.background}");
        }
            
        // Update characters
        if (!string.IsNullOrEmpty(currentNode.charLeft) && leftCharacter != null)
        {
            Sprite charSprite = Resources.Load<Sprite>("Sprites/Characters/" + currentNode.charLeft);
            if (charSprite != null)
            {
                leftCharacter.sprite = charSprite;
                leftCharacter.color = Color.white;
            }
            else
            {
                leftCharacter.color = Color.clear;
                Debug.LogWarning($"SimpleUIFix: Character sprite not found: Sprites/Characters/{currentNode.charLeft}");
            }
        }
        else if (leftCharacter != null)
        {
            leftCharacter.color = Color.clear;
        }
        
        // Right character - always show Walter White
        if (rightCharacter != null)
        {
            Sprite walterSprite = Resources.Load<Sprite>("Sprites/Characters/WalterWhite_front");
            if (walterSprite != null)
            {
                rightCharacter.sprite = walterSprite;
                rightCharacter.color = Color.white;
            }
            else
            {
                rightCharacter.color = Color.clear;
                Debug.LogWarning("SimpleUIFix: Walter White sprite not found: Sprites/Characters/WalterWhite_front");
            }
        }
        
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
            layout.preferredHeight = 40;
            
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