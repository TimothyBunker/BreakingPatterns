using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform mainCanvas;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private float referenceWidth = 1920f;
    [SerializeField] private float referenceHeight = 1080f;
    
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private float backgroundFadeDuration = 0.5f;
    
    [Header("Character Portraits")]
    [SerializeField] private RectTransform characterContainer;
    [SerializeField] private Image leftCharacter;
    [SerializeField] private Image rightCharacter;
    [SerializeField] private float characterFadeInDuration = 0.3f;
    [SerializeField] private Vector2 characterSlideDistance = new Vector2(100f, 0f);
    
    [Header("Dialogue Box")]
    [SerializeField] private RectTransform dialogueBox;
    [SerializeField] private Image dialogueBoxBackground;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private ScrollRect dialogueScrollRect;
    [SerializeField] private float dialogueBoxPadding = 40f;
    [SerializeField] private Color dialogueBoxColor = new Color(0, 0, 0, 0.8f);
    
    [Header("Stats Display")]
    [SerializeField] private RectTransform statsContainer;
    [SerializeField] private TextMeshProUGUI profitText;
    [SerializeField] private TextMeshProUGUI relationshipText;
    [SerializeField] private TextMeshProUGUI suspicionText;
    [SerializeField] private GameObject statsPrefab;
    
    [Header("Choice Buttons")]
    [SerializeField] private RectTransform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private float choiceSpacing = 10f;
    [SerializeField] private float choiceAnimationDelay = 0.1f;
    
    [Header("Effects")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private RectTransform effectsContainer;
    
    void Awake()
    {
        SetupCanvas();
        CreateUIElements();
    }
    
    void SetupCanvas()
    {
        if (mainCanvas == null)
        {
            // Find or create main canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("MainCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasRenderer>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            mainCanvas = canvas.GetComponent<RectTransform>();
        }
        
        // Setup canvas scaler for responsive UI
        if (canvasScaler == null)
        {
            canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
            }
        }
        
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
    }
    
    void CreateUIElements()
    {
        // Create background if missing
        if (backgroundImage == null)
        {
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(mainCanvas, false);
            backgroundImage = bgObj.AddComponent<Image>();
            backgroundImage.color = Color.black;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;
        }
        
        // Create dialogue box with modern design
        if (dialogueBox == null)
        {
            GameObject dialogueObj = new GameObject("DialogueBox");
            dialogueObj.transform.SetParent(mainCanvas, false);
            dialogueBox = dialogueObj.AddComponent<RectTransform>();
            
            // Position at bottom of screen with margins
            dialogueBox.anchorMin = new Vector2(0.1f, 0f);
            dialogueBox.anchorMax = new Vector2(0.9f, 0.35f);
            dialogueBox.offsetMin = new Vector2(0, 20);
            dialogueBox.offsetMax = new Vector2(0, -20);
            
            // Add background with rounded corners effect
            dialogueBoxBackground = dialogueObj.AddComponent<Image>();
            dialogueBoxBackground.color = dialogueBoxColor;
            
            // Add outline for better visibility
            var outline = dialogueObj.AddComponent<Outline>();
            outline.effectColor = new Color(1, 1, 1, 0.3f);
            outline.effectDistance = new Vector2(2, -2);
        }
        
        // Create scrollable dialogue text
        if (dialogueText == null)
        {
            GameObject scrollViewObj = new GameObject("DialogueScrollView");
            scrollViewObj.transform.SetParent(dialogueBox, false);
            
            dialogueScrollRect = scrollViewObj.AddComponent<ScrollRect>();
            
            RectTransform scrollRect = scrollViewObj.GetComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.sizeDelta = new Vector2(-dialogueBoxPadding * 2, -dialogueBoxPadding * 2);
            scrollRect.anchoredPosition = Vector2.zero;
            
            // Create content container
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollViewObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            
            // Create text
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(contentObj.transform, false);
            dialogueText = textObj.AddComponent<TextMeshProUGUI>();
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = new Vector2(1, 1);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            // Configure text settings
            dialogueText.fontSize = 24;
            dialogueText.color = Color.white;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
            dialogueText.lineSpacing = 10;
            dialogueText.margin = new Vector4(20, 20, 20, 20);
            
            // Add content size fitter
            var contentFitter = contentObj.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Configure scroll rect
            dialogueScrollRect.content = contentRect;
            dialogueScrollRect.viewport = scrollRect;
            dialogueScrollRect.vertical = true;
            dialogueScrollRect.horizontal = false;
        }
        
        // Create stats display with modern look
        if (statsContainer == null)
        {
            GameObject statsObj = new GameObject("StatsContainer");
            statsObj.transform.SetParent(mainCanvas, false);
            statsContainer = statsObj.AddComponent<RectTransform>();
            
            // Position at top of screen
            statsContainer.anchorMin = new Vector2(0, 0.9f);
            statsContainer.anchorMax = new Vector2(1, 1);
            statsContainer.offsetMin = new Vector2(20, 0);
            statsContainer.offsetMax = new Vector2(-20, -10);
            
            // Add horizontal layout
            var hlg = statsObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30;
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            
            // Add background
            var statsBg = statsObj.AddComponent<Image>();
            statsBg.color = new Color(0, 0, 0, 0.7f);
        }
        
        // Create effects container
        if (effectsContainer == null)
        {
            GameObject effectsObj = new GameObject("EffectsContainer");
            effectsObj.transform.SetParent(mainCanvas, false);
            effectsContainer = effectsObj.AddComponent<RectTransform>();
            effectsContainer.anchorMin = Vector2.zero;
            effectsContainer.anchorMax = Vector2.one;
            effectsContainer.sizeDelta = Vector2.zero;
            effectsContainer.anchoredPosition = Vector2.zero;
        }
    }
    
    public void SetBackgroundImage(Sprite sprite)
    {
        if (backgroundImage != null && sprite != null)
        {
            StartCoroutine(FadeBackground(sprite));
        }
    }
    
    IEnumerator FadeBackground(Sprite newSprite)
    {
        float elapsed = 0;
        Color startColor = backgroundImage.color;
        
        // Fade out
        while (elapsed < backgroundFadeDuration / 2)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0, elapsed / (backgroundFadeDuration / 2));
            backgroundImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        // Change sprite
        backgroundImage.sprite = newSprite;
        backgroundImage.preserveAspect = true;
        
        // Fade in
        elapsed = 0;
        while (elapsed < backgroundFadeDuration / 2)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsed / (backgroundFadeDuration / 2));
            backgroundImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        
        backgroundImage.color = Color.white;
    }
    
    public void CreateStatDisplay(string label, TextMeshProUGUI textComponent, Color color)
    {
        if (statsPrefab == null)
        {
            GameObject statObj = new GameObject($"Stat_{label}");
            statObj.transform.SetParent(statsContainer, false);
            
            // Add background
            var bg = statObj.AddComponent<Image>();
            bg.color = new Color(color.r, color.g, color.b, 0.2f);
            
            // Add text
            var text = statObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{label}: 0";
            text.fontSize = 22;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            
            // Add layout element
            var layoutElement = statObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 40;
            layoutElement.flexibleWidth = 1;
            
            // Store reference
            if (label.Contains("Profit")) profitText = text;
            else if (label.Contains("Relation")) relationshipText = text;
            else if (label.Contains("Suspicion")) suspicionText = text;
        }
    }
    
    public RectTransform GetEffectsContainer()
    {
        return effectsContainer;
    }
    
    public GameObject CreateFloatingTextPrefab()
    {
        if (floatingTextPrefab == null)
        {
            floatingTextPrefab = new GameObject("FloatingTextPrefab");
            var text = floatingTextPrefab.AddComponent<TextMeshProUGUI>();
            text.fontSize = 28;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            
            // Add outline for better visibility
            var outline = floatingTextPrefab.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);
            
            // Make it a prefab
            floatingTextPrefab.SetActive(false);
        }
        return floatingTextPrefab;
    }
}