using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

/// <summary>
/// GameInfoLayer provides real-time feedback to players about game state, actions, and events
/// It acts as a HUD element that can be displayed on screen or in worldspace
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
    public bool useColoredPanels = true;
    public bool debugMode = true;

    [Header("Panel Colors")]
    public Color gameStatePanelColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
    public Color spawnInfoPanelColor = new Color(0.2f, 0.3f, 0.2f, 0.8f);
    public Color battleInfoPanelColor = new Color(0.3f, 0.2f, 0.2f, 0.8f);
    public Color logPanelColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);

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
        // Apply panel colors if enabled
        if (useColoredPanels)
        {
            ApplyPanelColors();
        }

        // Initialize the UI content
        ClearAllInfo();
        AddLogEntry("Welcome to AR Card Battle! Place your cards to begin.");
    }

    /// <summary>
    /// Applies colors to all panels for better distinction
    /// </summary>
    private void ApplyPanelColors()
    {
        ApplyColorToPanel(gameStatePanel, gameStatePanelColor);
        ApplyColorToPanel(spawnInfoPanel, spawnInfoPanelColor);
        ApplyColorToPanel(battleInfoPanel, battleInfoPanelColor);
        ApplyColorToPanel(logPanel, logPanelColor);
    }

    /// <summary>
    /// Applies a color to a panel
    /// </summary>
    private void ApplyColorToPanel(RectTransform panel, Color color)
    {
        if (panel == null) return;

        Image image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }
        else
        {
            // If panel doesn't have an image component, add one
            image = panel.gameObject.AddComponent<Image>();
            image.color = color;
        }
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
                    break;
                case GameManager.GameState.MonsterTurn:
                    stateText += "Monster Turn";
                    break;
                case GameManager.GameState.Victory:
                    stateText += "Victory!";
                    break;
                case GameManager.GameState.Defeat:
                    stateText += "Defeat!";
                    break;
                default:
                    stateText += state.ToString();
                    break;
            }

            gameStateText.text = stateText;
        }
    }

    /// <summary>
    /// Register when a card is detected
    /// </summary>
    public void RegisterCardDetection(int cardType, int cardIndex)
    {
        string cardName = GetUnitTypeName(cardType);
        detectedCards[cardIndex] = cardName;

        UpdateSpawnInfo();
        AddLogEntry($"Card detected: {cardName}");
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
    }

    /// <summary>
    /// Register a battle action (attack, ability, etc.)
    /// </summary>
    public void RegisterBattleAction(string sourceName, string targetName, string actionType, int amount)
    {
        string actionText = $"{sourceName} {actionType} {targetName} for {amount} damage";
        AddLogEntry(actionText);

        // Update damage stats
        if (!damageDealt.ContainsKey(sourceName))
            damageDealt[sourceName] = 0;

        if (!damageReceived.ContainsKey(targetName))
            damageReceived[targetName] = 0;

        damageDealt[sourceName] += amount;
        damageReceived[targetName] += amount;

        UpdateBattleInfo();
    }

    /// <summary>
    /// Register when a unit is defeated
    /// </summary>
    public void RegisterUnitDeath(string unitName, bool isPlayer)
    {
        string deathText = $"{unitName} was defeated!";
        AddLogEntry(deathText);

        UpdateBattleInfo();
    }

    /// <summary>
    /// Update the spawn information display
    /// </summary>
    public void UpdateSpawnInfo()
    {
        if (spawnInfoText != null)
        {
            spawnInfo.Clear();

            // Cards detected
            spawnInfo.AppendLine("Cards Detected:");
            if (detectedCards.Count == 0)
            {
                spawnInfo.AppendLine("  None");
            }
            else
            {
                foreach (var card in detectedCards)
                {
                    spawnInfo.AppendLine($"  {card.Value}");
                }
            }

            // Player units
            spawnInfo.AppendLine("\nPlayer Units:");
            if (playerUnits.Count == 0)
            {
                spawnInfo.AppendLine("  None");
            }
            else
            {
                foreach (var unit in playerUnits)
                {
                    spawnInfo.AppendLine($"  {unit}");
                }
            }

            // Monster units
            spawnInfo.AppendLine("\nMonster Units:");
            if (monsterUnits.Count == 0)
            {
                spawnInfo.AppendLine("  None");
            }
            else
            {
                foreach (var unit in monsterUnits)
                {
                    spawnInfo.AppendLine($"  {unit}");
                }
            }

            // Display remaining cards needed
            int cardsNeeded = 3 - playerUnits.Count;
            if (cardsNeeded > 0)
            {
                spawnInfo.AppendLine($"\nPlace {cardsNeeded} more card(s) to begin battle");
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

            // Current active unit
            if (GameManager.Instance.currentState == GameManager.GameState.PlayerTurn &&
                GameManager.Instance.currentActiveUnit != null)
            {
                PlayerUnit active = GameManager.Instance.currentActiveUnit;
                battleInfo.AppendLine($"Active Unit: {active.unitName}");
                battleInfo.AppendLine($"Health: {active.currentHealth}/{active.maxHealth}");

                // Ability info
                if (active.CanUseAbility())
                {
                    battleInfo.AppendLine("Ability: Ready");
                }
                else
                {
                    battleInfo.AppendLine($"Ability: Cooldown ({active.currentCooldown})");
                }
            }

            // Unit status
            battleInfo.AppendLine("\nUnit Status:");

            // Player units status
            foreach (var unit in GameManager.Instance.playerUnits)
            {
                if (unit != null)
                {
                    string statusText = unit.isAlive ?
                        $"HP: {unit.currentHealth}/{unit.maxHealth}" : "DEFEATED";

                    battleInfo.AppendLine($"  {unit.unitName}: {statusText}");
                }
            }

            // Monster units status
            foreach (var unit in GameManager.Instance.monsterUnits)
            {
                if (unit != null)
                {
                    string statusText = unit.isAlive ?
                        $"HP: {unit.currentHealth}/{unit.maxHealth}" : "DEFEATED";

                    battleInfo.AppendLine($"  {unit.unitName}: {statusText}");
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
        string entry = showTimestamps ?
            $"[{System.DateTime.Now.ToString("HH:mm:ss")}] {message}" : message;

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
            log.AppendLine("Event Log:");

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
            gameStateText.text = "Current State: Setup - Place your cards";

        if (spawnInfoText != null)
            spawnInfoText.text = "No units spawned";

        if (battleInfoText != null)
            battleInfoText.text = "Battle not started";

        if (logText != null)
            logText.text = "";
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