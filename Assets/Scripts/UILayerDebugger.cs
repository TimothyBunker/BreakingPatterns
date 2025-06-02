using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class UILayerDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool autoFixLayers = true;
    [SerializeField] private KeyCode debugToggleKey = KeyCode.F12;
    
    private Dictionary<string, Transform> uiElements = new Dictionary<string, Transform>();
    private Canvas mainCanvas;
    private GameObject debugPanel;
    private TextMeshProUGUI debugText;
    
    void Start()
    {
        mainCanvas = FindFirstObjectByType<Canvas>();
        if (showDebugInfo)
            CreateDebugPanel();
            
        // Initial scan
        ScanUIHierarchy();
        if (autoFixLayers)
            FixAllLayers();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(debugToggleKey))
        {
            showDebugInfo = !showDebugInfo;
            if (debugPanel != null)
                debugPanel.SetActive(showDebugInfo);
        }
        
        if (showDebugInfo && debugText != null)
        {
            UpdateDebugInfo();
        }
    }
    
    void CreateDebugPanel()
    {
        debugPanel = new GameObject("UIDebugPanel");
        debugPanel.transform.SetParent(mainCanvas?.transform ?? transform);
        
        RectTransform rect = debugPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0.3f, 1);
        rect.offsetMin = new Vector2(10, 10);
        rect.offsetMax = new Vector2(-10, -10);
        
        Image bg = debugPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(debugPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = 14;
        debugText.color = Color.white;
        debugText.alignment = TextAlignmentOptions.TopLeft;
        
        // Set debug panel to highest layer
        UILayerManager.Instance.SetUILayer(debugPanel, UILayerManager.UILayer.Debug);
    }
    
    void ScanUIHierarchy()
    {
        uiElements.Clear();
        
        if (mainCanvas == null)
        {
            mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas == null) return;
        }
        
        // Scan all children
        foreach (Transform child in mainCanvas.transform)
        {
            RegisterUIElement(child);
        }
    }
    
    void RegisterUIElement(Transform element)
    {
        if (element == null) return;
        
        string key = element.name + "_" + element.GetInstanceID();
        uiElements[key] = element;
        
        // Recursively register children
        foreach (Transform child in element)
        {
            RegisterUIElement(child);
        }
    }
    
    void UpdateDebugInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>UI Layer Debug Info</b>");
        sb.AppendLine($"<color=#FFD700>Press {debugToggleKey} to toggle</color>");
        sb.AppendLine();
        
        // Get all UI elements sorted by sibling index
        var sortedElements = mainCanvas.transform.Cast<Transform>()
            .OrderBy(t => t.GetSiblingIndex())
            .ToList();
        
        sb.AppendLine("<b>Layer Order (Back to Front):</b>");
        for (int i = 0; i < sortedElements.Count; i++)
        {
            Transform element = sortedElements[i];
            if (element == debugPanel?.transform) continue; // Skip debug panel
            
            string layerName = GetExpectedLayer(element.name);
            string status = IsInCorrectPosition(element) ? "✓" : "✗";
            Color statusColor = IsInCorrectPosition(element) ? Color.green : Color.red;
            
            sb.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(statusColor)}>{status}</color> {i}: {element.name} ({layerName})");
        }
        
        // Check for common issues
        sb.AppendLine();
        sb.AppendLine("<b>Issues:</b>");
        List<string> issues = CheckForIssues();
        if (issues.Count == 0)
        {
            sb.AppendLine("<color=#00FF00>No issues found!</color>");
        }
        else
        {
            foreach (string issue in issues)
            {
                sb.AppendLine($"<color=#FF0000>• {issue}</color>");
            }
        }
        
        debugText.text = sb.ToString();
    }
    
    string GetExpectedLayer(string elementName)
    {
        string lower = elementName.ToLower();
        
        if (lower.Contains("background") && !lower.Contains("overlay"))
            return "Background";
        if (lower.Contains("overlay"))
            return "BackgroundOverlay";
        if (lower.Contains("character"))
            return "Characters";
        if (lower.Contains("dialogue") && !lower.Contains("content"))
            return "DialogueBox";
        if (lower.Contains("choice"))
            return "ChoiceButtons";
        if (lower.Contains("stats"))
            return "StatsPanel";
        if (lower.Contains("effect") || lower.Contains("floating"))
            return "Effects";
        if (lower.Contains("flash"))
            return "ScreenFlash";
        
        return "Unknown";
    }
    
    bool IsInCorrectPosition(Transform element)
    {
        string expectedLayer = GetExpectedLayer(element.name);
        int currentIndex = element.GetSiblingIndex();
        int expectedIndex = GetExpectedIndex(expectedLayer);
        
        // Allow some flexibility in ordering within the same layer type
        return Mathf.Abs(currentIndex - expectedIndex) <= 2;
    }
    
    int GetExpectedIndex(string layerName)
    {
        switch (layerName)
        {
            case "Background": return 0;
            case "BackgroundOverlay": return 1;
            case "Characters": return 2;
            case "DialogueBox": return 3;
            case "DialogueContent": return 4;
            case "ChoiceButtons": return 5;
            case "StatsPanel": return 6;
            case "Effects": return 7;
            case "ScreenFlash": return 8;
            default: return 99;
        }
    }
    
    List<string> CheckForIssues()
    {
        List<string> issues = new List<string>();
        
        // Check if backgrounds are behind characters
        Transform bg = FindElement("background");
        Transform chars = FindElement("character");
        if (bg != null && chars != null && bg.GetSiblingIndex() > chars.GetSiblingIndex())
        {
            issues.Add("Background is in front of characters!");
        }
        
        // Check if dialogue is behind effects
        Transform dialogue = FindElement("dialogue");
        Transform effects = FindElement("effects");
        if (dialogue != null && effects != null && dialogue.GetSiblingIndex() > effects.GetSiblingIndex())
        {
            issues.Add("Dialogue is in front of effects!");
        }
        
        // Check for duplicate canvases
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        if (allCanvases.Length > 1)
        {
            issues.Add($"Multiple canvases detected ({allCanvases.Length})!");
        }
        
        // Check for missing RectTransforms
        foreach (Transform child in mainCanvas.transform)
        {
            if (child.GetComponent<RectTransform>() == null)
            {
                issues.Add($"{child.name} is missing RectTransform!");
            }
        }
        
        return issues;
    }
    
    Transform FindElement(string nameContains)
    {
        if (mainCanvas == null) return null;
        
        foreach (Transform child in mainCanvas.transform)
        {
            if (child.name.ToLower().Contains(nameContains.ToLower()))
                return child;
        }
        return null;
    }
    
    public void FixAllLayers()
    {
        if (mainCanvas == null) return;
        
        Debug.Log("UILayerDebugger: Fixing all UI layers...");
        
        // Get all elements and sort them properly
        List<Transform> elements = new List<Transform>();
        foreach (Transform child in mainCanvas.transform)
        {
            elements.Add(child);
        }
        
        // Sort by expected layer order
        elements.Sort((a, b) => 
        {
            int indexA = GetExpectedIndex(GetExpectedLayer(a.name));
            int indexB = GetExpectedIndex(GetExpectedLayer(b.name));
            return indexA.CompareTo(indexB);
        });
        
        // Apply the correct order
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].SetSiblingIndex(i);
        }
        
        // Use UILayerManager to set proper layers
        UILayerManager.Instance.OrganizeUIHierarchy(mainCanvas.transform);
        
        Debug.Log("UILayerDebugger: Layer fix complete!");
    }
}