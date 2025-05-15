using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // Panel references
    public GameObject actionPanel;
    public GameObject targetSelectionPanel;
    public GameObject gameOverPanel;

    // Button references
    public Button attackButton;
    public Button abilityButton;
    public TextMeshProUGUI abilityButtonText;

    // Unit info display
    public TextMeshProUGUI currentUnitNameText;
    public TextMeshProUGUI currentUnitHealthText;

    // Game over screen
    public TextMeshProUGUI gameResultText;
    public Button restartButton;

    [Header("Layout Settings")]
    public bool forceApplyLayoutOnStart = true;
    public bool repositionPanels = true;
    public Vector2 actionPanelPosition = new Vector2(0, -300);
    public Vector2 targetPanelPosition = new Vector2(0, -300);
    public Vector2 gameOverPanelPosition = new Vector2(0, 0);

    // Reference to the style manager
    private StyleManager styleManager;
    private AudioManager audioManager;

    private void Awake()
    {
        // Find or create StyleManager
        styleManager = FindObjectOfType<StyleManager>();
        if (styleManager == null)
        {
            GameObject styleObj = new GameObject("StyleManager");
            styleManager = styleObj.AddComponent<StyleManager>();
        }

        // Get reference to audio manager
        audioManager = AudioManager.Instance;

        // Apply layouts early during scene load to prevent visual glitches
        if (forceApplyLayoutOnStart)
        {
            ApplyLayoutsToAllPanels();
            ApplyStyleToAllElements();
        }
    }

    private void Start()
    {
        // Set up button listeners
        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackButtonClicked);

        if (abilityButton != null)
            abilityButton.onClick.AddListener(OnAbilityButtonClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        // Hide all panels initially
        HideAllPanels();
    }

    // Apply style to all UI elements
    private void ApplyStyleToAllElements()
    {
        if (styleManager == null) return;

        // Style panels
        styleManager.StylePanel(actionPanel, "Action Panel");
        styleManager.StylePanel(targetSelectionPanel, "Select Target");
        styleManager.StylePanel(gameOverPanel, "Game Over");

        // Style buttons
        styleManager.StyleButton(attackButton, "Attack");
        styleManager.StyleButton(abilityButton, "Ability");
        styleManager.StyleButton(restartButton, "Restart");

        // Style text elements
        if (currentUnitNameText != null)
            styleManager.StyleText(currentUnitNameText, true);

        if (currentUnitHealthText != null)
            styleManager.StyleText(currentUnitHealthText);

        if (gameResultText != null)
            styleManager.StyleText(gameResultText, true);

        if (abilityButtonText != null)
            styleManager.StyleButtonText(abilityButtonText);
    }

    // Method to ensure layouts are applied (original method)
    public void ApplyLayoutsToAllPanels()
    {
        ApplyLayoutToPanel(actionPanel, actionPanelPosition);
        ApplyLayoutToPanel(targetSelectionPanel, targetPanelPosition);
        ApplyLayoutToPanel(gameOverPanel, gameOverPanelPosition);
    }

    // Original layout method with minor enhancements
    private void ApplyLayoutToPanel(GameObject panel, Vector2 position)
    {
        if (panel == null)
            return;

        // Add Vertical Layout Group
        VerticalLayoutGroup layoutGroup = panel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = panel.AddComponent<VerticalLayoutGroup>();

        // Configure layout group
        layoutGroup.padding = new RectOffset(15, 15, 15, 15); // Increased padding
        layoutGroup.spacing = 12; // Slightly increased spacing
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childScaleWidth = false;
        layoutGroup.childScaleHeight = false;

        // Add Content Size Fitter
        ContentSizeFitter sizeFitter = panel.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
            sizeFitter = panel.AddComponent<ContentSizeFitter>();

        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Position the panel
        if (repositionPanels)
        {
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = position;

                // Set a reasonable width
                rectTransform.sizeDelta = new Vector2(320, rectTransform.sizeDelta.y); // Slightly wider for better look
            }
        }

        // Apply layout elements to all children
        foreach (RectTransform child in panel.transform)
        {
            ConfigureChildElement(child);
        }
    }

    private void ConfigureChildElement(RectTransform child)
    {
        if (child == null)
            return;

        // Reset position and anchors
        child.anchoredPosition = Vector2.zero;
        child.anchorMin = new Vector2(0, 0);
        child.anchorMax = new Vector2(1, 0);
        child.pivot = new Vector2(0.5f, 0);

        // Add Layout Element component
        LayoutElement layoutElement = child.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = child.gameObject.AddComponent<LayoutElement>();

        // Set preferred height based on component type
        if (child.GetComponent<Button>() != null)
        {
            layoutElement.preferredHeight = 60;

            // Set text alignment for button text
            TextMeshProUGUI buttonText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.alignment = TextAlignmentOptions.Center;

                // Configure text rect transform
                RectTransform textRect = buttonText.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;
                }
            }
        }
        else if (child.GetComponent<Text>() != null)
        {
            layoutElement.preferredHeight = 40;
            child.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        }
        else if (child.GetComponent<TextMeshProUGUI>() != null)
        {
            bool isTitle = child.name.Contains("Title") || child.name.Contains("Result");
            layoutElement.preferredHeight = isTitle ? 50 : 40;
            child.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        }
        else
        {
            layoutElement.preferredHeight = 50;
        }
    }

    // Original methods for functionality
    public void UpdateUnitUI(PlayerUnit unit)
    {
        // Show action panel
        HideAllPanels();
        actionPanel.SetActive(true);

        // Update unit info
        if (currentUnitNameText != null)
            currentUnitNameText.text = unit.unitName;

        if (currentUnitHealthText != null)
        {
            // Style health text based on current health percentage
            float healthPercentage = (float)unit.currentHealth / unit.maxHealth;

            if (healthPercentage > 0.66f)
                currentUnitHealthText.color = styleManager.positiveTextColor;
            else if (healthPercentage > 0.33f)
                currentUnitHealthText.color = new Color(0.8f, 0.6f, 0.0f); // Yellow for medium health
            else
                currentUnitHealthText.color = styleManager.negativeTextColor;

            currentUnitHealthText.text = "HP: " + unit.currentHealth + "/" + unit.maxHealth;
        }

        // Update ability button
        if (abilityButtonText != null)
            abilityButtonText.text = GetAbilityName(unit);

        if (abilityButton != null)
        {
            abilityButton.interactable = unit.CanUseAbility();

            // Change visual appearance based on whether ability is available
            if (abilityButton.interactable)
            {
                Image btnImage = abilityButton.GetComponent<Image>();
                if (btnImage != null)
                    btnImage.color = styleManager.buttonNormalColor;

                if (abilityButtonText != null)
                    abilityButtonText.color = styleManager.buttonTextColor;
            }
            else
            {
                Image btnImage = abilityButton.GetComponent<Image>();
                if (btnImage != null)
                    btnImage.color = styleManager.buttonDisabledColor;

                if (abilityButtonText != null)
                    abilityButtonText.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }

    private string GetAbilityName(PlayerUnit unit)
    {
        // Return ability name based on unit type
        if (unit is Warrior) return "Whirlwind";
        if (unit is Knight) return "Shield Block";
        if (unit is Archer) return "Aimed Shot";
        if (unit is Mage) return "Fireball";
        if (unit is Rogue) return "Backstab";

        return "Ability";
    }

    private void OnAttackButtonClicked()
    {
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        // Add feedback animation on click
        if (styleManager != null)
            StartCoroutine(styleManager.ButtonClickAnimation(attackButton));

        // Show target selection UI
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(true);

        // Enable target selection mode
        if (CombatSystem.Instance != null)
            CombatSystem.Instance.StartTargetSelection(false);
    }

    private void OnAbilityButtonClicked()
    {
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        // Add feedback animation on click
        if (styleManager != null)
            StartCoroutine(styleManager.ButtonClickAnimation(abilityButton));

        // Show target selection UI
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(true);

        // Enable ability target selection mode
        if (CombatSystem.Instance != null)
            CombatSystem.Instance.StartTargetSelection(true);
    }

    public void ShowGameOverScreen(bool victory)
    {
        HideAllPanels();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Play appropriate sound
            if (audioManager != null)
            {
                if (victory)
                    audioManager.PlayVictorySound();
                else
                    audioManager.PlayDefeatSound();

                // Wait for game over sound to finish before playing menu music again
                StartCoroutine(PlayMenuMusicAfterDelay(3.0f));
            }

            // Add animation for game over screen
            if (styleManager != null)
                StartCoroutine(styleManager.PanelFadeInAnimation(gameOverPanel));
        }

        if (gameResultText != null)
        {
            // Update text and styling based on outcome
            gameResultText.text = victory ? "Victory!" : "Defeat!";
            gameResultText.color = victory ? styleManager.highlightTextColor : styleManager.negativeTextColor;
            gameResultText.fontSize += 4; // Make it larger

            // Add pulsing animation effect for victory
            if (victory)
                StartCoroutine(VictoryTextAnimation(gameResultText));
        }
    }

    private IEnumerator PlayMenuMusicAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (audioManager != null)
        {
            audioManager.PlayMusic(audioManager.menuTheme);
        }
    }

    private void OnRestartButtonClicked()
    {
        // Play button click sound
        if (audioManager != null)
        {
            audioManager.PlayButtonClickSound();
        }

        // Add feedback animation on click
        if (styleManager != null)
            StartCoroutine(styleManager.ButtonClickAnimation(restartButton));

        // Check if GameManager exists and use its restart method
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();

            // Reset all card detectors
            CardDetector[] allCards = FindObjectsByType<CardDetector>(FindObjectsSortMode.None);
            foreach (CardDetector card in allCards)
            {
                if (card != null)
                    card.ResetDetection();
            }
        }
        else
        {
            // Fallback to scene reload if GameManager doesn't exist
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void HideAllPanels()
    {
        // Hide all panels
        if (actionPanel != null)
            actionPanel.SetActive(false);

        if (targetSelectionPanel != null)
            targetSelectionPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    // Keeping a few custom animations that are specific to UIManager
    private System.Collections.IEnumerator VictoryTextAnimation(TextMeshProUGUI text)
    {
        if (text == null) yield break;

        float duration = 2f;
        float elapsed = 0;
        float pulseSpeed = 3f;
        float pulseAmount = 0.1f;

        Vector3 originalScale = text.transform.localScale;

        while (elapsed < duration)
        {
            float scale = 1 + Mathf.Sin(elapsed * pulseSpeed) * pulseAmount;
            text.transform.localScale = originalScale * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        text.transform.localScale = originalScale;
    }
}