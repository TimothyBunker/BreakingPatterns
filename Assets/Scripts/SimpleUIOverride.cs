using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SimpleUIOverride : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Canvas mainCanvas;
    
    [Header("New UI Elements")]
    [SerializeField] private Image newBackgroundImage;
    [SerializeField] private Image newLeftCharacter;
    [SerializeField] private Image newRightCharacter;
    [SerializeField] private TextMeshProUGUI newDialogueText;
    [SerializeField] private RectTransform newDialoguePanel;
    [SerializeField] private ScrollRect dialogueScrollRect;
    
    [Header("Options Display")]
    [SerializeField] private RectTransform optionsPanel;
    [SerializeField] private GameObject optionButtonPrefab;
    private List<GameObject> activeOptionButtons = new List<GameObject>();
    private int currentSelectedOption = 0;
    private string lastDialogueText = "";
    private int lastSelectedOption = -1;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    void Start()
    {
        EnsureEventSystem();
        StartCoroutine(SetupOverride());
    }
    
    IEnumerator SetupOverride()
    {
        // Wait for DialogueManager to initialize
        yield return new WaitForSeconds(0.1f);
        
        // Find DialogueManager if not assigned
        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();
            
        if (dialogueManager == null)
        {
            Debug.LogError("SimpleUIOverride: Cannot find DialogueManager!");
            yield break;
        }
        
        // Find or create canvas
        if (mainCanvas == null)
            mainCanvas = FindFirstObjectByType<Canvas>();
            
        if (mainCanvas == null)
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Setup audio
        SetupAudio();
        
        // Create our UI elements
        CreateSimpleUI();
        
        // Hide original UI
        HideOriginalUI();
        
        // Start intercepting
        StartCoroutine(InterceptAndDisplay());
        
        // Start scroll controls
        StartCoroutine(HandleScrollInput());
    }
    
    void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
    }
    
    IEnumerator HandleScrollInput()
    {
        // Removed scroll functionality as requested
        yield return null;
    }
    
    void PlayUISound(string soundName)
    {
        // Try to use AudioManager first
        var audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            if (soundName.Contains("click") || soundName.Contains("help"))
            {
                audioManager.PlayUINavigationSound();
                return;
            }
        }
        
        // Fallback to our own audio source
        if (audioSource == null) return;
        
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{soundName}");
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SimpleUIOverride: Could not find audio clip '{soundName}'");
        }
    }
    
    void CreateSimpleUI()
    {
        // Declare UI objects for proper ordering
        GameObject bgObj, leftPanel, rightPanel, leftCharObj, rightCharObj, dialogueObj, statsPanel;
        
        // Create background
        bgObj = new GameObject("NewBackground");
        bgObj.transform.SetParent(mainCanvas.transform, false);
        newBackgroundImage = bgObj.AddComponent<Image>();
        newBackgroundImage.color = Color.gray; // Default color
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.SetAsFirstSibling(); // Ensure it's at the back
        
        // Set proper layer order from back to front:
        // 1. Background (index 0)
        // 2. Character panels (index 1-2)  
        // 3. Characters (index 3-4)
        // 4. Dialogue panel (index 5)
        // 5. Stats panel (index 6)
        
        // Create semi-transparent panels behind characters (much skinnier)
        leftPanel = new GameObject("LeftCharacterPanel");
        leftPanel.transform.SetParent(mainCanvas.transform, false);
        Image leftPanelImg = leftPanel.AddComponent<Image>();
        leftPanelImg.color = new Color(0, 0, 0, 0.6f); // More opaque
        RectTransform leftPanelRect = leftPanel.GetComponent<RectTransform>();
        leftPanelRect.anchorMin = new Vector2(0, 0); // Full height
        leftPanelRect.anchorMax = new Vector2(0.18f, 1f); // Much skinnier (0-18%)
        
        rightPanel = new GameObject("RightCharacterPanel");
        rightPanel.transform.SetParent(mainCanvas.transform, false);
        Image rightPanelImg = rightPanel.AddComponent<Image>();
        rightPanelImg.color = new Color(0, 0, 0, 0.6f); // More opaque
        RectTransform rightPanelRect = rightPanel.GetComponent<RectTransform>();
        rightPanelRect.anchorMin = new Vector2(0.82f, 0); // Full height, further right
        rightPanelRect.anchorMax = new Vector2(1f, 1f); // Much skinnier (82-100%)
        
        // Create left character (larger and positioned lower)
        leftCharObj = new GameObject("NewLeftCharacter");
        leftCharObj.transform.SetParent(mainCanvas.transform, false);
        newLeftCharacter = leftCharObj.AddComponent<Image>();
        newLeftCharacter.preserveAspect = true;
        
        RectTransform leftRect = leftCharObj.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(-0.05f, 0f); // Start slightly off-screen for larger size
        leftRect.anchorMax = new Vector2(0.25f, 0.85f); // Larger and taller (0-85% height)
        leftRect.anchoredPosition = Vector2.zero;
        leftRect.sizeDelta = Vector2.zero;
        
        // Create right character (larger and positioned lower)
        rightCharObj = new GameObject("NewRightCharacter");
        rightCharObj.transform.SetParent(mainCanvas.transform, false);
        newRightCharacter = rightCharObj.AddComponent<Image>();
        newRightCharacter.preserveAspect = true;
        
        RectTransform rightRect = rightCharObj.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.75f, 0f); // Start further right for larger size
        rightRect.anchorMax = new Vector2(1.05f, 0.85f); // Larger and taller, slightly off-screen
        rightRect.anchoredPosition = Vector2.zero;
        rightRect.sizeDelta = Vector2.zero;
        
        // Create dialogue panel with scrollable text
        dialogueObj = new GameObject("NewDialoguePanel");
        dialogueObj.transform.SetParent(mainCanvas.transform, false);
        newDialoguePanel = dialogueObj.AddComponent<RectTransform>();
        
        Image panelBg = dialogueObj.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.9f);
        
        newDialoguePanel.anchorMin = new Vector2(0.19f, 0.25f); // Start after skinnier left panel, above options
        newDialoguePanel.anchorMax = new Vector2(0.81f, 0.5f); // End before skinnier right panel, reduced height
        newDialoguePanel.offsetMin = new Vector2(0, 10);
        newDialoguePanel.offsetMax = new Vector2(0, -10);
        
        // Create scroll view
        GameObject scrollViewObj = new GameObject("DialogueScrollView");
        scrollViewObj.transform.SetParent(dialogueObj.transform, false);
        dialogueScrollRect = scrollViewObj.AddComponent<ScrollRect>();
        
        RectTransform scrollViewRect = scrollViewObj.GetComponent<RectTransform>();
        scrollViewRect.anchorMin = Vector2.zero;
        scrollViewRect.anchorMax = Vector2.one;
        scrollViewRect.offsetMin = new Vector2(10, 10);
        scrollViewRect.offsetMax = new Vector2(-10, -10);
        
        // Create viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = Color.clear;
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        // Create content container
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 100); // Set initial height, width from anchors
        
        // Create text with proper layout settings
        newDialogueText = contentObj.AddComponent<TextMeshProUGUI>();
        newDialogueText.fontSize = 18;
        newDialogueText.color = Color.white;
        newDialogueText.alignment = TextAlignmentOptions.TopLeft;
        newDialogueText.text = "TEXT TEST - If you can see this, scrollable text is working!";
        newDialogueText.enableWordWrapping = true;
        newDialogueText.overflowMode = TextOverflowModes.Overflow;
        
        // Try to copy font from original dialogue text if available
        if (dialogueManager != null && dialogueManager.bodyLabel != null && dialogueManager.bodyLabel.font != null)
        {
            newDialogueText.font = dialogueManager.bodyLabel.font;
            Debug.Log($"SimpleUIOverride: Copied font from original dialogue: {dialogueManager.bodyLabel.font.name}");
        }
        
        // Add content size fitter with proper settings
        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        
        // Configure scroll rect
        dialogueScrollRect.content = contentRect;
        dialogueScrollRect.viewport = viewportRect;
        dialogueScrollRect.vertical = true;
        dialogueScrollRect.horizontal = false;
        dialogueScrollRect.movementType = ScrollRect.MovementType.Clamped;
        
        // Add scrollbar for visual feedback
        GameObject scrollbarObj = new GameObject("Scrollbar");
        scrollbarObj.transform.SetParent(dialogueObj.transform, false);
        
        RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.offsetMin = new Vector2(-20, 10);
        scrollbarRect.offsetMax = new Vector2(-5, -10);
        
        Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        
        Image scrollbarBg = scrollbarObj.AddComponent<Image>();
        scrollbarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Create scrollbar handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(scrollbarObj.transform, false);
        
        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;
        
        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImage;
        
        // Connect scrollbar to scroll rect
        dialogueScrollRect.verticalScrollbar = scrollbar;
        
        // Add minimal hint at the top of dialogue panel
        GameObject hintObj = new GameObject("ScrollHint");
        hintObj.transform.SetParent(dialogueObj.transform, false);
        
        RectTransform hintRect = hintObj.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0, 1);
        hintRect.anchorMax = new Vector2(1, 1);
        hintRect.offsetMin = new Vector2(0, -15);
        hintRect.offsetMax = new Vector2(0, 0);
        
        TextMeshProUGUI hintText = hintObj.AddComponent<TextMeshProUGUI>();
        hintText.text = "Navigate: ↑/↓ | Select: Enter/Space/Click";
        hintText.fontSize = 10;
        hintText.color = new Color(1, 1, 1, 0.3f);
        hintText.alignment = TextAlignmentOptions.Center;
        
        Debug.Log("SimpleUIOverride: Created scrollable text display with scrollbar and hint");
        
        // Create options panel
        GameObject optionsObj = new GameObject("OptionsPanel");
        optionsObj.transform.SetParent(mainCanvas.transform, false);
        optionsPanel = optionsObj.AddComponent<RectTransform>();
        
        // Position options panel below dialogue but above the bottom
        optionsPanel.anchorMin = new Vector2(0.19f, 0.05f);
        optionsPanel.anchorMax = new Vector2(0.81f, 0.25f);
        
        // Add semi-transparent background
        Image optionsBg = optionsObj.AddComponent<Image>();
        optionsBg.color = new Color(0, 0, 0, 0.7f);
        
        // Add vertical layout for options
        VerticalLayoutGroup optionsLayout = optionsObj.AddComponent<VerticalLayoutGroup>();
        optionsLayout.spacing = 10;
        optionsLayout.padding = new RectOffset(20, 20, 10, 10);
        optionsLayout.childAlignment = TextAnchor.UpperCenter;
        optionsLayout.childControlHeight = false;
        optionsLayout.childControlWidth = true;
        optionsLayout.childForceExpandWidth = true;
        
        Debug.Log("SimpleUIOverride: Created options panel");
        
        // Create stats panel at top (skinnier vertically)
        statsPanel = new GameObject("NewStatsPanel");
        statsPanel.transform.SetParent(mainCanvas.transform, false);
        RectTransform statsRect = statsPanel.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.1f, 0.92f);
        statsRect.anchorMax = new Vector2(0.9f, 0.98f);
        
        Image statsBg = statsPanel.AddComponent<Image>();
        statsBg.color = new Color(0, 0, 0, 0.8f);
        
        HorizontalLayoutGroup statsLayout = statsPanel.AddComponent<HorizontalLayoutGroup>();
        statsLayout.spacing = 30;
        statsLayout.padding = new RectOffset(20, 20, 5, 5);
        statsLayout.childAlignment = TextAnchor.MiddleCenter;
        
        // Copy stats from GameManager if they exist
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            // Hide original stats
            if (gm.profitText != null) gm.profitText.gameObject.SetActive(false);
            if (gm.relText != null) gm.relText.gameObject.SetActive(false);
            if (gm.suspText != null) gm.suspText.gameObject.SetActive(false);
            
            // Create new stat displays
            CreateStatDisplay("Profit", statsPanel.transform, gm.profitText);
            CreateStatDisplay("Relations", statsPanel.transform, gm.relText);
            CreateStatDisplay("Suspicion", statsPanel.transform, gm.suspText);
        }
        
        // Set proper layer ordering
        bgObj.transform.SetSiblingIndex(0);           // Background at back
        leftPanel.transform.SetSiblingIndex(1);       // Character panel 1
        rightPanel.transform.SetSiblingIndex(2);      // Character panel 2
        leftCharObj.transform.SetSiblingIndex(3);     // Left character
        rightCharObj.transform.SetSiblingIndex(4);    // Right character
        dialogueObj.transform.SetSiblingIndex(5);     // Dialogue panel
        optionsObj.transform.SetSiblingIndex(6);      // Options panel
        statsPanel.transform.SetSiblingIndex(7);      // Stats panel on top
        
        Debug.Log("SimpleUIOverride: UI creation complete with proper layering");
    }
    
    void CreateStatDisplay(string label, Transform parent, TMP_Text originalText)
    {
        GameObject statObj = new GameObject($"Stat_{label}");
        statObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI text = statObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 22;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        // Copy text from original if it exists
        if (originalText != null)
        {
            text.text = originalText.text;
            text.color = originalText.color;
            
            // Redirect GameManager reference to our new text
            if (label.Contains("Profit")) GameManager.Instance.profitText = text;
            else if (label.Contains("Relations")) GameManager.Instance.relText = text;
            else if (label.Contains("Suspicion")) GameManager.Instance.suspText = text;
        }
        
        LayoutElement layout = statObj.AddComponent<LayoutElement>();
        layout.flexibleWidth = 1;
        layout.preferredHeight = 30;
    }
    
    void HideOriginalUI()
    {
        Debug.Log("SimpleUIOverride: Hiding original UI elements");
        
        if (dialogueManager.bodyLabel != null)
            dialogueManager.bodyLabel.gameObject.SetActive(false);
        if (dialogueManager.bgImage != null)
            dialogueManager.bgImage.gameObject.SetActive(false);
        if (dialogueManager.charLeftImage != null)
            dialogueManager.charLeftImage.gameObject.SetActive(false);
        if (dialogueManager.charRightImage != null)
            dialogueManager.charRightImage.gameObject.SetActive(false);
            
        // Hide any parent containers
        if (dialogueManager.bodyLabel != null && dialogueManager.bodyLabel.transform.parent != null)
            dialogueManager.bodyLabel.transform.parent.gameObject.SetActive(false);
            
        // Also hide any other UI systems that might be conflicting
        var dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (dialogueUI != null)
            dialogueUI.gameObject.SetActive(false);
            
        var uiIntegration = FindFirstObjectByType<DialogueUIIntegration>();
        if (uiIntegration != null)
            uiIntegration.gameObject.SetActive(false);
    }
    
    IEnumerator InterceptAndDisplay()
    {
        bool firstUpdate = true;
        
        while (true)
        {
            yield return null;
            
            // Copy background
            if (dialogueManager.bgImage != null && dialogueManager.bgImage.sprite != null)
            {
                if (newBackgroundImage.sprite != dialogueManager.bgImage.sprite)
                {
                    newBackgroundImage.sprite = dialogueManager.bgImage.sprite;
                    newBackgroundImage.color = Color.white;
                    if (firstUpdate) Debug.Log($"SimpleUIOverride: Set background to {dialogueManager.bgImage.sprite.name}");
                }
            }
            
            // Copy characters
            if (dialogueManager.charLeftImage != null && dialogueManager.charLeftImage.sprite != null)
            {
                if (newLeftCharacter.sprite != dialogueManager.charLeftImage.sprite)
                {
                    newLeftCharacter.sprite = dialogueManager.charLeftImage.sprite;
                    newLeftCharacter.color = Color.white;
                    newLeftCharacter.enabled = true;
                    if (firstUpdate) Debug.Log($"SimpleUIOverride: Set left character to {dialogueManager.charLeftImage.sprite.name}");
                }
            }
            else
            {
                newLeftCharacter.enabled = false;
            }
            
            if (dialogueManager.charRightImage != null && dialogueManager.charRightImage.sprite != null)
            {
                if (newRightCharacter.sprite != dialogueManager.charRightImage.sprite)
                {
                    newRightCharacter.sprite = dialogueManager.charRightImage.sprite;
                    newRightCharacter.color = Color.white;
                    newRightCharacter.enabled = true;
                    if (firstUpdate) Debug.Log($"SimpleUIOverride: Set right character to {dialogueManager.charRightImage.sprite.name}");
                }
            }
            else
            {
                newRightCharacter.enabled = false;
            }
            
            // Copy text
            if (dialogueManager.bodyLabel != null)
            {
                if (firstUpdate) Debug.Log($"SimpleUIOverride: bodyLabel exists, text = '{dialogueManager.bodyLabel.text}'");
                
                if (!string.IsNullOrEmpty(dialogueManager.bodyLabel.text))
                {
                    string fullText = dialogueManager.bodyLabel.text;
                    
                    // Always update options to track selection changes
                    ParseAndDisplayDialogue(fullText);
                    
                    if (firstUpdate)
                    {
                        Debug.Log($"SimpleUIOverride: Initial dialogue and options setup (length: {fullText.Length})");
                    }
                }
                else if (firstUpdate)
                {
                    Debug.Log("SimpleUIOverride: bodyLabel text is empty or null");
                }
            }
            else if (firstUpdate)
            {
                Debug.Log("SimpleUIOverride: bodyLabel is null");
            }
            
            firstUpdate = false;
        }
    }
    
    void ParseAndDisplayDialogue(string fullText)
    {
        // Check if we need to update dialogue text
        bool dialogueChanged = (fullText != lastDialogueText);
        
        if (dialogueChanged)
        {
            lastDialogueText = fullText;
            
            // Split the text to find where options start
            string[] lines = fullText.Split('\n');
            List<string> dialogueLines = new List<string>();
            List<string> optionLines = new List<string>();
            
            bool inOptions = false;
            foreach (string line in lines)
            {
                // Check if this line is an option (starts with number and dot)
                if (!inOptions && System.Text.RegularExpressions.Regex.IsMatch(line.Trim(), @"^(\s*|<[^>]+>)*\d+\."))
                {
                    inOptions = true;
                }
                
                if (inOptions)
                {
                    optionLines.Add(line);
                }
                else
                {
                    dialogueLines.Add(line);
                }
            }
            
            // Display dialogue text (without options)
            string dialogueText = string.Join("\n", dialogueLines).TrimEnd();
            newDialogueText.text = dialogueText;
            
            // Force immediate layout rebuild
            newDialogueText.ForceMeshUpdate();
            
            // Update options display
            UpdateOptionsDisplay(optionLines);
            
            // Reset scroll to top when text changes
            if (dialogueScrollRect != null && dialogueScrollRect.content != null)
            {
                Canvas.ForceUpdateCanvases();
                dialogueScrollRect.verticalNormalizedPosition = 1f;
            }
        }
        else
        {
            // Just update option highlighting if selection changed
            UpdateOptionHighlighting();
        }
    }
    
    void UpdateOptionsDisplay(List<string> optionLines)
    {
        // Clear existing option buttons
        foreach (var button in activeOptionButtons)
        {
            if (button != null)
                Destroy(button);
        }
        activeOptionButtons.Clear();
        
        if (optionsPanel == null || optionLines.Count == 0)
            return;
        
        // Get the current selected option from DialogueManager using reflection
        if (dialogueManager != null)
        {
            var optionIdxField = dialogueManager.GetType().GetField("optionIdx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (optionIdxField != null)
            {
                currentSelectedOption = (int)optionIdxField.GetValue(dialogueManager);
            }
        }
        
        // Parse options and create buttons
        int actualOptionIndex = 0;
        for (int i = 0; i < optionLines.Count; i++)
        {
            string optionLine = optionLines[i];
            if (string.IsNullOrWhiteSpace(optionLine))
                continue;
            
            // Create option button with proper selection state
            bool isSelected = (actualOptionIndex == currentSelectedOption);
            GameObject optionObj = CreateOptionButton(optionLine, actualOptionIndex, isSelected);
            activeOptionButtons.Add(optionObj);
            actualOptionIndex++;
        }
    }
    
    GameObject CreateOptionButton(string optionText, int index, bool isSelected)
    {
        GameObject buttonObj = new GameObject($"Option_{index}");
        buttonObj.transform.SetParent(optionsPanel, false);
        
        // Add background image
        Image bg = buttonObj.AddComponent<Image>();
        bg.color = isSelected ? new Color(1f, 0.843f, 0f, 0.9f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Add Button component for click handling
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = bg;
        
        // Set button colors for hover/press states
        ColorBlock colors = button.colors;
        colors.normalColor = isSelected ? new Color(1f, 0.843f, 0f, 0.9f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);
        colors.highlightedColor = new Color(1f, 0.843f, 0f, 0.7f);
        colors.pressedColor = new Color(1f, 0.843f, 0f, 1f);
        colors.selectedColor = new Color(1f, 0.843f, 0f, 0.9f);
        button.colors = colors;
        
        // Add click handler
        int optionIndex = index;
        button.onClick.AddListener(() => OnOptionClicked(optionIndex));
        
        // Add layout element
        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 45;
        layout.flexibleWidth = 1;
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 5);
        textRect.offsetMax = new Vector2(-15, -5);
        
        // Clean up the option text (remove selection markers)
        string cleanText = optionText;
        cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"<color=#FFD700>> </color>", "");
        cleanText = cleanText.Replace("> ", "  ").Trim();
        
        text.text = cleanText;
        text.fontSize = 18;
        text.color = isSelected ? Color.black : Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = true;
        
        // Add subtle shadow for depth
        if (isSelected)
        {
            Shadow shadow = buttonObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.3f);
            shadow.effectDistance = new Vector2(2, -2);
        }
        
        return buttonObj;
    }
    
    void UpdateOptionHighlighting()
    {
        if (dialogueManager == null || activeOptionButtons.Count == 0)
            return;
        
        // Get the current selected option from DialogueManager using reflection
        int newSelectedOption = 0;
        var optionIdxField = dialogueManager.GetType().GetField("optionIdx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (optionIdxField != null)
        {
            newSelectedOption = (int)optionIdxField.GetValue(dialogueManager);
        }
        
        // Only update if selection changed
        if (newSelectedOption != lastSelectedOption && newSelectedOption >= 0 && newSelectedOption < activeOptionButtons.Count)
        {
            lastSelectedOption = newSelectedOption;
            currentSelectedOption = newSelectedOption;
            
            // Update button appearances
            for (int i = 0; i < activeOptionButtons.Count; i++)
            {
                if (activeOptionButtons[i] != null)
                {
                    bool isSelected = (i == currentSelectedOption);
                    Image bg = activeOptionButtons[i].GetComponent<Image>();
                    if (bg != null)
                    {
                        bg.color = isSelected ? new Color(1f, 0.843f, 0f, 0.9f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    }
                    
                    TextMeshProUGUI text = activeOptionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.color = isSelected ? Color.black : Color.white;
                    }
                    
                    // Update button color block
                    Button button = activeOptionButtons[i].GetComponent<Button>();
                    if (button != null)
                    {
                        ColorBlock colors = button.colors;
                        colors.normalColor = isSelected ? new Color(1f, 0.843f, 0f, 0.9f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);
                        button.colors = colors;
                    }
                }
            }
        }
    }
    
    void OnOptionClicked(int optionIndex)
    {
        Debug.Log($"SimpleUIOverride: Option {optionIndex} clicked");
        
        // Play selection sound
        PlayUISound("default-choice");
        
        // Set the optionIdx in DialogueManager using reflection
        if (dialogueManager != null && optionIndex >= 0 && optionIndex < activeOptionButtons.Count)
        {
            var optionIdxField = dialogueManager.GetType().GetField("optionIdx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (optionIdxField != null)
            {
                optionIdxField.SetValue(dialogueManager, optionIndex);
            }
            
            // Trigger the selection after a brief delay to show the selection
            StartCoroutine(TriggerSelectionAfterDelay(optionIndex));
        }
    }
    
    IEnumerator TriggerSelectionAfterDelay(int optionIndex)
    {
        yield return new WaitForSeconds(0.1f);
        
        // Find the OnOptionSelected method in DialogueManager via reflection
        var onOptionSelectedMethod = dialogueManager.GetType().GetMethod("OnOptionSelected", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (onOptionSelectedMethod != null)
        {
            onOptionSelectedMethod.Invoke(dialogueManager, new object[] { optionIndex });
        }
        else
        {
            Debug.LogError("SimpleUIOverride: Could not find OnOptionSelected method in DialogueManager");
        }
    }
    
    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("SimpleUIOverride: Created EventSystem for UI interaction");
        }
    }
}