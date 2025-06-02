using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FeedbackSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform feedbackContainer;
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Image screenFlashImage;
    [SerializeField] private GameObject criticalEffectPrefab;
    
    [Header("Animation Settings")]
    [SerializeField] private float floatDuration = 2f;
    [SerializeField] private float floatHeight = 100f;
    [SerializeField] private AnimationCurve floatCurve;
    [SerializeField] private AnimationCurve scaleCurve;
    
    [Header("Colors")]
    [SerializeField] private Color profitColor = Color.green;
    [SerializeField] private Color lossColor = Color.red;
    [SerializeField] private Color criticalSuccessColor = Color.yellow;
    [SerializeField] private Color criticalFailureColor = Color.magenta;
    [SerializeField] private Color suspicionColor = new Color(1f, 0.5f, 0f);
    
    [Header("Screen Effects")]
    [SerializeField] private float screenShakeIntensity = 5f;
    [SerializeField] private float screenShakeDuration = 0.3f;
    
    private Camera mainCamera;
    private Vector3 originalCameraPos;
    private AudioManager audioManager;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
            originalCameraPos = mainCamera.transform.position;
        audioManager = FindFirstObjectByType<AudioManager>();
        
        // Initialize animation curves if not set
        if (floatCurve == null || floatCurve.keys.Length == 0)
        {
            floatCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
        
        if (scaleCurve == null || scaleCurve.keys.Length == 0)
        {
            scaleCurve = new AnimationCurve();
            scaleCurve.AddKey(0f, 0.5f);
            scaleCurve.AddKey(0.1f, 1.2f);
            scaleCurve.AddKey(0.5f, 1f);
            scaleCurve.AddKey(1f, 0.8f);
        }
        
        if (screenFlashImage == null)
        {
            CreateScreenFlashImage();
        }
    }
    
    void CreateScreenFlashImage()
    {
        GameObject flashObj = new GameObject("ScreenFlash");
        flashObj.transform.SetParent(transform);
        screenFlashImage = flashObj.AddComponent<Image>();
        screenFlashImage.color = Color.clear;
        RectTransform rect = flashObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }
    
    public void ShowStatChangeFeedback(StatChangeResult result, Vector3 worldPosition)
    {
        StartCoroutine(ShowFeedbackSequence(result, worldPosition));
    }
    
    IEnumerator ShowFeedbackSequence(StatChangeResult result, Vector3 worldPosition)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        
        // Show critical effect first
        if (result.criticalType != CriticalType.None)
        {
            ShowCriticalEffect(result.criticalType, screenPos);
            yield return new WaitForSeconds(0.3f);
        }
        
        // Show individual stat changes
        float delay = 0f;
        
        if (result.profitChange != 0)
        {
            ShowFloatingText(result.profitChange, result.profitExpected, "Profit", screenPos + Vector3.right * delay * 50);
            delay += 0.2f;
        }
        
        if (result.relationshipChange != 0)
        {
            ShowFloatingText(result.relationshipChange, result.relationshipExpected, "Relationships", screenPos + Vector3.right * delay * 50);
            delay += 0.2f;
        }
        
        if (result.suspicionChange != 0)
        {
            ShowFloatingText(result.suspicionChange, result.suspicionExpected, "Suspicion", screenPos + Vector3.right * delay * 50);
            
            // Extra effects for high suspicion
            if (GameManager.Instance.Suspicion > 80)
            {
                StartCoroutine(DangerFlash());
            }
        }
        
        // Screen shake for dramatic changes
        if (Mathf.Abs(result.profitChange) > 20 || Mathf.Abs(result.suspicionChange) > 15)
        {
            StartCoroutine(ScreenShake());
        }
    }
    
    void ShowFloatingText(int actualValue, int expectedValue, string statName, Vector3 position)
    {
        if (floatingTextPrefab == null)
        {
            floatingTextPrefab = CreateFloatingTextPrefab();
        }
        
        Transform parent = feedbackContainer ?? transform;
        GameObject textObj = Instantiate(floatingTextPrefab, parent);
        
        if (textObj == null)
        {
            Debug.LogError("Failed to instantiate floating text prefab");
            return;
        }
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = textObj.AddComponent<RectTransform>();
        }
        rect.position = position;
        
        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textObj.AddComponent<TextMeshProUGUI>();
        }
        
        // Format text
        string sign = actualValue > 0 ? "+" : "";
        text.text = $"{sign}{actualValue} {statName}";
        
        // Show variance if different from expected
        if (actualValue != expectedValue && expectedValue != 0)
        {
            float variance = ((float)actualValue / expectedValue - 1f) * 100f;
            string varianceText = variance > 0 ? $" (+{variance:0}%)" : $" ({variance:0}%)";
            text.text += $"<size=14>{varianceText}</size>";
        }
        
        // Set color
        if (statName == "Suspicion")
            text.color = suspicionColor;
        else
            text.color = actualValue > 0 ? profitColor : lossColor;
        
        // Animate
        StartCoroutine(AnimateFloatingText(textObj, rect));
    }
    
    GameObject CreateFloatingTextPrefab()
    {
        GameObject prefab = new GameObject("FloatingText");
        TextMeshProUGUI text = prefab.AddComponent<TextMeshProUGUI>();
        text.fontSize = 24;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        return prefab;
    }
    
    IEnumerator AnimateFloatingText(GameObject textObj, RectTransform rect)
    {
        if (textObj == null || rect == null)
        {
            Debug.LogError("AnimateFloatingText: null object or rect");
            yield break;
        }
        
        Vector3 startPos = rect.position;
        float elapsed = 0f;
        var text = textObj.GetComponent<TextMeshProUGUI>();
        
        if (text == null)
        {
            Debug.LogError("AnimateFloatingText: no TextMeshProUGUI component");
            Destroy(textObj);
            yield break;
        }
        
        while (elapsed < floatDuration && textObj != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / floatDuration;
            
            // Move up
            if (rect != null)
            {
                rect.position = startPos + Vector3.up * floatHeight * floatCurve.Evaluate(progress);
                
                // Scale
                float scale = scaleCurve.Evaluate(progress);
                rect.localScale = Vector3.one * scale;
            }
            
            // Fade
            if (text != null)
            {
                Color c = text.color;
                c.a = 1f - progress;
                text.color = c;
            }
            
            yield return null;
        }
        
        if (textObj != null)
            Destroy(textObj);
    }
    
    void ShowCriticalEffect(CriticalType type, Vector3 position)
    {
        // Flash screen
        Color flashColor = type == CriticalType.Success ? criticalSuccessColor : criticalFailureColor;
        StartCoroutine(ScreenFlash(flashColor, 0.3f));
        
        // Play special sound
        if (audioManager != null)
        {
            if (type == CriticalType.Success)
                audioManager.PlayPositiveStatSound();
            else
                audioManager.PlayNegativeStatSound();
        }
        
        // Show critical text
        GameObject criticalText = Instantiate(floatingTextPrefab ?? CreateFloatingTextPrefab(), feedbackContainer ?? transform);
        var text = criticalText.GetComponent<TextMeshProUGUI>();
        text.text = type == CriticalType.Success ? "CRITICAL SUCCESS!" : "CRITICAL FAILURE!";
        text.color = flashColor;
        text.fontSize = 36;
        
        RectTransform rect = criticalText.GetComponent<RectTransform>();
        rect.position = position + Vector3.up * 50;
        
        StartCoroutine(AnimateCriticalText(criticalText, rect));
    }
    
    IEnumerator AnimateCriticalText(GameObject textObj, RectTransform rect)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Punch scale effect
            float scale = 1f + Mathf.Sin(progress * Mathf.PI * 4f) * 0.2f * (1f - progress);
            rect.localScale = Vector3.Lerp(startScale, Vector3.one, progress) * scale;
            
            // Rotate slightly
            rect.rotation = Quaternion.Euler(0, 0, Mathf.Sin(progress * Mathf.PI * 8f) * 5f);
            
            // Fade out
            var text = textObj.GetComponent<TextMeshProUGUI>();
            Color c = text.color;
            c.a = 1f - Mathf.Pow(progress, 2f);
            text.color = c;
            
            yield return null;
        }
        
        Destroy(textObj);
    }
    
    IEnumerator ScreenFlash(Color color, float duration)
    {
        screenFlashImage.color = color;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color c = color;
            c.a = Mathf.Lerp(0.5f, 0f, elapsed / duration);
            screenFlashImage.color = c;
            yield return null;
        }
        
        screenFlashImage.color = Color.clear;
    }
    
    IEnumerator DangerFlash()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return ScreenFlash(Color.red * 0.3f, 0.2f);
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    IEnumerator ScreenShake()
    {
        // Re-acquire camera reference in case scene changed
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("ScreenShake: No main camera found");
                yield break;
            }
            originalCameraPos = mainCamera.transform.position;
        }
        
        float elapsed = 0f;
        
        while (elapsed < screenShakeDuration && mainCamera != null)
        {
            elapsed += Time.deltaTime;
            float progress = 1f - (elapsed / screenShakeDuration);
            
            Vector3 offset = Random.insideUnitSphere * screenShakeIntensity * progress;
            offset.z = 0;
            
            if (mainCamera != null && mainCamera.transform != null)
                mainCamera.transform.position = originalCameraPos + offset;
            
            yield return null;
        }
        
        if (mainCamera != null && mainCamera.transform != null)
            mainCamera.transform.position = originalCameraPos;
    }
}