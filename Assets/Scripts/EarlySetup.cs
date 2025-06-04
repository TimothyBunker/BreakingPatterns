using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-200)] // Execute before everything else
public class EarlySetup : MonoBehaviour
{
    private Canvas mainCanvas;
    private GameObject statsPanel;
    private GameObject dialoguePanel;
    private TextMeshProUGUI profitText;
    private TextMeshProUGUI relationText;
    private TextMeshProUGUI suspicionText;
    
    void Awake()
    {
        Debug.Log("EarlySetup: Starting early initialization...");
        
        // Create main UI canvas
        CreateMainCanvas();
        
        // Create stats panel at top
        CreateStatsPanel();
        
        // Create dialogue panel with background
        CreateDialoguePanel();
        
        // Set up all DialogueManagers immediately
        SetupAllDialogueManagers();
        
        Debug.Log("EarlySetup: Early initialization complete");
    }
    
    void CreateMainCanvas()
    {
        GameObject canvasObj = new GameObject("EarlySetupCanvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Ensure EventSystem exists
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        Debug.Log("EarlySetup: Created main canvas");
    }
    
    void CreateStatsPanel()
    {
        // Create stats panel at top of screen
        statsPanel = new GameObject("StatsPanel");
        statsPanel.transform.SetParent(mainCanvas.transform, false);
        
        RectTransform statsRect = statsPanel.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.1f, 0.92f);
        statsRect.anchorMax = new Vector2(0.9f, 0.98f);
        
        // Add background
        Image statsBg = statsPanel.AddComponent<Image>();
        statsBg.color = new Color(0, 0, 0, 0.7f);
        
        // Add layout group
        HorizontalLayoutGroup layout = statsPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 30;
        layout.padding = new RectOffset(20, 20, 5, 5);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = false;
        layout.childForceExpandWidth = true;
        
        // Create stat text elements
        profitText = CreateStatText("Profit: $0", statsPanel.transform);
        relationText = CreateStatText("Relations: 50 (Business Partner)", statsPanel.transform);
        suspicionText = CreateStatText("Suspicion: 0/100", statsPanel.transform);
        
        // Connect stats to GameManager if it exists
        ConnectStatsToGameManager();
        
        Debug.Log("EarlySetup: Created stats panel");
    }
    
    TextMeshProUGUI CreateStatText(string text, Transform parent)
    {
        GameObject textObj = new GameObject("StatText");
        textObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        
        LayoutElement layoutElement = textObj.AddComponent<LayoutElement>();
        layoutElement.flexibleWidth = 1;
        layoutElement.preferredHeight = 25;
        
        return tmp;
    }
    
    void CreateDialoguePanel()
    {
        // Create dialogue panel with background
        dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(mainCanvas.transform, false);
        
        RectTransform dialogueRect = dialoguePanel.AddComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0.25f, 0.45f);
        dialogueRect.anchorMax = new Vector2(0.75f, 0.65f);
        
        // Add background
        Image dialogueBg = dialoguePanel.AddComponent<Image>();
        dialogueBg.color = new Color(0, 0, 0, 0.8f);
        
        // Create text container inside the panel
        GameObject textContainer = new GameObject("TextContainer");
        textContainer.transform.SetParent(dialoguePanel.transform, false);
        
        RectTransform textRect = textContainer.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);
        
        Debug.Log("EarlySetup: Created dialogue panel");
    }
    
    void SetupAllDialogueManagers()
    {
        // Find all DialogueManagers in the scene
        DialogueManager[] dialogueManagers = FindObjectsByType<DialogueManager>(FindObjectsSortMode.None);
        
        if (dialogueManagers.Length == 0)
        {
            Debug.Log("EarlySetup: No DialogueManagers found yet, will try again later");
            return;
        }
        
        foreach (DialogueManager dm in dialogueManagers)
        {
            SetupSingleDialogueManager(dm);
        }
        
        Debug.Log($"EarlySetup: Set up {dialogueManagers.Length} DialogueManager(s)");
    }
    
    void SetupSingleDialogueManager(DialogueManager dm)
    {
        Debug.Log($"EarlySetup: Setting up DialogueManager '{dm.name}'");
        
        // Create a hidden canvas for UI references
        GameObject dummyCanvas = new GameObject($"DummyCanvas_{dm.name}");
        Canvas canvas = dummyCanvas.AddComponent<Canvas>();
        canvas.enabled = false;
        
        // Create all required UI components
        if (dm.bodyLabel == null)
        {
            GameObject textObj = new GameObject("DummyBodyLabel");
            textObj.transform.SetParent(dummyCanvas.transform, false);
            dm.bodyLabel = textObj.AddComponent<TextMeshProUGUI>();
            Debug.Log($"EarlySetup: Created bodyLabel for {dm.name}");
        }
        
        if (dm.bgImage == null)
        {
            GameObject bgObj = new GameObject("DummyBG");
            bgObj.transform.SetParent(dummyCanvas.transform, false);
            dm.bgImage = bgObj.AddComponent<Image>();
            Debug.Log($"EarlySetup: Created bgImage for {dm.name}");
        }
        
        if (dm.charLeftImage == null)
        {
            GameObject charObj = new GameObject("DummyCharLeft");
            charObj.transform.SetParent(dummyCanvas.transform, false);
            dm.charLeftImage = charObj.AddComponent<Image>();
            Debug.Log($"EarlySetup: Created charLeftImage for {dm.name}");
        }
        
        if (dm.charRightImage == null)
        {
            GameObject charObj = new GameObject("DummyCharRight");
            charObj.transform.SetParent(dummyCanvas.transform, false);
            dm.charRightImage = charObj.AddComponent<Image>();
            Debug.Log($"EarlySetup: Created charRightImage for {dm.name}");
        }
        
        Debug.Log($"EarlySetup: Completed setup for DialogueManager '{dm.name}'");
    }
    
    void ConnectStatsToGameManager()
    {
        if (GameManager.Instance != null)
        {
            // Use reflection to assign our stat texts to GameManager
            var profitField = typeof(GameManager).GetField("profitText");
            var relField = typeof(GameManager).GetField("relText");
            var suspField = typeof(GameManager).GetField("suspText");
            
            if (profitField != null) profitField.SetValue(GameManager.Instance, profitText);
            if (relField != null) relField.SetValue(GameManager.Instance, relationText);
            if (suspField != null) suspField.SetValue(GameManager.Instance, suspicionText);
            
            // Force GameManager to update the UI with current values
            GameManager.Instance.GetType().GetMethod("UpdateUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(GameManager.Instance, null);
            
            Debug.Log("EarlySetup: Connected stats to GameManager and updated UI");
        }
        else
        {
            Debug.LogWarning("EarlySetup: GameManager not found, stats will show default values");
        }
    }
    
    void Start()
    {
        // Try again in Start in case DialogueManagers were created after Awake
        DialogueManager[] dialogueManagers = FindObjectsByType<DialogueManager>(FindObjectsSortMode.None);
        foreach (DialogueManager dm in dialogueManagers)
        {
            if (dm.bodyLabel == null || dm.charRightImage == null)
            {
                Debug.Log($"EarlySetup: Found unsetup DialogueManager in Start, fixing now");
                SetupSingleDialogueManager(dm);
            }
        }
        
        // Try to connect stats again in case GameManager was created after Awake
        if (GameManager.Instance != null)
        {
            ConnectStatsToGameManager();
        }
    }
}