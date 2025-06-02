using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickSetup : MonoBehaviour
{
    [Header("Run this, then delete this component")]
    public bool runSetup = false;
    
    void Start()
    {
        if (runSetup)
            SetupEverything();
    }
    
    void SetupEverything()
    {
        Debug.Log("=== QUICK SETUP STARTING ===");
        
        // 1. Create Canvas if needed
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("Creating Canvas...");
            GameObject canvasObj = new GameObject("GameCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // 2. Create UI elements
        Debug.Log("Creating UI elements...");
        
        // Background
        GameObject bgObj = new GameObject("BackgroundImage");
        bgObj.transform.SetParent(canvas.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Character containers
        GameObject charLeft = new GameObject("CharacterLeft");
        charLeft.transform.SetParent(canvas.transform, false);
        Image charLeftImage = charLeft.AddComponent<Image>();
        RectTransform leftRect = charLeft.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0, 0.3f);
        leftRect.anchorMax = new Vector2(0.3f, 0.8f);
        
        GameObject charRight = new GameObject("CharacterRight");
        charRight.transform.SetParent(canvas.transform, false);
        Image charRightImage = charRight.AddComponent<Image>();
        RectTransform rightRect = charRight.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.7f, 0.3f);
        rightRect.anchorMax = new Vector2(1f, 0.8f);
        
        // Text box
        GameObject textBox = new GameObject("DialogueTextBox");
        textBox.transform.SetParent(canvas.transform, false);
        Image textBoxBg = textBox.AddComponent<Image>();
        textBoxBg.color = new Color(0, 0, 0, 0.8f);
        RectTransform textBoxRect = textBox.GetComponent<RectTransform>();
        textBoxRect.anchorMin = new Vector2(0.1f, 0);
        textBoxRect.anchorMax = new Vector2(0.9f, 0.3f);
        textBoxRect.offsetMin = new Vector2(0, 20);
        
        // Text
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(textBox.transform, false);
        TextMeshProUGUI dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);
        dialogueText.fontSize = 20;
        dialogueText.color = Color.white;
        
        // Stats
        GameObject statsPanel = new GameObject("StatsPanel");
        statsPanel.transform.SetParent(canvas.transform, false);
        RectTransform statsRect = statsPanel.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0.9f);
        statsRect.anchorMax = new Vector2(1, 1);
        
        HorizontalLayoutGroup statsLayout = statsPanel.AddComponent<HorizontalLayoutGroup>();
        statsLayout.spacing = 50;
        statsLayout.padding = new RectOffset(50, 50, 10, 10);
        statsLayout.childAlignment = TextAnchor.MiddleCenter;
        
        // Create stat texts
        var profitText = CreateStatText("Profit: $0", statsPanel.transform);
        var relText = CreateStatText("Relations: 0", statsPanel.transform);
        var suspText = CreateStatText("Suspicion: 0/100", statsPanel.transform);
        
        // 3. Create or find DialogueManager
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.Log("Creating DialogueManager...");
            GameObject dmObj = new GameObject("DialogueManager");
            dialogueManager = dmObj.AddComponent<DialogueManager>();
        }
        
        // Assign references
        dialogueManager.bodyLabel = dialogueText;
        dialogueManager.bgImage = bgImage;
        dialogueManager.charLeftImage = charLeftImage;
        dialogueManager.charRightImage = charRightImage;
        
        Debug.Log("DialogueManager references assigned!");
        
        // 4. Create or find GameManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.Log("Creating GameManager...");
            GameObject gmObj = new GameObject("GameManager");
            gameManager = gmObj.AddComponent<GameManager>();
        }
        
        // Assign stat references
        gameManager.profitText = profitText;
        gameManager.relText = relText;
        gameManager.suspText = suspText;
        
        Debug.Log("GameManager references assigned!");
        
        // 5. Create DialogueJsonLoader
        DialogueJsonLoader jsonLoader = FindFirstObjectByType<DialogueJsonLoader>();
        if (jsonLoader == null)
        {
            Debug.Log("Creating DialogueJsonLoader...");
            GameObject loaderObj = new GameObject("DialogueJsonLoader");
            jsonLoader = loaderObj.AddComponent<DialogueJsonLoader>();
            jsonLoader.targetManager = dialogueManager;
        }
        
        Debug.Log("=== SETUP COMPLETE ===");
        Debug.Log("Now press Play again to start the game!");
        
        // Disable this component
        this.enabled = false;
    }
    
    TextMeshProUGUI CreateStatText(string text, Transform parent)
    {
        GameObject obj = new GameObject("Stat_" + text.Split(':')[0]);
        obj.transform.SetParent(parent, false);
        
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        
        LayoutElement layout = obj.AddComponent<LayoutElement>();
        layout.preferredWidth = 200;
        
        return tmp;
    }
}