using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// StyleManager provides centralized styling for UI elements across the game
/// </summary>
public class StyleManager : MonoBehaviour
{
    // Singleton pattern
    public static StyleManager Instance { get; private set; }

    [Header("Panel Styling")]
    public Color panelBackgroundColor = new Color(0.8f, 0.7f, 0.5f, 0.9f); // Sandy/wooden color
    public Color panelHeaderColor = new Color(0.6f, 0.4f, 0.2f, 0.9f); // Darker brown for headers

    [Header("Button Styling")]
    public Color buttonNormalColor = new Color(0.6f, 0.4f, 0.2f); // Brown for buttons
    public Color buttonHighlightColor = new Color(0.7f, 0.5f, 0.3f); // Lighter brown for highlights
    public Color buttonPressedColor = new Color(0.5f, 0.3f, 0.1f); // Darker brown for pressed state
    public Color buttonDisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Gray for disabled state

    [Header("Text Styling")]
    public Color headerTextColor = new Color(0.4f, 0.2f, 0.1f); // Dark brown for headers
    public Color standardTextColor = new Color(0.3f, 0.2f, 0.1f); // Brown for standard text
    public Color buttonTextColor = Color.white; // White for button text
    public Color positiveTextColor = new Color(0.0f, 0.5f, 0.0f); // Green for health, positive events
    public Color negativeTextColor = new Color(0.7f, 0.0f, 0.0f); // Red for damage, negative events
    public Color highlightTextColor = new Color(0.9f, 0.5f, 0.1f); // Orange-brown for highlights

    [Header("Effect Settings")]
    public Color shadowColor = new Color(0, 0, 0, 0.5f);
    public Vector2 shadowOffset = new Vector2(2, -2);
    public float buttonAnimationDuration = 0.05f;
    public float panelFadeDuration = 0.5f;

    [Header("UI Sprites")]
    public Sprite panelSprite;
    public Sprite buttonSprite;
    public Sprite buttonHighlightSprite;
    public Sprite buttonPressedSprite;

    [Header("Animation Settings")]
    public bool useAnimatedEffects = true;
    public float flashDuration = 0.3f;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #region Panel Styling

