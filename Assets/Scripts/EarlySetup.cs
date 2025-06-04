using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-200)] // Execute before everything else
public class EarlySetup : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("EarlySetup: Starting early initialization...");
        
        // Set up all DialogueManagers immediately
        SetupAllDialogueManagers();
        
        Debug.Log("EarlySetup: Early initialization complete");
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
    }
}