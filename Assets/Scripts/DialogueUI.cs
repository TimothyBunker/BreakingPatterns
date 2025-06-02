using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueUI : MonoBehaviour
{
    [Header("Main Layout")]
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private float backgroundTransitionTime = 0.5f;
    
    [Header("Character Display")]
    [SerializeField] private RectTransform characterPanel;
    [SerializeField] private Image leftCharacterImage;
    [SerializeField] private Image rightCharacterImage;
    [SerializeField] private float characterFadeTime = 0.3f;
    [SerializeField] private Vector2 characterSize = new Vector2(400, 600);
    
    [Header("Dialogue Panel")]
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private Image dialoguePanelBg;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueBodyText;
    [SerializeField] private ScrollRect dialogueScrollRect;
    
    [Header("Choice Display")]
    [SerializeField] private RectTransform choicePanel;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private VerticalLayoutGroup choiceLayoutGroup;
    private List<GameObject> activeChoiceButtons = new List<GameObject>();
    
    [Header("Stats Display")]
    [SerializeField] private RectTransform statsPanel;
    [SerializeField] private GameObject statItemPrefab;
    
    [Header("Visual Settings")]
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color selectedChoiceColor = new Color(1f, 0.843f, 0f, 1f);
    [SerializeField] private float panelMargin = 50f;
    
    void Awake()
    {
        SetupResponsiveUI();
        CreateUIElements();
    }
    
    void SetupResponsiveUI()
    {
        // Get or create canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
            
            // Setup canvas scaler for responsive design
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }
            
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }
    
    void CreateUIElements()
    {
        var layerManager = UILayerManager.Instance;
        
        // Create background layer
        if (backgroundImage == null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform, false);
            backgroundImage = bgObj.AddComponent<Image>();
            backgroundImage.color = Color.black;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            layerManager.SetUILayer(bgObj, UILayerManager.UILayer.Background);
            
            // Add overlay for better text readability
            GameObject overlayObj = new GameObject("BackgroundOverlay");
            overlayObj.transform.SetParent(transform, false);
            backgroundOverlay = overlayObj.AddComponent<Image>();
            backgroundOverlay.color = new Color(0, 0, 0, 0.3f);
            
            RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            
            layerManager.SetUILayer(overlayObj, UILayerManager.UILayer.BackgroundOverlay);
        }
        
        // Create character panel
        if (characterPanel == null)
        {
            GameObject charPanelObj = new GameObject("CharacterPanel");
            charPanelObj.transform.SetParent(transform, false);
            characterPanel = charPanelObj.AddComponent<RectTransform>();
            
            // Position characters in middle area
            characterPanel.anchorMin = new Vector2(0, 0.3f);
            characterPanel.anchorMax = new Vector2(1, 0.8f);
            characterPanel.sizeDelta = Vector2.zero;
            
            // Create character images
            CreateCharacterImage(ref leftCharacterImage, "LeftCharacter", new Vector2(0.15f, 0.5f));
            CreateCharacterImage(ref rightCharacterImage, "RightCharacter", new Vector2(0.85f, 0.5f));
            
            layerManager.SetUILayer(charPanelObj, UILayerManager.UILayer.Characters);
        }
        
        // Create dialogue panel with modern design
        if (dialoguePanel == null)
        {
            GameObject dialogueObj = new GameObject("DialoguePanel");
            dialogueObj.transform.SetParent(transform, false);
            dialoguePanel = dialogueObj.AddComponent<RectTransform>();
            
            // Position at bottom with responsive margins
            dialoguePanel.anchorMin = new Vector2(0.05f, 0.05f);
            dialoguePanel.anchorMax = new Vector2(0.95f, 0.35f);
            dialoguePanel.pivot = new Vector2(0.5f, 0);
            
            // Add background with blur effect simulation
            dialoguePanelBg = dialogueObj.AddComponent<Image>();
            dialoguePanelBg.color = panelColor;
            
            // Add subtle shadow
            Shadow shadow = dialogueObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(0, -5);
            
            // Create content container with padding
            GameObject contentObj = new GameObject("DialogueContent");
            contentObj.transform.SetParent(dialoguePanel, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(panelMargin, panelMargin);
            contentRect.offsetMax = new Vector2(-panelMargin, -panelMargin);
            
            // Add vertical layout
            VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            
            // Create speaker name
            GameObject speakerObj = new GameObject("SpeakerName");
            speakerObj.transform.SetParent(contentObj.transform, false);
            speakerNameText = speakerObj.AddComponent<TextMeshProUGUI>();
            speakerNameText.fontSize = 28;
            speakerNameText.fontStyle = FontStyles.Bold;
            speakerNameText.color = new Color(1f, 0.9f, 0.3f);
            
            // Create dialogue body with scroll
            GameObject scrollObj = new GameObject("DialogueScroll");
            scrollObj.transform.SetParent(contentObj.transform, false);
            dialogueScrollRect = scrollObj.AddComponent<ScrollRect>();
            
            RectTransform scrollRect = scrollObj.GetComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(0, 200);
            
            LayoutElement scrollLayout = scrollObj.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1;
            
            // Create viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = new Vector2(0, 1);
            
            Image viewportMask = viewportObj.AddComponent<Image>();
            viewportMask.color = Color.clear;
            viewportObj.AddComponent<Mask>().showMaskGraphic = false;
            
            // Create content for scroll
            GameObject textContentObj = new GameObject("TextContent");
            textContentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform textContentRect = textContentObj.GetComponent<RectTransform>();
            textContentRect.anchorMin = new Vector2(0, 1);
            textContentRect.anchorMax = new Vector2(1, 1);
            textContentRect.pivot = new Vector2(0.5f, 1);
            
            dialogueBodyText = textContentObj.AddComponent<TextMeshProUGUI>();
            dialogueBodyText.fontSize = 22;
            dialogueBodyText.color = Color.white;
            dialogueBodyText.lineSpacing = 5;
            
            ContentSizeFitter csf = textContentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            dialogueScrollRect.content = textContentRect;
            dialogueScrollRect.viewport = viewportRect;
            
            layerManager.SetUILayer(dialogueObj, UILayerManager.UILayer.DialogueBox);
        }
        
        // Create stats panel
        if (statsPanel == null)
        {
            GameObject statsObj = new GameObject("StatsPanel");
            statsObj.transform.SetParent(transform, false);
            statsPanel = statsObj.AddComponent<RectTransform>();
            
            // Position at top
            statsPanel.anchorMin = new Vector2(0.1f, 0.9f);
            statsPanel.anchorMax = new Vector2(0.9f, 0.98f);
            
            // Add background
            Image statsBg = statsObj.AddComponent<Image>();
            statsBg.color = new Color(0, 0, 0, 0.7f);
            
            // Add horizontal layout
            HorizontalLayoutGroup hlg = statsObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 50;
            hlg.padding = new RectOffset(30, 30, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlHeight = true;
            hlg.childControlWidth = false;
            hlg.childForceExpandWidth = true;
            
            layerManager.SetUILayer(statsObj, UILayerManager.UILayer.StatsPanel);
        }
        
        // Organize hierarchy at the end
        layerManager.OrganizeUIHierarchy(transform);
    }
    
    void CreateCharacterImage(ref Image characterImage, string name, Vector2 anchorPos)
    {
        GameObject charObj = new GameObject(name);
        charObj.transform.SetParent(characterPanel, false);
        characterImage = charObj.AddComponent<Image>();
        characterImage.preserveAspect = true;
        characterImage.color = Color.clear;
        
        RectTransform charRect = charObj.GetComponent<RectTransform>();
        charRect.anchorMin = anchorPos;
        charRect.anchorMax = anchorPos;
        charRect.sizeDelta = characterSize;
        charRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Add subtle shadow for depth
        Shadow shadow = charObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(5, -5);
    }
    
    public void DisplayDialogue(string speaker, string body, List<DialogueOption> options, int selectedIndex)
    {
        // Update speaker name
        if (speakerNameText != null && !string.IsNullOrEmpty(speaker))
        {
            speakerNameText.text = speaker;
            speakerNameText.gameObject.SetActive(true);
        }
        else
        {
            speakerNameText.gameObject.SetActive(false);
        }
        
        // Update dialogue body
        if (dialogueBodyText != null)
        {
            dialogueBodyText.text = body;
            Canvas.ForceUpdateCanvases();
            dialogueScrollRect.verticalNormalizedPosition = 1f;
        }
        
        // Update choices
        UpdateChoices(options, selectedIndex);
    }
    
    void UpdateChoices(List<DialogueOption> options, int selectedIndex)
    {
        // Clear existing choices
        foreach (var button in activeChoiceButtons)
        {
            Destroy(button);
        }
        activeChoiceButtons.Clear();
        
        if (choicePanel == null)
        {
            CreateChoicePanel();
        }
        
        // Create new choice buttons
        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            GameObject buttonObj = CreateChoiceButton(option, i, i == selectedIndex);
            activeChoiceButtons.Add(buttonObj);
        }
    }
    
    void CreateChoicePanel()
    {
        GameObject choiceObj = new GameObject("ChoicePanel");
        choiceObj.transform.SetParent(dialoguePanel, false);
        choicePanel = choiceObj.AddComponent<RectTransform>();
        
        choicePanel.anchorMin = new Vector2(0, 0);
        choicePanel.anchorMax = new Vector2(1, 1);
        choicePanel.offsetMin = new Vector2(panelMargin, panelMargin);
        choicePanel.offsetMax = new Vector2(-panelMargin, -panelMargin);
        
        choiceLayoutGroup = choiceObj.AddComponent<VerticalLayoutGroup>();
        choiceLayoutGroup.spacing = 10;
        choiceLayoutGroup.childControlHeight = false;
        choiceLayoutGroup.childControlWidth = true;
        choiceLayoutGroup.childForceExpandWidth = true;
    }
    
    GameObject CreateChoiceButton(DialogueOption option, int index, bool isSelected)
    {
        GameObject buttonObj = new GameObject($"Choice_{index}");
        buttonObj.transform.SetParent(choicePanel, false);
        
        // Add layout element
        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 50;
        
        // Add background
        Image bg = buttonObj.AddComponent<Image>();
        bg.color = isSelected ? selectedChoiceColor : new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 5);
        textRect.offsetMax = new Vector2(-20, -5);
        
        // Format choice text with stat preview
        string choiceText = $"{index + 1}. {option.text}";
        if (option.profit != 0 || option.relationships != 0 || option.suspicion != 0)
        {
            choiceText += " [";
            List<string> stats = new List<string>();
            
            if (option.profit != 0)
            {
                int min = option.profit - Mathf.Abs(option.profit) / 3;
                int max = option.profit + Mathf.Abs(option.profit) / 3;
                string color = option.profit > 0 ? "#4CAF50" : "#E53935";
                stats.Add($"<color={color}>P: {min}~{max}</color>");
            }
            
            if (option.relationships != 0)
            {
                int min = option.relationships - Mathf.Abs(option.relationships) / 3;
                int max = option.relationships + Mathf.Abs(option.relationships) / 3;
                string color = option.relationships > 0 ? "#4CAF50" : "#E53935";
                stats.Add($"<color={color}>R: {min}~{max}</color>");
            }
            
            if (option.suspicion != 0)
            {
                int min = option.suspicion - Mathf.Abs(option.suspicion) / 3;
                int max = option.suspicion + Mathf.Abs(option.suspicion) / 3;
                string color = "#FF9800";
                stats.Add($"<color={color}>S: {min}~{max}</color>");
            }
            
            choiceText += string.Join(" | ", stats) + "]";
        }
        
        text.text = choiceText;
        text.fontSize = 20;
        text.color = isSelected ? Color.black : Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        
        return buttonObj;
    }
    
    public void SetBackground(Sprite sprite)
    {
        if (backgroundImage != null && sprite != null)
        {
            StartCoroutine(TransitionBackground(sprite));
        }
    }
    
    IEnumerator TransitionBackground(Sprite newSprite)
    {
        float elapsed = 0;
        
        while (elapsed < backgroundTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / backgroundTransitionTime;
            backgroundOverlay.color = new Color(0, 0, 0, Mathf.Lerp(0.3f, 1f, t));
            yield return null;
        }
        
        backgroundImage.sprite = newSprite;
        backgroundImage.preserveAspect = true;
        elapsed = 0;
        
        while (elapsed < backgroundTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / backgroundTransitionTime;
            backgroundOverlay.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0.3f, t));
            yield return null;
        }
    }
    
    public void SetCharacter(Sprite sprite, bool isLeft)
    {
        Image targetImage = isLeft ? leftCharacterImage : rightCharacterImage;
        if (targetImage != null)
        {
            StartCoroutine(FadeCharacter(targetImage, sprite));
        }
    }
    
    IEnumerator FadeCharacter(Image characterImage, Sprite newSprite)
    {
        if (newSprite == null)
        {
            // Fade out
            float elapsed = 0;
            Color startColor = characterImage.color;
            while (elapsed < characterFadeTime)
            {
                elapsed += Time.deltaTime;
                characterImage.color = Color.Lerp(startColor, Color.clear, elapsed / characterFadeTime);
                yield return null;
            }
            characterImage.sprite = null;
        }
        else
        {
            // Fade in with new sprite
            characterImage.sprite = newSprite;
            characterImage.preserveAspect = true;
            
            float elapsed = 0;
            while (elapsed < characterFadeTime)
            {
                elapsed += Time.deltaTime;
                characterImage.color = Color.Lerp(Color.clear, Color.white, elapsed / characterFadeTime);
                yield return null;
            }
        }
    }
}