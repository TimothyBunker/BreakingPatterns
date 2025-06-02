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
        // Create background
        GameObject bgObj = new GameObject("NewBackground");
        bgObj.transform.SetParent(mainCanvas.transform, false);
        newBackgroundImage = bgObj.AddComponent<Image>();
        newBackgroundImage.color = Color.gray; // Default color
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.SetAsFirstSibling(); // Ensure it's at the back
        
        // Create left character
        GameObject leftCharObj = new GameObject("NewLeftCharacter");
        leftCharObj.transform.SetParent(mainCanvas.transform, false);
        newLeftCharacter = leftCharObj.AddComponent<Image>();
        newLeftCharacter.preserveAspect = true;
        
        RectTransform leftRect = leftCharObj.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0, 0.2f);
        leftRect.anchorMax = new Vector2(0.3f, 0.8f);
        leftRect.anchoredPosition = Vector2.zero;
        leftRect.sizeDelta = Vector2.zero;
        
        // Create right character  
        GameObject rightCharObj = new GameObject("NewRightCharacter");
        rightCharObj.transform.SetParent(mainCanvas.transform, false);
        newRightCharacter = rightCharObj.AddComponent<Image>();
        newRightCharacter.preserveAspect = true;
        
        RectTransform rightRect = rightCharObj.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.7f, 0.2f);
        rightRect.anchorMax = new Vector2(1f, 0.8f);
        rightRect.anchoredPosition = Vector2.zero;
        rightRect.sizeDelta = Vector2.zero;
        
        // Create dialogue panel
        GameObject dialogueObj = new GameObject("NewDialoguePanel");
        dialogueObj.transform.SetParent(mainCanvas.transform, false);
        newDialoguePanel = dialogueObj.AddComponent<RectTransform>();
        
        Image panelBg = dialogueObj.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);
        
        newDialoguePanel.anchorMin = new Vector2(0.1f, 0);
        newDialoguePanel.anchorMax = new Vector2(0.9f, 0.3f);
        newDialoguePanel.offsetMin = new Vector2(0, 20);
        newDialoguePanel.offsetMax = new Vector2(0, 0);
        
        // Create dialogue text
        GameObject textObj = new GameObject("NewDialogueText");
        textObj.transform.SetParent(newDialoguePanel, false);
        newDialogueText = textObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);
        
        newDialogueText.fontSize = 20;
        newDialogueText.color = Color.white;
        newDialogueText.alignment = TextAlignmentOptions.TopLeft;
    }
    
    void HideOriginalUI()
    {
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
    }
    
    IEnumerator InterceptAndDisplay()
    {
        while (true)
        {
            yield return null;
            
            // Copy background
            if (dialogueManager.bgImage != null && dialogueManager.bgImage.sprite != null)
            {
                newBackgroundImage.sprite = dialogueManager.bgImage.sprite;
                newBackgroundImage.color = Color.white;
            }
            
            // Copy characters
            if (dialogueManager.charLeftImage != null && dialogueManager.charLeftImage.sprite != null)
            {
                newLeftCharacter.sprite = dialogueManager.charLeftImage.sprite;
                newLeftCharacter.color = Color.white;
                newLeftCharacter.enabled = true;
            }
            else
            {
                newLeftCharacter.enabled = false;
            }
            
            if (dialogueManager.charRightImage != null && dialogueManager.charRightImage.sprite != null)
            {
                newRightCharacter.sprite = dialogueManager.charRightImage.sprite;
                newRightCharacter.color = Color.white;
                newRightCharacter.enabled = true;
            }
            else
            {
                newRightCharacter.enabled = false;
            }
            
            // Copy text
            if (dialogueManager.bodyLabel != null && !string.IsNullOrEmpty(dialogueManager.bodyLabel.text))
            {
                newDialogueText.text = dialogueManager.bodyLabel.text;
            }
        }
    }
}