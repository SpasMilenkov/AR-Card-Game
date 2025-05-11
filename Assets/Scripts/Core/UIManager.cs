using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    public bool forceApplyLayoutOnStart = true;  // Enable this to force layouts on mobile
    public bool repositionPanels = true;         // Enable to position panels
    public Vector2 actionPanelPosition = new Vector2(0, -300);
    public Vector2 targetPanelPosition = new Vector2(0, -300);
    public Vector2 gameOverPanelPosition = new Vector2(0, 0);

    private void Awake()
    {
        // Apply layouts early during scene load to prevent visual glitches
        if (forceApplyLayoutOnStart)
        {
            ApplyLayoutsToAllPanels();
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

    // Method to ensure layouts are applied
    public void ApplyLayoutsToAllPanels()
    {
        ApplyLayoutToPanel(actionPanel, actionPanelPosition);
        ApplyLayoutToPanel(targetSelectionPanel, targetPanelPosition);
        ApplyLayoutToPanel(gameOverPanel, gameOverPanelPosition);
    }

    private void ApplyLayoutToPanel(GameObject panel, Vector2 position)
    {
        if (panel == null)
            return;

        // Add Vertical Layout Group
        VerticalLayoutGroup layoutGroup = panel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = panel.AddComponent<VerticalLayoutGroup>();

        // Configure layout group
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.spacing = 10;
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
                rectTransform.sizeDelta = new Vector2(300, rectTransform.sizeDelta.y);
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

    public void UpdateUnitUI(PlayerUnit unit)
    {
        // Show action panel
        HideAllPanels();
        actionPanel.SetActive(true);

        // Update unit info
        if (currentUnitNameText != null)
            currentUnitNameText.text = unit.unitName;

        if (currentUnitHealthText != null)
            currentUnitHealthText.text = "HP: " + unit.currentHealth + "/" + unit.maxHealth;

        // Update ability button
        if (abilityButtonText != null)
            abilityButtonText.text = GetAbilityName(unit);

        if (abilityButton != null)
            abilityButton.interactable = unit.CanUseAbility();
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
        // Show target selection UI
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(true);

        // Enable target selection mode
        if (CombatSystem.Instance != null)
            CombatSystem.Instance.StartTargetSelection(false);
    }

    private void OnAbilityButtonClicked()
    {
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
            gameOverPanel.SetActive(true);

        if (gameResultText != null)
            gameResultText.text = victory ? "Victory!" : "Defeat!";
    }

    private void OnRestartButtonClicked()
    {
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
}