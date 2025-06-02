using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    
    void Start()
    {
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
        
        // Create our UI elements
        CreateSimpleUI();
        
        // Hide original UI
        HideOriginalUI();
        
        // Start intercepting
        StartCoroutine(InterceptAndDisplay());
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
        
        // Create semi-transparent panels behind characters
        leftPanel = new GameObject("LeftCharacterPanel");
        leftPanel.transform.SetParent(mainCanvas.transform, false);
        Image leftPanelImg = leftPanel.AddComponent<Image>();
        leftPanelImg.color = new Color(0, 0, 0, 0.3f);
        RectTransform leftPanelRect = leftPanel.GetComponent<RectTransform>();
        leftPanelRect.anchorMin = new Vector2(0, 0.15f);
        leftPanelRect.anchorMax = new Vector2(0.35f, 0.85f);
        
        rightPanel = new GameObject("RightCharacterPanel");
        rightPanel.transform.SetParent(mainCanvas.transform, false);
        Image rightPanelImg = rightPanel.AddComponent<Image>();
        rightPanelImg.color = new Color(0, 0, 0, 0.3f);
        RectTransform rightPanelRect = rightPanel.GetComponent<RectTransform>();
        rightPanelRect.anchorMin = new Vector2(0.65f, 0.15f);
        rightPanelRect.anchorMax = new Vector2(1f, 0.85f);
        
        // Create left character (larger size)
        leftCharObj = new GameObject("NewLeftCharacter");
        leftCharObj.transform.SetParent(mainCanvas.transform, false);
        newLeftCharacter = leftCharObj.AddComponent<Image>();
        newLeftCharacter.preserveAspect = true;
        
        RectTransform leftRect = leftCharObj.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0.05f, 0.15f);
        leftRect.anchorMax = new Vector2(0.35f, 0.85f);
        leftRect.anchoredPosition = Vector2.zero;
        leftRect.sizeDelta = Vector2.zero;
        
        // Create right character (larger size)
        rightCharObj = new GameObject("NewRightCharacter");
        rightCharObj.transform.SetParent(mainCanvas.transform, false);
        newRightCharacter = rightCharObj.AddComponent<Image>();
        newRightCharacter.preserveAspect = true;
        
        RectTransform rightRect = rightCharObj.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.65f, 0.15f);
        rightRect.anchorMax = new Vector2(0.95f, 0.85f);
        rightRect.anchoredPosition = Vector2.zero;
        rightRect.sizeDelta = Vector2.zero;
        
        // Create dialogue panel with scrollable text
        dialogueObj = new GameObject("NewDialoguePanel");
        dialogueObj.transform.SetParent(mainCanvas.transform, false);
        newDialoguePanel = dialogueObj.AddComponent<RectTransform>();
        
        Image panelBg = dialogueObj.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.9f);
        
        newDialoguePanel.anchorMin = new Vector2(0.1f, 0);
        newDialoguePanel.anchorMax = new Vector2(0.9f, 0.35f);
        newDialoguePanel.offsetMin = new Vector2(0, 20);
        newDialoguePanel.offsetMax = new Vector2(0, 0);
        
        // Create scroll view for dialogue text
        GameObject scrollViewObj = new GameObject("DialogueScrollView");
        scrollViewObj.transform.SetParent(newDialoguePanel, false);
        
        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        
        RectTransform scrollRectTransform = scrollViewObj.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(10, 10);
        scrollRectTransform.offsetMax = new Vector2(-10, -10);
        
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
        
        // Create dialogue text
        newDialogueText = contentObj.AddComponent<TextMeshProUGUI>();
        newDialogueText.fontSize = 18;
        newDialogueText.color = Color.white;
        newDialogueText.alignment = TextAlignmentOptions.TopLeft;
        
        // Add content size fitter
        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Configure scroll rect
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        
        // Store reference
        dialogueScrollRect = scrollRect;
        
        // Create stats panel at top
        statsPanel = new GameObject("NewStatsPanel");
        statsPanel.transform.SetParent(mainCanvas.transform, false);
        RectTransform statsRect = statsPanel.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.1f, 0.9f);
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
        statsPanel.transform.SetSiblingIndex(6);      // Stats panel on top
        
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
            if (dialogueManager.bodyLabel != null && !string.IsNullOrEmpty(dialogueManager.bodyLabel.text))
            {
                if (newDialogueText.text != dialogueManager.bodyLabel.text)
                {
                    newDialogueText.text = dialogueManager.bodyLabel.text;
                    if (firstUpdate) Debug.Log($"SimpleUIOverride: Set text (length: {dialogueManager.bodyLabel.text.Length})");
                    
                    // Reset scroll position to top when text changes
                    if (dialogueScrollRect != null)
                    {
                        Canvas.ForceUpdateCanvases();
                        dialogueScrollRect.verticalNormalizedPosition = 1f;
                    }
                }
            }
            
            firstUpdate = false;
        }
    }
}