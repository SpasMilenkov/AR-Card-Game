using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI gameTitleText;
    public Button playButton;
    public Image backgroundImageComponent;
    public Canvas mainCanvas;

    [Header("Scene Reference")]
    public string gameSceneName = "GameScene";

    [Header("Visual Settings")]
    public Color titleColor = new Color(0.9f, 0.5f, 0.1f);
    public Color buttonColor = new Color(0.6f, 0.4f, 0.2f);
    public Color buttonTextColor = Color.white;
    public Sprite backgroundImage;
    public Sprite buttonSprite;

    [Header("Audio Settings")]
    public bool playMenuMusicOnStart = true;

    // Reference to StyleManager (optional)
    private StyleManager styleManager;
    private AudioManager audioManager;
    private CanvasScaler canvasScaler;

    private void Start()
    {
        // Get reference to AudioManager
        audioManager = AudioManager.Instance;

        // Play menu music if available
        if (audioManager != null && playMenuMusicOnStart)
        {
            audioManager.PlayMusic(audioManager.menuTheme);
        }

        // Initialize style manager if available
        styleManager = FindObjectOfType<StyleManager>();

        // Set up canvas scaler
        SetupCanvasScaler();

        // Set up background
        SetupBackground();

        // Set up button listener
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        // Apply styling
        ApplyStyles();

        // Play entrance animations
        StartCoroutine(PlayMenuAnimation());
    }

    private void SetupCanvasScaler()
    {
        // Get or add CanvasScaler component
        if (mainCanvas != null)
        {
            canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
                canvasScaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();

            // Configure canvas scaler for responsive UI
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        }
    }

    private void SetupBackground()
    {
        if (backgroundImageComponent != null && backgroundImage != null)
        {
            backgroundImageComponent.sprite = backgroundImage;
            backgroundImageComponent.preserveAspect = false;

            // Make background stretch to fill screen
            RectTransform bgRect = backgroundImageComponent.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
        }
    }

    private void ApplyStyles()
    {
        if (styleManager != null)
        {
            // Use StyleManager if available
            ApplyStylesWithManager();
        }
        else
        {
            // Apply custom styling
            ApplyCustomStyles();
        }
    }

    private void ApplyStylesWithManager()
    {
        if (gameTitleText != null)
            styleManager.StyleText(gameTitleText, true);

        if (playButton != null)
            styleManager.StyleButton(playButton, "Play");
    }

    private void ApplyCustomStyles()
    {
        // Style title
        if (gameTitleText != null)
        {
            gameTitleText.color = titleColor;
            gameTitleText.fontSize = 48;
            gameTitleText.fontStyle = FontStyles.Bold;

            // Make title responsive to screen size
            gameTitleText.enableAutoSizing = true;
            gameTitleText.fontSizeMin = 32;
            gameTitleText.fontSizeMax = 72;

            // Add shadow for better readability
            AddShadowIfNeeded(gameTitleText.gameObject, new Vector2(3, -3));
        }

        // Style button
        if (playButton != null)
        {
            // Style button background
            Image buttonImage = playButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (buttonSprite != null)
                    buttonImage.sprite = buttonSprite;
                buttonImage.color = buttonColor;
            }

            // Style button text
            TextMeshProUGUI buttonText = playButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "PLAY";
                buttonText.color = buttonTextColor;
                buttonText.fontSize = 24;
                buttonText.fontStyle = FontStyles.Bold;

                // Make text responsive
                buttonText.enableAutoSizing = true;
                buttonText.fontSizeMin = 18;
                buttonText.fontSizeMax = 36;
            }

            // Add shadow to button
            AddShadowIfNeeded(playButton.gameObject, new Vector2(4, -4));
        }
    }

    private void AddShadowIfNeeded(GameObject targetObject, Vector2 distance)
    {
        Shadow shadow = targetObject.GetComponent<Shadow>();
        if (shadow == null)
            shadow = targetObject.AddComponent<Shadow>();

        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = distance;
    }

    private void OnPlayButtonClicked()
    {
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
            // Stop menu music when transitioning to game scene
            audioManager.StopMusic();
        }

        // Play button animation and load game scene
        StartCoroutine(PlayButtonClickAnimation());
        Invoke(nameof(LoadGameScene), 0.3f);
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator PlayButtonClickAnimation()
    {
        if (playButton == null) yield break;

        RectTransform rect = playButton.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;

        // Shrink animation
        rect.localScale = originalScale * 0.9f;
        yield return new WaitForSeconds(0.1f);
        rect.localScale = originalScale;

        // Scale-up animation
        float duration = 0.2f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float scale = Mathf.Lerp(1.0f, 1.2f, elapsed / duration);
            rect.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator PlayMenuAnimation()
    {
        // Animate title first
        yield return StartCoroutine(AnimateTitle());

        // Short delay before button appears
        yield return new WaitForSeconds(0.3f);

        // Then animate button
        yield return StartCoroutine(AnimateButton());
    }

    private IEnumerator AnimateTitle()
    {
        if (gameTitleText == null) yield break;

        // Setup title animation
        CanvasGroup titleGroup = GetOrAddComponent<CanvasGroup>(gameTitleText.gameObject);
        titleGroup.alpha = 0;

        RectTransform titleRect = gameTitleText.GetComponent<RectTransform>();
        Vector2 targetPosition = titleRect.anchoredPosition;
        titleRect.anchoredPosition = targetPosition - new Vector2(0, 50);

        // Fade in and move up
        float duration = 0.8f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            titleGroup.alpha = Mathf.Lerp(0, 1, t);
            titleRect.anchoredPosition = Vector2.Lerp(
                targetPosition - new Vector2(0, 50),
                targetPosition,
                t
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final state is correct
        titleGroup.alpha = 1;
        titleRect.anchoredPosition = targetPosition;
    }

    private IEnumerator AnimateButton()
    {
        if (playButton == null) yield break;

        // Setup button animation
        CanvasGroup buttonGroup = GetOrAddComponent<CanvasGroup>(playButton.gameObject);
        buttonGroup.alpha = 0;

        RectTransform buttonRect = playButton.GetComponent<RectTransform>();
        Vector3 originalScale = buttonRect.localScale;
        buttonRect.localScale = Vector3.zero;

        // Scale up and fade in
        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            buttonGroup.alpha = Mathf.Lerp(0, 1, t);
            buttonRect.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final state is correct
        buttonGroup.alpha = 1;
        buttonRect.localScale = originalScale;

        // Add continuous pulse animation
        StartCoroutine(PulseButton(buttonRect, originalScale));
    }

    private IEnumerator PulseButton(RectTransform buttonRect, Vector3 originalScale)
    {
        if (buttonRect == null) yield break;

        while (true) // Continuous pulse
        {
            // Full pulse cycle using sin wave
            float duration = 2.0f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                float scale = 1.0f + 0.05f * Mathf.Sin((elapsed / duration) * Mathf.PI * 2);
                buttonRect.localScale = originalScale * scale;

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    // Helper method to get or add a component
    private T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
            component = obj.AddComponent<T>();
        return component;
    }
}