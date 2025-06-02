using UnityEngine;
using UnityEngine.UI;

public class UILayerManager : MonoBehaviour
{
    // Define layer order (lower numbers = further back)
    public enum UILayer
    {
        Background = 0,
        BackgroundOverlay = 1,
        Characters = 2,
        DialogueBox = 3,
        DialogueContent = 4,
        ChoiceButtons = 5,
        StatsPanel = 6,
        Effects = 7,
        ScreenFlash = 8,
        Debug = 9
    }
    
    private static UILayerManager instance;
    public static UILayerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<UILayerManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("UILayerManager");
                    instance = obj.AddComponent<UILayerManager>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    
    public void SetUILayer(GameObject obj, UILayer layer)
    {
        if (obj == null) return;
        
        // Set sibling index based on layer
        obj.transform.SetSiblingIndex((int)layer);
        
        // Also set canvas sorting order if it has a Canvas component
        Canvas canvas = obj.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = (int)layer * 10;
        }
        
        // Set sorting order for any child canvases
        Canvas[] childCanvases = obj.GetComponentsInChildren<Canvas>(true);
        foreach (var childCanvas in childCanvases)
        {
            childCanvas.overrideSorting = true;
            childCanvas.sortingOrder = (int)layer * 10 + 1;
        }
    }
    
    public void OrganizeUIHierarchy(Transform canvasTransform)
    {
        if (canvasTransform == null) return;
        
        // Find and organize all UI elements
        foreach (Transform child in canvasTransform)
        {
            string childName = child.name.ToLower();
            
            if (childName.Contains("background") && !childName.Contains("overlay"))
            {
                SetUILayer(child.gameObject, UILayer.Background);
            }
            else if (childName.Contains("overlay"))
            {
                SetUILayer(child.gameObject, UILayer.BackgroundOverlay);
            }
            else if (childName.Contains("character"))
            {
                SetUILayer(child.gameObject, UILayer.Characters);
            }
            else if (childName.Contains("dialogue") && !childName.Contains("content"))
            {
                SetUILayer(child.gameObject, UILayer.DialogueBox);
            }
            else if (childName.Contains("choice"))
            {
                SetUILayer(child.gameObject, UILayer.ChoiceButtons);
            }
            else if (childName.Contains("stats"))
            {
                SetUILayer(child.gameObject, UILayer.StatsPanel);
            }
            else if (childName.Contains("effect") || childName.Contains("floating"))
            {
                SetUILayer(child.gameObject, UILayer.Effects);
            }
            else if (childName.Contains("flash"))
            {
                SetUILayer(child.gameObject, UILayer.ScreenFlash);
            }
        }
    }
}