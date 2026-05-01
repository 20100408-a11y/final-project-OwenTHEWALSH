using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HIM : MonoBehaviour
{
    [Header("Visibility Settings")]
    [SerializeField]
    private Renderer himRenderer;

    [SerializeField]
    private Renderer additionalRenderer;

    [SerializeField]
    private float shadowAlpha = 0.1f;

    [SerializeField]
    private float visibleAlpha = 1f;

    [SerializeField]
    private float fadeDuration = 0.5f;

    [Header("Text Settings")]
    [SerializeField]
    private TextMeshProUGUI textDisplay;

    [SerializeField]
    private float wordDelay = 0.5f;

    private Coroutine fadeCoroutine;
    private Coroutine typingCoroutine;
    private bool isVisible = false;
    private Material himMaterial;
    private Material additionalMaterial;

    private void Start()
    {
        if (himRenderer == null)
        {
            himRenderer = GetComponent<Renderer>();
        }

        if (himRenderer != null)
        {
            himMaterial = himRenderer.material;
            SetAlpha(himMaterial, shadowAlpha);
        }

        if (additionalRenderer != null)
        {
            additionalMaterial = additionalRenderer.material;
            SetAlpha(additionalMaterial, shadowAlpha);
        }
    }

    private void Update()
    {
        // Visibility logic only - no terror mode tracking
    }

    public void ShowHIM(string dialogueText = "")
    {
        // Ensure the GameObject is active before starting coroutines
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeRoutine(visibleAlpha));
        isVisible = true;

        if (!string.IsNullOrEmpty(dialogueText))
        {
            ShowTextBox(dialogueText);
        }
    }

    [SerializeField]
    private string sentenceToDisplay = "Hello...";

    public void ShowTextBox(string text = "")
    {
        string displayText = string.IsNullOrEmpty(text) ? sentenceToDisplay : text;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeOutText(displayText));
        Debug.Log($"HIM Says: {displayText}");
    }

    public void HideTextBox()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (textDisplay != null)
        {
            textDisplay.text = string.Empty;
        }

        Debug.Log("Text box hidden");
    }

    private IEnumerator TypeOutText(string sentence)
    {
        if (textDisplay == null)
        {
            Debug.LogWarning("HIM: TextMeshProUGUI component not assigned");
            yield break;
        }

        textDisplay.text = string.Empty;
        string[] words = sentence.Split(' ');
        string displayedText = string.Empty;

        foreach (string word in words)
        {
            displayedText += word + " ";
            textDisplay.text = displayedText.TrimEnd();
            yield return new WaitForSeconds(wordDelay);
        }

        typingCoroutine = null;
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (himMaterial == null)
            yield break;

        Color startHIMColor = himMaterial.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            float newAlpha = Mathf.Lerp(startHIMColor.a, targetAlpha, t);
            
            SetAlpha(himMaterial, newAlpha);
            
            if (additionalMaterial != null)
            {
                SetAlpha(additionalMaterial, newAlpha);
            }

            yield return null;
        }

        SetAlpha(himMaterial, targetAlpha);
        if (additionalMaterial != null)
        {
            SetAlpha(additionalMaterial, targetAlpha);
        }

        fadeCoroutine = null;
    }

    private void SetAlpha(Material material, float alpha)
    {
        if (material == null)
            return;

        Color color = material.color;
        color.a = alpha;
        material.color = color;
    }

    public bool IsVisible()
    {
        return isVisible;
    }
}