    /// <summary>
    /// Apply styling to a panel
    /// </summary>
    public void StylePanel(GameObject panel, string headerText = null)
    {
        if (panel == null) return;

        // Apply panel background
        if (panel.TryGetComponent<Image>(out var panelImage))
        {
            // Use custom sprite if available
            if (panelSprite != null)
            {
                panelImage.sprite = panelSprite;
                panelImage.type = Image.Type.Sliced;
            }

            panelImage.color = panelBackgroundColor;
        }

        // Add shadow for depth
        if (!panel.TryGetComponent<Shadow>(out var shadow))
            shadow = panel.AddComponent<Shadow>();

        shadow.effectColor = shadowColor;
        shadow.effectDistance = shadowOffset;

        // Add a decorative header if text is provided
        if (!string.IsNullOrEmpty(headerText))
            AddPanelHeader(panel, headerText);

        // Add panel outline
        if (!panel.TryGetComponent<Outline>(out var outline))
            outline = panel.AddComponent<Outline>();

        outline.effectColor = new Color(0.4f, 0.3f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(1, -1);
    }

    /// <summary>
    /// Adds a decorative header to a panel
    /// </summary>
    private void AddPanelHeader(GameObject panel, string headerText)
    {
        if (panel == null) return;

        // Look for existing header
        Transform headerTransform = panel.transform.Find("PanelHeader");
        GameObject headerObj;

        if (headerTransform != null)
        {
            headerObj = headerTransform.gameObject;
            TextMeshProUGUI headerTextComponent = headerObj.GetComponentInChildren<TextMeshProUGUI>();
            if (headerTextComponent != null)
                headerTextComponent.text = headerText;
            return;
        }

        // Create header object
        headerObj = new GameObject("PanelHeader");
        headerObj.transform.SetParent(panel.transform, false);

        // Set up RectTransform
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.sizeDelta = new Vector2(0, 30);
        headerRect.anchoredPosition = new Vector2(0, 15);

        // Add background image
        Image headerImage = headerObj.AddComponent<Image>();
        headerImage.color = panelHeaderColor;

        // Add text
        GameObject textObj = new GameObject("HeaderText");
        textObj.transform.SetParent(headerObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = headerText;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = 16;
        text.fontStyle = FontStyles.Bold;

        // Add shadow to text
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = shadowColor;
        textShadow.effectDistance = new Vector2(1, -1);
    }

    #endregion

    #region Button Styling

    /// <summary>
    /// Apply styling to a button
    /// </summary>
    public void StyleButton(Button button, string buttonText = null)
    {
        if (button == null) return;

        // Apply background image
        if (button.TryGetComponent<Image>(out var buttonImage))
        {
            // Apply button sprite if available
            if (buttonSprite != null)
            {
                buttonImage.sprite = buttonSprite;
                buttonImage.type = Image.Type.Sliced;
            }

            buttonImage.color = buttonNormalColor;
        }

        // Set up button transitions
        if (buttonHighlightSprite != null && buttonPressedSprite != null)
        {
            // Use sprite swap if we have the sprites
            button.transition = Selectable.Transition.SpriteSwap;

            SpriteState spriteState = new SpriteState();
            spriteState.highlightedSprite = buttonHighlightSprite;
            spriteState.pressedSprite = buttonPressedSprite;
            button.spriteState = spriteState;
        }
        else
        {
            // Otherwise use color tint
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = buttonNormalColor;
            colors.highlightedColor = buttonHighlightColor;
            colors.pressedColor = buttonPressedColor;
            colors.disabledColor = buttonDisabledColor;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        // Add shadow effect
        if (!button.gameObject.TryGetComponent<Shadow>(out var shadow))
            shadow = button.gameObject.AddComponent<Shadow>();

        shadow.effectColor = shadowColor;
        shadow.effectDistance = new Vector2(2, -2);

        // Style or update button text
        if (!string.IsNullOrEmpty(buttonText))
        {
            TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent == null)
            {
                // Create text game object if it doesn't exist
                GameObject textObj = new GameObject("ButtonText");
                textObj.transform.SetParent(button.transform, false);

                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(5, 5);
                textRect.offsetMax = new Vector2(-5, -5);

                textComponent = textObj.AddComponent<TextMeshProUGUI>();
            }

            // Set text properties
            textComponent.text = buttonText;
            StyleButtonText(textComponent);
        }
    }

    /// <summary>
    /// Style text that appears on buttons
    /// </summary>
    public void StyleButtonText(TextMeshProUGUI text)
    {
        if (text == null) return;

        text.color = buttonTextColor;
        text.fontSize = 14;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;

        // Add shadow effect
        if (!text.gameObject.TryGetComponent<Shadow>(out var shadow))
            shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = shadowColor;
        shadow.effectDistance = new Vector2(1, -1);
    }

    #endregion

    #region Text Styling

    /// <summary>
    /// Style a text element
    /// </summary>
    public void StyleText(TextMeshProUGUI text, bool isHeader = false)
    {
        if (text == null) return;

        // Set basic properties
        text.color = isHeader ? headerTextColor : standardTextColor;
        text.fontSize = isHeader ? 16 : 14;
        text.fontStyle = isHeader ? FontStyles.Bold : FontStyles.Normal;

        // Add shadow effect
        if (!text.gameObject.TryGetComponent<Shadow>(out var shadow))
            shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.3f);
        shadow.effectDistance = new Vector2(1, -1);

        // Add margin to text
        if (text.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.offsetMin = new Vector2(10, 10);
            rectTransform.offsetMax = new Vector2(-10, -10);
        }
    }

    #endregion

    #region Animation Effects

    /// <summary>
    /// Flash a panel with a color
    /// </summary>
    public System.Collections.IEnumerator FlashPanel(RectTransform panel, Color flashColor)
    {
        if (panel == null || !useAnimatedEffects) yield break;

        if (!panel.TryGetComponent<Image>(out var panelImage)) yield break;

        Color originalColor = panelImage.color;

        // Flash fading effect
        float duration = flashDuration;
        float elapsed = 0;

        while (elapsed < duration)
        {
            // Fade in then out
            float t = elapsed / duration;
            float fade = (t <= 0.5f) ? t * 2f : 2f - t * 2f; // 0->1->0 over duration

            panelImage.color = Color.Lerp(originalColor, flashColor, fade);

            elapsed += Time.deltaTime;
            yield return null;
        }

        panelImage.color = originalColor;
    }

    /// <summary>
    /// Animate button click
    /// </summary>
    public System.Collections.IEnumerator ButtonClickAnimation(Button button)
    {
        if (button == null || !useAnimatedEffects) yield break;

        RectTransform rect = button.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;

        // Slight shrink on press
        rect.localScale = originalScale * 0.95f;

        yield return new WaitForSeconds(buttonAnimationDuration);

        // Return to normal size
        rect.localScale = originalScale;
    }

    /// <summary>
    /// Fade in a panel
    /// </summary>
    public System.Collections.IEnumerator PanelFadeInAnimation(GameObject panel)
    {
        if (panel == null || !useAnimatedEffects) yield break;

        if (!panel.TryGetComponent<CanvasGroup>(out var canvasGroup))
            canvasGroup = panel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0;

        float duration = panelFadeDuration;
        float elapsed = 0;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    #endregion
}