using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class QuickEndSceneSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    [Tooltip("Click this in Play mode to auto-create the UI")]
    public bool createUIStructure = false;
    
    void Update()
    {
        if (createUIStructure)
        {
            createUIStructure = false;
            CreateEndSceneUI();
        }
    }
    
    void CreateEndSceneUI()
    {
        // Get or create Canvas
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add Canvas Scaler
        CanvasScaler scaler = GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add Graphic Raycaster
        if (!GetComponent<GraphicRaycaster>())
            gameObject.AddComponent<GraphicRaycaster>();
        
        // Create Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.08f, 1f);
        RectTransform bgRect = bg.rectTransform;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Create Content Panel
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(transform, false);
        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(1600, 900);
        
        // Create Ending Title
        GameObject titleObj = new GameObject("EndingTitle");
        titleObj.transform.SetParent(contentPanel.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "THE END";
        title.fontSize = 72;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
        RectTransform titleRect = title.rectTransform;
        titleRect.anchoredPosition = new Vector2(0, 300);
        titleRect.sizeDelta = new Vector2(800, 100);
        
        // Create Ending Description
        GameObject descObj = new GameObject("EndingDescription");
        descObj.transform.SetParent(contentPanel.transform, false);
        TextMeshProUGUI desc = descObj.AddComponent<TextMeshProUGUI>();
        desc.text = "Your journey has concluded";
        desc.fontSize = 36;
        desc.alignment = TextAlignmentOptions.Center;
        desc.color = new Color(0.8f, 0.8f, 0.8f);
        RectTransform descRect = desc.rectTransform;
        descRect.anchoredPosition = new Vector2(0, 200);
        descRect.sizeDelta = new Vector2(1000, 60);
        
        // Create Character Image
        GameObject charObj = new GameObject("CharacterImage");
        charObj.transform.SetParent(contentPanel.transform, false);
        Image charImg = charObj.AddComponent<Image>();
        charImg.color = new Color(1, 1, 1, 0.5f);
        RectTransform charRect = charImg.rectTransform;
        charRect.anchoredPosition = new Vector2(-500, 0);
        charRect.sizeDelta = new Vector2(400, 600);
        
        // Create Epilogue Text
        GameObject epilogueObj = new GameObject("EpilogueText");
        epilogueObj.transform.SetParent(contentPanel.transform, false);
        TextMeshProUGUI epilogue = epilogueObj.AddComponent<TextMeshProUGUI>();
        epilogue.text = "Every choice led you here...";
        epilogue.fontSize = 24;
        epilogue.alignment = TextAlignmentOptions.Left;
        epilogue.color = new Color(0.9f, 0.9f, 0.9f);
        RectTransform epilogueRect = epilogue.rectTransform;
        epilogueRect.anchoredPosition = new Vector2(100, 0);
        epilogueRect.sizeDelta = new Vector2(800, 400);
        
        // Create Stats Text
        GameObject statsObj = new GameObject("StatsText");
        statsObj.transform.SetParent(contentPanel.transform, false);
        TextMeshProUGUI stats = statsObj.AddComponent<TextMeshProUGUI>();
        stats.text = "Final Stats:\nProfit: $0\nRelationships: Unknown\nSuspicion: 0%";
        stats.fontSize = 28;
        stats.alignment = TextAlignmentOptions.Left;
        stats.color = new Color(0.7f, 0.9f, 0.7f);
        RectTransform statsRect = stats.rectTransform;
        statsRect.anchoredPosition = new Vector2(600, 0);
        statsRect.sizeDelta = new Vector2(400, 300);
        
        // Create Button Panel
        GameObject buttonPanel = new GameObject("ButtonPanel");
        buttonPanel.transform.SetParent(contentPanel.transform, false);
        RectTransform buttonPanelRect = buttonPanel.AddComponent<RectTransform>();
        buttonPanelRect.anchoredPosition = new Vector2(0, -350);
        buttonPanelRect.sizeDelta = new Vector2(500, 60);
        HorizontalLayoutGroup hlg = buttonPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 50;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        
        // Create Restart Button
        GameObject restartBtn = new GameObject("RestartButton");
        restartBtn.transform.SetParent(buttonPanel.transform, false);
        Image restartImg = restartBtn.AddComponent<Image>();
        restartImg.color = new Color(0.2f, 0.6f, 0.2f);
        Button restartButton = restartBtn.AddComponent<Button>();
        restartButton.targetGraphic = restartImg;
        
        GameObject restartText = new GameObject("Text");
        restartText.transform.SetParent(restartBtn.transform, false);
        TextMeshProUGUI restartTMP = restartText.AddComponent<TextMeshProUGUI>();
        restartTMP.text = "New Game";
        restartTMP.fontSize = 24;
        restartTMP.alignment = TextAlignmentOptions.Center;
        restartTMP.color = Color.white;
        RectTransform restartTextRect = restartTMP.rectTransform;
        restartTextRect.anchorMin = Vector2.zero;
        restartTextRect.anchorMax = Vector2.one;
        restartTextRect.sizeDelta = Vector2.zero;
        
        LayoutElement restartLE = restartBtn.AddComponent<LayoutElement>();
        restartLE.preferredWidth = 200;
        restartLE.preferredHeight = 60;
        
        // Create Quit Button
        GameObject quitBtn = new GameObject("QuitButton");
        quitBtn.transform.SetParent(buttonPanel.transform, false);
        Image quitImg = quitBtn.AddComponent<Image>();
        quitImg.color = new Color(0.6f, 0.2f, 0.2f);
        Button quitButton = quitBtn.AddComponent<Button>();
        quitButton.targetGraphic = quitImg;
        
        GameObject quitText = new GameObject("Text");
        quitText.transform.SetParent(quitBtn.transform, false);
        TextMeshProUGUI quitTMP = quitText.AddComponent<TextMeshProUGUI>();
        quitTMP.text = "Quit";
        quitTMP.fontSize = 24;
        quitTMP.alignment = TextAlignmentOptions.Center;
        quitTMP.color = Color.white;
        RectTransform quitTextRect = quitTMP.rectTransform;
        quitTextRect.anchorMin = Vector2.zero;
        quitTextRect.anchorMax = Vector2.one;
        quitTextRect.sizeDelta = Vector2.zero;
        
        LayoutElement quitLE = quitBtn.AddComponent<LayoutElement>();
        quitLE.preferredWidth = 200;
        quitLE.preferredHeight = 60;
        
        // Add Background Image component (for dynamic backgrounds)
        GameObject bgImageObj = new GameObject("BackgroundImage");
        bgImageObj.transform.SetParent(transform, false);
        bgImageObj.transform.SetAsFirstSibling(); // Behind everything
        Image bgImage = bgImageObj.AddComponent<Image>();
        bgImage.color = new Color(1, 1, 1, 0.3f);
        RectTransform bgImageRect = bgImage.rectTransform;
        bgImageRect.anchorMin = Vector2.zero;
        bgImageRect.anchorMax = Vector2.one;
        bgImageRect.sizeDelta = Vector2.zero;
        
        // Try to auto-wire EndSceneController if it exists
        EndSceneController controller = FindObjectOfType<EndSceneController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<EndSceneController>();
        }
        
        // Wire up the UI elements
        controller.endingTitleText = title;
        controller.endingDescriptionText = desc;
        controller.epilogueText = epilogue;
        controller.statsText = stats;
        controller.backgroundImage = bgImage;
        controller.characterImage = charImg;
        controller.restartButton = restartButton;
        controller.quitButton = quitButton;
        
        Debug.Log("EndScene UI created successfully! EndSceneController has been wired up.");
    }
}