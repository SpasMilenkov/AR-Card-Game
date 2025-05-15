using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

/// <summary>
/// GameInfoLayer provides themed real-time feedback to players about game state, actions, and events
/// Now uses StyleManager for consistent styling
/// </summary>
public class GameInfoLayer : MonoBehaviour
{
    #region Singleton
    // Singleton instance
    public static GameInfoLayer Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Find or create StyleManager
        styleManager = FindObjectOfType<StyleManager>();
        if (styleManager == null)
        {
            GameObject styleObj = new GameObject("StyleManager");
            styleManager = styleObj.AddComponent<StyleManager>();
        }
    }
    #endregion

    [Header("UI References")]
    public RectTransform gameStatePanel;
    public TextMeshProUGUI gameStateText;
    public RectTransform spawnInfoPanel;
    public TextMeshProUGUI spawnInfoText;
    public RectTransform battleInfoPanel;
    public TextMeshProUGUI battleInfoText;
    public RectTransform logPanel;
    public TextMeshProUGUI logText;

    [Header("UI Settings")]
    public int maxLogEntries = 20;
    public bool showTimestamps = true;
    public bool useColoredText = true;
    public bool debugMode = true;

    // Reference to the style manager
    private StyleManager styleManager;

    // Private fields for tracking game info
    private List<string> logEntries = new List<string>();
    private StringBuilder spawnInfo = new StringBuilder();
    private StringBuilder battleInfo = new StringBuilder();

    // Tracking of detected cards and spawned units
    private Dictionary<int, string> detectedCards = new Dictionary<int, string>();
    private List<string> playerUnits = new List<string>();
    private List<string> monsterUnits = new List<string>();

    // Battle statistics
    private Dictionary<string, int> damageDealt = new Dictionary<string, int>();
    private Dictionary<string, int> damageReceived = new Dictionary<string, int>();

    private void Start()
    {
        // Apply styles to panels
        ApplyStylesToAllPanels();

        // Initialize the UI content
        ClearAllInfo();
        AddLogEntry("Welcome to AR Card Battle! Place your cards to begin.");

        // Animate panel entrances
        if (styleManager != null && styleManager.useAnimatedEffects)
        {
            StartCoroutine(StagedPanelEntrance());
        }
    }

    private System.Collections.IEnumerator StagedPanelEntrance()
    {
        // Ensure all panels start invisible
        SetPanelAlpha(gameStatePanel, 0);
        SetPanelAlpha(spawnInfoPanel, 0);
        SetPanelAlpha(battleInfoPanel, 0);
        SetPanelAlpha(logPanel, 0);

        // Fade in panels one by one
        yield return StartCoroutine(FadeInPanel(gameStatePanel, 0.5f));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(FadeInPanel(spawnInfoPanel, 0.5f));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(FadeInPanel(battleInfoPanel, 0.5f));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(FadeInPanel(logPanel, 0.5f));
    }

    private void SetPanelAlpha(RectTransform panel, float alpha)
    {
        if (panel == null) return;

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = alpha;
    }

    private System.Collections.IEnumerator FadeInPanel(RectTransform panel, float duration)
    {
        if (panel == null) yield break;

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0;

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    /// <summary>
    /// Applies styling to all panels using StyleManager
    /// </summary>
    private void ApplyStylesToAllPanels()
    {
        if (styleManager == null) return;

        // Style panels with headers
        styleManager.StylePanel(gameStatePanel.gameObject, "Game State");
        styleManager.StylePanel(spawnInfoPanel.gameObject, "Spawn Info");
        styleManager.StylePanel(battleInfoPanel.gameObject, "Battle Info");
        styleManager.StylePanel(logPanel.gameObject, "Event Log");

        // Style text components
        if (gameStateText != null) styleManager.StyleText(gameStateText, true);
        if (spawnInfoText != null) styleManager.StyleText(spawnInfoText);
        if (battleInfoText != null) styleManager.StyleText(battleInfoText);
        if (logText != null) styleManager.StyleText(logText);
    }

    /// <summary>
    /// Updates the game state information display
    /// </summary>
    public void UpdateGameStateInfo(GameManager.GameState state)
    {
        if (gameStateText != null)
        {
            string stateText = "Current State: ";

            switch (state)
            {
                case GameManager.GameState.Setup:
                    stateText += "Setup - Place your cards";
                    break;
                case GameManager.GameState.PlayerTurn:
                    stateText += "Player Turn - Select an action";

                    // Highlight player turn with a subtle animation if enabled
                    if (styleManager != null && styleManager.useAnimatedEffects)
                        StartCoroutine(HighlightStateChange(gameStatePanel));
                    break;
                case GameManager.GameState.MonsterTurn:
                    stateText += "Monster Turn";

                    // Alert of monster turn with a different animation if enabled
                    if (styleManager != null && styleManager.useAnimatedEffects)
                        StartCoroutine(PulsePanelColor(gameStatePanel, new Color(0.8f, 0.5f, 0.3f)));
                    break;
                case GameManager.GameState.Victory:
                    stateText += "<color=#FFD700>Victory!</color>";

                    // Celebrate with animation if enabled
                    if (styleManager != null && styleManager.useAnimatedEffects)
                        StartCoroutine(VictoryAnimation(gameStatePanel));
                    break;
                case GameManager.GameState.Defeat:
                    stateText += "<color=#FF0000>Defeat!</color>";

                    // Visualize defeat if enabled
                    if (styleManager != null && styleManager.useAnimatedEffects)
                        StartCoroutine(DefeatAnimation(gameStatePanel));
                    break;
                default:
                    stateText += state.ToString();
                    break;
            }

            gameStateText.text = stateText;
        }
    }

    // Animation for state changes - keeping custom animations
    private System.Collections.IEnumerator HighlightStateChange(RectTransform panel)
    {
        if (panel == null) yield break;

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null) yield break;

        Color originalColor = panelImage.color;
        Color highlightColor = new Color(0.9f, 0.8f, 0.6f);

        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            panelImage.color = Color.Lerp(highlightColor, originalColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        panelImage.color = originalColor;
    }

    private System.Collections.IEnumerator PulsePanelColor(RectTransform panel, Color pulseColor)
    {
        if (panel == null) yield break;

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null) yield break;

        Color originalColor = panelImage.color;

        // Pulse twice
        for (int i = 0; i < 2; i++)
        {
            // Fade to pulse color
            float duration = 0.3f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                panelImage.color = Color.Lerp(originalColor, pulseColor, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Fade back to original
            elapsed = 0;
            while (elapsed < duration)
            {
                panelImage.color = Color.Lerp(pulseColor, originalColor, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }

        panelImage.color = originalColor;
    }

    private System.Collections.IEnumerator VictoryAnimation(RectTransform panel)
    {
        if (panel == null) yield break;

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null) yield break;

        Color originalColor = panelImage.color;
        Color victoryColor = new Color(1f, 0.9f, 0.2f);

        // Golden glow effect
        float duration = 1.0f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            panelImage.color = Color.Lerp(originalColor, victoryColor, Mathf.PingPong(elapsed * 2, 1));
            elapsed += Time.deltaTime;
            yield return null;
        }

        panelImage.color = originalColor;
    }

    private System.Collections.IEnumerator DefeatAnimation(RectTransform panel)
    {
        if (panel == null) yield break;

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null) yield break;

        Color originalColor = panelImage.color;
        Color defeatColor = new Color(0.8f, 0.2f, 0.2f);

        // Red flash effect
        float duration = 0.8f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            panelImage.color = Color.Lerp(defeatColor, originalColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        panelImage.color = originalColor;
    }

    /// <summary>
    /// Register when a card is detected
    /// </summary>
    public void RegisterCardDetection(int cardType, int cardIndex)
    {
        string cardName = GetUnitTypeName(cardType);
        detectedCards[cardIndex] = cardName;

        UpdateSpawnInfo();

        // Use color-coded log entry
        if (useColoredText && styleManager != null)
        {
            AddLogEntry($"Card detected: <color=#{ColorUtility.ToHtmlStringRGB(styleManager.highlightTextColor)}>{cardName}</color>");
        }
        else
        {
            AddLogEntry($"Card detected: {cardName}");
        }

        // Add visual feedback
        if (styleManager != null && styleManager.useAnimatedEffects)
            StartCoroutine(styleManager.FlashPanel(spawnInfoPanel, new Color(0.9f, 0.8f, 0.4f)));
    }

    /// <summary>
    /// Register when a unit is spawned (player or monster)
    /// </summary>
    public void RegisterUnitSpawn(string unitName, bool isPlayer)
    {
        if (isPlayer)
        {
            playerUnits.Add(unitName);
        }
        else
        {
            monsterUnits.Add(unitName);
        }

        UpdateSpawnInfo();

        // Use color-coded log entry
        string teamType = isPlayer ? "Player" : "Monster";
        Color textColor = isPlayer ?
            (styleManager != null ? styleManager.positiveTextColor : Color.green) :
            (styleManager != null ? styleManager.negativeTextColor : Color.red);

        if (useColoredText)
        {
            AddLogEntry($"{teamType} unit spawned: <color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{unitName}</color>");
        }
        else
        {
            AddLogEntry($"{teamType} unit spawned: {unitName}");
        }

        // Add spawn animation effect
        if (styleManager != null && styleManager.useAnimatedEffects)
        {
            Color flashColor = isPlayer ? new Color(0.4f, 0.7f, 0.4f) : new Color(0.7f, 0.4f, 0.4f);
            StartCoroutine(styleManager.FlashPanel(spawnInfoPanel, flashColor));
        }
    }

    /// <summary>
    /// Register a battle action (attack, ability, etc.)
    /// </summary>
    public void RegisterBattleAction(string sourceName, string targetName, string actionType, int amount)
    {
        // Format the battle action text
        string actionText;

        if (useColoredText && styleManager != null)
        {
            string sourceColor = ColorUtility.ToHtmlStringRGB(styleManager.highlightTextColor);
            string targetColor = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);
            string damageColor = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);

            actionText = $"<color=#{sourceColor}>{sourceName}</color> " +
                        $"{actionType} <color=#{targetColor}>{targetName}</color> for " +
                        $"<color=#{damageColor}>{amount} damage</color>";
        }
        else
        {
            actionText = $"{sourceName} {actionType} {targetName} for {amount} damage";
        }

        AddLogEntry(actionText);

        // Update damage stats
        if (!damageDealt.ContainsKey(sourceName))
            damageDealt[sourceName] = 0;

        if (!damageReceived.ContainsKey(targetName))
            damageReceived[targetName] = 0;

        damageDealt[sourceName] += amount;
        damageReceived[targetName] += amount;

        UpdateBattleInfo();

        // Add battle action visual effects if enabled
        if (styleManager != null && styleManager.useAnimatedEffects)
        {
            StartCoroutine(styleManager.FlashPanel(battleInfoPanel, new Color(0.8f, 0.6f, 0.3f)));
            StartCoroutine(ScrollLogEffect());
        }
    }

    // Visual effect for log entries
    private System.Collections.IEnumerator ScrollLogEffect()
    {
        if (logText == null) yield break;

        RectTransform logRect = logText.GetComponent<RectTransform>();
        if (logRect == null) yield break;

        // Get original position
        Vector3 originalPosition = logRect.localPosition;

        // Small jump effect
        float jumpHeight = 10f;
        float jumpDuration = 0.3f;
        float elapsed = 0;

        while (elapsed < jumpDuration)
        {
            float yOffset = jumpHeight * Mathf.Sin(elapsed / jumpDuration * Mathf.PI);
            logRect.localPosition = originalPosition + new Vector3(0, yOffset, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to original position
        logRect.localPosition = originalPosition;
    }

    /// <summary>
    /// Register when a unit is defeated
    /// </summary>
    public void RegisterUnitDeath(string unitName, bool isPlayer)
    {
        string deathText;

        if (useColoredText && styleManager != null)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);
            deathText = $"<color=#{colorHex}>{unitName} was defeated!</color>";
        }
        else
        {
            deathText = $"{unitName} was defeated!";
        }

        AddLogEntry(deathText);
        UpdateBattleInfo();

        // Add defeat visual effect if enabled
        if (styleManager != null && styleManager.useAnimatedEffects)
            StartCoroutine(UnitDefeatEffect(battleInfoPanel));
    }

    // Visual feedback for unit defeat
    private System.Collections.IEnumerator UnitDefeatEffect(RectTransform panel)
    {
        if (panel == null) yield break;

        // Shake effect
        Vector2 originalPosition = panel.anchoredPosition;
        float shakeAmount = 5f;
        float shakeDuration = 0.5f;
        float elapsed = 0;

        while (elapsed < shakeDuration)
        {
            float strength = (shakeDuration - elapsed) / shakeDuration; // Fade out shake

            // Random shake offset
            Vector2 offset = new Vector2(
                Random.Range(-shakeAmount, shakeAmount) * strength,
                Random.Range(-shakeAmount, shakeAmount) * strength
            );

            panel.anchoredPosition = originalPosition + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset to original position
        panel.anchoredPosition = originalPosition;
    }

    /// <summary>
    /// Update the spawn information display
    /// </summary>
    public void UpdateSpawnInfo()
    {
        if (spawnInfoText != null)
        {
            spawnInfo.Clear();

            // Cards detected section with styled text
            if (useColoredText && styleManager != null)
            {
                spawnInfo.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(styleManager.headerTextColor)}><b>Cards Detected:</b></color>");
            }
            else
            {
                spawnInfo.AppendLine("Cards Detected:");
            }

            if (detectedCards.Count == 0)
            {
                spawnInfo.AppendLine("  None");
            }
            else
            {
                foreach (var card in detectedCards)
                {
                    if (useColoredText && styleManager != null)
                    {
                        spawnInfo.AppendLine($"  <color=#{ColorUtility.ToHtmlStringRGB(styleManager.highlightTextColor)}>{card.Value}</color>");
                    }
                    else
                    {
                        spawnInfo.AppendLine($"  {card.Value}");
                    }
                }
            }

            // Add decorative divider text
            spawnInfo.AppendLine("\n----- ----- -----\n");

            // Player units section
            if (useColoredText && styleManager != null)
            {
                spawnInfo.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(styleManager.headerTextColor)}><b>Player Units:</b></color>");
            }
            else
            {
                spawnInfo.AppendLine("Player Units:");
            }

            if (playerUnits.Count == 0)
            {
                spawnInfo.AppendLine("  None");
            }
            else
            {
                foreach (var unit in playerUnits)
                {
                    if (useColoredText && styleManager != null)
                    {
                        spawnInfo.AppendLine($"  <color=#{ColorUtility.ToHtmlStringRGB(styleManager.positiveTextColor)}>{unit}</color>");
                    }
                    else
                    {
                        spawnInfo.AppendLine($"  {unit}");
                    }
                }
            }

            // Add decorative divider text
            spawnInfo.AppendLine("\n----- ----- -----\n");

            // Monster units section
            if (useColoredText && styleManager != null)
            {
                spawnInfo.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(styleManager.headerTextColor)}><b>Monster Units:</b></color>");
            }
            else
            {
                spawnInfo.AppendLine("Monster Units:");
            }

            if (monsterUnits.Count == 0)
            {
                spawnInfo.AppendLine("  None");
            }
            else
            {
                foreach (var unit in monsterUnits)
                {
                    if (useColoredText && styleManager != null)
                    {
                        spawnInfo.AppendLine($"  <color=#{ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor)}>{unit}</color>");
                    }
                    else
                    {
                        spawnInfo.AppendLine($"  {unit}");
                    }
                }
            }

            // Display remaining cards needed with highlighted text
            int cardsNeeded = 3 - playerUnits.Count;
            if (cardsNeeded > 0)
            {
                spawnInfo.AppendLine();

                if (useColoredText && styleManager != null)
                {
                    spawnInfo.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(styleManager.highlightTextColor)}><b>Place {cardsNeeded} more card(s) to begin battle</b></color>");
                }
                else
                {
                    spawnInfo.AppendLine($"Place {cardsNeeded} more card(s) to begin battle");
                }
            }

            spawnInfoText.text = spawnInfo.ToString();
        }
    }

    /// <summary>
    /// Update the battle information display
    /// </summary>
    public void UpdateBattleInfo()
    {
        if (battleInfoText != null && GameManager.Instance != null)
        {
            battleInfo.Clear();

            // Current active unit info
            if (GameManager.Instance.currentState == GameManager.GameState.PlayerTurn &&
                GameManager.Instance.currentActiveUnit != null)
            {
                PlayerUnit active = GameManager.Instance.currentActiveUnit;

                if (useColoredText && styleManager != null)
                {
                    battleInfo.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(styleManager.headerTextColor)}><b>Active Unit:</b></color> " +
                                        $"<color=#{ColorUtility.ToHtmlStringRGB(styleManager.highlightTextColor)}>{active.unitName}</color>");

                    // Health with color based on percentage
                    float healthPercent = (float)active.currentHealth / active.maxHealth;
                    string healthColorHex;

                    if (healthPercent > 0.66f)
                        healthColorHex = ColorUtility.ToHtmlStringRGB(styleManager.positiveTextColor);
                    else if (healthPercent > 0.33f)
                        healthColorHex = ColorUtility.ToHtmlStringRGB(new Color(0.9f, 0.6f, 0.1f)); // Orange
                    else
                        healthColorHex = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);

                    battleInfo.AppendLine($"Health: <color=#{healthColorHex}>{active.currentHealth}/{active.maxHealth}</color>");

                    // Ability status
                    string abilityColorHex = active.CanUseAbility() ?
                                           ColorUtility.ToHtmlStringRGB(styleManager.positiveTextColor) :
                                           ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);

                    string abilityStatus = active.CanUseAbility() ?
                                         "Ready" :
                                         $"Cooldown ({active.currentCooldown})";

                    battleInfo.AppendLine($"Ability: <color=#{abilityColorHex}>{abilityStatus}</color>");
                }
                else
                {
                    battleInfo.AppendLine($"Active Unit: {active.unitName}");
                    battleInfo.AppendLine($"Health: {active.currentHealth}/{active.maxHealth}");
                    battleInfo.AppendLine($"Ability: {(active.CanUseAbility() ? "Ready" : $"Cooldown ({active.currentCooldown})")}");
                }
            }

            // Add decorative divider text
            battleInfo.AppendLine("\n----- ----- -----\n");

            // Unit status section
            if (useColoredText && styleManager != null)
            {
                battleInfo.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(styleManager.headerTextColor)}><b>Unit Status:</b></color>");
            }
            else
            {
                battleInfo.AppendLine("Unit Status:");
            }

            // Player units status
            foreach (var unit in GameManager.Instance.playerUnits)
            {
                if (unit != null)
                {
                    if (useColoredText && styleManager != null)
                    {
                        string unitColorHex = ColorUtility.ToHtmlStringRGB(styleManager.highlightTextColor);
                        string statusColorHex;
                        string statusText;

                        if (unit.isAlive)
                        {
                            float healthPercent = (float)unit.currentHealth / unit.maxHealth;

                            if (healthPercent > 0.66f)
                                statusColorHex = ColorUtility.ToHtmlStringRGB(styleManager.positiveTextColor);
                            else if (healthPercent > 0.33f)
                                statusColorHex = ColorUtility.ToHtmlStringRGB(new Color(0.9f, 0.6f, 0.1f));
                            else
                                statusColorHex = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);

                            statusText = $"HP: {unit.currentHealth}/{unit.maxHealth}";
                        }
                        else
                        {
                            statusColorHex = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);
                            statusText = "DEFEATED";
                        }

                        battleInfo.AppendLine($"  <color=#{unitColorHex}>{unit.unitName}</color>: <color=#{statusColorHex}>{statusText}</color>");
                    }
                    else
                    {
                        string statusText = unit.isAlive ?
                                          $"HP: {unit.currentHealth}/{unit.maxHealth}" : "DEFEATED";

                        battleInfo.AppendLine($"  {unit.unitName}: {statusText}");
                    }
                }
            }

            // Add mini divider
            battleInfo.AppendLine("\n- - - - -\n");

            // Monster units status
            foreach (var unit in GameManager.Instance.monsterUnits)
            {
                if (unit != null)
                {
                    if (useColoredText && styleManager != null)
                    {
                        string unitColorHex = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);
                        string statusColorHex;
                        string statusText;

                        if (unit.isAlive)
                        {
                            float healthPercent = (float)unit.currentHealth / unit.maxHealth;

                            if (healthPercent > 0.66f)
                                statusColorHex = ColorUtility.ToHtmlStringRGB(styleManager.positiveTextColor);
                            else if (healthPercent > 0.33f)
                                statusColorHex = ColorUtility.ToHtmlStringRGB(new Color(0.9f, 0.6f, 0.1f));
                            else
                                statusColorHex = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);

                            statusText = $"HP: {unit.currentHealth}/{unit.maxHealth}";
                        }
                        else
                        {
                            statusColorHex = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);
                            statusText = "DEFEATED";
                        }

                        battleInfo.AppendLine($"  <color=#{unitColorHex}>{unit.unitName}</color>: <color=#{statusColorHex}>{statusText}</color>");
                    }
                    else
                    {
                        string statusText = unit.isAlive ?
                                          $"HP: {unit.currentHealth}/{unit.maxHealth}" : "DEFEATED";

                        battleInfo.AppendLine($"  {unit.unitName}: {statusText}");
                    }
                }
            }

            // Display battle stats if any
            if (damageDealt.Count > 0 || damageReceived.Count > 0)
            {
                battleInfo.AppendLine("\n----- ----- -----\n");

                if (useColoredText && styleManager != null)
                {
                    battleInfo.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(styleManager.headerTextColor)}><b>Battle Stats:</b></color>");
                }
                else
                {
                    battleInfo.AppendLine("Battle Stats:");
                }

                // Display top damage dealer if any
                if (damageDealt.Count > 0)
                {
                    // Find top damage dealer
                    string topDealer = "";
                    int topDamage = 0;

                    foreach (var entry in damageDealt)
                    {
                        if (entry.Value > topDamage)
                        {
                            topDealer = entry.Key;
                            topDamage = entry.Value;
                        }
                    }

                    if (useColoredText && styleManager != null)
                    {
                        string highlightHex = ColorUtility.ToHtmlStringRGB(styleManager.highlightTextColor);
                        string damageHex = ColorUtility.ToHtmlStringRGB(styleManager.negativeTextColor);

                        battleInfo.AppendLine($"  Top Damage: <color=#{highlightHex}>{topDealer}</color> (<color=#{damageHex}>{topDamage}</color>)");
                    }
                    else
                    {
                        battleInfo.AppendLine($"  Top Damage: {topDealer} ({topDamage})");
                    }
                }
            }

            battleInfoText.text = battleInfo.ToString();
        }
    }

    /// <summary>
    /// Add an entry to the event log
    /// </summary>
    public void AddLogEntry(string message)
    {
        // Format with timestamp if enabled
        string entry;

        if (showTimestamps)
        {
            string timeString = System.DateTime.Now.ToString("HH:mm:ss");

            if (useColoredText)
            {
                // Gray-brown timestamp color
                string timeColor = ColorUtility.ToHtmlStringRGB(new Color(0.6f, 0.5f, 0.4f));
                entry = $"<color=#{timeColor}>[{timeString}]</color> {message}";
            }
            else
            {
                entry = $"[{timeString}] {message}";
            }
        }
        else
        {
            entry = message;
        }

        // Add to log entries
        logEntries.Add(entry);

        // Keep log at max length
        while (logEntries.Count > maxLogEntries)
        {
            logEntries.RemoveAt(0);
        }

        // Update log text
        UpdateLogText();
    }

    /// <summary>
    /// Updates the log text UI with current entries
    /// </summary>
    private void UpdateLogText()
    {
        if (logText != null)
        {
            StringBuilder log = new StringBuilder();

            if (useColoredText && styleManager != null)
            {
                string headerHex = ColorUtility.ToHtmlStringRGB(styleManager.headerTextColor);
                log.AppendLine($"<color=#{headerHex}><b>Event Log:</b></color>");
            }
            else
            {
                log.AppendLine("Event Log:");
            }

            log.AppendLine();

            foreach (string entry in logEntries)
            {
                log.AppendLine(entry);
            }

            logText.text = log.ToString();
        }
    }

    /// <summary>
    /// Clear all information displays
    /// </summary>
    public void ClearAllInfo()
    {
        logEntries.Clear();
        detectedCards.Clear();
        playerUnits.Clear();
        monsterUnits.Clear();
        damageDealt.Clear();
        damageReceived.Clear();

        if (gameStateText != null)
        {
            if (useColoredText && styleManager != null)
            {
                string stateHex = ColorUtility.ToHtmlStringRGB(styleManager.headerTextColor);
                gameStateText.text = $"Current State: <color=#{stateHex}>Setup - Place your cards</color>";
            }
            else
            {
                gameStateText.text = "Current State: Setup - Place your cards";
            }
        }

        if (spawnInfoText != null)
        {
            if (useColoredText && styleManager != null)
            {
                string promptHex = ColorUtility.ToHtmlStringRGB(styleManager.highlightTextColor);
                spawnInfoText.text = $"<color=#{promptHex}>Place your cards to spawn units</color>";
            }
            else
            {
                spawnInfoText.text = "Place your cards to spawn units";
            }
        }

        if (battleInfoText != null)
        {
            if (useColoredText && styleManager != null)
            {
                string infoHex = ColorUtility.ToHtmlStringRGB(styleManager.standardTextColor);
                battleInfoText.text = $"<color=#{infoHex}>Battle not started</color>";
            }
            else
            {
                battleInfoText.text = "Battle not started";
            }
        }

        if (logText != null)
        {
            UpdateLogText();
        }

        // Highlight all panels when reset if effects are enabled
        if (styleManager != null && styleManager.useAnimatedEffects)
        {
            StartCoroutine(ResetHighlightAllPanels());
        }
    }

    private System.Collections.IEnumerator ResetHighlightAllPanels()
    {
        // Highlight each panel one by one
        if (styleManager != null)
        {
            Color highlightColor = new Color(0.9f, 0.8f, 0.6f);

            yield return StartCoroutine(styleManager.FlashPanel(gameStatePanel, highlightColor));
            yield return new WaitForSeconds(0.1f);

            yield return StartCoroutine(styleManager.FlashPanel(spawnInfoPanel, highlightColor));
            yield return new WaitForSeconds(0.1f);

            yield return StartCoroutine(styleManager.FlashPanel(battleInfoPanel, highlightColor));
            yield return new WaitForSeconds(0.1f);

            yield return StartCoroutine(styleManager.FlashPanel(logPanel, highlightColor));
        }
    }

    /// <summary>
    /// Helper method to get unit type name from card type
    /// </summary>
    private string GetUnitTypeName(int cardType)
    {
        string[] unitTypeNames =
        {
            "Archer", "Knight", "Mage", "Warrior", "Rogue"
        };

        if (cardType >= 0 && cardType < unitTypeNames.Length)
        {
            return unitTypeNames[cardType];
        }
        return $"Unknown ({cardType})";
    }
}